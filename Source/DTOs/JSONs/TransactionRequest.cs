namespace Communicator.Source.DTOs.JSONs
{
	public class TransactionRequest
	{
		public string transactionType { get; set; }
		public float amount { get; set; }
		public Payment payment { get; set; }
	}
}
