using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;
using System;

namespace Communicator.WebSockets
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
				json.Add("successful", str.Equals(ErrorCodes.OK));
				json.Add("data", str);
			}
			else
			{
				json.Add("successful", true);
				json.Add("data", JToken.FromObject(obj));
			}
			return Encoding.UTF8.GetBytes(json.ToString());
		}

		public static byte[] GetErrorResponse(byte[] bytes)
		{
			var data = Encoding.UTF8.GetString(RemoveTrailingZeros(bytes));
			ErrorLog("Invalid JSON", data);
			var json = new JObject
			{
				{ "dataType", "ErrorResponse" },
				{ "successful", false },
				{ "data", "Invalid JSON: " + data },
			};
			return Encoding.UTF8.GetBytes(json.ToString());
		}

		private static byte[] RemoveTrailingZeros(byte[] bytes)
		{
			var i = bytes.Length - 1;
			while (bytes[i] == 0)
			{
				--i;
			}
			var temp = new byte[i + 1];
			Array.Copy(bytes, temp, i + 1);
			return temp;
		}

		private static void ErrorLog(string message, string data)
		{
			var log = DateTime.Now.ToString();
			log += " " + message + "\n" + data + "\n\n";
			File.AppendAllText("Logs\\error_log.txt", log);
		}
	}
}
