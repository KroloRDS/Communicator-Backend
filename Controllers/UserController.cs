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
			var response = _service.Add(request);
			return response.Equals(Error.OK) ? Ok() : BadRequest(response);
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

			var response = _service.Delete((int)id);
			return response.Equals(Error.OK) ? Ok() : BadRequest(response);
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

			var response = _service.UpdateBankAccount((int)id, account);
			return response.Equals(Error.OK) ? Ok() : BadRequest(response);
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

			var response = _service.UpdateCredentials((int)id, request);
			return response.Equals(Error.OK) ? Ok() : BadRequest(response);
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
			var user = _service.Login(request);
			if (user != null)
			{
				HttpContext.Session.SetInt32("userId", user.ID);
				return Ok(user);
			}

			return BadRequest();
		}

		[HttpGet]
		[Route("is_logged_in")]
		public IActionResult IsLoggedIn()
		{
			int? id = HttpContext.Session.GetInt32("userId");
			return id == null ? Accepted() : GetByID((int)id);
		}
	}
}
