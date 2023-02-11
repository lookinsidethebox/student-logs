namespace Core.Models
{
	public class EducationMaterialListItemModel
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public string? Description { get; set; }
		public int Type { get; set; }
		public bool IsActive { get; set; }
		public string? FilePath { get; set; }
		public bool IsFirst { get; set; }
		public bool IsFinal { get; set; }
	}
}
