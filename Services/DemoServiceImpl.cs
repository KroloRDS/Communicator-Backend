using System.Collections.Generic;
using System.Linq;

using Communicator.Repositories;
using Communicator.Entities;
using Communicator.DTOs;

namespace Communicator.Services
{
	public class DemoServiceImpl : IDemoService
	{
		private readonly CommunicatorDbContex _context;

		public DemoServiceImpl(CommunicatorDbContex context)
		{
			_context = context;
		}

		public void Add(DemoRequest request)
		{
			_context.DemoEntity.Add(new DemoEntity
			{
				Name = request.Name
			});
			_context.SaveChanges();
		}

		public List<DemoResponse> GetAll()
		{
			return _context.DemoEntity.Select(x => new DemoResponse(x.ID, x.Name)).ToList();
		}
	}
}
