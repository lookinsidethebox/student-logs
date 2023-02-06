namespace Core.Models
{
	public class SaveUserModel
	{
		public int Id { get; set; }
		public string Email { get; set; } = null!;
		public string? Password { get; set; }
		public string FirstName { get; set; } = null!;
		public string LastName { get; set; } = null!;
		public int Role { get; set; }
		public int? SortType { get; set; }
	}
}
