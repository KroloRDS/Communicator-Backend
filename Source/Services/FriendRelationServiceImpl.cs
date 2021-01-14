using System.Collections.Generic;
using System.Linq;

using Communicator.HelperClasses;
using Communicator.Entities;
using Communicator.DTOs;

namespace Communicator.Services
{
	public class FriendRelationServiceImpl : IFriendRelationService
	{
		private readonly CommunicatorDbContex _context;

		public FriendRelationServiceImpl(CommunicatorDbContex context)
		{
			_context = context;
		}

		public string Add(FriendRelationRequest request)
		{
			if (request.FriendListOwnerID == request.FriendID)
			{
				return Error.CANNOT_ADD_YOURSELF;
			}

			var relations = FindRelations(request);
			if (relations.Count == 2)
			{
				return Accept(request);
			}

			if (_context.UserEntity.FirstOrDefault(x => x.ID == request.FriendID) == null)
			{
				return string.Format(Error.CANNOT_FIND_USER, request.FriendID);
			}
			if (_context.UserEntity.FirstOrDefault(x => x.ID == request.FriendListOwnerID) == null)
			{
				return string.Format(Error.CANNOT_FIND_USER, request.FriendListOwnerID);
			}

			_context.FriendRelationEntity.Add(new FriendRelationEntity
			{
				FriendListOwnerID = request.FriendListOwnerID,
				FriendID = request.FriendID,
				Accepted = true
			});
			_context.FriendRelationEntity.Add(new FriendRelationEntity
			{
				FriendListOwnerID = request.FriendID,
				FriendID = request.FriendListOwnerID,
				Accepted = false
			});
			_context.SaveChanges();
			return Error.OK;
		}

		public string Delete(FriendRelationRequest request)
		{
			if (request.FriendListOwnerID == request.FriendID)
			{
				return Error.CANNOT_DELETE_YOURSELF;
			}

			var relations = FindRelations(request);
			if (relations.Count != 2)
			{
				return string.Format(Error.RELATION_DOES_NOT_EXIST, request.FriendID, request.FriendListOwnerID);
			}

			foreach (var relation in relations)
			{
				_context.FriendRelationEntity.Remove(relation);
			}
			_context.SaveChanges();
			return Error.OK;
		}

		public string Accept(FriendRelationRequest request)
		{
			if (request.FriendListOwnerID == request.FriendID)
			{
				return Error.CANNOT_ADD_YOURSELF;
			}

			var relation = _context.FriendRelationEntity.FirstOrDefault(x =>
				(x.FriendListOwnerID == request.FriendListOwnerID) &&
				(x.FriendID == request.FriendID));

			if (relation == null || relation.Accepted)
			{
				return Error.RELATION_DOES_NOT_EXIST_OR_ACCEPTED;
			}

			relation.Accepted = true;
			_context.SaveChanges();
			return Error.OK;
		}

		public List<UserWithLastMessageResponse> GetFriendList(int userId, bool accepted)
		{
			List<FriendRelationEntity> friendList = _context.FriendRelationEntity
				.Where(x => x.FriendListOwnerID == userId && x.Accepted == accepted).ToList();

			var userList =
				(from friend in friendList
				 select new UserServiceImpl(_context)
				 .GetByID(friend.FriendID))
				 .ToList();
			
			return userList.Select(x => new UserWithLastMessageResponse
			{
				BankAccount = x.BankAccount,
				Email = x.Email,
				ID = x.ID,
				Login = x.Login,
				PublicKey = x.PublicKey,
				LastMessage = 
				new MessageSerciveImpl(_context).GetLastMessage(userId, x.ID)
			}).ToList();
		}

		public List<int> GetFriendListIDs(int userId, bool accepted)
		{
			List<FriendRelationEntity> friendList = _context.FriendRelationEntity
				.Where(x => x.FriendListOwnerID == userId && x.Accepted == accepted).ToList();

			return (from friend in friendList
					select friend.FriendID).ToList();
		}

		private List<FriendRelationEntity> FindRelations(FriendRelationRequest request)
		{
			return _context.FriendRelationEntity.Where(x =>
				((x.FriendListOwnerID == request.FriendListOwnerID) &&
				(x.FriendID == request.FriendID)) ||
				((x.FriendID == request.FriendListOwnerID) &&
				(x.FriendListOwnerID == request.FriendID)))
				.ToList();
		}
	}
}
