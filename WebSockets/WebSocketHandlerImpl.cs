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

		public async Task Handle(WebSocket webSocket, CommunicatorDbContex db)
		{
			var buffer = new byte[1024 * 4];
			WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			while (!result.CloseStatus.HasValue)
			{
				var response = await Task.Run(() => ProcessRequest(webSocket, buffer, db));
				Array.Clear(buffer, 0, buffer.Length);

				await webSocket.SendAsync(new ArraySegment<byte>(response), result.MessageType, result.EndOfMessage, CancellationToken.None);
				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
			_webSocketList.Remove(GetID(webSocket));
		}

		private byte[] ProcessRequest(WebSocket webSocket, byte[] bytes, CommunicatorDbContex db)
		{
			var friendRelationService = new FriendRelationServiceImpl(db);
			var messageSercive = new MessageSerciveImpl(db);
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
			
			var request = json.Value<string>("request");
			var data = json.SelectToken("data");

			return _webSocketList.ContainsValue(webSocket) ?
				ProcessLoggedInUserRequest(friendRelationService, messageSercive, userService, request, data, GetID(webSocket)) :
				ProcessLoggedOutUserRequest(userService, request, data, webSocket);
		}

		private byte[] ProcessLoggedOutUserRequest(
			IUserService userService,
			string request,
			JToken data,
			WebSocket webSocket
			)
		{
			switch (request)
			{
				case "Echo":
					return Encoding.UTF8.GetBytes(data.ToString());
				case "LogIn":
					return Login(request, webSocket,
						userService.Login(data.ToObject<UserLoginRequest>()));
				case "Register":
					return GetResponse(request,
						userService.Add(data.ToObject<UserCreateNewRequest>()));
				default:
					return GetResponse(request, false);
			}
		}

		private byte[] ProcessLoggedInUserRequest(
			IFriendRelationService friendRelationService,
			IMessageService messageService,
			IUserService userService,
			string request,
			JToken data,
			int userId
			)
		{
			switch (request)
			{
				//General
				case "LogOut":
					_webSocketList.Remove(userId);
					return GetResponse(request, true);
				//FriendList
				case "SendFriendRequest":
					return GetResponse(request,
						friendRelationService.Add(CreateRequest(userId, data.ToObject<int>())));
				case "AcceptFriendRequest":
					return GetResponse(request,
						friendRelationService.Accept(CreateRequest(userId, data.ToObject<int>())));
				case "RemoveFriend":
					return GetResponse(request,
						friendRelationService.Delete(CreateRequest(userId, data.ToObject<int>())));
				case "GetFriendList":
					return GetResponse(request,
						friendRelationService.GetFriendList(userId, data.ToObject<bool>()));
				//Messages
				case "SendMessage":
					return GetResponse(request,
						messageService.Add(userId, data.ToObject<MessageCreateNewRequest>()));
				case "DeleteMessage":
					return GetResponse(request,
						messageService.Delete(data.ToObject<int>()));
				case "SetMessageSeen":
					return GetResponse(request,
						messageService.UpdateSeen(data.ToObject<int>()));
				case "UpdateMessageContent":
					return GetResponse(request,
						messageService.UpdateContent(
							data.SelectToken("messageId").ToObject<int>(),
							data.SelectToken("messageContent").ToObject<string>()));
				case "GetMessage":
					return GetResponse(request,
						messageService.GetByID(
							userId, data.SelectToken("messageId").ToObject<int>()));
				case "GetMessagesBatch":
					return GetResponse(request,
						messageService.GetBatch(userId, data.ToObject<MessageGetBatchRequest>()));
				//User
				case "DeleteUser":
					return GetResponse(request, userService.Delete(userId));
				case "UpdateBankAccount":
					return GetResponse(request,
						userService.UpdateBankAccount(userId, data.ToObject<string>()));
				case "UpdateUserCredentials":
					return GetResponse(request,
						userService.UpdateCredentials(userId, data.ToObject<UserUpdateCredentialsRequest>()));
				case "GetUser":
					return GetResponse(request, userService.GetByID(data.ToObject<int>()));
				default:
					return GetResponse(request, false);
			}
		}

		private static byte[] GetResponse(string requestName, Object obj)
		{
			var json = new JObject
			{
				{ "request", requestName },
			};
			if (obj is bool boolean)
			{
				json.Add("data", new JObject
				{
					{ "value", boolean },
				});
			}
			else
			{
				json.Add("data", JObject.FromObject(obj));
			}
			return Encoding.UTF8.GetBytes(json.ToString());
		}

		private int GetID(WebSocket webSocket)
		{
			return _webSocketList.FirstOrDefault(x => x.Value == webSocket).Key;
		}

		private byte[] Login(string request, WebSocket webSocket, int id)
		{
			if (id != -1)
			{
				_webSocketList.Add(id, webSocket);
				return GetResponse(request, true);
			}
			return GetResponse(request, false);
		}

		private static FriendRelationRequest CreateRequest(int userId, int friendId)
		{
			return new FriendRelationRequest
			{
				FriendListOwnerID = userId,
				FriendID = friendId
			};
		}

		private static byte[] GetErrorResponse(byte[] bytes)
		{
			var data = Encoding.UTF8.GetString(RemoveTrailingZeros(bytes));
			var json = new JObject
			{
				{ "request", "unknown" },
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
	}
}
