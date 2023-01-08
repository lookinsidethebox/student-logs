namespace Core.Models
{
	public class SaveSurveyModel
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public List<SaveQuestionModel> Questions { get; set; } = null!;
	}

	public class SaveQuestionModel
	{
		public string Title { get; set; } = null!;
		public List<string>? Answers { get; set; }
	}
}
