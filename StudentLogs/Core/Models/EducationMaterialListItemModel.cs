namespace Core.Models
{
	public class EducationMaterialListItemModel
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public int Type { get; set; }
		public bool IsActive { get; set; }
	}
}
