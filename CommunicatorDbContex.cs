using Microsoft.EntityFrameworkCore;

using Communicator.Entities;

namespace Communicator
{
	public class CommunicatorDbContex : DbContext
	{
		public CommunicatorDbContex(DbContextOptions<CommunicatorDbContex> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<FriendRelationEntity>()
				.HasKey(x => new { x.FriendListOwnerID, x.FriendID });
		}

		public DbSet<UserEntity> UserEntity { get; set; }
		public DbSet<MessageEntity> MessageEntity { get; set; }
		public DbSet<FriendRelationEntity> FriendRelationEntity { get; set; }
		public DbSet<PaymentEntity> PaymentEntity { get; set; }
	}
}
