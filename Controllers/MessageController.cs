using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

using Communicator.Services;
using Communicator.DTOs;

namespace Communicator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class MessageController : Controller
	{
		private readonly IMessageService _service;

		public MessageController(IMessageService service)
		{
			_service = service;
		}

		[HttpPost]
		public IActionResult Add(MessageRequest request)
		{
			return _service.Add(request) ? Ok() : BadRequest();
		}

		[HttpDelete]
		public IActionResult Delete(int id)
		{
			return _service.Delete(id) ? Ok() : BadRequest();
		}

		[HttpPut]
		public IActionResult Update(int id)
		{
			return _service.Update(id) ? Ok() : BadRequest();
		}

		[HttpGet][Route("GetByID")]
		public IActionResult GetByID(int id, bool sender)
		{
			var response = _service.GetMessage(id, sender);
			return response != null ? Ok(response) : BadRequest();
		}

		[HttpGet][Route("GetMessages")]
		public List<MessageResponse> GetMessages(DateTime time, int userId, int friendId)
		{
			return _service.GetMessages(time, userId, friendId);
		}
	}
}
