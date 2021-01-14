using System;

namespace Communicator.Entities
{
	public class PaymentEntity
	{
		public enum Statuses : int { PENDING = 0, SUCCEDED = 1, FAILED = 2 }

		public int ID { get; set; }
		public int UserID { get; set; }
		public DateTime DateTime { get; set; }
		public int Status { get; set; }
		public float Amount { get; set; }
		public string Currency { get; set; }
	}
}
