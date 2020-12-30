﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Linq;
using System.Net;
using System.IO;
using System;

using Communicator.Services;
using Communicator.DTOs;

namespace Communicator.WebSockets
{
	public class WebSocketHandlerImpl : IWebSocketHandler
	{
		private readonly Dictionary<int, WebSocket> _webSocketList;

		public WebSocketHandlerImpl()
		{
			_webSocketList = new Dictionary<int, WebSocket>();
		}

		public async Task Handle(ISession session, WebSocket webSocket, CommunicatorDbContex db)
		{
			var buffer = new byte[1024 * 4];
			WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			while (!result.CloseStatus.HasValue)
			{
				var response = await Task.Run(() => ProcessRequest(session, webSocket, db, buffer));
				Array.Clear(buffer, 0, buffer.Length);

				await webSocket.SendAsync(new ArraySegment<byte>(response), result.MessageType, result.EndOfMessage, CancellationToken.None);
				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
			_webSocketList.Remove(GetID(webSocket));
		}

		private byte[] ProcessRequest(ISession session, WebSocket webSocket, CommunicatorDbContex db, byte[] bytes)
		{
			var friendRelationService = new FriendRelationServiceImpl(db);
			var messageSercive = new MessageSerciveImpl(db);
			var paymentService = new PaymentServiceImpl(db);
			var userService = new UserServiceImpl(db);

			JObject json;
			try
			{
				json = JObject.Parse(Encoding.UTF8.GetString(bytes));
			}
			catch
			{
				return GetErrorResponse(bytes);
			}

			var data = json.SelectToken("data");
			var request = json.Value<string>("dataType");
			request = request.Substring(0, request.LastIndexOf("Request"));

			int? id = session.GetInt32("userId");
			if (id == null)
			{
				return ProcessLoggedOutUserRequest(userService, request, data);
			}

			_webSocketList[(int)id] = webSocket;
			return request.Equals("LogOut") ?
				Logout(session, request, (int)id) :
				ProcessLoggedInUserRequest(friendRelationService, messageSercive, paymentService, userService, request, data, (int)id);
		}

		private byte[] Logout(ISession session, string request, int id)
		{
			session.Clear();
			_webSocketList.Remove(id);
			return GetResponse(request, ErrorCodes.OK);
		}

		private static byte[] ProcessLoggedOutUserRequest(IUserService userService, string request, JToken data)
		{
			return request switch
			{
				"Echo" => Encoding.UTF8.GetBytes(data.ToString()),
				"IsLoggedInRequest" => GetResponse(request, ErrorCodes.NOT_LOGGED_IN),
				"Register" => GetResponse(request, userService.Add(data.ToObject<UserCreateNewRequest>())),
				_ => GetResponse(request, string.Format(ErrorCodes.CANNOT_FIND_REQUEST_OR_UNAUTHORIZED, request)),
			};
		}

		private static byte[] ProcessLoggedInUserRequest(
			IFriendRelationService friendRelationService, IMessageService messageService, IPaymentService paymentService, IUserService userService,
			string request, JToken data, int userId)
		{
			return request switch
			{
				//General
				"Echo" => Encoding.UTF8.GetBytes(data.ToString()),
				"IsLoggedInRequest" => GetResponse(request, ErrorCodes.OK),
				"Register" => GetResponse(request, ErrorCodes.REGISTER_WHILE_LOGGED_IN),
				//FriendList
				"AddFriend" => GetResponse(request, friendRelationService.Add(
					CreateRequest(userId, data.First.ToObject<int>()))),
				"AcceptFriend" => GetResponse(request, friendRelationService.Accept(
					CreateRequest(userId, data.First.ToObject<int>()))),
				"RemoveFriend" => GetResponse(request, friendRelationService.Delete(
					CreateRequest(userId, data.First.ToObject<int>()))),
				"GetFriendList" => GetResponse(request, friendRelationService.GetFriendList(
					userId, true)),
				"GetPendingFriendList" => GetResponse(request, friendRelationService.GetFriendList(
					userId, true)),
				//Messages
				"SendMessage" => GetResponse(request, messageService.Add(
					userId, data.ToObject<MessageCreateNewRequest>())),
				"DeleteMessage" => GetResponse(request, messageService.Delete(
					data.First.ToObject<int>())),
				"SetMessageSeen" => GetResponse(request, messageService.UpdateSeen(
					data.First.ToObject<int>())),
				"UpdateMessageContent" => GetResponse(request, messageService.UpdateContent(
					data.SelectToken("messageId").ToObject<int>(),
					data.SelectToken("messageContent").ToObject<string>())),
				"GetMessage" => GetResponse(request, messageService.GetByID(
					userId, data.First.ToObject<int>())),
				"GetMessagesBatch" => GetResponse(request, messageService.GetBatch(
					userId, data.ToObject<MessageGetBatchRequest>())),
				//Payment
				"MakePayment" => MakePayment(paymentService, request, data, userId),
				//User
				"DeleteUser" => GetResponse(request, userService.Delete(userId)),
				"UpdateBankAccount" => GetResponse(request, userService.UpdateBankAccount(
					userId, data.First.ToObject<string>())),
				"UpdateUserCredentials" => GetResponse(request, userService.UpdateCredentials(
					userId, data.ToObject<UserUpdateCredentialsRequest>())),
				"GetUser" => GetResponse(request, userService.GetByID(
					data.First.ToObject<int>())),
				_ => GetResponse(request, string.Format(ErrorCodes.CANNOT_FIND_REQUEST, request)),
			};
		}

		private static byte[] MakePayment(IPaymentService paymentService, string requestName, JToken data, int userId)
		{
			var card = data.SelectToken("card");
			var request = new PaymentRequest
			{
				Amount = data.Value<float>("amount"),
				CardCode = card.Value<int>("cvv"),
				CardNumber = card.Value<int>("number"),
				ExpirationDate = card.Value<string>("expirationDate")
			};

			var response = paymentService.Add(userId, request);

			if (!response.Response.Equals(ErrorCodes.OK))
			{
				return GetResponse(requestName, response);
			}

			var authorizeNetResponse = SendAuthorizeNetRequest(request);
			if (!authorizeNetResponse.Equals(ErrorCodes.OK))
			{
				paymentService.UpdateStatus(response.ID, false);
				return GetResponse(requestName, authorizeNetResponse);
			}
			return GetResponse(requestName, paymentService.UpdateStatus(response.ID, true));
		}

		private static string SendAuthorizeNetRequest(PaymentRequest request)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://apitest.authorize.net/xml/v1/request.api");
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				streamWriter.Write(CreateAuthorizeNetRequest(request).ToString());
			}

			using var streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream());
			var response = JObject.Parse(streamReader.ReadToEnd());

			return response.SelectToken("transactionResponse").Value<string>("responseCode").Equals("1") &&
				response.SelectToken("messages").Value<string>("resultCode").Equals("Ok") ?
				ErrorCodes.OK : response.SelectToken("errors").Values().First().Value<string>("errorText");
		}

		private static JObject CreateAuthorizeNetRequest(PaymentRequest request)
		{
			return new JObject
			{
				{ "createTransactionRequest", new JObject
				{
					{ "merchantAuthentication", new JObject
					{
						{ "name", "3mK8nGVR2Pc" },
						{ "transactionKey", "3G26PhZAX82f44pF" },
					}
					},
					{ "transactionRequest", new JObject
					{
						{ "transactionType", "authCaptureTransaction" },
						{ "amount", request.Amount.ToString() },
						{ "payment", new JObject
						{
							{ "creditCard", new JObject
							{
								{ "cardNumber", request.CardNumber.ToString() },
								{ "expirationDate", request.ExpirationDate.ToString() },
								{ "cardCode", request.CardCode.ToString() },
							}
							},
						}
						},
					}
					},
				}
				},
			};
		}

		private void SendRequests(List<int> idList, string requestName, int? paramId = null)
		{
			//TODO: UpdateMessages(userId), UpdateUserData(userId), UpdateFriendList(), UpdatePendingFriendList()
			foreach (int id in idList)
			{
				if (_webSocketList.ContainsKey(id))
				{
					_webSocketList[id].SendAsync(GetRequest(requestName, paramId),
						WebSocketMessageType.Binary, true, CancellationToken.None);
				}
			}
		}

		private static byte[] GetRequest(string requestName, int? paramId = null)
		{
			var json = new JObject
			{
				{ "dataType", requestName + "Response" },
			};
			if (paramId != null)
			{
				json.Add("data", new JObject
				{
					{ "id", paramId },
				});
			}
			return Encoding.UTF8.GetBytes(json.ToString());
		}

		private static byte[] GetResponse(string requestName, Object obj)
		{
			var json = new JObject
			{
				{ "dataType", requestName + "Response" },
			};
			if (obj is string str)
			{
				json.Add("successful", str.Equals(ErrorCodes.OK));
				json.Add("data", str);
			}
			else
			{
				json.Add("successful", true);
				json.Add("data", JToken.FromObject(obj));
			}
			return Encoding.UTF8.GetBytes(json.ToString());
		}

		private static byte[] GetErrorResponse(byte[] bytes)
		{
			var data = Encoding.UTF8.GetString(RemoveTrailingZeros(bytes));
			ErrorLog("Invalid JSON", data);
			var json = new JObject
			{
				{ "dataType", "ErrorResponse" },
				{ "successful", false },
				{ "data", "Invalid JSON: " + data },
			};
			return Encoding.UTF8.GetBytes(json.ToString());
		}

		private static byte[] RemoveTrailingZeros(byte[] bytes)
		{
			var i = bytes.Length - 1;
			while (bytes[i] == 0)
			{
				--i;
			}
			var temp = new byte[i + 1];
			Array.Copy(bytes, temp, i + 1);
			return temp;
		}

		private static void ErrorLog(string message, string data)
		{
			var log = DateTime.Now.ToString();
			log += " " + message + "\n" + data + "\n\n";
			File.AppendAllText("Logs\\error_log.txt", log);
		}

		private int GetID(WebSocket webSocket)
		{
			return _webSocketList.FirstOrDefault(x => x.Value == webSocket).Key;
		}

		private static FriendRelationRequest CreateRequest(int userId, int friendId)
		{
			return new FriendRelationRequest
			{
				FriendListOwnerID = userId,
				FriendID = friendId
			};
		}
	}
}
