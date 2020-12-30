namespace Communicator.DTOs
{
	public class PaymentRequest
	{
		public float Amount { get; set; }
		public long CardNumber { get; set; }
		public int CardCode { get; set; }
		public string ExpirationDate { get; set; }
	}
}
