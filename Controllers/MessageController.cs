using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return _service.Add(request) ? Ok() : BadRequest();
		}

		[HttpDelete]
		public IActionResult Delete(int id)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return _service.Delete(id) ? Ok() : BadRequest();
		}

		[HttpPut]
		public IActionResult Update(int id)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return _service.Update(id) ? Ok() : BadRequest();
		}

		[HttpGet]
		[Route("GetByID")]
		public IActionResult GetByID(int id, bool sender)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			var response = _service.GetMessage(id, sender);
			return response != null ? Ok(response) : BadRequest();
		}

		[HttpGet]
		[Route("GetMessages")]
		public IActionResult GetMessages(DateTime time, int userId, int friendId)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return Ok(_service.GetMessages(time, userId, friendId));
		}
	}
}
