using Newtonsoft.Json.Linq;

namespace Communicator.Source.DTOs.JSONs
{
	public class JsonUpdateRequest : Json
	{
		public JsonUpdateRequest(string requestName, int? paramId = null)
		{
			dataType = "Update" + requestName;
			if (paramId != null)
			{
				data = JToken.FromObject(paramId).ToString();
			}
		}
	}
}
