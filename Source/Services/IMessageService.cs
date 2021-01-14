using System.Collections.Generic;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IMessageService
	{
		string Add(int userId, MessageCreateNewRequest request);
		string Delete(int id);
		string UpdateSeen(int id);
		string UpdateContent(int id, string content);
		MessageResponse GetByID(int userId, int messageId);
		List<MessageResponse> GetBatch(int userId, MessageGetBatchRequest request);
	}
}
