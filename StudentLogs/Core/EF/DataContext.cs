using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.EF
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions options) : base(options) { }

		public DbSet<User> Users { get; set; } = null!;
		public DbSet<EducationMaterial> Materials { get; set; } = null!;
	}
}
