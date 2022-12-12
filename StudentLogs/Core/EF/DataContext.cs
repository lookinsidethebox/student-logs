using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.EF
{
	public class DataContext : DbContext
	{
		public DbSet<User> Users { get; set; } = null!;
	}
}
