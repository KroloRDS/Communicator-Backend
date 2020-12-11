namespace Communicator.DTOs
{
	public class PaymentRequest
	{
		public float Amount { get; set; }
		public int CardNumber { get; set; }
		public int CardCode { get; set; }
		public string ExpirationDate { get; set; }
	}
}
