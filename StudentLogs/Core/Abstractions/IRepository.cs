namespace Core.Abstractions
{
	public interface IRepository<T> where T : IBaseObject
	{
        Task<IEnumerable<T>> GetAsync();
        IEnumerable<T> GetByPredicate(Func<T, bool> predicate);
        Task<T> GetByIdAsync(int id);
        Task CreateAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(int id);
    }
}
