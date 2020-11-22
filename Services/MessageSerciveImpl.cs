using System.Collections.Generic;
using System.Linq;
using System;

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

		public bool Add(MessageRequest request)
		{
			if (request.SenderID == request.ReceiverID)
			{
				return false;
			}

			if (_context.UserEntity.FirstOrDefault(x => x.ID == request.SenderID) == null ||
			_context.UserEntity.FirstOrDefault(x => x.ID == request.ReceiverID) == null)
			{
				return false;
			}

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
			return true;
		}

		public bool Delete(int id)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == id);
			if (message == null)
			{
				return false;
			}

			_context.MessageEntity.Remove(message);
			_context.SaveChanges();
			return true;
		}

		public bool Update(int id)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == id);
			if (message != null)
			{
				return false;
			}

			message.SeenByReceiver = true;
			_context.SaveChanges();
			return true;
		}

		public MessageResponse GetMessage(int id, bool sender)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == id);
			if (message == null)
			{
				return null;
			}

			return new MessageResponse
			{
				ID = message.ID,
				SenderID = message.SenderID,
				ReceiverID = message.ReceiverID,
				Content = sender ?
				message.SenderEncryptedContent :
				message.ReceiverEncryptedContent,
				SentDateTime = message.SentDateTime,
				SeenByReceiver = message.SeenByReceiver
			};
		}

		public List<MessageResponse> GetMessages(DateTime time, int userId, int friendId)
		{
			if (userId == friendId)
			{
				return null;
			}

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
