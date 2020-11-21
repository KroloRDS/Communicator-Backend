using Microsoft.EntityFrameworkCore;

using Communicator.Entities;

namespace Communicator.Repositories
{
	public class CommunicatorDbContex : DbContext
	{
		public CommunicatorDbContex(DbContextOptions<CommunicatorDbContex> options)
			: base(options)
		{
		}

		public DbSet<DemoEntity> DemoEntity { get; set; }
		public DbSet<UserEntity> UserEntity { get; set; }
		public DbSet<MessageEntity> MessageEntity { get; set; }
	}
}
