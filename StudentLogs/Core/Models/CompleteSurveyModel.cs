namespace Core.Models
{
	public class CompleteSurveyModel
	{
		public int SurveyId { get; set; }
		public int EducationMaterialId { get; set; }
		public List<SurveyAnswerModel> Answers { get; set; } = null!;
	}

	public class SurveyAnswerModel
	{ 
		public int QuestionId { get; set; }
		public int? AnswerId { get; set; }
		public string? Value { get; set; }
	}
}
