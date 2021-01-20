using Newtonsoft.Json.Linq;
using System.Text;

namespace Communicator.Source.DTOs.JSONs
{
	public class Json
	{
		public string dataType { get; set; }
		public object data { get; set; }

		public byte[] GetBytes()
		{
			var json = JObject.FromObject(this);
			return Encoding.UTF8.GetBytes(json.ToString());
		}
	}
}
