namespace Communicator.DTOs
{
	public class MessageRequest
	{
		public int SenderID { get; set; }
		public int ReceiverID { get; set; }
		public string Content { get; set; }
	}
}
