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
		[Route("add")]
		public IActionResult Add(UserCreateNewRequest request)
		{
			return _service.Add(request) ? Ok() : BadRequest();
		}

		[HttpDelete]
		[Route("delete")]
		public IActionResult Delete()
		{
			int? id = HttpContext.Session.GetInt32("userId");
			if (id == null)
			{
				return StatusCode(440);
			}
			return _service.Delete((int)id) ? Ok() : BadRequest();
		}

		[HttpPut]
		[Route("update_bank_account")]
		public IActionResult UpdateBankAccount(string account)
		{
			int? id = HttpContext.Session.GetInt32("userId");
			if (id == null)
			{
				return StatusCode(440);
			}
			return _service.UpdateBankAccount((int)id, account) ? Ok() : BadRequest();
		}

		[HttpPut]
		[Route("update_credentials")]
		public IActionResult UpdateCredentials(UserUpdateCredentialsRequest request)
		{
			int? id = HttpContext.Session.GetInt32("userId");
			if (id == null)
			{
				return StatusCode(440);
			}
			return _service.UpdateCredentials((int)id, request) ? Ok() : BadRequest();
		}

		[HttpGet]
		[Route("get_by_id")]
		public IActionResult GetByID(int id)
		{
			if (HttpContext.Session.GetInt32("userId") == null)
			{
				return StatusCode(440);
			}
			var response = _service.GetByID(id);
			return response != null ? Ok(response) : BadRequest();
		}

		[HttpPost]
		[Route("login")]
		public IActionResult Login(UserLoginRequest request)
		{
			int id = _service.Login(request);
			if (id != -1)
			{
				HttpContext.Session.SetInt32("userId", id);
				return Ok(HttpContext.Session.Id);
			}
			return BadRequest();
		}
	}
}
