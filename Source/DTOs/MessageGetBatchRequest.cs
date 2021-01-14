using System;

namespace Communicator.DTOs
{
	public class MessageGetBatchRequest
	{
		public DateTime Timestamp { get; set; }
		public int FriendID { get; set; }
		public int Amount { get; set; }
	}
}
