using System.Collections.Generic;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IUserService
	{
		void Add(UserRequest request);
		void Delete(int id);
		UserResponse GetByID(int id);
	}
}
