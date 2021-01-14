using Newtonsoft.Json.Linq;
using System.Text;
using System;

namespace Communicator.HelperClasses
{
	public class JsonHandler
	{
		public static byte[] GetUpdateRequest(string requestName, int? paramId = null)
		{
			var json = new JObject
			{
				{ "dataType", "Update" + requestName },
			};
			if (paramId != null)
			{
				json.Add("data", new JObject
				{
					{ "id", paramId },
				});
			}
			return Encoding.UTF8.GetBytes(json.ToString());
		}

		public static byte[] GetResponse(string requestName, Object obj)
		{
			var json = new JObject
			{
				{ "dataType", requestName + "Response" },
			};
			if (obj is string str)
			{
				json.Add("successful", str.Equals(Error.OK));
				json.Add("data", str);
			}
			else
			{
				json.Add("successful", true);
				json.Add("data", JToken.FromObject(obj));
			}
			return Encoding.UTF8.GetBytes(json.ToString());
		}

		public static byte[] GetErrorResponse(Exception exception, string data = null)
		{
			Error.Log(exception, data);
			var json = new JObject
			{
				{ "dataType", "ErrorResponse" },
				{ "successful", false },
				{ "exception", exception.Message },
			};

			if (data != null)
			{
				json.Add("additionalInfo", data);
			}

			return Encoding.UTF8.GetBytes(json.ToString());
		}
	}
}
