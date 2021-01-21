using Communicator.HelperClasses;

namespace Communicator.Source.DTOs.JSONs
{
	public class JsonResponse : Json
	{
		public JsonResponse(string requestName, object obj)
		{
			dataType = requestName + "Response";
			if (obj is string str)
			{
				successful = str.Equals(Error.OK);
				data = str;
			}
			else
			{
				successful = true;
				data = obj;
			}
		}

		public bool successful { get; }
	}
}
