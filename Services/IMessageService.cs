using System.Collections.Generic;
using System;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IMessageService
	{
		bool Add(MessageCreateNewRequest request);
		bool Delete(int id);
		bool UpdateSeen(int id);
		bool UpdateContent(int id, string content);
		MessageResponse GetByID(int id, bool sender);
		List<MessageResponse> GetBatch(int userId, MessageGetBatchRequest request);
	}
}
