using System.Collections.Generic;
using System;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IMessageService
	{
		bool Add(MessageRequest request);
		bool Delete(int id);
		bool UpdateSeen(int id);
		bool UpdateContent(int id, string content);
		MessageResponse GetByID(int id, bool sender);
		List<MessageResponse> GetBatch(DateTime time, int userId, int friendId);
	}
}
