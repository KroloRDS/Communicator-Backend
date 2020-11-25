﻿using System.Collections.Generic;
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

		public bool Add(FriendRelationRequest request)
		{
			if (request.FriendListOwnerID == request.FriendID)
			{
				return false;
			}

			var relations = FindRelations(request);
			if (relations.Count == 2)
			{
				return Accept(request);
			}

			if (_context.UserEntity.FirstOrDefault(x => x.ID == request.FriendID) == null ||
				_context.UserEntity.FirstOrDefault(x => x.ID == request.FriendListOwnerID) == null)
			{
				return false;
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
			return true;
		}

		public bool Delete(FriendRelationRequest request)
		{
			if (request.FriendListOwnerID == request.FriendID)
			{
				return false;
			}

			var relations = FindRelations(request);
			if (relations.Count != 2)
			{
				return false;
			}

			foreach (var relation in relations)
			{
				_context.FriendRelationEntity.Remove(relation);
			}
			_context.SaveChanges();
			return true;
		}

		public bool Accept(FriendRelationRequest request)
		{
			if (request.FriendListOwnerID == request.FriendID)
			{
				return false;
			}

			var relation = _context.FriendRelationEntity.FirstOrDefault(x =>
				(x.FriendListOwnerID == request.FriendListOwnerID) &&
				(x.FriendID == request.FriendID));

			if (relation == null || relation.Accepted)
			{
				return false;
			}

			relation.Accepted = true;
			_context.SaveChanges();
			return true;
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
