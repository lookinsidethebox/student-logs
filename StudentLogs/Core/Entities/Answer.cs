namespace Core.Entities
{
	public class Answer : BaseObject
	{
		public Question Question { get; set; } = null!;
		public int QuestionId { get; set; }

		public string Value { get; set; } = null!;
	}
}
