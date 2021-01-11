using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IPaymentService
	{
		PaymentResponse Add(int userId, PaymentRequest request);
		string UpdateStatus(int id, bool status);
		string SendAuthorizeNetRequest(PaymentRequest request);
	}
}
