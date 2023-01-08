namespace Core.Entities
{
	public class UserAnswer : BaseObject
	{
		public User User { get; set; } = null!;
		public int UserId { get; set; }

		public Survey Survey { get; set; } = null!;
		public int SurveyId { get; set; }

		public Question Question { get; set; } = null!;
		public int QuestionId { get; set; }

		public Answer? Answer { get; set; }
		public int? AnswerId { get; set; }

		public string? TextAnswer { get; set; }
	}
}
