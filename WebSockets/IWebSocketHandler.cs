using System.Threading.Tasks;
using System.Net.WebSockets;

namespace Communicator.WebSockets
{
	public interface IWebSocketHandler
	{
		Task Handle(WebSocket webSocket, CommunicatorDbContex db);
	}
}
