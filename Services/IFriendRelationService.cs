using System.Collections.Generic;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IFriendRelationService
	{
		bool Add(FriendRelationRequest request);
		bool Delete(FriendRelationRequest request);
		bool Accept(FriendRelationRequest request);
		List<UserResponse> GetFriendList(int userId, bool accepted);
	}
}
