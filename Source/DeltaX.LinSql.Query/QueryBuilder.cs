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

        public QueryStream Parse(TableQueryFactory tableFactory = null, QueryStream stream = null)
        {
            tableFactory ??= TableQueryFactory.GetInstance(); 
            stream = stream ?? new QueryStream(null, Builder.GetTables()); 

            // DELETE
            if (Builder.TableDeleteType != null)
            {
                return ParseDelete(tableFactory, stream, Builder.TableDeleteType, Builder.TableDeleteEntity);
            }
            // UPDATE TABLE
            else if (Builder.TableUpdate != null)
            {
                return ParseUpdate(tableFactory, stream, Builder.TableUpdate.GetType(), Builder.TableUpdate);
            }
            // UPDATE WITH SET
            else if (Builder.ExpressionSet.Any())
            {
                return ParseUpdateSet(tableFactory, stream);
            }
            // SELECT
            else if (Builder.TableSelectType!=null)
            { 
                return ParseSelect(tableFactory, stream, Builder.TableSelectType, Builder.TableSelectEntity);
            }
            // SELECT
            else
            {
                return ParseSelect(tableFactory, stream);
            }
        }

        private QueryStream ParseDelete(TableQueryFactory tableFactory, QueryStream stream, Type tableType, object entity)
        {
            var tableDelete = tableFactory.GetTable(tableType);

            stream.AddSql($"DELETE {tableDelete.Identifier}");

            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE
            int whereAdded = ParseWhere(stream);
            if (whereAdded == 0 && entity != null)
            {
                stream.AddSql("\nWHERE ");
                var pks = tableDelete.GetPrimaryKeysColumn();
                foreach (var pk in pks)
                {
                    stream.AddTableField(tableType, pk.DtoFieldName);
                    stream.AddOperator("=");
                    stream.AddParameter(pk.GetPropertyInfo().GetValue(entity), $"{tableDelete.Identifier}_{pk.DtoFieldName}");
                }
            }

            return stream;
        }
        
        private QueryStream ParseUpdate(TableQueryFactory tableFactory, QueryStream stream, Type tableType, object entity)
        { 
            var tableUpdate = tableFactory.GetTable(tableType);
            stream.AddSql($"UPDATE {tableUpdate.Identifier}");

            var first = true;
            foreach (var col in tableUpdate.GetUpdateColumns())
            {
                var colName = tableFactory.DialectQuery.Encapsulation(col.DbColumnName, tableUpdate.Identifier);
                stream.AddSql(first ? $"\n\tSET {colName} = " : $"\n\t, {colName} = ");
                stream.AddParameter(col.GetPropertyInfo().GetValue(entity), $"{tableUpdate.Identifier}_{col.DtoFieldName}");
                first = false;
            }

            // FROM
            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE 
            int whereAdded = ParseWhere(stream);
            if (whereAdded == 0)
            {
                stream.AddSql("\nWHERE ");
                var pks = tableUpdate.GetPrimaryKeysColumn();
                foreach (var pk in pks)
                {
                    stream.AddTableField(tableType, pk.DtoFieldName);
                    stream.AddOperator("=");
                    stream.AddParameter(pk.GetPropertyInfo().GetValue(entity), $"{tableUpdate.Identifier}_{pk.DtoFieldName}");
                }
            }

            return stream;
        }
        
        private QueryStream ParseUpdateSet(TableQueryFactory tableFactory, QueryStream stream)
        {
            var allowedTables = Builder.GetTables();

            var identifiers = Builder.ExpressionSet.Select(s => tableFactory.GetTable(s.Key).Identifier);
            stream.AddSql($"UPDATE {string.Join(", ", identifiers)}");

            var first = true;
            foreach (var value in Builder.ExpressionSet.SelectMany(s => s.Value))
            {

                var member = QueryHelper.GetFirstMemberExpression(value.property, allowedTables) as MemberExpression;
                if (member != null)
                {
                    stream.AddSql(first ? "\n\tSET " : "\n\t, ");
                    var fieldName = member.Member.Name;
                    stream.AddTableField(member.Expression.Type, fieldName);
                    stream.AddOperator("=");

                    if (value.value is Expression valExpr)
                    {
                        var valueMember = QueryHelper.GetFirstMemberExpression(valExpr, allowedTables) as MemberExpression;
                        if (valueMember != null && valExpr is LambdaExpression lambdaExpression && lambdaExpression.Body.NodeType == ExpressionType.MemberAccess)
                        {
                            stream.AddTableField(valueMember.Expression.Type, valueMember.Member.Name);
                        }
                        else if (valueMember != null && valueMember.NodeType != ExpressionType.Parameter)
                        {
                            var qp = new QueryParser(stream);
                            qp.Visit(valExpr);
                        }
                        else
                        {
                            stream.AddParameter(value.value, fieldName);
                        }
                    }
                    else
                    {
                        stream.AddParameter(value.value, fieldName);
                    }

                    first = false;
                }
            }

            // FROM
            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE 
            ParseWhere(stream); 

            return stream;
        }

        private QueryStream ParseSelect(TableQueryFactory tableFactory, QueryStream stream)
        {
            bool selecAdded = false;
            foreach (var expression in Builder.ExpressionSelect)
            {
                stream.AddSql(selecAdded ? "\n\t, " : "SELECT ");
                selecAdded = true;

                var s = new SelectParser(expression);
                stream.AddSql(s.GetSql());
            }
            // // SELECT ALL
            // if (!selecAdded)
            // {
            //     stream.AddSql("SELECT ");
            //     var tablesColumnsSelect = Builder.GetTables()
            //         .Select(t => tableFactory.GetTable(t))
            //         .Select(t => tableFactory.DialectQuery.GetSelectColumns(t, t.Identifier));
            // 
            //     stream.AddSql(string.Join("\n\t, ", tablesColumnsSelect));
            // }

            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE
            ParseWhere(stream);

            return stream;
        }

        private QueryStream ParseSelect(TableQueryFactory tableFactory, QueryStream stream, Type entityType, object entity)
        {  
            var tableSelect = tableFactory.GetTable(entityType);

            // SELECT
            stream.AddSql("SELECT ");
            var tablesColumnsSelect = tableFactory.DialectQuery.GetSelectColumns(tableSelect, tableSelect.Identifier);
            stream.AddSql($"\n\t{tablesColumnsSelect}");

            // FROM
            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE
            int whereAdded = ParseWhere(stream);
            if (whereAdded == 0 && entity != null)
            {
                stream.AddSql("\nWHERE ");
                var pks = tableSelect.GetPrimaryKeysColumn();
                foreach (var pk in pks)
                {
                    stream.AddTableField(entityType, pk.DtoFieldName);
                    stream.AddOperator("=");
                    stream.AddParameter(pk.GetPropertyInfo().GetValue(entity), $"{tableSelect.Identifier}_{pk.DtoFieldName}");
                    whereAdded++;
                }
            }

            return stream;
        }

        private int ParseFrom(TableQueryFactory tableFactory, QueryStream stream)
        {
            var tableFrom = tableFactory.GetTable(Builder.GetTables().First());
            var tableName = tableFactory.DialectQuery.GetTableName(tableFrom, tableFrom.Identifier);
            stream.AddSql($" \nFROM {tableName}");
            return 1;
        }

        private int ParseJoin(TableQueryFactory tableFactory, QueryStream stream)
        {
            foreach (var table in Builder.ExpressionJoin)
            {
                var tableJoin = tableFactory.GetTable(table.Key);
                var tableName = tableFactory.DialectQuery.GetTableName(tableJoin, tableJoin.Identifier);

                stream.AddSql($" \nJOIN {tableName} ON ");
                var qp = new QueryParser(stream);
                qp.Visit(table.Value);
            }
            return Builder.ExpressionJoin.Count();
        }

        private int ParseWhere(QueryStream stream)
        {
            bool whereAdded = false;
            foreach (var expression in Builder.ExpressionWhere)
            {
                stream.AddSql(whereAdded ? "\n\tAND " : "\nWHERE ");
                whereAdded = true;

                var qp = new QueryParser(stream);
                qp.Visit(expression);
            }

            return Builder.ExpressionWhere.Count();
        }


    }

    public class QueryBuilder<T1> : QueryBuilder, IQueryBuilder<T1>
        where T1 : class
    {
        public QueryBuilder(TableQueryBuilder builder = null) : base(builder)
        {
            AddTable<T1>();
        }

        public IQueryBuilder<T1, T2> Join<T2>(Expression<Func<T1, T2, bool>> joinOn) where T2 : class
        {
            var ret = new QueryBuilder<T1, T2>(Builder);
            Builder.Join<T2>(joinOn);
            return ret;
        }

        public IQueryBuilder<T1> Select(Expression<Func<T1, object>> properties)
        {
            Builder.Select(properties);
            return this;
        }

        public IQueryBuilder<T1> Select(T1 entity)
        {
            Builder.SelectEntity<T1>(entity);
            return this;
        } 

        public IQueryBuilder<T1> SelectAll()
        {
            Builder.SelectEntity<T1>(null);
            return this;
        }

        public IQueryBuilder<T1> Where(Expression<Func<T1, bool>> properties)
        {
            Builder.Where(properties);
            return this;
        }

        public IQueryBuilder<T1> Delete(T1 entity = null)
        {
            Builder.Delete<T1>(entity);
            return this;
        }

        public IQueryBuilder<T1> Set<P>(Expression<Func<T1, P>> property, P value)
        {
            Builder.Set<T1>(property, value);
            return this;
        }

        public IQueryBuilder<T1> Set<P>(Expression<Func<T1, P>> property, Expression<Func<P>> value)
        {
            Builder.Set<T1>(property, value);
            return this;
        }

        public IQueryBuilder<T1> Update(T1 table)
        {
            Builder.Update<T1>(table);
            return this;
        }
    }

    public class QueryBuilder<T1, T2> : QueryBuilder<T1>, IQueryBuilder<T1, T2>
        where T1 : class
        where T2 : class
    {
        public QueryBuilder(TableQueryBuilder builder = null) : base(builder)
        {
            Builder.AddTable<T2>();
        }

        public IQueryBuilder<T1, T2, T3> Join<T3>(Expression<Func<T1, T2, T3, bool>> joinOn)
            where T3 : class
        {
            var ret = new QueryBuilder<T1, T2, T3>(Builder);
            Builder.Join<T3>(joinOn);
            return ret;
        }

        public IQueryBuilder<T1, T2> Select(Expression<Func<T1, T2, object>> properties)
        {
            Builder.Select(properties);
            return this;
        }

        public IQueryBuilder<T1, T2> Where(Expression<Func<T1, T2, bool>> properties)
        {
            Builder.Where(properties);
            return this;
        }

        public IQueryBuilder<T1, T2> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, P>> value)
        {
            Builder.Set<T1>(property, value);
            return this;
        }
    }

    public class QueryBuilder<T1, T2, T3> : QueryBuilder<T1, T2>, IQueryBuilder<T1, T2, T3>
        where T1 : class
        where T2 : class
        where T3 : class
    {
        public QueryBuilder(TableQueryBuilder builder = null) : base(builder)
        {
            Builder.AddTable<T3>();
        }

        public IQueryBuilder<T1, T2, T3, T4> Join<T4>(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
            where T4 : class
        {
            var ret = new QueryBuilder<T1, T2, T3, T4>(Builder);
            Builder.Join<T4>(joinOn);
            return ret;
        }

        public IQueryBuilder<T1, T2, T3> Select(Expression<Func<T1, T2, T3, object>> properties)
        {
            Builder.Select(properties);
            return this;
        }

        public IQueryBuilder<T1, T2, T3> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, T3, P>> value)
        {
            Builder.Set<T1>(property, value);
            return this;
        }

        public IQueryBuilder<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> properties)
        {
            Builder.Where(properties);
            return this;
        }
    }

    public class QueryBuilder<T1, T2, T3, T4> : QueryBuilder<T1, T2, T3>, IQueryBuilder<T1, T2, T3, T4>
        where T1 : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        public QueryBuilder(TableQueryBuilder builder = null) : base(builder)
        {
            Builder.AddTable<T4>();
        }

        public IQueryBuilder<T1, T2, T3, T4> Select(Expression<Func<T1, T2, T3, T4, object>> properties)
        {
            Builder.Select(properties);
            return this;
        }

        public IQueryBuilder<T1, T2, T3, T4> Set<P>(Expression<Func<T1, P>> property, Expression<Func<T2, T3, T4, P>> value)
        {
            Builder.Set<T1>(property, value);
            return this;
        }

        public IQueryBuilder<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> properties)
        {
            Builder.Where(properties);
            return this;
        }
    }
}

