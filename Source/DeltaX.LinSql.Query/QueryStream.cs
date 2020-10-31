namespace DeltaX.LinSql.Query
{
    using DeltaX.LinSql.Table;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class QueryStream
    {
        private StringBuilder sql = new StringBuilder();
        private Dictionary<string, object> Parameters = new Dictionary<string, object>();
        private Dictionary<ITableConfiguration, HashSet<ColumnConfiguration>> tablesConfigured = new Dictionary<ITableConfiguration, HashSet<ColumnConfiguration>>();
        private TableQueryFactory tableFactory;
        private int nextArgumentGenerator = 0;

        public QueryStream(TableQueryFactory tableFactory = null)
        {
            this.tableFactory = tableFactory ?? TableQueryFactory.GetInstance();
        }

        private string GetNewArgId() => $"arg_{nextArgumentGenerator++}";

        public IDictionary<string, object> GetParameters()
        {
            return Parameters;
        }

        public IDictionary<ITableConfiguration, HashSet<ColumnConfiguration>> GetTableColumns()
        {
            return tablesConfigured;
        }

        public string GetSql()
        {
            return sql.ToString();
        }

        public bool IsConfiguredTable(Type tableType)
        {
            return tableFactory.IsConfiguredTable(tableType);
        }

        public void AddOperator(string op)
        {
            sql.Append($" {op} ");
        }

        public bool AddColumn(Type tableType, string columnName)
        {
            if (!tableFactory.IsConfiguredTable(tableType))
            {
                return false;
            }

            var table = tableFactory.GetTable(tableType);
            if (!tablesConfigured.ContainsKey(table))
            {
                tablesConfigured.Add(table, new HashSet<ColumnConfiguration>());
            }
            var column = table.Columns.FirstOrDefault(c => c.DtoFieldName == columnName);
            if (column != null)
            {
                tablesConfigured[table].Add(column);
                sql.Append(tableFactory.DialectQuery.Encapsulation(column.DbColumnName, table.Identifier));
            }
            return column != null;
        }

        public void AddParameter(object val)
        {
            if (val == null)
            {
                sql.Append("NULL");
                return;
            }

            var argId = GetNewArgId();
            Parameters.Add(argId, val);

            sql.Append($"@{argId}");
        }

    }
}

