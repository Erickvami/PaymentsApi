
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace SparkTech.Data.Repositories {
    public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
    {   
        /// <summary>
        /// Delete a row from database based on its id
        /// also allowing to soft delete the row
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isSoft"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task DeleteAsync(int id, bool isSoft = false)
        {
            var entity = await context.Set<T>().FindAsync(id);
            if (isSoft)
            {                 
                if (entity == null) return;

                // boolean property called IsDeleted for soft delete.
                var property = typeof(T).GetProperty("IsDeleted");
                if (property != null)
                {
                    property.SetValue(entity, true);
                    await UpdateAsync(entity);
                }
                else throw new InvalidOperationException("Soft delete not supported for this entity.");
                return;
            }

            if (entity != null)
            {
                context.Set<T>().Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Consult all rows from database entity
        /// and allows to include relationships to current result
        /// </summary>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetAll(System.Linq.Expressions.Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
        {
            var query = context.Set<T>().AsQueryable();
            // Check if is deleted
            if (typeof(T).GetProperty("IsDeleted") != null)
                query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
            
            foreach (var include in includes) query = query.Include(include);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Consult a list of data from database based on conditions
        /// and allows to include relationships to current result
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetBy(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
        {
            var query = context.Set<T>().AsQueryable();
            foreach (var include in includes) query = query.Include(include);

            return await query.Where(expression).ToListAsync();
        }

        // Consult one item by id from database allowing to include relationships to current result
        public async Task<T?> GetById(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = context.Set<T>();

            foreach (var include in includes) query = query.Include(include);

            return await query.FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Id") == id);
        }

        /// <summary>
        /// Insert a new row into the database, attaching related entities if they already exist.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Insert(T entity)
        {
            var entry = context.Entry(entity);
            // Iterate over the navigation properties (relationships) of the entity
            foreach (var navigation in entry.Navigations)
                // Check if the navigation property is a collection of related entities
                if (navigation.CurrentValue is IEnumerable<object> relatedEntities)
                    // Attach each related entity to the context if it already exists in the database
                    foreach (var relatedEntity in relatedEntities)
                        context.Attach(relatedEntity);
                // If the navigation property is a single related entity, attach it to the context
                else if (navigation.CurrentValue != null)
                    context.Attach(navigation.CurrentValue);
            
            await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Insert several rows into database
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task InsertRange(IEnumerable<T> entities)
        {
            context.Set<T>().AddRange(entities);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Verifies if id exist in database table
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<bool> Exist(int entityId)
        {
            return await context.Set<T>().AnyAsync(a => GetId(a) == entityId);
        }

        /// <summary>
        /// Updates an entity from database
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task UpdateAsync(T entity)
        {
            var entry = context.Entry(entity);
            // Iterate over the navigation properties (relationships) of the entity
            foreach (var navigation in entry.Navigations)
                // Check if the navigation property is a collection of related entities
                if (navigation.CurrentValue is IEnumerable<object> relatedEntities)
                    // Attach each related entity to the context if it already exists in the database
                    foreach (var relatedEntity in relatedEntities)
                        context.Attach(relatedEntity);
                // If the navigation property is a single related entity, attach it to the context
                else if (navigation.CurrentValue != null)
                    context.Attach(navigation.CurrentValue);
                    
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateWithRelationshipsAsync(T entity)
        {
            var entry = context.Entry(entity);

            foreach (var navigation in entry.Navigations)
            {
                if (navigation.CurrentValue is IEnumerable<object> relatedEntities)
                {
                    foreach (var relatedEntity in relatedEntities)
                    {
                        var relatedEntry = context.Entry(relatedEntity);
                        if (relatedEntry.State == EntityState.Detached)
                        {
                            context.Attach(relatedEntity);
                        }
                    }
                }
                else if (navigation.CurrentValue != null)
                {
                    var relatedEntry = context.Entry(navigation.CurrentValue);
                    if (relatedEntry.State == EntityState.Detached)
                    {
                        context.Attach(navigation.CurrentValue);
                    }
                }
            }

            context.Set<T>().Update(entity);

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Allows to do many transactions and do rollback if one action fails
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task ExecuteTransaction(Func<Task> action)
        {
            var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await action();
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private int GetId(T entity)
        {
            var propertyInfo = typeof(T).GetProperty("Id");
            if (propertyInfo == null)
                throw new InvalidOperationException("Entity does not have an Id property.");

            return (int)(propertyInfo.GetValue(entity) ?? 0);
        }
    }
}