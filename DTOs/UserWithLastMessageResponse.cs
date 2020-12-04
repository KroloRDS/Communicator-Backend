namespace Communicator.DTOs
{
	public class UserWithLastMessageResponse
	{
		public int ID { get; set; }
		public string Login { get; set; }
		public string Email { get; set; }
		public string BankAccount { get; set; }
		public string PublicKey { get; set; }
		public MessageResponse LastMessage { get; set; }
	}
}
