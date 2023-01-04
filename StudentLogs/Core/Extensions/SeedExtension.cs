using Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extensions
{
	public static class SeedExtension
	{
        public static void AddSeed<T>(this IServiceCollection services)
            where T : class, ISeed
        {
            services.AddTransient<ISeed, T>();
        }

        public static async Task RunSeedAsync(this IServiceProvider provider)
        {
            using (var scope = provider.CreateScope())
            {
                var seeds = scope.ServiceProvider.GetServices<ISeed>();
                await RunSeedAsync(seeds);
            }
        }

        private static async Task RunSeedAsync(IEnumerable<ISeed> seeds)
        {
            if (seeds != null)
            {
                foreach (var seed in seeds)
                {
                    await seed.SeedAsync();
                }
            }
        }
    }
}
