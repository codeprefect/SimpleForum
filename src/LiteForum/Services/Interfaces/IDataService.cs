using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LiteForum.Services.Interfaces
{
    public interface IDataService<TContext, TEntity>
    {
        #region all the getters first

        Task<IEnumerable<TEntity>> GetAllAsync(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null);

        Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null);

        Task<TEntity> GetOneAsync(
            Expression<Func<TEntity, bool>> filter = null,
            string includeProperties = null);

        Task<TEntity> GetFirstAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null);

        Task<TEntity> GetByIdAsync(object id);

        Task<int> GetCountAsync(Expression<Func<TEntity, bool>> filter = null);

        Task<bool> GetExistsAsync(Expression<Func<TEntity, bool>> filter = null);

        #endregion the getters ends here

        #region and here are the create, update, save and delete

        TEntity Create(TEntity entity, string createdBy = null);

        TEntity Update(TEntity entity, string modifiedBy = null);

        void Delete(object id, string deletedBy = null);

        void Delete(TEntity entity, string deletedBy = null);

        Task SaveAsync();

        #endregion and they end here
    }
}
