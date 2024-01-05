namespace Core.Entities
{
	public class Survey : BaseObject
	{
		public string Title { get; set; } = null!;
		public bool RandomOrder { get; set; }
	}
}
