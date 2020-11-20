using System.Collections.Generic;

using Communicator.DTOs;

namespace Communicator.Services
{
	public interface IDemoService
	{
		List<DemoResponse> GetAll();
		void Add(DemoRequest request);
	}
}
