namespace Core.Models
{
	public class SaveSurveyModel
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public bool RandomOrder { get; set; }
		public List<SaveQuestionModel> Questions { get; set; } = null!;
	}

	public class SaveQuestionModel
	{
		public string Title { get; set; } = null!;
		public bool WithoutAnswers { get; set; }
		public List<SaveAnswerModel>? Answers { get; set; }
	}

	public class SaveAnswerModel
	{
		public string Title { get; set; } = null!;
	}
}
