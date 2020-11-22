using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

using Communicator.Services;
using Communicator.DTOs;

namespace Communicator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class MessageController : ControllerBase
	{
		private readonly IMessageService _service;

		public MessageController(IMessageService service)
		{
			_service = service;
		}

		[HttpPost]
		public void Add(MessageRequest request)
		{
			_service.Add(request);
		}

		[HttpDelete]
		public void Delete(int id)
		{
			_service.Delete(id);
		}

		[HttpPut]
		public void Update(int id)
		{
			_service.Update(id);
		}

		[HttpGet]
		[Route("GetByID")]
		public MessageResponse GetByID(int id, bool sender)
		{
			return _service.GetMessage(id, sender);
		}

		[HttpGet]
		[Route("GetMessages")]
		public List<MessageResponse> GetMessages(DateTime time, int userId, int friendId)
		{
			return _service.GetMessages(time, userId, friendId);
		}
	}
}
