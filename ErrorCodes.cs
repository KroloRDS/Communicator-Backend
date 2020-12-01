namespace Communicator
{
	public class ErrorCodes
	{
		public static readonly string OK = "OK";
		public static readonly string CANNOT_ADD_YOURSELF = "Cannot add yourself to friend list";
		public static readonly string CANNOT_MESSAGE_YOURSELF = "Cannot send message to yourself";
		public static readonly string CANNOT_DELETE_YOURSELF = "Cannot delete yourself from friend list";
		public static readonly string CANNOT_FIND_USER = "Cannot find user with ID: {0}";
		public static readonly string CANNOT_FIND_MESSAGE = "Cannot find message with ID: {0}";
		public static readonly string RELATION_DOES_NOT_EXIST = "Relation between user ID: {0} and user ID: {1} does not exist";
		public static readonly string RELATION_DOES_NOT_EXIST_OR_ACCEPTED = "Relation between user ID: {0} and user ID: {1} does not exist or was already accepted";
		public static readonly string LOGIN_EXIST = "User with this login ({0}) already exist";
		public static readonly string EMAIL_EXIST = "User with this e-mail address ({0}) already exist";
		public static readonly string INVALID_OLD_PASSWORD = "Invalid old password";
		public static readonly string CANNOT_FIND_REQUEST = "Cannot find request \"{0}\"";
		public static readonly string CANNOT_FIND_REQUEST_OR_UNAUTHORIZED = "Request \"{0}\" requires user to be logged in or it does not exist";
		public static readonly string NOT_LOGGED_IN = "User is not logged in";
		public static readonly string REGISTER_WHILE_LOGGED_IN = "Cannot register new account while being logged in";
	}
}
