using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Net.WebSockets;

using Communicator.HelperClasses;

namespace Communicator.WebSockets
{
	public interface IWebSocketHandler
	{
		Task Handle(ISession session, WebSocket webSocket, CommunicatorDbContex db);
	}
}
