﻿using System.Security.Cryptography;
using System.Net.Mail;
using System.Text;
using System.Linq;
using System.Net;
using System;

using Communicator.Source.HelperClasses;
using Communicator.HelperClasses;
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
			if (request.Login == null || request.Password == null)
			{
				return Error.LOGIN_OR_PW_NULL;
			}

			if (_context.UserEntity.FirstOrDefault(x => x.Login == request.Login) != null)
			{
				return string.Format(Error.LOGIN_EXIST, request.Login);
			}
			if (_context.UserEntity.FirstOrDefault(x => x.Email == request.Email) != null)
			{
				return string.Format(Error.EMAIL_EXIST, request.Email);
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

			SendWelcomeEmail(request.Email);

			return Error.OK;
		}

		public string Delete(int id)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user == null)
			{
				return string.Format(Error.CANNOT_FIND_USER, id);
			}

			_context.FriendRelationEntity.RemoveRange(
				_context.FriendRelationEntity.Where(
					x => x.FriendID == id || x.FriendListOwnerID == id));

			_context.MessageEntity.RemoveRange(
				_context.MessageEntity.Where(
					x => x.SenderID == id || x.ReceiverID == id));

			_context.UserEntity.Remove(user);
			_context.SaveChanges();
			return Error.OK;
		}

		public string UpdateBankAccount(int id, string account)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user == null)
			{
				return string.Format(Error.CANNOT_FIND_USER, id);
			}

			user.BankAccount = account;
			_context.SaveChanges();
			return Error.OK;
		}

		public string UpdateCredentials(int id, UserUpdateCredentialsRequest request)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user == null)
			{
				return string.Format(Error.CANNOT_FIND_USER, id);
			}

			var loginReq = new UserLoginRequest
			{
				Login = user.Login,
				Password = request.OldPassword
			};
			if (Login(loginReq) == null)
			{
				return Error.INVALID_OLD_PASSWORD;
			}

			if (request.Login != user.Login)
			{
				if (_context.UserEntity.FirstOrDefault(x => x.Login == request.Login) == null)
				{
					user.Login = request.Login;
				}
				else
				{
					return string.Format(Error.LOGIN_EXIST, request.Login);
				}
			}

			if (request.NewPassword != request.OldPassword)
			{
				int random = new Random().Next();
				string pw = request.NewPassword + random.ToString();
				user.PasswordHash = Hash(pw);
				user.Salt = random;
			}

			_context.SaveChanges();
			return Error.OK;
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

		private static void SendWelcomeEmail(string userEmail)
		{
			var config = new MailConfig();
			if (config.Host == null)
			{
				Error.Log(new Exception(), "SMTP host is null");
				return;
			}

			var client = new SmtpClient()
			{
				Host = config.Host,
				Port = config.Port,
				Credentials = new NetworkCredential(config.Login, config.Password),
				EnableSsl = true,
			};

			try
			{
				client.Send(new MailMessage(config.Login, userEmail)
				{
					Subject = "Thank you for registering",
					Body = "Thank you for registering in our app. We hope you'll have a great time!"
				});
			}
			catch (Exception exception)
			{
				Error.Log(exception);
			}
		}
	}
}
