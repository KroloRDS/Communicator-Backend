﻿namespace Communicator.DTOs
{
	public class UserResponse
	{
		public int ID { get; set; }
		public string Login { get; set; }
		public string Email { get; set; }
		public string BankAccount { get; set; }
		public string PasswordHash { get; set; }
		public int Salt { get; set; }
		public string PublicKey { get; set; }
	}
}
