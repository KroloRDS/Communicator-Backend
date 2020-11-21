using Microsoft.AspNetCore.Mvc;

using Communicator.Services;
using Communicator.DTOs;

namespace Communicator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IUserService _service;

		public UserController(IUserService service)
		{
			_service = service;
		}

		[HttpPost]
		public void Add(UserRequest request)
		{
			_service.Add(request);
		}

		[HttpDelete]
		public void Delete(int id)
		{
			_service.Delete(id);
		}

		[HttpGet]
		public UserResponse GetByID(int id)
		{
			return _service.GetByID(id);
		}
	}
}
