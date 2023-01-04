using Core.Enums;

namespace Core.Entities
{
	public class User : BaseObject
	{
		public string Title { get { return $"{LastName} {FirstName}"; } }

		public string Email { get; set; } = null!;

		public string PasswordHash { get; set; } = null!;

		public string FirstName { get; set; } = null!;

		public string LastName { get; set; } = null!;

		public UserRole Role { get; set; }

		public SortType SortType { get; set; }
	}
}
