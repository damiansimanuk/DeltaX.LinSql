using DeltaX.LinSql.Table;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DeltaX.LinSql.Query
{
    public interface IQueryBuilder
    {
        TableQueryBuilder Builder { get; }
        void AddTable<T>();
        (string sql, IDictionary<string, object> parameters) GetSqlParameters(TableQueryFactory tableFactory = null);
        Type GetTable(Type type);
        Type GetTable<T>();
        QueryStream Parse(TableQueryFactory tableFactory = null, QueryStream stream = null);
    }

    public interface IQueryBuilder<T1> : IQueryBuilder
        where T1 : class
    {
        IQueryBuilder<T1> Delete(T1 entity = null);
        IQueryBuilder<T1, T2> Join<T2>(Expression<Func<T1, T2, bool>> joinOn) where T2 : class;
        IQueryBuilder<T1> Select(Expression<Func<T1, object>> properties);
        IQueryBuilder<T1> Select<TEntity>(TEntity entity = null) where TEntity : class;
        IQueryBuilder<T1> SelectAll();
        IQueryBuilder<T1> Set<P>(Expression<Func<T1, P>> property, P value);
        IQueryBuilder<T1> Set<P>(Expression<Func<T1, P>> property, Expression<Func<P>> value);
        IQueryBuilder<T1> Update(T1 table);
        IQueryBuilder<T1> Where(Expression<Func<T1, bool>> properties);
        IQueryBuilder<T1> OrderBy<P>(Expression<Func<T1, P>> property, bool ascendant = true);
        IQueryBuilder<T1> Limit(int skipCount, int rowsPerPage);
        IQueryBuilder<T1> As<P>(Expression<Func<T1, P>> property, string columnAlias);
    }

    public interface IQueryBuilder<T1, T2> : IQueryBuilder<T1>
        where T1 : class
        where T2 : class
    {
        IQueryBuilder<T1, T2, T3> Join<T3>(Expression<Func<T1, T2, T3, bool>> joinOn) where T3 : class;
        IQueryBuilder<T1, T2> Select(Expression<Func<T1, T2, object>> properties);
        new IQueryBuilder<T1, T2> SelectAll();
        IQueryBuilder<T1, T2> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, P>> value);
        IQueryBuilder<T1, T2> Where(Expression<Func<T1, T2, bool>> properties);
        IQueryBuilder<T1, T2> OrderBy<P>(Expression<Func<T1, T2, P>> property, bool ascendant = true);
        IQueryBuilder<T1, T2> Limit(int skipCount, int rowsPerPage);
        IQueryBuilder<T1, T2> As<P>(Expression<Func<T1, T2, P>> property, string columnAlias);
    }

    public interface IQueryBuilder<T1, T2, T3> : IQueryBuilder<T1, T2>
        where T1 : class
        where T2 : class
        where T3 : class
    {

        IQueryBuilder<T1, T2, T3, T4> Join<T4>(Expression<Func<T1, T2, T3, T4, bool>> joinOn) where T4 : class;
        IQueryBuilder<T1, T2, T3> Select(Expression<Func<T1, T2, T3, object>> properties);
        new IQueryBuilder<T1, T2, T3> SelectAll();
        IQueryBuilder<T1, T2, T3> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, T3, P>> value);
        IQueryBuilder<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> properties);
        IQueryBuilder<T1, T2, T3> OrderBy<P>(Expression<Func<T1, T2, T3, P>> property, bool ascendant = true);
        IQueryBuilder<T1, T2, T3> Limit(int skipCount, int rowsPerPage);
        IQueryBuilder<T1, T2, T3> As<P>(Expression<Func<T1, T2, T3, P>> property, string columnAlias);
    }

    public interface IQueryBuilder<T1, T2, T3, T4> : IQueryBuilder<T1, T2, T3>
        where T1 : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        IQueryBuilder<T1, T2, T3, T4, T5> Join<T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn) where T5 : class;
        IQueryBuilder<T1, T2, T3, T4> Select(Expression<Func<T1, T2, T3, T4, object>> properties);
        new IQueryBuilder<T1, T2, T3, T4> SelectAll();
        IQueryBuilder<T1, T2, T3, T4> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, T3, T4, P>> value);
        IQueryBuilder<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> properties);
        IQueryBuilder<T1, T2, T3, T4> OrderBy<P>(Expression<Func<T1, T2, T3, T4, P>> property, bool ascendant = true);
        IQueryBuilder<T1, T2, T3, T4> Limit(int skipCount, int rowsPerPage);
        IQueryBuilder<T1, T2, T3, T4> As<P>(Expression<Func<T1, T2, T3, T4, P>> property, string columnAlias);        
    }

    public interface IQueryBuilder<T1, T2, T3, T4, T5> : IQueryBuilder<T1, T2, T3, T4>
       where T1 : class
       where T2 : class
       where T3 : class
       where T4 : class
       where T5 : class
    {
        IQueryBuilder<T1, T2, T3, T4, T5, T6> Join<T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn) where T6 : class;
        IQueryBuilder<T1, T2, T3, T4, T5> Select(Expression<Func<T1, T2, T3, T4, T5, object>> properties);
        new IQueryBuilder<T1, T2, T3, T4, T5> SelectAll();
        IQueryBuilder<T1, T2, T3, T4, T5> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, T3, T4, T5, P>> value);
        IQueryBuilder<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> properties);
        IQueryBuilder<T1, T2, T3, T4, T5> OrderBy<P>(Expression<Func<T1, T2, T3, T4, T5, P>> property, bool ascendant = true);
        IQueryBuilder<T1, T2, T3, T4, T5> Limit(int skipCount, int rowsPerPage);
        IQueryBuilder<T1, T2, T3, T4, T5> As<P>(Expression<Func<T1, T2, T3, T4, T5, P>> property, string columnAlias);
    }

    public interface IQueryBuilder<T1, T2, T3, T4, T5, T6> : IQueryBuilder<T1, T2, T3, T4, T5>
       where T1 : class
       where T2 : class
       where T3 : class
       where T4 : class
       where T5 : class
       where T6 : class
    {
        IQueryBuilder<T1, T2, T3, T4, T5, T6> Select(Expression<Func<T1, T2, T3, T4, T5, T6, object>> properties);
        new IQueryBuilder<T1, T2, T3, T4, T5, T6> SelectAll();
        IQueryBuilder<T1, T2, T3, T4, T5, T6> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, T3, T4, T5, T6, P>> value);
        IQueryBuilder<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> properties);
        IQueryBuilder<T1, T2, T3, T4, T5, T6> OrderBy<P>(Expression<Func<T1, T2, T3, T4, T5, T6, P>> property, bool ascendant = true);
        IQueryBuilder<T1, T2, T3, T4, T5, T6> Limit(int skipCount, int rowsPerPage);
        IQueryBuilder<T1, T2, T3, T4, T5, T6> As<P>(Expression<Func<T1, T2, T3, T4, T5, T6, P>> property, string columnAlias);
    }
}
