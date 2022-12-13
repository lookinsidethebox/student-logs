using Core.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Core.Helpers
{
	public interface IDataContextOptionsHelper
	{
		DbContextOptions<DataContext> GetDataContextOptions();
	}

	public class DataContextOptionsHelper : IDataContextOptionsHelper
	{
		private readonly IConfiguration _configuration;

		public DataContextOptionsHelper(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public DbContextOptions<DataContext> GetDataContextOptions()
		{
			var connectionString = _configuration.GetConnectionString("DefaultConnection");
			var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
			return optionsBuilder.UseNpgsql(connectionString).Options;
		}
	}
}
