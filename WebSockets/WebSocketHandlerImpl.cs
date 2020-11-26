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
				var getResponse = Task.Run(() => ProcessRequest(webSocket, buffer, db));
				var response = await getResponse;

				await webSocket.SendAsync(new ArraySegment<byte>(buffer), result.MessageType, result.EndOfMessage, CancellationToken.None);
				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
		}

		private byte[] ProcessRequest(WebSocket webSocket, byte[] bytes, CommunicatorDbContex db)
		{
			var friendRelationService = new FriendRelationServiceImpl(db);
			var messageSercive = new MessageSerciveImpl(db);
			var userService = new UserServiceImpl(db);

			var json = JObject.Parse(Encoding.UTF8.GetString(bytes));
			var request = json.Value<string>("request");
			var data = json.SelectToken("data");

			return _webSocketList.ContainsValue(webSocket) ?
				ProcessLoggedInUserRequest(friendRelationService, messageSercive, userService, request, data, webSocket) :
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
					return ToBytes(data);
				case "LogIn":
					int id = userService.Login(data.ToObject<UserLoginRequest>());
					if (id != -1)
					{
						_webSocketList.Add(id, webSocket);
						return ToBytes(true);
					}
					return ToBytes(false);
				case "Register":
					return ToBytes(userService.Add(data.ToObject<UserCreateNewRequest>()));
				default:
					return ToBytes(false);
			}
		}

		private byte[] ProcessLoggedInUserRequest(
			IFriendRelationService friendRelationService,
			IMessageService messageService,
			IUserService userService,
			string request,
			JToken data,
			WebSocket webSocket
			)
		{
			switch (request)
			{
				case "LogOut":
					var item = _webSocketList.First(x => x.Value == webSocket);
					_webSocketList.Remove(item.Key);
					return ToBytes(true);
				default:
					return ToBytes(false);
			}
		}

		private static byte[] ToBytes(Object obj)
		{
			return Encoding.UTF8.GetBytes(JObject.FromObject(obj).ToString());
		}
	}
}
