using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System;

using Communicator.Entities;
using Communicator.DTOs;

namespace Communicator.Services
{
	public class UserServiceImpl : IUserService
	{
		private readonly CommunicatorDbContex _context;

		public UserServiceImpl(CommunicatorDbContex context)
		{
			_context = context;
		}
		
		public string Add(UserCreateNewRequest request)
		{
			if (_context.UserEntity.FirstOrDefault(x => x.Login == request.Login) != null)
			{
				return "User with this login (" + request.Login + ") already exist";
			}
			if (_context.UserEntity.FirstOrDefault(x => x.Email == request.Email) != null)
			{
				return "User with this e-mail address (" + request.Email + ") already exist";
			}

			int random = new Random().Next();
			string pw = request.Password + random.ToString();

			_context.UserEntity.Add(new UserEntity
			{
				Login = request.Login,
				Email = request.Email,
				PasswordHash = Hash(pw),
				Salt = random,
				PublicKey = "???" //TODO: generate from password = private key
			});
			_context.SaveChanges();
			return "OK";
		}

		public string Delete(int id)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user == null)
			{
				return "Cannot find user with ID: " + id;
			}

			_context.FriendRelationEntity.RemoveRange(
				_context.FriendRelationEntity.Where(
					x => x.FriendID == id || x.FriendListOwnerID == id));

			_context.MessageEntity.RemoveRange(
				_context.MessageEntity.Where(
					x => x.SenderID == id || x.ReceiverID == id));

			_context.UserEntity.Remove(user);
			_context.SaveChanges();
			return "OK";
		}

		public string UpdateBankAccount(int id, string account)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user == null)
			{
				return "Cannot find user with ID: " + id;
			}

			user.BankAccount = account;
			_context.SaveChanges();
			return "OK";
		}

		public string UpdateCredentials(int id, UserUpdateCredentialsRequest request)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user == null)
			{
				return "Cannot find user with ID: " + id;
			}

			var loginReq = new UserLoginRequest
			{
				Login = user.Login,
				Password = request.OldPassword
			};
			if (Login(loginReq) == null)
			{
				return "Invalid old password";
			}

			if (request.Login != user.Login)
			{
				if (_context.UserEntity.FirstOrDefault(x => x.Login == request.Login) == null)
				{
					user.Login = request.Login;
				}
				else
				{
					return "User with this login (" + request.Login + ") already exist";
				}
			}

			if (request.Email != user.Email)
			{
				if (_context.UserEntity.FirstOrDefault(x => x.Email == request.Email) == null)
				{
					user.Email = request.Email;
				}
				else
				{
					return "User with this e-mail address (" + request.Email + ") already exist";
				}
			}

			if (request.NewPassword != request.OldPassword)
			{
				int random = new Random().Next();
				string pw = request.NewPassword + random.ToString();
				user.PasswordHash = Hash(pw);
			}

			_context.SaveChanges();
			return "OK";
			
		}

		public UserResponse Login(UserLoginRequest request)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.Login == request.Login);
			if (user == null || user.PasswordHash != Hash(request.Password += user.Salt))
			{
				return null;
			}

			return new UserResponse
			{
				ID = user.ID,
				Login = user.Login,
				Email = user.Email,
				BankAccount = user.BankAccount,
				PublicKey = user.PublicKey
			};
		}

		public UserResponse GetByID(int id)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user == null)
			{
				return null;
			}

			return new UserResponse
			{
				ID = user.ID,
				Login = user.Login,
				Email = user.Email,
				BankAccount = user.BankAccount,
				PublicKey = user.PublicKey
			};
		}

		private static string Hash(string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			bytes = SHA512Managed.Create().ComputeHash(bytes);
			return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}
	}
}
