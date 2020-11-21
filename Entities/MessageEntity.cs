using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Communicator.Entities
{
	public class MessageEntity
	{
		public int ID { get; set; }
		public int SenderID  { get; set; }
		public int ReceiverID { get; set; }
		public string SenderEncryptedContent { get; set; }
		public string ReceiverEncryptedContent { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime SentDateTime { get; }
		public bool SeenByReceiver { get; set; }
	}
}
