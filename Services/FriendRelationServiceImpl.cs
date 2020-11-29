using System.Collections.Generic;
using System.Linq;

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
				return "Cannot add yourself to friend list";
			}

			var relations = FindRelations(request);
			if (relations.Count == 2)
			{
				return Accept(request);
			}

			if (_context.UserEntity.FirstOrDefault(x => x.ID == request.FriendID) == null)
			{
				return "Cannot user with ID: " + request.FriendID;
			}
			if (_context.UserEntity.FirstOrDefault(x => x.ID == request.FriendListOwnerID) == null)
			{
				return "Cannot find user with ID: " + request.FriendListOwnerID;
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
			return "OK";
		}

		public string Delete(FriendRelationRequest request)
		{
			if (request.FriendListOwnerID == request.FriendID)
			{
				return "Cannot delete yourself to friend list";
			}

			var relations = FindRelations(request);
			if (relations.Count != 2)
			{
				return "Relation between user ID: " + request.FriendID + 
					" and user ID: " + request.FriendListOwnerID + " does not exist";
			}

			foreach (var relation in relations)
			{
				_context.FriendRelationEntity.Remove(relation);
			}
			_context.SaveChanges();
			return "OK";
		}

		public string Accept(FriendRelationRequest request)
		{
			if (request.FriendListOwnerID == request.FriendID)
			{
				return "Cannot add yourself to friend list";
			}

			var relation = _context.FriendRelationEntity.FirstOrDefault(x =>
				(x.FriendListOwnerID == request.FriendListOwnerID) &&
				(x.FriendID == request.FriendID));

			if (relation == null || relation.Accepted)
			{
				return "Friend relation does not exist or already accepted";
			}

			relation.Accepted = true;
			_context.SaveChanges();
			return "OK";
		}

		public List<UserResponse> GetFriendList(int userId, bool accepted)
		{
			List<FriendRelationEntity> friendList = _context.FriendRelationEntity
				.Where(x => x.FriendListOwnerID == userId && x.Accepted == accepted).ToList();

			return (from friend in friendList
					select new UserServiceImpl(_context)
					.GetByID(friend.FriendID))
					.ToList();
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
