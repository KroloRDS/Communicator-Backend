using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IUserService
	{
		bool Add(UserCreateNewRequest request);
		bool Delete(int id);
		int Login(UserLoginRequest request);
		bool UpdateBankAccount(int id, string account);
		bool UpdateCredentials(int id, UserUpdateCredentialsRequest request);
		UserResponse GetByID(int id);
	}
}
