using Core.Abstractions;
using Core.EF;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Services;

namespace Core
{
	public class SeedData : ISeed
	{
		private readonly IDataContextOptionsHelper _dataContextOptionsHelper;
		private readonly IPasswordService _passwordService;

		public SeedData(IDataContextOptionsHelper dataContextOptionsHelper,
			IPasswordService passwordService)
		{
			_dataContextOptionsHelper = dataContextOptionsHelper;
			_passwordService = passwordService;
		}

		public async Task SeedAsync()
		{
			var options = _dataContextOptionsHelper.GetDataContextOptions();

			using (var db = new DataContext(options))
			{
				var repo = new BaseRepository<User>(db);
				var admin = repo.GetByPredicate(x => x.Email == "Admin").FirstOrDefault();

				if (admin == null)
				{
					admin = new User
					{
						FirstName = "Admin",
						LastName = "Admin",
						Email = "Admin",
						PasswordHash = _passwordService.GenerateHash(),
						Role = UserRole.Admin,
						SortType = SortType.NotSet
					};

					await repo.CreateAsync(admin);
				}
			}
		}
	}
}
