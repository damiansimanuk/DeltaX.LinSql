namespace DeltaX.LinSql.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class TableQueryBuilder
    {
        private HashSet<Type> Tables { get; set; }
        public List<Expression> ExpressionWhere { get; private set; }
        public List<Expression> ExpressionSelect { get; private set; }
        public Dictionary<Type, Expression> ExpressionJoin { get; private set; }

        public TableQueryBuilder()
        {
            Tables = new HashSet<Type>();
            ExpressionWhere = new List<Expression>();
            ExpressionSelect = new List<Expression>();
            ExpressionJoin = new Dictionary<Type, Expression>();
        }

        public IEnumerable<Type> GetTables()
        {
            return Tables;
        }

        public void AddTable<T>()
        {
            Tables.Add(typeof(T));
        }

        public Type GetTable(Type type)
        {
            return Tables.FirstOrDefault(t => t == type)
                 ?? throw new ArgumentException($"Table for Type {type} is not configurated!");
        }

        internal void Where(Expression whereCondition)
        {  
            ExpressionWhere.Add(whereCondition); 
        }

        internal void Select(Expression properties)
        {
            ExpressionSelect.Add(properties);
        }

        internal void Join<T>(Expression properties)
        {
            var table = GetTable(typeof(T));
            ExpressionJoin[table] = properties;
        }
    }
}

