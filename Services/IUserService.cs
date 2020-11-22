using System.Collections.Generic;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IUserService
	{
		bool Add(UserRequest request);
		bool Delete(int id);
		UserResponse GetByID(int id);
	}
}
