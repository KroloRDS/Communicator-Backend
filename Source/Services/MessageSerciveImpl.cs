using System.Collections.Generic;
using System.Linq;
using System;

using Communicator.HelperClasses;
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

		public string Add(int userId, MessageCreateNewRequest request)
		{
			if (userId == request.ReceiverID)
			{
				return Error.CANNOT_MESSAGE_YOURSELF;
			}

			if (_context.UserEntity.FirstOrDefault(x => x.ID == userId) == null)
			{
				return string.Format(Error.CANNOT_FIND_USER, userId);
			}
			if (_context.UserEntity.FirstOrDefault(x => x.ID == request.ReceiverID) == null)
			{
				return string.Format(Error.CANNOT_FIND_USER, request.ReceiverID);
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
			return Error.OK;
		}

		public string Delete(int id)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == id);
			if (message == null)
			{
				return string.Format(Error.CANNOT_FIND_MESSAGE, id);
			}

			_context.MessageEntity.Remove(message);
			_context.SaveChanges();
			return Error.OK;
		}

		public string UpdateSeen(int id)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == id);
			if (message == null)
			{
				return string.Format(Error.CANNOT_FIND_MESSAGE, id);
			}

			message.SeenByReceiver = true;
			_context.SaveChanges();
			return Error.OK;
		}

		public string UpdateContent(int id, string content)
		{
			var message = _context.MessageEntity.FirstOrDefault(x => x.ID == id);
			if (message == null)
			{
				return string.Format(Error.CANNOT_FIND_MESSAGE, id);
			}

			message.SenderEncryptedContent = content; //TODO: encrypt
			message.ReceiverEncryptedContent = content; //TODO: encrypt
			_context.SaveChanges();
			return Error.OK;
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
				return new List<MessageResponse>();
			}

			var list = _context.MessageEntity
				.Where(x => ((x.SenderID == userId && x.ReceiverID == request.FriendID) ||
				(x.ReceiverID == userId && x.SenderID == request.FriendID)) &&
				x.SentDateTime < request.Timestamp)
				.OrderByDescending(x => x.SentDateTime)
				.Take(request.Amount)
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

			list.Reverse();
			return list;
		}

		public MessageResponse GetLastMessage(int userId, int friendId)
		{
			var list = GetBatch(userId, new MessageGetBatchRequest
			{
				Amount = 1,
				FriendID = friendId,
				Timestamp = DateTime.UtcNow,
			});
			
			if (list.Count != 1)
			{
				return null;
			}

			var message = list.First();
			message.Content = message.Content.Substring(0, 17) + "...";
			return message;
		}
	}
}
