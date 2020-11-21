using Microsoft.EntityFrameworkCore;

namespace Communicator.Repositories
{
	public class CommunicatorDbContex : DbContext
	{
		public CommunicatorDbContex(DbContextOptions<CommunicatorDbContex> options)
			: base(options)
		{
		}

		public DbSet<DemoEntity> DemoEntity { get; set; }
	}
}
