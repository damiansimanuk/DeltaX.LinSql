namespace DeltaX.LinSql.Query
{
    using DeltaX.LinSql.Table;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class QueryBuilder : IQueryBuilder
    {
        public TableQueryBuilder Builder { get; protected set; }

        internal QueryBuilder(TableQueryBuilder builder = null)
        {
            Builder = builder ?? new TableQueryBuilder();
        }

        public void AddTable<T>()
        {
            Builder.AddTable<T>();
        }

        public Type GetTable<T>()
        {
            return Builder.GetTable(typeof(T));
        }

        public Type GetTable(Type type)
        {
            return Builder.GetTable(type);
        }

        public (string sql, IDictionary<string, object> parameters) GetSqlParameters(TableQueryFactory tableFactory = null)
        {
            var stream = Parse(tableFactory);
            return (stream.GetSql(), stream.GetParameters());
        }

        public QueryStream Parse(TableQueryFactory tableFactory = null)
        {
            tableFactory ??= TableQueryFactory.GetInstance();
            var allowedTables = Builder.GetTables();
            var stream = new QueryStream(null, allowedTables);

            // Table FROM
            var typeFirst = Builder.GetTables().First();
            var tableFirst = tableFactory.GetTable(typeFirst);

            // DELETE
            if (Builder.MakeDelete)
            {
                stream.AddSql($"DELETE {tableFirst.Identifier}");
            }
            // UPDATE FIRST TABLE
            else if (Builder.MakeUpdate && Builder.TableUpdate != null && Builder.TableUpdate.GetType() == typeFirst)
            {
                stream.AddSql($"UPDATE {tableFirst.Identifier}");

                var first = true;
                foreach (var col in tableFirst.GetUpdateColumns())
                {
                    var colName = tableFactory.DialectQuery.Encapsulation(col.DbColumnName, tableFirst.Identifier);
                    stream.AddSql(first
                        ? $"\n\tSET {colName} = "
                        : $"\n\t, {colName} = ");
                    stream.AddParameter(col.GetPropertyInfo().GetValue(Builder.TableUpdate));
                    first = false;
                }
            }
            // UPDATE WITH SET
            else if (Builder.MakeUpdate && Builder.TableUpdate == null)
            {
                var identifiers = Builder.ExpressionSet.Select(s => tableFactory.GetTable(s.Key).Identifier);
                stream.AddSql($"UPDATE {string.Join(", ", identifiers)}");

                var first = true;
                foreach (var value in Builder.ExpressionSet.SelectMany(s => s.Value))
                {
                    var member = QueryHelper.GetFirstMemberExpression(value.Item1, allowedTables) as MemberExpression;
                    if (member != null)
                    {
                        stream.AddSql(first ? "\n\tSET " : "\n\t, ");
                        stream.AddColumn(member.Expression.Type, member.Member.Name);
                        stream.AddOperator("=");
                        stream.AddParameter(value.Item2);
                        first = false;
                    }
                }
            }
            // SELECT
            else
            {
                bool selecAdded = false;
                foreach (var expression in Builder.ExpressionSelect)
                {
                    stream.AddSql(selecAdded ? "\n\t, " : "SELECT ");
                    selecAdded = true;

                    var s = new SelectParser(expression);
                    stream.AddSql(s.GetSql());
                }
                // SELECT ALL
                if (!selecAdded)
                {
                    stream.AddSql("SELECT ");
                    var tablesColumnsSelect = Builder.GetTables()
                        .Select(t => tableFactory.GetTable(t))
                        .Select(t => tableFactory.DialectQuery.GetSelectColumns(t, t.Identifier));

                    stream.AddSql(string.Join(", ", tablesColumnsSelect));
                }
            }

            // FROM  
            var tableName = tableFactory.DialectQuery.GetTableName(tableFirst, tableFirst.Identifier);
            stream.AddSql($" \nFROM {tableName}");

            // JOIN
            foreach (var table in Builder.ExpressionJoin)
            {
                var tableJoin = tableFactory.GetTable(table.Key);
                tableName = tableFactory.DialectQuery.GetTableName(tableJoin, tableJoin.Identifier);

                stream.AddSql($" \nJOIN {tableName} ON ");
                var qp = new QueryParser(stream);
                qp.Visit(table.Value);
            }

            // WHERE
            bool whereAdded = false;
            foreach (var expression in Builder.ExpressionWhere)
            {
                stream.AddOperator(whereAdded ? "\n\tAND" : "\nWHERE");
                whereAdded = true;

                var qp = new QueryParser(stream);
                qp.Visit(expression);
            }

            return stream;
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
            var ret = new QueryBuilder<T1, T2>(Builder);
            Builder.Join<T2>(joinOn);
            return ret;
        }

        public QueryBuilder<T1> Select(Expression<Func<T1, object>> properties)
        {
            Builder.Select(properties);
            return this;
        }

        public QueryBuilder<T1> Where(Expression<Func<T1, bool>> properties)
        {
            Builder.Where(properties);
            return this;
        }

        public QueryBuilder<T1> Delete()
        {
            Builder.Delete();
            return this;
        }

        public QueryBuilder<T1> Set<P1>(Expression<Func<T1, P1>> property, P1 value)
        {
            Builder.Set<T1>(property, value);
            return this;
        }

        public QueryBuilder<T1> Set<P1>(Expression<Func<T1, P1>> property, Expression<Func<P1>> value)
        {
            Builder.Set<T1>(property, value);
            return this;
        }

        public QueryBuilder<T1> Update(T1 table)
        { 
            Builder.Update<T1>(table);
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

        public QueryBuilder<T1, T2> Select(Expression<Func<T1, T2, object>> properties)
        {
            Builder.Select(properties);
            return this;
        }

        public QueryBuilder<T1, T2> Where(Expression<Func<T1, T2, bool>> properties)
        {
            Builder.Where(properties);
            return this;
        }

        // public IQueryBuilder<T1, T2> Select(
        //     Expression<Func<T1, object>> properties1,
        //     Expression<Func<T2, object>> properties2)
        // {
        //     Builder.Select<T1>(properties1);
        //     Builder.Select<T2>(properties2);
        //     return this;
        // }
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

        // public IQueryBuilder<T1, T2, T3> Select(
        //     Expression<Func<T1, object>> properties1,
        //     Expression<Func<T2, object>> properties2,
        //     Expression<Func<T3, object>> properties3)
        // {
        //     Builder.Select(properties1);
        //     Builder.Select(properties2);
        //     Builder.Select(properties3);
        //     return this;
        // }
    }

    public class QueryBuilder<T1, T2, T3, T4> : QueryBuilder<T1, T2, T3>, IQueryBuilder<T1, T2, T3, T4>
    {
        public QueryBuilder(TableQueryBuilder builder = null) : base(builder)
        {
            Builder.AddTable<T4>();
        }

        // public IQueryBuilder<T1, T2, T3, T4> Select(
        //   Expression<Func<T1, object>> properties1,
        //   Expression<Func<T2, object>> properties2,
        //   Expression<Func<T3, object>> properties3,
        //   Expression<Func<T4, object>> properties4)
        // {
        //     Builder.Select(properties1);
        //     Builder.Select(properties2);
        //     Builder.Select(properties3);
        //     Builder.Select(properties4);
        //     return this;
        // }
    }
}
