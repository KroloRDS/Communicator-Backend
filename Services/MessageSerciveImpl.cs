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

		public bool Add(int userId, MessageCreateNewRequest request)
		{
			if (userId == request.ReceiverID)
			{
				return false;
			}

			if (_context.UserEntity.FirstOrDefault(x => x.ID == userId) == null ||
			_context.UserEntity.FirstOrDefault(x => x.ID == request.ReceiverID) == null)
			{
				return false;
			}

			_context.MessageEntity.Add(new MessageEntity
			{
				SenderID = userId,
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

		public bool UpdateSeen(int id)
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

		public bool UpdateContent(int id, string content)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == id);
			if (message != null)
			{
				return false;
			}

			message.SenderEncryptedContent = content; //TODO: encrypt
			message.ReceiverEncryptedContent = content; //TODO: encrypt
			_context.SaveChanges();
			return true;
		}

		public MessageResponse GetByID(int userId, int messageId)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == messageId);
			if (message == null)
			{
				return null;
			}

			return new MessageResponse
			{
				ID = message.ID,
				SenderID = message.SenderID,
				ReceiverID = message.ReceiverID,
				Content = userId == message.SenderID ?
				message.SenderEncryptedContent :
				message.ReceiverEncryptedContent,
				SentDateTime = message.SentDateTime,
				SeenByReceiver = message.SeenByReceiver
			};
		}

		public List<MessageResponse> GetBatch(int userId, MessageGetBatchRequest request)
		{
			if (userId == request.FriendID)
			{
				return null;
			}

			return _context.MessageEntity
				.Where(x => ((x.SenderID == userId && x.ReceiverID == request.FriendID) ||
				(x.ReceiverID == userId && x.SenderID == request.FriendID)) &&
				x.SentDateTime < request.Timestamp)
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
