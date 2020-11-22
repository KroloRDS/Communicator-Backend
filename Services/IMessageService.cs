using System.Collections.Generic;
using System;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IMessageService
	{
		bool Add(MessageRequest request);
		bool Delete(int id);
		bool Update(int id);
		MessageResponse GetMessage(int id, bool sender);
		List<MessageResponse> GetMessages(DateTime time, int userId, int friendId);
	}
}
