
using System.Linq.Expressions;

namespace SparkTech.Data.Repositories {
    public interface IRepository<T> where T: class {
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetBy(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
        Task<T?> GetById(int id, params Expression<Func<T, object>>[] includes);
        Task DeleteAsync(int id, bool isSoft = false);
        Task Insert(T entity);
        Task InsertRange(IEnumerable<T> entities);
        Task<bool> Exist(int entityId);
        Task ExecuteTransaction(Func<Task> action);
        Task UpdateAsync(T entity);
        Task UpdateWithRelationshipsAsync(T entity);
    }
}