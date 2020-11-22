using System.Collections.Generic;
using System.Linq;
using System;

using Communicator.Repositories;
using Communicator.Entities;
using Communicator.DTOs;

namespace Communicator.Services
{
	public class MessageSerciveImpl : IMessageService
	{
		private readonly CommunicatorDbContex _context;

		public MessageSerciveImpl(CommunicatorDbContex context)
		{
			_context = context;
		}

		public void Add(MessageRequest request)
		{
			_context.MessageEntity.Add(new MessageEntity
			{
				SenderID = request.SenderID,
				ReceiverID = request.ReceiverID,
				SenderEncryptedContent = request.Content, //TODO: encrypt
				ReceiverEncryptedContent = request.Content, //TODO: encrypt
				SentDateTime = DateTime.UtcNow,
				SeenByReceiver = false
			});
			_context.SaveChanges();
		}

		public void Delete(int id)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == id);
			if (message != null)
			{
				_context.MessageEntity.Remove(message);
				_context.SaveChanges();
			}
			//TODO: return codes
		}

		public void Update(int id)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == id);
			if (message != null)
			{
				message.SeenByReceiver = true;
				_context.SaveChanges();
			}
			//TODO: return codes
		}

		public List<MessageResponse> GetMessages(DateTime time, int userId, int friendId)
		{
			return _context.MessageEntity
				.Where(x => ((x.SenderID == userId && x.ReceiverID == friendId) ||
				(x.ReceiverID == userId && x.SenderID == friendId)) &&
				x.SentDateTime < time)
				.OrderBy(x => x.SentDateTime)
				.Take(50)
				.Select(x => new MessageResponse
				{
					ID = x.ID,
					SenderID = x.SenderID,
					ReceiverID = x.ReceiverID,
					Content = x.SenderID == userId ? 
						x.SenderEncryptedContent :
						x.ReceiverEncryptedContent,
					SentDateTime = x.SentDateTime,
					SeenByReceiver = x.SeenByReceiver
				}).ToList();
		}
	}
}
