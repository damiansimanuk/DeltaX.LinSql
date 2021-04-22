namespace DeltaX.LinSql.Query
{
    using DeltaX.LinSql.Table;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    public class QueryStream
    {
        private StringBuilder sql = new StringBuilder();
        private Dictionary<string, object> Parameters = new Dictionary<string, object>();
        //  private Dictionary<ITableConfiguration, HashSet<ColumnConfiguration>> tableColumns = new Dictionary<ITableConfiguration, HashSet<ColumnConfiguration>>();
        private TableQueryFactory tableFactory;
        private int paramGeneratorOffset = 0;

        public QueryStream(TableQueryFactory tableFactory = null, IEnumerable<Type> allowedTables = null, int? paramGeneratorOffset = null)
        {
            this.paramGeneratorOffset = paramGeneratorOffset ?? 0;
            this.tableFactory = tableFactory ?? TableQueryFactory.GetInstance();
            this.AllowedTables = allowedTables ?? this.tableFactory.GetConfiguredTables().Select(t => t.Key).ToArray();
        }

        private string GetNewParameterId() => $"arg_{Parameters.Count + paramGeneratorOffset}";

        public IEnumerable<Type> AllowedTables { get; private set; }

        public IDictionary<string, object> GetParameters()
        {
            return Parameters.ToDictionary(e => e.Key,
                e =>
                {
                    if (e.Value is Expression node)
                    {
                        return QueryHelper.GetValueFromExpression(node);
                    }
                    else
                    {
                        return e.Value;
                    }
                });
        }

        // public IDictionary<ITableConfiguration, HashSet<ColumnConfiguration>> GetTableColumns()
        // {
        //     return tableColumns;
        // }

        public string GetSql()
        {
            return sql.ToString();
        }

        public bool IsAllowed(Type tableType)
        {
            return AllowedTables.Contains(tableType) && tableFactory.IsConfiguredTable(tableType);
        }

        public void AddOperator(string op)
        {
            sql.Append($" {op} ");
        }

        public void AddSql(string sqlExpression)
        {
            sql.Append(sqlExpression);
        }

        public void OpenBrace()
        {
            sql.Append("(");
        }

        public void CloseBrace()
        {
            sql.Append(")");
        }

        public void AddLikePrefix()
        {
            sql.Append("'%' + ");
        }

        internal void AddLikeSuffix()
        {
            sql.Append("+ '%'");
        }

        public void AddLike(bool not = false)
        {
            AddOperator(not ? "NOT LIKE" : "LIKE");
        }

        public void AddIn(bool not = false, IList<string> elements = null)
        {
            AddOperator(not ? "NOT IN" : "IN");

            if (elements != null)
            {
                sql.Append("(" + string.Join(", ", elements) + ")");
            }
        }

        public void AddBoolean(bool not)
        {
            sql.Append(not ? " = 0" : " <> 0");
        }

        public void AddIsNull(bool not)
        {
            sql.Append(not ? " IS NOT NULL" : " IS NULL");
        }

        public void AddIsNullOrEmpty(Type tableType, string columnName, bool not)
        {
            var table = tableFactory.GetTable(tableType); 
            var column = table.Columns.FirstOrDefault(c => c.DtoFieldName == columnName);
            if (column != null)
            { 
                var property = tableFactory.DialectQuery.Encapsulation(column.DbColumnName, table.Identifier);
                sql.Append(not ? $"ISNULL({property}, '') <> ''" : $"ISNULL({property}, '') = ''");
            }
        }

        public bool AddTableField(Type tableType, string fieldName, string identifier = null)
        {
            if (!tableFactory.IsConfiguredTable(tableType))
            {
                return false;
            }

            var table = tableFactory.GetTable(tableType);
            var column = table.Columns.FirstOrDefault(c => c.DtoFieldName == fieldName);
            if (column != null)
            {
                sql.Append(tableFactory.DialectQuery.Encapsulation(column.DbColumnName, identifier ?? table.Identifier));
            }
            return column != null;
        }

        public void AddParameter(object val, string argId = null)
        {
            if (val == null)
            {
                sql.Append("NULL");
                return;
            }

            argId ??= GetNewParameterId();
            Parameters.Add(argId, val);

            sql.Append($"@{argId}");
        }

        public void AddExpression(Expression val, string argId = null)
        {
            argId ??= GetNewParameterId();
            Parameters.Add(argId, val);

            sql.Append($"@{argId}");
        }

        public void AddColumnSelector(object val)
        {
            sql.Append(sql.Length > 1 ? $", {val}" : $"{val}");
            return;
        }

        public bool AddColumnSelector(Type tableType, string columnName, string columnAlias = null)
        {
            if (!tableFactory.IsConfiguredTable(tableType))
            {
                return false;
            }

            var table = tableFactory.GetTable(tableType);
            var column = table.Columns.FirstOrDefault(c => c.DtoFieldName == columnName);
            if (column != null)
            {
                var dbColumn = tableFactory.DialectQuery.GetColumnFormated(column, table.Identifier);
                sql.Append(sql.Length > 1 ? $", {dbColumn}" : dbColumn);
            }
            return column != null;
        }
    }
}

