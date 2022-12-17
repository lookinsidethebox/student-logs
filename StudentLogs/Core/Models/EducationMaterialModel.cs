namespace Core.Models
{
	public class EducationMaterialModel
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public int Type { get; set; }
		public bool IsFirst { get; set; }
		public bool IsFinal { get; set; }
		public string Text { get; set; }
		public int SurveyId { get; set; }
	}
}
