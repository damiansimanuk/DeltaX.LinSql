using Dapper;
using DeltaX.LinSql.Query;
using DeltaX.LinSql.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeltaX.RepositoryDemo1
{ 

    public class DapperRepositoryBase
    {
        protected TableQueryFactory queryFactory;
        protected IDbConnection db;
        protected ILogger logger;

        public DapperRepositoryBase(IDbConnection db, TableQueryFactory queryFactory, ILogger logger = null)
        {
            this.queryFactory = queryFactory;
            this.db = db;
            this.logger = logger;
        }

        public Task DeleteAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            var query = queryFactory.GetDeleteQuery<TEntity>();
            logger.LogDebug("DeleteAsync query:{query} entity:{@entity}", query, entity);
            return db.ExecuteAsync(query, entity);
        }

        public Task DeleteAsync<TEntity>(string whereClause, object param)
            where TEntity : class
        {
            var query = queryFactory.GetDeleteQuery<TEntity>(whereClause);
            logger.LogDebug("DeleteAsync query:{query} whereClause:{whereClause} param:{@param}", query, whereClause, param);
            return db.ExecuteAsync(query, param);
        }


        public async Task<TEntity> InsertAsync<TEntity>(TEntity item, IEnumerable<string> fieldsToInsert = null)
            where TEntity : class
        {
            var query = queryFactory.GetInsertQuery<TEntity>(fieldsToInsert);

            var identityColumn = queryFactory.GetTable<TEntity>().GetIdentityColumn();
            if (identityColumn != null)
            {
                query += "; " + queryFactory.DialectQuery.IdentityQueryFormatSql;
                logger.LogDebug("InsertAsync query:{query} item:{@item}", query, item);
                var fieldId = await db.ExecuteScalarAsync(query, item);

                // Set Property Value  
                var propertyColumn = identityColumn.GetPropertyInfo();
                propertyColumn.SetValue(item, Convert.ChangeType(fieldId, propertyColumn.PropertyType));
            }
            else
            {
                logger.LogDebug("InsertAsync query:{query} item:{@item}", query, item);
                await db.ExecuteAsync(query, item);
            }

            return item;
        }

        public Task<Tkey> InsertAsync<TEntity, Tkey>(TEntity item, IEnumerable<string> fieldsToInsert = null)
            where TEntity : class
        {
            var query = queryFactory.GetInsertQuery<TEntity>(fieldsToInsert);
            query += "; " + queryFactory.DialectQuery.IdentityQueryFormatSql;
            logger.LogDebug("InsertAsync query:{query} item:{@item}", query, item);

            return db.ExecuteScalarAsync<Tkey>(query, item);
        }

        public Task<TEntity> GetAsync<TEntity>(object param)
            where TEntity : class
        {
            var query = queryFactory.GetSingleQuery<TEntity>();
            logger.LogDebug("GetAsync query:{query} param:{@param}", query, param);

            return db.QueryFirstOrDefaultAsync<TEntity>(query, param);
        }

        public Task<TEntity> GetAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            return GetAsync<TEntity>((object)entity);
        }

        public Task<TEntity> GetAsync<TEntity>(string whereClause, object param)
            where TEntity : class
        {
            var query = queryFactory.GetSingleQuery<TEntity>(whereClause);
            logger.LogDebug("GetAsync query:{query} whereClause:{whereClause} param:{@param}", query, whereClause, param);

            return db.QueryFirstOrDefaultAsync<TEntity>(query, param);
        }

        public Task<IEnumerable<TEntity>> GetPagedListAsync<TEntity>(int skipCount = 0, int rowsPerPage = 1000,
            string whereClause = null, string orderByClause = null, object param = null)
           where TEntity : class
        {
            if (!string.IsNullOrEmpty(whereClause))
            {
                if (!whereClause.TrimStart().StartsWith("WHERE", true, null))
                {
                    whereClause = "WHERE " + whereClause.Trim();
                }
            }
            if (!string.IsNullOrEmpty(orderByClause))
            {
                if (!orderByClause.TrimStart().StartsWith("ORDER BY", true, null))
                {
                    orderByClause = "ORDER BY " + orderByClause.Trim();
                }
            }

            var query = queryFactory.GetPagedListQuery<TEntity>(skipCount, rowsPerPage, whereClause, orderByClause);
            logger.LogDebug("GetPagedListAsync query:{query} whereClause:{whereClause} param:{@param}", query, whereClause, param);

            return db.QueryAsync<TEntity>(query, param);
        }

        public Task<IEnumerable<TEntity>> GetItemsAsync<TEntity>(
            Expression<Func<TEntity, bool>> propertiesCondition)
           where TEntity : class
        {
            var q = new QueryBuilder<TEntity>()
                .Where(propertiesCondition);

            var stream = q.Parse(queryFactory);
            var query = stream.GetSql();
            var param = stream.GetParameters();

            logger.LogDebug("GetPagedListAsync query:{query} param:{@param}", query, param);

            return db.QueryAsync<TEntity>(query, param);
        }

        public Task<int> UpdateAsync<TEntity>(string whereClause, object param, IEnumerable<string> fieldsToSet = null)
            where TEntity : class
        {
            var query = queryFactory.GetUpdateQuery<TEntity>(whereClause, fieldsToSet);
            logger.LogDebug("UpdateAsync query:{query} whereClause:{whereClause} param:{@param}", query, whereClause, param);

            return db.ExecuteAsync(query, param);
        }

        public Task<int> UpdateAsync<TEntity>(TEntity entity, IEnumerable<string> fieldsToSet = null)
           where TEntity : class
        {
            var query = queryFactory.GetUpdateQuery<TEntity>(null, fieldsToSet);
            logger.LogDebug("UpdateAsync query:{query} entity:{@entity}", query, entity);

            return db.ExecuteAsync(query, entity);
        }

        public Task<long> GetCountAsync<TEntity>(TEntity entity)
           where TEntity : class
        {
            var query = queryFactory.GetCountQuery<TEntity>();
            logger.LogDebug("GetCountAsync query:{query} entity:{@entity}", query, entity);

            return db.ExecuteScalarAsync<long>(query, entity);
        }

        public Task<long> GetCountAsync<TEntity>(string whereClause, object param)
            where TEntity : class
        {
            var query = queryFactory.GetCountQuery<TEntity>(whereClause);
            logger.LogDebug("GetCountAsync query:{query} whereClause:{whereClause} param:{@param}", query, whereClause, param);

            return db.ExecuteScalarAsync<long>(query, param);
        }      
    }
}
