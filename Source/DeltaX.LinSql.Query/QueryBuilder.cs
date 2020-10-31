namespace DeltaX.LinSql.Query
{
    using System;
    using System.Linq.Expressions;

    public class QueryBuilder : IQueryBuilder
    {
        public TableQueryBuilder Builder { get; protected set; }

        public QueryBuilder(TableQueryBuilder builder = null)
        {
            Builder = builder ?? new TableQueryBuilder();
        }

        public void AddTable<T>()
        {
            Builder.AddTable<T>();
        }

        public TableConfig GetTableConfig<T>()
        {
            return Builder.GetTableConfig<T>();
        } 
    }

    public class QueryBuilder<T1> : QueryBuilder, IQueryBuilder<T1>
    {
        public QueryBuilder(TableQueryBuilder builder = null) : base(builder)
        {
            AddTable<T1>();
        }

        public QueryBuilder<T1, T2> Join<T2>(Expression<Func<T1, T2, bool>> joinOn)
        {
            return new QueryBuilder<T1, T2>(Builder);
        }

        public IQueryBuilder<T1> Select(Expression<Func<T1, object>> properties)
        {
            Builder.Select(properties);
            return this;
        }
    }

    public class QueryBuilder<T1, T2> : QueryBuilder<T1>, IQueryBuilder<T1, T2>
    {
        public QueryBuilder(TableQueryBuilder builder = null) : base(builder)
        {
            Builder.AddTable<T2>();
        }

        public QueryBuilder<T1, T2, T3> Join<T3>(Expression<Func<T1, T2, T3, bool>> joinOn)
        {
            return new QueryBuilder<T1, T2, T3>(Builder);
        }

        public IQueryBuilder<T1, T2> Select(
            Expression<Func<T1, object>> properties1,
            Expression<Func<T2, object>> properties2)
        {
            Builder.Select<T1>(properties1);
            Builder.Select<T2>(properties2);
            return this;
        }
    }

    public class QueryBuilder<T1, T2, T3> : QueryBuilder<T1, T2>, IQueryBuilder<T1, T2, T3>
    {
        public QueryBuilder(TableQueryBuilder builder = null) : base(builder)
        {
            Builder.AddTable<T3>();
        }

        public QueryBuilder<T1, T2, T3, T4> Join<T4>(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
        {
            return new QueryBuilder<T1, T2, T3, T4>(Builder);
        }

        public IQueryBuilder<T1, T2, T3> Select(
            Expression<Func<T1, object>> properties1,
            Expression<Func<T2, object>> properties2,
            Expression<Func<T3, object>> properties3)
        {
            Builder.Select(properties1);
            Builder.Select(properties2);
            Builder.Select(properties3);
            return this;
        }
    }

    public class QueryBuilder<T1, T2, T3, T4> : QueryBuilder<T1, T2, T3>, IQueryBuilder<T1, T2, T3, T4>
    {
        public QueryBuilder(TableQueryBuilder builder = null) : base(builder)
        {
            Builder.AddTable<T4>();
        }

        public IQueryBuilder<T1, T2, T3, T4> Select(
          Expression<Func<T1, object>> properties1,
          Expression<Func<T2, object>> properties2,
          Expression<Func<T3, object>> properties3,
          Expression<Func<T4, object>> properties4)
        {
            Builder.Select(properties1);
            Builder.Select(properties2);
            Builder.Select(properties3);
            Builder.Select(properties4);
            return this;
        }
    }
}
