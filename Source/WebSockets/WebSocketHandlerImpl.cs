using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Linq;
using System;

using Communicator.HelperClasses;
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
			_webSocketList.Remove(_webSocketList.FirstOrDefault(x => x.Value == webSocket).Key);
		}

		private byte[] ProcessRequest(ISession session, WebSocket webSocket, CommunicatorDbContex db, byte[] bytes)
		{
			var services = new Service(db);

			JObject json;
			try
			{
				json = JObject.Parse(Encoding.UTF8.GetString(bytes));
			}
			catch (Exception exception)
			{
				return JsonHandler.GetErrorResponse(exception, Encoding.UTF8.GetString(bytes));
			}

			JToken data;
			string request;
			try
			{
				data = json.SelectToken("data");
				request = json.Value<string>("dataType");
				request = request.Substring(0, request.LastIndexOf("Request"));
			}
			catch (Exception exception)
			{
				return JsonHandler.GetErrorResponse(exception, json.ToString());
			}

			int? id;
			try
			{
				id = session.GetInt32("userId");
			}
			catch (Exception exception)
			{
				return JsonHandler.GetErrorResponse(exception, session.ToString());
			}

			try
			{
				if (id == null)
				{
					return ProcessLoggedOutUserRequest(services, request, data);
				}

				_webSocketList[(int)id] = webSocket;
				return request.Equals("LogOut") ?
					Logout(session, request, (int)id) :
					ProcessGeneralRequest(services, request, data, (int)id);
			}
			catch (Exception exception)
			{
				return JsonHandler.GetErrorResponse(exception);
			}
		}

		private byte[] Logout(ISession session, string request, int id)
		{
			session.Clear();
			_webSocketList.Remove(id);
			return JsonHandler.GetResponse(request, Error.OK);
		}

		private static byte[] ProcessLoggedOutUserRequest(Service services, string request, JToken data)
		{
			return request switch
			{
				"Echo" => Encoding.UTF8.GetBytes(data.ToString()),
				"IsLoggedInRequest" => JsonHandler.GetResponse(request, Error.NOT_LOGGED_IN),
				"Register" => JsonHandler.GetResponse(request, services.User.Add(data.ToObject<UserCreateNewRequest>())),
				_ => JsonHandler.GetResponse(request, string.Format(Error.CANNOT_FIND_REQUEST_OR_UNAUTHORIZED, request)),
			};
		}

		private byte[] ProcessGeneralRequest(Service services, string request, JToken data, int userId)
		{
			return request switch
			{
				"Echo" => Encoding.UTF8.GetBytes(data.ToString()),
				"IsLoggedInRequest" => JsonHandler.GetResponse(request, Error.OK),
				"Register" => JsonHandler.GetResponse(request, Error.REGISTER_WHILE_LOGGED_IN),
				_ => ProcessFriendListRequest(services, request, data, userId),
			};
		}

		private byte[] ProcessFriendListRequest(Service services, string request, JToken data, int userId)
		{
			var service = services.FriendRelation;
			switch (request)
			{
				case "AddFriend":
					var friendId = data.First.ToObject<int>();
					var response = JsonHandler.GetResponse(request, service.Add(CreateRequest(userId, friendId)));

					SendNotification(friendId, "PendingFriendList");
					return response;

				case "AcceptFriend":
					friendId = data.First.ToObject<int>();
					response = JsonHandler.GetResponse(request, service.Accept(CreateRequest(userId, friendId)));

					SendNotification(friendId, "FriendList");
					return response;

				case "RemoveFriend":
					friendId = data.First.ToObject<int>();
					response = JsonHandler.GetResponse(request, service.Delete(CreateRequest(userId, friendId)));

					SendNotification(friendId, "FriendList");
					return response;

				case "GetFriendList":
					return JsonHandler.GetResponse(request, service.GetFriendList(userId, true));

				case "GetPendingFriendList":
					return JsonHandler.GetResponse(request, service.GetFriendList(userId, false));
				default:
					return ProcessMessageRequest(services, request, data, userId);
			}
		}

		private byte[] ProcessMessageRequest(Service services, string request, JToken data, int userId)
		{
			var service = services.Message;
			switch (request)
			{
				case "SendMessage":
					var message = data.ToObject<MessageCreateNewRequest>();
					var response = JsonHandler.GetResponse(request, service.Add(userId, message));

					SendNotification(message.ReceiverID, "Message", userId);
					return response;

				case "DeleteMessage":
					var messageId = data.First.ToObject<int>();
					var receiverID = services.Message.GetByID(userId, messageId).ReceiverID;
					response = JsonHandler.GetResponse(request, service.Delete(messageId));

					SendNotification(receiverID, "Message", userId);
					return response;

				case "SetMessageSeen":
					messageId = data.First.ToObject<int>();
					receiverID = services.Message.GetByID(userId, messageId).ReceiverID;
					response = JsonHandler.GetResponse(request, service.UpdateSeen(messageId));

					SendNotification(receiverID, "Message", userId);
					return response;

				case "UpdateMessageContent":
					messageId = data.SelectToken("messageId").ToObject<int>();
					receiverID = services.Message.GetByID(userId, messageId).ReceiverID;
					response = JsonHandler.GetResponse(request, service.UpdateContent(messageId,
						data.SelectToken("messageContent").ToObject<string>()));

					SendNotification(receiverID, "Message", userId);
					return response;

				case "GetMessage":
					return JsonHandler.GetResponse(request, service.GetByID(
						userId, data.First.ToObject<int>()));

				case "GetMessagesBatch":
					return JsonHandler.GetResponse(request, service.GetBatch(
						userId, data.ToObject<MessageGetBatchRequest>()));
				default:
					return ProcessUserRequest(services, request, data, userId);

			}
		}

		private byte[] ProcessUserRequest(Service services, string request, JToken data, int userId)
		{
			switch (request)
			{
				case "DeleteUser":
					var response = JsonHandler.GetResponse(request, services.User.Delete(userId));
					SendNotificationToAllFriends(services.FriendRelation, userId);
					return response;

				case "UpdateBankAccount":
					return JsonHandler.GetResponse(request, services.User.UpdateBankAccount(
						userId, data.First.ToObject<string>()));

				case "UpdateUserCredentials":
					response = JsonHandler.GetResponse(request, services.User.UpdateCredentials(
						userId, data.ToObject<UserUpdateCredentialsRequest>()));

					SendNotificationToAllFriends(services.FriendRelation, userId);
					return response;

				case "GetUser":
					return JsonHandler.GetResponse(request, services.User.GetByID(
						data.First.ToObject<int>()));
				default:
					return ProcessPaymentRequest(services, request, data, userId);
			}
		}

		private byte[] ProcessPaymentRequest(Service services, string request, JToken data, int userId)
		{
			return request switch
			{
				"MakePayment" => services.Payment.MakePayment(request, data, userId),
				_ => JsonHandler.GetResponse(request, string.Format(Error.CANNOT_FIND_REQUEST, request)),
			};
		}

		private void SendNotification(int id, string requestName, int? paramId = null)
		{
			if (_webSocketList.ContainsKey(id))
			{
				_webSocketList[id].SendAsync(JsonHandler.GetUpdateRequest(requestName, paramId),
					WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

		private void SendNotificationToAllFriends(IFriendRelationService friendRelationService, int userId)
		{
			foreach (var friendId in friendRelationService.GetFriendListIDs(userId, true))
			{
				SendNotification(friendId, "FriendList");
			}
			foreach (var friendId in friendRelationService.GetFriendListIDs(userId, false))
			{
				SendNotification(friendId, "PendingFriendList");
			}
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
