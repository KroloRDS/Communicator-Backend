using Newtonsoft.Json.Linq;
using System.IO;
using System;

using Communicator.HelperClasses;

namespace Communicator.Source.HelperClasses
{
	public class MailConfig
	{
		public MailConfig()
		{
			try
			{
				var appsettings = JObject.Parse(File.ReadAllText("appsettings.json"));
				Port = appsettings.SelectToken("Email").Value<int>("Port");
				Host = appsettings.SelectToken("Email").Value<string>("Host");
				Login = appsettings.SelectToken("Email").Value<string>("Login");
				Password = appsettings.SelectToken("Email").Value<string>("Password");
			}
			catch (Exception exception)
			{
				Error.Log(exception);
				Port = 0;
				Host = null;
				Login = null;
				Password = null;
			}
		}

		public int Port { get; }
		public string Host { get; }
		public string Login { get; }
		public string Password { get; }
	}
}
