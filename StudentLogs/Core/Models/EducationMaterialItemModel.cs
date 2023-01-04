namespace Core.Models
{
	public class EducationMaterialItemModel
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public string? Description { get; set; }
		public string? FilePath { get; set; }
		public int Type { get; set; }
		public string? Text { get; set; }
		public int? SurveyId { get; set; }
		public int ClickCount { get; set; }
		public int AnswerCount { get; set; }
		public int PlayCount { get; set; }
	}
}
