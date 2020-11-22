using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IUserService
	{
		bool Add(UserRequest request);
		bool Delete(int id);
		bool Login(string login, string pw);
		bool UpdateBankAccount(int id, string account);
		bool UpdateCredentials(int id, UserRequest request, string oldPassword);
		UserResponse GetByID(int id);
	}
}
