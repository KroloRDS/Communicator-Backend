namespace Communicator.DTOs
{
	public class DemoResponse
	{
		public DemoResponse(int id, string name)
		{
			ID = id;
			Name = name;
		}

		public int ID { get; set; }
		public string Name { get; set; } 
	}
}
