using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

using Communicator.Services;
using Communicator.DTOs;

namespace Communicator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DemoController : ControllerBase
	{
		private readonly IDemoService _service;

		public DemoController(IDemoService service)
		{
			_service = service;
		}

		[HttpGet]
		public List<DemoResponse> GetAll()
		{
			return _service.GetAll();
		}

		[HttpPost]
		public void Add(DemoRequest request)
		{
			_service.Add(request);
		}
	}
}
