namespace Communicator.DTOs
{
	public class MessageCreateNewRequest
	{
		public int SenderID { get; set; }
		public int ReceiverID { get; set; }
		public string Content { get; set; }
	}
}
