using System.Collections.Generic;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IMessageService
	{
		bool Add(int userId, MessageCreateNewRequest request);
		bool Delete(int id);
		bool UpdateSeen(int id);
		bool UpdateContent(int id, string content);
		MessageResponse GetByID(int userId, int messageId);
		List<MessageResponse> GetBatch(int userId, MessageGetBatchRequest request);
	}
}
