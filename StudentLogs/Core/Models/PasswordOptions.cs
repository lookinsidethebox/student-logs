namespace Core.Models
{
	public class PasswordOptions
	{
		public string Salt { get; set; }
		public int Iterations { get; set; }
		public int KeySize { get; set; }
		public string DefaultAdminPassword { get; set; }
	}
}
