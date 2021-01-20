namespace Communicator.Source.DTOs.JSONs
{
	public class CreateTransactionRequest
	{
		public MerchantAuthentication merchantAuthentication { get; set; }
		public TransactionRequest transactionRequest { get; set; }
	}
}
