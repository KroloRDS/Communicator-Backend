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
				succesful = str.Equals(Error.OK);
				data = str;
			}
			else
			{
				succesful = true;
				data = obj;
			}
		}

		public bool succesful { get; }
	}
}
