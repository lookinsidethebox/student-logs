namespace Core.Models
{
	public class SurveyModel
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public bool IsCompleted { get; set; }
		public IEnumerable<QuestionModel> Questions { get; set; } = null!;
	} 

	public class QuestionModel
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public string? Value { get; set; } = null!;
		public IEnumerable<AnswerModel>? Answers { get; set; }
	}

	public class AnswerModel
	{
		public int Id { get; set; }
		public bool IsSelected { get; set; }
		public string Title { get; set; } = null!;
	}
}
