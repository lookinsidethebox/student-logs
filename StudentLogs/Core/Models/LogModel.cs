namespace Core.Models
{
	public class LogModel
	{
		public string User { get; set; } = null!;
		public int UserId { get; set; }
		public string CreateDate { get; set; } = null!;
		public string Type { get; set; } = null!;
		public string EducationMaterial { get; set; } = null!;
	}
}
