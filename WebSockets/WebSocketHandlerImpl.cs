using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Linq;
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
			_webSocketList.Remove(_webSocketList.FirstOrDefault(x => x.Value == webSocket).Key);
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
				return JsonHandler.GetErrorResponse(bytes);
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
			return JsonHandler.GetResponse(request, ErrorCodes.OK);
		}

		private static byte[] ProcessLoggedOutUserRequest(IUserService userService, string request, JToken data)
		{
			return request switch
			{
				"Echo" => Encoding.UTF8.GetBytes(data.ToString()),
				"IsLoggedInRequest" => JsonHandler.GetResponse(request, ErrorCodes.NOT_LOGGED_IN),
				"Register" => JsonHandler.GetResponse(request, userService.Add(data.ToObject<UserCreateNewRequest>())),
				_ => JsonHandler.GetResponse(request, string.Format(ErrorCodes.CANNOT_FIND_REQUEST_OR_UNAUTHORIZED, request)),
			};
		}

		private byte[] ProcessLoggedInUserRequest(
			IFriendRelationService friendRelationService, IMessageService messageService, IPaymentService paymentService, IUserService userService,
			string request, JToken data, int userId)
		{
			switch (request)
			{
				//General
				case "Echo":
					return Encoding.UTF8.GetBytes(data.ToString());

				case "IsLoggedInRequest":
					return JsonHandler.GetResponse(request, ErrorCodes.OK);

				case "Register":
					return JsonHandler.GetResponse(request, ErrorCodes.REGISTER_WHILE_LOGGED_IN);

				//FriendList
				case "AddFriend":
					var friendId = data.First.ToObject<int>();
					var response = JsonHandler.GetResponse(request, friendRelationService.Add(CreateRequest(userId, friendId)));
					
					SendNotification(friendId, "PendingFriendList");
					return response;

				case "AcceptFriend":
					friendId = data.First.ToObject<int>();
					response = JsonHandler.GetResponse(request, friendRelationService.Accept(CreateRequest(userId, friendId)));
					
					SendNotification(friendId, "FriendList");
					return response;

				case "RemoveFriend":
					friendId = data.First.ToObject<int>();
					response = JsonHandler.GetResponse(request, friendRelationService.Delete(CreateRequest(userId, friendId)));
					
					SendNotification(friendId, "FriendList");
					return response;

				case "GetFriendList":
					return JsonHandler.GetResponse(request, friendRelationService.GetFriendList(userId, true));

				case "GetPendingFriendList":
					return JsonHandler.GetResponse(request, friendRelationService.GetFriendList(userId, false));

				//Messages
				case "SendMessage":
					var message = data.ToObject<MessageCreateNewRequest>();
					response = JsonHandler.GetResponse(request, messageService.Add(userId, message));
					
					SendNotification(message.ReceiverID, "Message", userId);
					return response;

				case "DeleteMessage":
					var messageId = data.First.ToObject<int>();
					var receiverID = messageService.GetByID(userId, messageId).ReceiverID;
					response = JsonHandler.GetResponse(request, messageService.Delete(messageId));

					SendNotification(receiverID, "Message", userId);
					return response;

				case "SetMessageSeen":
					messageId = data.First.ToObject<int>();
					receiverID = messageService.GetByID(userId, messageId).ReceiverID;
					response = JsonHandler.GetResponse(request, messageService.UpdateSeen(messageId));
					
					SendNotification(receiverID, "Message", userId);
					return response;

				case "UpdateMessageContent":
					messageId = data.SelectToken("messageId").ToObject<int>();
					receiverID = messageService.GetByID(userId, messageId).ReceiverID;
					response = JsonHandler.GetResponse(request, messageService.UpdateContent(messageId,
						data.SelectToken("messageContent").ToObject<string>()));
					
					SendNotification(receiverID, "Message", userId);
					return response;

				case "GetMessage":
					return JsonHandler.GetResponse(request, messageService.GetByID(
						userId, data.First.ToObject<int>()));

				case "GetMessagesBatch":
					return JsonHandler.GetResponse(request, messageService.GetBatch(
						userId, data.ToObject<MessageGetBatchRequest>()));

				//Payment
				case "MakePayment":
					return paymentService.MakePayment(request, data, userId);

				//User
				case "DeleteUser":
					response = JsonHandler.GetResponse(request, userService.Delete(userId));
					SendNotificationToAllFriends(friendRelationService, userId);
					return response;

				case "UpdateBankAccount":
					return JsonHandler.GetResponse(request, userService.UpdateBankAccount(
						userId, data.First.ToObject<string>()));

				case "UpdateUserCredentials":
					response = JsonHandler.GetResponse(request, userService.UpdateCredentials(
						userId, data.ToObject<UserUpdateCredentialsRequest>()));

					SendNotificationToAllFriends(friendRelationService, userId);
					return response;

				case "GetUser":
					return JsonHandler.GetResponse(request, userService.GetByID(
						data.First.ToObject<int>()));

				default:
					return JsonHandler.GetResponse(request, string.Format(ErrorCodes.CANNOT_FIND_REQUEST, request));
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
