using Core.Enums;

namespace Core.Entities
{
	public class User : BaseObject
	{
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public UserRole Role { get; set; }
		public SortType SortType { get; set; }
	}
}
