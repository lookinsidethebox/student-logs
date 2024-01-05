namespace Core.Entities
{
	public class Question : BaseObject
	{
		public Survey Survey { get; set; } = null!;
		public int SurveyId { get; set; }

		public string Value { get; set; } = null!;

		public bool HasAnswers { get; set; }

		public bool WithoutAnswers { get; set; }
	}
}
