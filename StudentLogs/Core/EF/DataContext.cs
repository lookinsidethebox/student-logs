using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.EF
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions options) : base(options)
		{
			AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		}

		public DbSet<User> Users { get; set; } = null!;
		public DbSet<EducationMaterial> Materials { get; set; } = null!;
		public DbSet<Log> Logs { get; set; } = null!;
	}
}
