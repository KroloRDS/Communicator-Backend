using System;

namespace Communicator.Entities
{
	public class MessageEntity
	{
		public int ID { get; set; }
		public int SenderID  { get; set; }
		public int ReceiverID { get; set; }
		public string SenderEncryptedContent { get; set; }
		public string ReceiverEncryptedContent { get; set; }
		public DateTime SentDateTime { get; set; }
		public bool SeenByReceiver { get; set; }
	}
}
