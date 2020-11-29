using System.Collections.Generic;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IFriendRelationService
	{
		string Add(FriendRelationRequest request);
		string Delete(FriendRelationRequest request);
		string Accept(FriendRelationRequest request);
		List<UserResponse> GetFriendList(int userId, bool accepted);
		List<int> GetFriendListIDs(int userId, bool accepted);
	}
}
