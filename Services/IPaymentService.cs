using Newtonsoft.Json.Linq;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IPaymentService
	{
		byte[] MakePayment(string requestName, JToken data, int userId);
	}
}
