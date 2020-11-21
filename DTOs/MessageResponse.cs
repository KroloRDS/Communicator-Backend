using System;

namespace Communicator.DTOs
{
	public class MessageResponse
	{
		public int ID { get; set; }
		public int SenderID { get; set; }
		public int ReceiverID { get; set; }
		public string Content { get; set; }
		public DateTime SentDateTime { get; set; }
		public bool SeenByReceiver { get; set; }
	}
}
