using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IUserService
	{
		string Add(UserCreateNewRequest request);
		string Delete(int id);
		UserResponse Login(UserLoginRequest request);
		string UpdateBankAccount(int id, string account);
		string UpdateCredentials(int id, UserUpdateCredentialsRequest request);
		UserResponse GetByID(int id);
	}
}
