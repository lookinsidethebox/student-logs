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
		public List<SaveAnswerModel>? Answers { get; set; }
	}

	public class SaveAnswerModel
	{
		public string Title { get; set; } = null!;
	}
}
