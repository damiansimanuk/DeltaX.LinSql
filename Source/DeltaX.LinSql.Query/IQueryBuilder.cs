﻿using DeltaX.LinSql.Table;
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
        IQueryBuilder<T1> SelectAll();
        IQueryBuilder<T1> Set<P>(Expression<Func<T1, P>> property, P value);
        IQueryBuilder<T1> Set<P>(Expression<Func<T1, P>> property, Expression<Func<P>> value);
        IQueryBuilder<T1> Update(T1 table);
        IQueryBuilder<T1> Where(Expression<Func<T1, bool>> properties);
    }

    public interface IQueryBuilder<T1, T2> : IQueryBuilder<T1>
        where T1 : class
        where T2 : class
    {
        IQueryBuilder<T1, T2, T3> Join<T3>(Expression<Func<T1, T2, T3, bool>> joinOn) where T3: class;
        IQueryBuilder<T1, T2> Select(Expression<Func<T1, T2, object>> properties);
        IQueryBuilder<T1, T2> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, P>> value);
        IQueryBuilder<T1, T2> Where(Expression<Func<T1, T2, bool>> properties);
    }

    public interface IQueryBuilder<T1, T2, T3> : IQueryBuilder<T1, T2>
        where T1 : class
        where T2 : class
        where T3 : class
    {

        IQueryBuilder<T1, T2, T3, T4> Join<T4>(Expression<Func<T1, T2, T3, T4, bool>> joinOn) where T4 : class;
        IQueryBuilder<T1, T2, T3> Select(Expression<Func<T1, T2, T3, object>> properties);
        IQueryBuilder<T1, T2, T3> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, T3, P>> value);
        IQueryBuilder<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> properties);
    }

    public interface IQueryBuilder<T1, T2, T3, T4> : IQueryBuilder<T1, T2, T3>
        where T1 : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        IQueryBuilder<T1, T2, T3, T4> Select(Expression<Func<T1, T2, T3, T4, object>> properties);
        IQueryBuilder<T1, T2, T3, T4> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, T3, T4, P>> value);
        IQueryBuilder<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> properties);
    }
}
