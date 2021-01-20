using System;

using Communicator.HelperClasses;

namespace Communicator.Source.DTOs.JSONs
{
	public class JsonErrorResponse : Json
	{
		public JsonErrorResponse(Exception exception, string data = null)
		{
			dataType = "ErrorResponse";
			succesful = false;
			this.exception = exception.Message;
			this.data = data;

			Error.Log(exception, data);
		}

		public bool succesful { get; }
		public string exception { get; }
	}
}
