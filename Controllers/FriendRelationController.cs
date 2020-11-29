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
		public IActionResult Add(int friendId)
		{
			int? id = HttpContext.Session.GetInt32("userId");
			if (id == null)
			{
				return StatusCode(440);
			}
			return _service.Add(CreateRequest(id, friendId)) ? Ok() : BadRequest();
		}

		[HttpDelete]
		[Route("delete")]
		public IActionResult Delete(int friendId)
		{
			int? id = HttpContext.Session.GetInt32("userId");
			if (id == null)
			{
				return StatusCode(440);
			}
			return _service.Delete(CreateRequest(id, friendId)) ? Ok() : BadRequest();
		}

		[HttpPut]
		[Route("accept")]
		public IActionResult Accept(int friendId)
		{
			int? id = HttpContext.Session.GetInt32("userId");
			if (id == null)
			{
				return StatusCode(440);
			}
			return _service.Accept(CreateRequest(id, friendId)) ? Ok() : BadRequest();
		}

		[HttpGet]
		[Route("get_friend_list")]
		public IActionResult GetFriendList()
		{
			int? id = HttpContext.Session.GetInt32("userId");
			return id == null ? StatusCode(440) : Ok(_service.GetFriendList((int)id, true));
		}

		[HttpGet]
		[Route("get_pending_friend_list")]
		public IActionResult GetPendingFriendList()
		{
			int? id = HttpContext.Session.GetInt32("userId");
			return id == null ? StatusCode(440) : Ok(_service.GetFriendList((int)id, false));
		}

		private static FriendRelationRequest CreateRequest(int? userId, int friendId)
		{
			return new FriendRelationRequest
			{
				FriendListOwnerID = (int)userId,
				FriendID = friendId
			};
		}
	}
}
