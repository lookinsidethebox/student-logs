namespace Core.Models
{
	public class SaveUserModel
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int Role { get; set; }
		public int? SortType { get; set; }
	}
}
