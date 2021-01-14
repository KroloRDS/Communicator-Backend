namespace Communicator.DTOs
{
	public class UserUpdateCredentialsRequest
	{
		public string Login { get; set; }
		public string NewPassword { get; set; }
		public string OldPassword { get; set; }
	}
}
