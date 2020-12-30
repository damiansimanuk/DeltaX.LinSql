using Dapper;
using DeltaX.LinSql.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeltaX.RepositoryDemo1
{
    public class DapperQuery
    {
        IDbConnection db;

        public Task<IEnumerable<TEntity>> Single<TEntity>(
            IQueryBuilder<TEntity> q,
            Expression<Func<TEntity, object>> properties)
           where TEntity : class
        {
            q.Select(properties);
            (var query, var param) = q.GetSqlParameters();
            return db.QueryAsync<TEntity>(query, param);
        }

        public Task<int> ExecuteAsync<TEntity>(IQueryBuilder<TEntity> q)
           where TEntity : class
        {
            (var query, var param) = q.GetSqlParameters();
            return db.ExecuteAsync(query, param);
        }
    }

    public static class DapperExtension
    {
        public static Task<IEnumerable<TEntity>> QueryAsync<TEntity>(this IDbConnection db, IQueryBuilder<TEntity> q)
            where TEntity : class
        {
            (var query, var param) = q.GetSqlParameters();
            return db.QueryAsync<TEntity>(query, param);
        }

        public static Task<int> ExecuteAsync<TEntity>(this IDbConnection db, IQueryBuilder<TEntity> q)
           where TEntity : class
        {
            (var query, var param) = q.GetSqlParameters();
            return db.ExecuteAsync(query, param);
        }

        public static void test()
        {
            // IDbConnection db = null;
            // var q = new QueryBuilder<string,int>(); 
            // 
            // db.QueryAsync(q);
        }
    }
}
