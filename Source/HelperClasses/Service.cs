using Communicator.Services;

namespace Communicator.HelperClasses
{
	public class Service
	{
		public Service(CommunicatorDbContex db)
		{
			FriendRelation = new FriendRelationServiceImpl(db);
			Message = new MessageSerciveImpl(db);
			Payment = new PaymentServiceImpl(db);
			User = new UserServiceImpl(db);
		}

		public IFriendRelationService FriendRelation { get; }
		public IMessageService Message { get; }
		public IPaymentService Payment { get; }
		public IUserService User { get; }
	}
}
