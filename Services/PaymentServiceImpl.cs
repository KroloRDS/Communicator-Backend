using System.Linq;
using System;

using Communicator.Entities;
using Communicator.DTOs;

namespace Communicator.Services
{
	public class PaymentServiceImpl : IPaymentService
	{
		private readonly CommunicatorDbContex _context;
		private readonly string _currency = "PLN";

		public PaymentServiceImpl(CommunicatorDbContex context)
		{
			_context = context;
		}

		public PaymentResponse Add(int userId, PaymentRequest request)
		{
			var payment = new PaymentEntity
			{
				Amount = request.Amount,
				Currency = _currency,
				DateTime = DateTime.UtcNow,
				Status = (int)PaymentEntity.Statuses.PENDING,
				UserID = userId
			};

			_context.PaymentEntity.Add(payment);
			_context.SaveChanges();

			return new PaymentResponse
			{
				ID = payment.ID,
				Response = ErrorCodes.OK
			};
		}

		public string UpdateStatus(int id, bool status)
		{
			var payment = _context.PaymentEntity.FirstOrDefault(x => x.ID == id);
			if (payment == null)
			{
				return string.Format(ErrorCodes.CANNOT_FIND_PAYMENT, id);
			}

			payment.Status = status ? (int)PaymentEntity.Statuses.SUCCEDED : (int)PaymentEntity.Statuses.FAILED;
			_context.SaveChanges();
			return ErrorCodes.OK;
		}
	}
}
