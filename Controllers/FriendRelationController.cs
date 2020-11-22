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
		[Route("add")]
		public IActionResult Add(FriendRelationRequest request)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return _service.Add(request) ? Ok() : BadRequest();
		}

		[HttpDelete]
		[Route("delete")]
		public IActionResult Delete(FriendRelationRequest request)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return _service.Delete(request) ? Ok() : BadRequest();
		}

		[HttpPut]
		[Route("accept")]
		public IActionResult Accept(FriendRelationRequest request)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return _service.Accept(request) ? Ok() : BadRequest();
		}

		[HttpGet]
		[Route("get_friend_list")]
		public IActionResult GetFriendList(int userId, bool accepted)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			return Ok(_service.GetFriendList(userId, accepted));
		}
	}
}
