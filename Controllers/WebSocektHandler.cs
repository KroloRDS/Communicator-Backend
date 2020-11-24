using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System;

using Communicator.Services;

namespace Communicator.Controllers
{
	public class WebSocketHandler
	{
		private readonly IFriendRelationService _friendRelationService;
		private readonly IMessageService _messageService;
		private readonly IUserService _userService;

		public WebSocketHandler(IFriendRelationService friendRelationService, IMessageService messageService, IUserService userService)
		{
			_friendRelationService = friendRelationService;
			_messageService = messageService;
			_userService = userService;
			//TODO: inject service instances
		}


		public async Task Handle(HttpContext context, WebSocket webSocket)
		{
			var buffer = new byte[1024 * 4];
			WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			//TODO: deserialize
			//get method name
			//get parameters, etc
			while (!result.CloseStatus.HasValue)
			{
				await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}
			await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
		}
	}
}
