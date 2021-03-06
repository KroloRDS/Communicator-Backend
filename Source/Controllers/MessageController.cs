﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Communicator.HelperClasses;
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
		[Route("add")]
		public IActionResult Add(MessageCreateNewRequest request)
		{
			int? userId = HttpContext.Session.GetInt32("userId");
			if (userId == null)
			{
				return StatusCode(440);
			}

			var response = _service.Add((int)userId, request);
			return response.Equals(Error.OK) ? Ok() : BadRequest(response);
		}

		[HttpDelete]
		[Route("delete")]
		public IActionResult Delete(int id)
		{
			if (HttpContext.Session.GetInt32("userId") == null)
			{
				return StatusCode(440);
			}

			var response = _service.Delete(id);
			return response.Equals(Error.OK) ? Ok() : BadRequest(response);
		}

		[HttpPut]
		[Route("update_seen")]
		public IActionResult UpdateSeen(int id)
		{
			if (HttpContext.Session.GetInt32("userId") == null)
			{
				return StatusCode(440);
			}

			var response = _service.UpdateSeen(id);
			return response.Equals(Error.OK) ? Ok() : BadRequest(response);
		}

		[HttpPut]
		[Route("update_content")]
		public IActionResult UpdateContent(int id, string content)
		{
			if (HttpContext.Session.GetInt32("userId") == null)
			{
				return StatusCode(440);
			}

			var response = _service.UpdateContent(id, content);
			return response.Equals(Error.OK) ? Ok() : BadRequest(response);
		}

		[HttpGet]
		[Route("get_by_id")]
		public IActionResult GetByID(int userId, int messageId)
		{
			if (HttpContext.Session.GetInt32("userId") == null)
			{
				return StatusCode(440);
			}

			var response = _service.GetByID(userId, messageId);
			return response != null ? Ok() : BadRequest();
		}

		[HttpGet]
		[Route("get_batch")]
		public IActionResult GetBatch(MessageGetBatchRequest request)
		{
			int? userId = HttpContext.Session.GetInt32("userId");
			if (userId == null)
			{
				return StatusCode(440);
			}

			return Ok(_service.GetBatch((int)userId, request));
		}
	}
}
