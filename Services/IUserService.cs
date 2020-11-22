using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IUserService
	{
		bool Add(UserRequest request);
		bool Delete(int id);
		bool Login(string login, string pw);
		UserResponse GetByID(int id);
	}
}
