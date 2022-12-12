using Core.Abstractions;
using Core.EF;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
	public class BaseRepository<T> : IRepository<T> where T : BaseObject
	{
		private readonly DataContext _context;
		private readonly DbSet<T> _dbSet;

		public BaseRepository(DataContext context)
		{
			_context = context;
			_dbSet = context.Set<T>();
		}

		public virtual async Task<IEnumerable<T>> GetAsync()
		{
			return await _dbSet.AsNoTracking().ToListAsync();
		}

		public virtual IEnumerable<T> GetByPredicate(Func<T, bool> predicate)
		{
			return _dbSet.AsNoTracking().Where(predicate).ToList();
		}

		public virtual async Task<T> GetByIdAsync(int id)
		{
			return await _dbSet.FindAsync(id);
		}

		public virtual async Task CreateAsync(T item)
		{
			await _dbSet.AddAsync(item);
			await _context.SaveChangesAsync();
		}

		public virtual async Task UpdateAsync(T item)
		{
			_context.Entry(item).State = EntityState.Modified;
			await _context.SaveChangesAsync();
		}

		public virtual async Task DeleteAsync(int id)
		{
			var item = await GetByIdAsync(id);

			if (item != null)
			{
				_dbSet.Remove(item);
				await _context.SaveChangesAsync();
			}
		}
	}
}
