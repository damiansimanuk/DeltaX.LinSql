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

        public static Task<int> ExecuteAsync(this IDbConnection db, IQueryBuilder q)
        {
            (var sql, var param) = q.GetSqlParameters();
            return db.ExecuteAsync(sql, param);
        }

        public static Task<TResult> ExecuteScalarAsync<TResult>(this IDbConnection db, IQueryBuilder q)
        {
            (var sql, var param) = q.GetSqlParameters();
            return db.ExecuteScalarAsync<TResult>(sql, param);
        }

        public class Poco
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Updated { get; set; }
            public bool Active { get; set; }
        }

        public class Poco2
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Updated { get; set; }
            public bool Active { get; set; }
        }

        public static void test()
        {
            IDbConnection db = null;
            var q = new QueryBuilder<Poco>()
                .Where(t => !t.Active && t.Id == 1)
                .Set(t1 => t1.Name, "Alfredo");

            int affectedRows = db.ExecuteAsync(q).Result;

            var poco = new Poco { Id = 1, Active = true, Name = "Alfredo" };
            var q2 = new QueryBuilder<Poco>().Update(poco);

            int affectedRows2 = db.ExecuteAsync(q2).Result;

            (var sql, var param) = new QueryBuilder<Poco>()
                .Where(t => t.Active)
                .SelectAll()
                .GetSqlParameters();

            var result = db.QueryAsync<Poco>(sql, param).Result;

            (var sql2, var param2) = new QueryBuilder<Poco>()
                .Join<Poco2>((t1, t2) => t1.Id == t2.Id)
                .Where((t1, t2) => t1.Active && t2.Active && t2.Id == 123)
                .Select<Poco>(null)
                .GetSqlParameters();

            var result2 = db.QueryAsync<Poco>(sql2, param2).Result;
        }

        public static void ExampleSelect()
        {
            IDbConnection db = null;  

            (var sql, var param) = new QueryBuilder<Poco>()
                .Join<Poco2>((t1, t2) => t1.Id == t2.Id)
                .Where((t1, t2) => t1.Active && t2.Active && t2.Id == 123)
                .Select<Poco>(null)
                .GetSqlParameters();

            var resultList = db.QueryAsync<Poco>(sql, param).Result;
        }

        public static void ExampleUpdate()
        {
            IDbConnection db = null;

            (var sql, var param) = new QueryBuilder<Poco>()
                .Join<Poco2>((t1, t2) => t1.Id == t2.Id)
                .Where((t1, t2) => t1.Active && t2.Active && t2.Id == 123)
                .Set((t1) => t1.Name, "Nada")
                .GetSqlParameters();

            var result = db.ExecuteAsync(sql, param).Result;
        }
 
    }
}
