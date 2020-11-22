using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Communicator.Services;
using Communicator.DTOs;

namespace Communicator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class FriendRelationController : Controller
	{
		private readonly IFriendRelationService _service;

		public FriendRelationController(IFriendRelationService service)
		{
			_service = service;
		}

		[HttpPost]
		public IActionResult Add(FriendRelationRequest request)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return _service.Add(request) ? Ok() : BadRequest();
		}

		[HttpDelete]
		public IActionResult Delete(FriendRelationRequest request)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return _service.Delete(request) ? Ok() : BadRequest();
		}

		[HttpPut]
		public IActionResult Accept(FriendRelationRequest request)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return _service.Accept(request) ? Ok() : BadRequest();
		}

		[HttpGet]
		public IActionResult GetFriends(int userId, bool accepted)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return Ok(_service.GetFriends(userId, accepted));
		}
	}
}
