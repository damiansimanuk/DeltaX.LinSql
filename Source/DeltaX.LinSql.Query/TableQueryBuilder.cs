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
        public Dictionary<Type, List< (Expression property, object value)>> ExpressionSet { get; private set; } 
        public bool IsSetValue { get; private set; }
        public object TableUpdate { get; private set; }
        public object TableDeleteEntity { get; private set; }
        public Type TableDeleteType { get; private set; }
        public object TableSelectEntity { get; private set; }
        public Type TableSelectType { get; private set; }
        // public HashSet<object> TablesSelect { get; private set; }


        public TableQueryBuilder()
        {
            Tables = new HashSet<Type>();
            ExpressionWhere = new List<Expression>();
            ExpressionSelect = new List<Expression>();
            ExpressionJoin = new Dictionary<Type, Expression>();
            ExpressionSet = new Dictionary<Type, List<(Expression, object)>>();
            // TablesSelect = new HashSet<object>(); 
        }

        public IEnumerable<Type> GetTables()
        {
            return Tables;
        }

        public void AddTable<T>()
        {
            var t = typeof(T);
            if (!Tables.Contains(t))
            {
                Tables.Add(typeof(T));
            }
        }

        public Type GetTable(Type type)
        {
            return Tables.FirstOrDefault(t => t == type)
                 ?? throw new ArgumentException($"Table for Type {type} is not configurated!");
        }

        private void AssertException(bool validCondition, string message)
        {
            if (!validCondition)
            {
                throw new Exception(message);
            }
        }

        internal void Where(Expression whereCondition)
        {  
            ExpressionWhere.Add(whereCondition); 
        }

        internal void Select(Expression properties)
        {
            AssertException(TableDeleteType == null, "Can't select element with delete statement!");

            ExpressionSelect.Add(properties);
        }

        internal void SelectEntity<T>(T entity = null) where T : class
        {
            AssertException(TableDeleteType == null, "Can't select element with delete statement!");
            GetTable(typeof(T));

            TableSelectType = typeof(T);
            TableSelectEntity = entity;
        }

        internal void Join<T>(Expression properties)
        {
            var table = GetTable(typeof(T));
            ExpressionJoin[table] = properties;
        }


        internal void Set<T>(Expression property, object value)
        {
            AssertException(TableDeleteType == null, "Can't update element with delete statement!");
            AssertException(!ExpressionSelect.Any(), "Can't update element with select statement!");
            AssertException(ExpressionWhere.Any(), "Can't update element without where statement!");

            var table = GetTable(typeof(T));
            if (!ExpressionSet.ContainsKey(table))
            {
                ExpressionSet[table] = new List<(Expression, object)>();
            }
            ExpressionSet[table].Add((property, value));
            IsSetValue = true;
        }

        internal void Update<T>(T table)
        {
            AssertException(TableUpdate == null, "Can't update multiple tables!");
            GetTable(typeof(T));

            TableUpdate = table;
            IsSetValue = true;
        }

        internal void Delete<T>(T entity = null) where T : class
        {
            AssertException(!ExpressionSelect.Any(), "Can't delete element with select statement!");
            AssertException(ExpressionWhere.Any() || entity != null, "Can't delete element without where statement!");

            TableDeleteType = typeof(T);
            TableDeleteEntity = entity;
        }
    }
}

