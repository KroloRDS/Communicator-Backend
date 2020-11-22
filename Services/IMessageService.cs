using System.Collections.Generic;
using System;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IMessageService
	{
		void Add(MessageRequest request);
		void Delete(int id);
		void Update(int id);
		MessageResponse GetMessage(int id, bool sender);
		List<MessageResponse> GetMessages(DateTime time, int userId, int friendId);
	}
}
