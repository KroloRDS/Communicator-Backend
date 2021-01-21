using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Linq;
using System;

using Communicator.Source.DTOs.JSONs;
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

		public async Task Handle(ISession session, WebSocket webSocket, Service services)
		{
			var buffer = new byte[1024 * 4];
			WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			while (!result.CloseStatus.HasValue)
			{
				var response = await Task.Run(() => ParseRequest(session, webSocket, services, buffer));
				Array.Clear(buffer, 0, buffer.Length);

				await webSocket.SendAsync(new ArraySegment<byte>(response), result.MessageType, result.EndOfMessage, CancellationToken.None);
				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
			_webSocketList.Remove(_webSocketList.FirstOrDefault(x => x.Value == webSocket).Key);
		}

		private byte[] ParseRequest(ISession session, WebSocket webSocket, Service services, byte[] bytes)
		{
			JObject json;
			try
			{
				json = JObject.Parse(Encoding.UTF8.GetString(bytes));
			}
			catch (Exception exception)
			{
				return new JsonErrorResponse(exception, Encoding.UTF8.GetString(bytes)).GetBytes();
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
				return new JsonErrorResponse(exception, json.ToString()).GetBytes();
			}

			try
			{
				int? id = session.GetInt32("userId");
				return ProcessRequest(session, webSocket, services, data, request, id);
			}
			catch (Exception exception)
			{
				return new JsonErrorResponse(exception).GetBytes();
			}
		}

		private byte[] ProcessRequest(ISession session, WebSocket webSocket, Service services, JToken data, string request, int? id)
		{
			if (id == null)
			{
				return ProcessLoggedOutUserRequest(services, request, data);
			}

			_webSocketList[(int)id] = webSocket;
			return request.Equals("LogOut") ?
				Logout(session, request, (int)id) :
				ProcessFriendListRequest(services, request, data, (int)id);
		}

		private static byte[] ProcessLoggedOutUserRequest(Service services, string request, JToken data)
		{
			return request switch
			{
				"Echo" => Encoding.UTF8.GetBytes(data.ToString()),
				"IsLoggedInRequest" => new JsonResponse(request, Error.NOT_LOGGED_IN).GetBytes(),
				"Register" => new JsonResponse(request, services.User.Add(data.ToObject<UserCreateNewRequest>())).GetBytes(),
				_ => new JsonResponse(request, string.Format(Error.CANNOT_FIND_REQUEST_OR_UNAUTHORIZED, request)).GetBytes(),
			};
		}

		private byte[] Logout(ISession session, string request, int id)
		{
			session.Clear();
			_webSocketList.Remove(id);
			return new JsonResponse(request, Error.OK).GetBytes();
		}

		private byte[] ProcessFriendListRequest(Service services, string request, JToken data, int userId)
		{
			var service = services.FriendRelation;
			switch (request)
			{
				case "AddFriend":
					var friendId = data.First.ToObject<int>();
					var response = new JsonResponse(request, service.Add(CreateRequest(userId, friendId)));

					SendNotification(friendId, "PendingFriendList");
					return response.GetBytes();

				case "AcceptFriend":
					friendId = data.First.ToObject<int>();
					response = new JsonResponse(request, service.Accept(CreateRequest(userId, friendId)));

					SendNotification(friendId, "FriendList");
					return response.GetBytes();

				case "RemoveFriend":
					friendId = data.First.ToObject<int>();
					response = new JsonResponse(request, service.Delete(CreateRequest(userId, friendId)));

					SendNotification(friendId, "FriendList");
					return response.GetBytes();

				case "GetFriendList":
					return new JsonResponse(request, service.GetFriendList(userId, true)).GetBytes();

				case "GetPendingFriendList":
					return new JsonResponse(request, service.GetFriendList(userId, false)).GetBytes();
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
					var response = new JsonResponse(request, service.Add(userId, message));

					SendNotification(message.ReceiverID, "Message", userId);
					return response.GetBytes();

				case "DeleteMessage":
					var messageId = data.First.ToObject<int>();
					var receiverID = services.Message.GetByID(userId, messageId).ReceiverID;
					response = new JsonResponse(request, service.Delete(messageId));

					SendNotification(receiverID, "Message", userId);
					return response.GetBytes();

				case "SetMessageSeen":
					messageId = data.First.ToObject<int>();
					receiverID = services.Message.GetByID(userId, messageId).ReceiverID;
					response = new JsonResponse(request, service.UpdateSeen(messageId));

					SendNotification(receiverID, "Message", userId);
					return response.GetBytes();

				case "UpdateMessageContent":
					messageId = data.SelectToken("messageId").ToObject<int>();
					receiverID = services.Message.GetByID(userId, messageId).ReceiverID;
					response = new JsonResponse(request, service.UpdateContent(messageId,
						data.SelectToken("messageContent").ToObject<string>()));

					SendNotification(receiverID, "Message", userId);
					return response.GetBytes();

				case "GetMessage":
					return new JsonResponse(request, service.GetByID(
						userId, data.First.ToObject<int>())).GetBytes();

				case "GetMessagesBatch":
					return new JsonResponse(request, service.GetBatch(
						userId, data.ToObject<MessageGetBatchRequest>())).GetBytes();
				default:
					return ProcessUserRequest(services, request, data, userId);
			}
		}

		private byte[] ProcessUserRequest(Service services, string request, JToken data, int userId)
		{
			switch (request)
			{
				case "DeleteUser":
					var response = new JsonResponse(request, services.User.Delete(userId));
					SendNotificationToAllFriends(services.FriendRelation, userId);
					return response.GetBytes();

				case "UpdateBankAccount":
					return new JsonResponse(request, services.User.UpdateBankAccount(
						userId, data.First.ToObject<string>())).GetBytes();

				case "UpdateUserCredentials":
					response = new JsonResponse(request, services.User.UpdateCredentials(
						userId, data.ToObject<UserUpdateCredentialsRequest>()));

					SendNotificationToAllFriends(services.FriendRelation, userId);
					return response.GetBytes();

				case "GetUser":
					return new JsonResponse(request, services.User.GetByID(
						data.First.ToObject<int>())).GetBytes();
				default:
					return ProcessOtherRequest(services, request, data, userId);
			}
		}

		private static byte[] ProcessOtherRequest(Service services, string request, JToken data, int userId)
		{
			return request switch
			{
				"Echo" => Encoding.UTF8.GetBytes(data.ToString()),
				"IsLoggedInRequest" => new JsonResponse(request, Error.OK).GetBytes(),
				"Register" => new JsonResponse(request, Error.REGISTER_WHILE_LOGGED_IN).GetBytes(),
				"MakePayment" => services.Payment.MakePayment(request, data, userId),
				_ => new JsonResponse(request, string.Format(Error.CANNOT_FIND_REQUEST, request)).GetBytes(),
			};
		}

		private void SendNotification(int id, string requestName, int? paramId = null)
		{
			if (_webSocketList.ContainsKey(id))
			{
				_webSocketList[id].SendAsync(new JsonUpdateRequest(requestName, paramId).GetBytes(),
					WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

		private void SendNotificationToAllFriends(IFriendRelationService friendRelationService, int userId)
		{
			foreach (var friendId in friendRelationService.GetFriendListIDs(userId, true))
			{
				SendNotification(friendId, "FriendList");
				SendNotification(friendId, "Message", userId);
			}
			foreach (var friendId in friendRelationService.GetFriendListIDs(userId, false))
			{
				SendNotification(friendId, "PendingFriendList");
				SendNotification(friendId, "Message", userId);
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
