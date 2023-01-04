namespace Core.Models
{
	public class LogModel
	{
		public string User { get; set; } = null!;
		public DateTime CreateDate { get; set; }
		public string Type { get; set; } = null!;
		public string EducationMaterial { get; set; } = null!;
	}
}
