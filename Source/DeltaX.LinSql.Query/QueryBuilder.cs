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
            else if (Builder.TableSelect.Any())
            {
                return ParseSelect(tableFactory, stream, Builder.TableSelect);
            }
            // SELECT
            else
            {
                return ParseSelect(tableFactory, stream);
            }
        }

        private QueryStream ParseDelete(TableQueryFactory tableFactory, QueryStream stream, Type entityType, object entity)
        {
            var table = tableFactory.GetTable(entityType);
            stream.AddSql($"DELETE {table.Identifier}");

            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE
            ParseWhere(tableFactory, stream, entityType, entity);

            return stream;
        }

        private QueryStream ParseInsert(TableQueryFactory tableFactory, QueryStream stream, Type entityType, object entity)
        {
            var table = tableFactory.GetTable(entityType);

            var columns = tableFactory.DialectQuery.GetInsertColumns(table);
            stream.AddSql($"DELETE {table.Identifier}");

            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE
            ParseWhere(tableFactory, stream, entityType, entity);

            return stream;
        }

        private QueryStream ParseUpdate(TableQueryFactory tableFactory, QueryStream stream, Type entityType, object entity)
        {
            var tableUpdate = tableFactory.GetTable(entityType);
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
            ParseWhere(tableFactory, stream, entityType, entity);

            return stream;
        }

        private QueryStream ParseUpdateSet(TableQueryFactory tableFactory, QueryStream stream)
        {
            var allowedTables = Builder.GetTables();

            var identifiers = Builder.ExpressionSet.Select(s => tableFactory.GetTable(s.Key).Identifier);
            stream.AddSql($"UPDATE {string.Join(", ", identifiers)}");

            var first = true;
            foreach (var pair in Builder.ExpressionSet)
            {
                var table = tableFactory.GetTable(pair.Key);
                foreach (var value in pair.Value)
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
                                var qp = new ExpressionQueryParser(stream);
                                qp.Visit(valExpr);
                            }
                            else
                            {
                                stream.AddParameter(value.value, $"{table.Identifier}_{fieldName}");
                            }
                        }
                        else
                        {
                            stream.AddParameter(value.value, $"{table.Identifier}_{fieldName}");
                        }

                        first = false;
                    }
                }
            }

            // FROM
            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE 
            ParseWhere(tableFactory, stream);

            return stream;
        }

        private QueryStream ParseSelect(TableQueryFactory tableFactory, QueryStream stream)
        {
            bool selecAdded = false;
            foreach (var expression in Builder.ExpressionSelect)
            {
                stream.AddSql(selecAdded ? "\n\t, " : "SELECT ");
                selecAdded = true;

                var s = new ExpressionSelectParser(expression);
                stream.AddSql(s.GetSql());
            }

            // Add Extra Alias
            if (Builder.ExpressionAlias.Any())
            {
                this.AddSelectAlias(stream);
            }

            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE
            ParseWhere(tableFactory, stream);
            // ORDER BY
            ParseOrderBy(tableFactory, stream);
            // LIMIT
            ParseLimit(tableFactory, stream);

            return stream;
        }

        private QueryStream ParseSelect(TableQueryFactory tableFactory, QueryStream stream, Dictionary<Type, object> entities)
        {
            // SELECT
            stream.AddSql("SELECT ");
            var tablesColumnsSelect = entities
                .Select(t => new { table = tableFactory.GetTable(t.Key), entity = t.Value })
                .Select(t => tableFactory.DialectQuery.GetSelectColumns(t.table, t.table.Identifier));
            stream.AddSql($"\n\t{string.Join("\n\t, ", tablesColumnsSelect)}");
            
            // Add Extra Alias
            if (Builder.ExpressionAlias.Any())
            {
                this.AddSelectAlias(stream);
            }

            // FROM
            ParseFrom(tableFactory, stream);
            // JOIN
            ParseJoin(tableFactory, stream);
            // WHERE
            ParseWhere(tableFactory, stream, entities.Keys.First(), entities.Values.First());
            // ORDER BY
            ParseOrderBy(tableFactory, stream);
            // LIMIT
            ParseLimit(tableFactory, stream);

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
                var qp = new ExpressionQueryParser(stream);
                qp.Visit(table.Value);
            }
            return Builder.ExpressionJoin.Count();
        }

        private int ParseWhere(TableQueryFactory tableFactory, QueryStream stream, Type entityType = null, object entity = null)
        {
            int whereAdded = 0;
            foreach (var expression in Builder.ExpressionWhere)
            {
                stream.AddSql(whereAdded > 0 ? "\n\tAND " : "\nWHERE ");

                var qp = new ExpressionQueryParser(stream);
                qp.Visit(expression);
                whereAdded++;
            }

            if (whereAdded == 0 && entityType != null && entity != null)
            {
                var table = tableFactory.GetTable(entityType);
                stream.AddSql("\nWHERE ");
                var pks = table.GetPrimaryKeysColumn();
                foreach (var pk in pks)
                {
                    stream.AddTableField(entityType, pk.DtoFieldName);
                    stream.AddOperator("=");
                    stream.AddParameter(pk.GetPropertyInfo().GetValue(entity), $"{table.Identifier}_{pk.DtoFieldName}");
                    whereAdded++;
                }
            }

            return whereAdded;
        }

        public void AddSelectAlias(QueryStream stream)
        {
            var allowedTables = Builder.GetTables();
            foreach (var pair in Builder.ExpressionAlias)
            {
                var member = QueryHelper.GetFirstMemberExpression(pair.property, allowedTables) as MemberExpression;
                stream.AddColumnSelector(member.Expression.Type, member.Member.Name, pair.alias);
            }
        }

        private int ParseOrderBy(TableQueryFactory tableFactory, QueryStream stream)
        {
            var allowedTables = Builder.GetTables();
            var idx = 0;
            foreach (var pair in Builder.ExpressionOrder)
            {
                var member = QueryHelper.GetFirstMemberExpression(pair.property, allowedTables) as MemberExpression;
                var fieldName = member.Member.Name;
                stream.AddOperator(idx == 0 ? "ORDER BY" : ",");
                stream.AddTableField(member.Expression.Type, fieldName);
                stream.AddOperator(pair.ascendant ? "ASC" : "DESC");
                idx++;
            }
            return idx;
        }

        private int ParseLimit(TableQueryFactory tableFactory, QueryStream stream)
        {
            if (Builder.ExpressionLimit == null)
            {
                return 0;
            }
            var sql = tableFactory.DialectQuery.LimitFormatSql
                .Replace("{SkipCount}", Builder.ExpressionLimit.Value.skipCount.ToString())
                .Replace("{RowsPerPage}", Builder.ExpressionLimit.Value.rowsPerPage.ToString());

            stream.AddSql(sql);
            return 1;
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

        public IQueryBuilder<T1> Select<TEntity>(TEntity entity = null) 
            where TEntity : class
        {
            Builder.SelectEntity(typeof(TEntity), entity);
            return this;
        }

        public IQueryBuilder<T1> SelectAll()
        {
            Builder.SelectEntity(typeof(T1), null);
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

        public IQueryBuilder<T1> OrderBy<P>(Expression<Func<T1, P>> property, bool ascendant = true)
        {
            Builder.OrderBy(property, ascendant);
            return this;
        }

        public IQueryBuilder<T1> Limit(int skipCount, int rowsPerPage)
        {
            Builder.Limit(skipCount, rowsPerPage);
            return this;
        }

        public IQueryBuilder<T1> As<P>(Expression<Func<T1, P>> property, string columnAlias)
        {
            Builder.As(property, columnAlias);
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


        public new IQueryBuilder<T1, T2> SelectAll()
        {
            Builder.SelectEntity(typeof(T1), null);
            Builder.SelectEntity(typeof(T2), null);
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

        public IQueryBuilder<T1, T2> OrderBy<P>(Expression<Func<T1, T2, P>> property, bool ascendant = true)
        {
            Builder.OrderBy(property, ascendant);
            return this;
        }

        public IQueryBuilder<T1, T2> Limit(int skipCount, int rowsPerPage)
        {
            Builder.Limit(skipCount, rowsPerPage);
            return this;
        }

        public IQueryBuilder<T1, T2> As<P>(Expression<Func<T1, T2, P>> property, string columnAlias)
        {
            Builder.As(property, columnAlias);
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

        public IQueryBuilder<T1, T2, T3> OrderBy<P>(Expression<Func<T1, T2, T3, P>> property, bool ascendant = true)
        {
            Builder.OrderBy(property, ascendant);
            return this;
        }

        public IQueryBuilder<T1, T2, T3> Limit(int skipCount, int rowsPerPage)
        {
            Builder.Limit(skipCount, rowsPerPage);
            return this;
        }

        public IQueryBuilder<T1, T2, T3> As<P>(Expression<Func<T1, T2, T3, P>> property, string columnAlias)
        {
            Builder.As(property, columnAlias);
            return this;
        }

        public IQueryBuilder<T1, T2, T3> Select(Expression<Func<T1, T2, T3, object>> properties)
        {
            Builder.Select(properties);
            return this;
        } 

        public new IQueryBuilder<T1, T2, T3> SelectAll()
        {
            Builder.SelectEntity(typeof(T1), null);
            Builder.SelectEntity(typeof(T2), null);
            Builder.SelectEntity(typeof(T3), null);
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

        public new IQueryBuilder<T1, T2, T3, T4> SelectAll()
        {
            Builder.SelectEntity(typeof(T1), null);
            Builder.SelectEntity(typeof(T2), null);
            Builder.SelectEntity(typeof(T3), null);
            Builder.SelectEntity(typeof(T4), null);
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

