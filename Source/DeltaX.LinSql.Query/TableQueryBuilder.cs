namespace DeltaX.LinSql.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
     
    public class TableQueryBuilder
    {
        private List<TableConfig> Tables { get; set; }

        public TableQueryBuilder()
        {
            Tables = new List<TableConfig>();
        }

        public void AddTable<T>()
        {
            Tables.Add(new TableConfig { TableType = typeof(T) });
        }

        public TableConfig GetTableConfig<T>()
        {
            return Tables.FirstOrDefault(t => t.TableType == typeof(T))
                 ?? throw new ArgumentException($"Table for Type {typeof(T)} is not configurated!");
        }

        public void Where<T>(Expression<Func<T, bool>> whereCondition)
        {
            var table = GetTableConfig<T>();

            table.ExpressionWhere = whereCondition;
            new QueryParser(whereCondition);
        }

        public void Select<T>(Expression<Func<T, object>> properties)
        {
            var table = GetTableConfig<T>();

            table.ExpressionSelect = properties;
        }
    }
}
