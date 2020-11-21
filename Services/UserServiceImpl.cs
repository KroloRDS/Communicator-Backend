﻿using System.Collections.Generic;
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
		public void Add(UserRequest request)
		{
			int random = new Random().Next();
			string pw = request.Password + random.ToString();

			_context.UserEntity.Add(new UserEntity
			{
				Login = request.Login,
				Email = request.Email,
				BankAccount = request.BankAccount,
				PasswordHash = pw.GetHashCode(),
				Salt = random,
				PublicKey = "???" //TODO: generate from password = private key
			});
			_context.SaveChanges();
		}

		public void Delete(int id)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user != null)
			{
				_context.UserEntity.Remove(user);
				_context.SaveChanges();
			}
			//TODO: return codes
		}

		public UserResponse GetByID(int id)
		{
			var user = _context.UserEntity.FirstOrDefault(x => x.ID == id);
			if (user == null)
			{
				return null;
				//TODO: return codes
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
	}
}
