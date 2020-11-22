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
			return _service.Delete(id) ? Ok() : BadRequest();
		}

		[HttpGet]
		public IActionResult GetByID(int id)
		{
			var response = _service.GetByID(id);
			return response != null ? Ok(response) : BadRequest();
		}
	}
}
