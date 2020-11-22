using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Communicator.Services;
using Communicator.DTOs;

namespace Communicator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserController : Controller
	{
		private readonly IUserService _service;

		public UserController(IUserService service)
		{
			_service = service;
		}

		[HttpPost]
		public IActionResult Add(UserRequest request)
		{
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

		[HttpGet]
		public IActionResult GetByID(int id)
		{
			if (HttpContext.Session.GetInt32("active") != 1)
			{
				return StatusCode(440);
			}
			var response = _service.GetByID(id);
			return response != null ? Ok(response) : BadRequest();
		}

		[HttpGet]
		[Route("Login")]
		public IActionResult Login(string login, string pw)
		{
			if (_service.Login(login, pw))
			{
				HttpContext.Session.SetInt32("active", 1);
				return Ok(HttpContext.Session.Id);
			}
			return BadRequest();
		}
	}
}
