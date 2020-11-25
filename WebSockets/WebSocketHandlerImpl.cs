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
		private readonly IFriendRelationService _friendRelationService;
		private readonly IMessageService _messageService;
		private readonly IUserService _userService;
		private readonly Dictionary<int, WebSocket> _webSocketList;

		public WebSocketHandlerImpl(IFriendRelationService friendRelationService, IMessageService messageService, IUserService userService)
		{
			_webSocketList = new Dictionary<int, WebSocket>();
			_friendRelationService = friendRelationService;
			_messageService = messageService;
			_userService = userService;
		}

		public async Task Handle(WebSocket webSocket)
		{
			var buffer = new byte[1024 * 4];
			WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			while (!result.CloseStatus.HasValue)
			{
				var getResponse = Task.Run(() => ProcessRequest(webSocket, buffer));
				var response = await getResponse;

				await webSocket.SendAsync(new ArraySegment<byte>(buffer), result.MessageType, result.EndOfMessage, CancellationToken.None);
				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
		}

		private static byte[] ToBytes(Object obj)
		{
			return Encoding.UTF8.GetBytes(JObject.FromObject(obj).ToString());
		}

		private byte[] ProcessRequest(WebSocket webSocket, byte[] bytes)
		{
			var json = JObject.Parse(Encoding.UTF8.GetString(bytes));
			var request = json.Value<string>("request");
			var data = json.SelectToken("data");

			if (_webSocketList.ContainsValue(webSocket))
			{
				return ProcessLoggedInUserRequest(request, data, webSocket);
			}

			switch (request)
			{
				case "LogIn":
					int id = _userService.Login(data.ToObject<UserLoginRequest>());
					if (id != -1)
					{
						_webSocketList.Add(id, webSocket);
						return ToBytes(true);
					}
					return ToBytes(false);
				case "Register":
					return ToBytes(_userService.Add(data.ToObject<UserCreateNewRequest>()));
				default:
					return ToBytes(false);
			}
		}

		private byte[] ProcessLoggedInUserRequest(string request, JToken data, WebSocket webSocket)
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
	}
}
