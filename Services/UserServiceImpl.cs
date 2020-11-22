using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System;

using Communicator.Repositories;
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
		
		public bool Add(UserRequest request)
		{
			int random = new Random().Next();
			string pw = request.Password + random.ToString();

			_context.UserEntity.Add(new UserEntity
			{
				Login = request.Login,
				Email = request.Email,
				BankAccount = request.BankAccount,
				PasswordHash = Hash(pw),
				Salt = random,
				PublicKey = "???" //TODO: generate from password = private key
			});
			_context.SaveChanges();
			return true;
		}

		public bool Delete(int id)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user == null)
			{
				return false;
			}

			_context.UserEntity.Remove(user);
			_context.SaveChanges();
			return true;
		}

		public bool Login(string login, string pw)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.Login == login);
			if (user == null)
			{
				return false;
			}

			pw += user.Salt;
			return user.PasswordHash == Hash(pw);
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
				PasswordHash = user.PasswordHash,
				Salt = user.Salt,
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
