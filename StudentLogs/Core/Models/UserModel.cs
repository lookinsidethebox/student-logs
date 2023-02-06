namespace Core.Models
{
	public class UserModel
	{
		public int Id { get; set; }

		public string Email { get; set; } = null!;

		public string FirstName { get; set; } = null!;

		public string LastName { get; set; } = null!;

		public int Role { get; set; }

		public string RoleText { get; set; } = null!;

		public int SortType { get; set; }

		public string SortText { get; set; } = null!;
	}
}
