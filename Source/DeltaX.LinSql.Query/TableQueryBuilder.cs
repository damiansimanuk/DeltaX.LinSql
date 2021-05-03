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
        public List<(Type type, Expression property, JoinType joinType)> ExpressionJoin { get; private set; }
        public Dictionary<Type, List<(Expression property, object value)>> ExpressionSet { get; private set; }
        public List<(Expression property, bool ascendant)> ExpressionOrder { get; private set; }
        public List<(Expression property, string alias)> ExpressionAlias { get; private set; }
        public (int skipCount, int rowsPerPage)? ExpressionLimit { get; private set; }
        public bool IsSetValue { get; private set; }
        public object TableUpdate { get; private set; }
        public object TableDeleteEntity { get; private set; }
        public Type TableDeleteType { get; private set; }
        public Dictionary<Type, object> TableSelect { get; private set; }

        public TableQueryBuilder()
        {
            Tables = new HashSet<Type>();
            ExpressionWhere = new List<Expression>();
            ExpressionSelect = new List<Expression>();
            ExpressionJoin = new List<(Type type, Expression property, JoinType joinType)>();
            ExpressionSet = new Dictionary<Type, List<(Expression, object)>>();
            ExpressionOrder = new List<(Expression property, bool ascendant)>();
            ExpressionLimit = null;
            TableSelect = new Dictionary<Type, object>();
            ExpressionAlias = new List<(Expression property, string alias)>();
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

        internal void SelectEntity(Type entityType, object entity = null)
        {
            AssertException(TableDeleteType == null, "Can't select element with delete statement!");
            GetTable(entityType);

            TableSelect[entityType] = entity;
        }

        internal void Join<T>(Expression properties, JoinType joinType = JoinType.Join)
        {
            var table = GetTable(typeof(T));
            ExpressionJoin.Add((table, properties, joinType));
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

        internal void OrderBy(Expression property, bool ascendant = true)
        {
            AssertException(ExpressionSelect.Any() || TableSelect.Any(), "Can't Order element without select statement!");
            ExpressionOrder.Add((property, ascendant));
        }

        internal void Limit(int skipCount, int rowsPerPage)
        {
            AssertException(ExpressionSelect.Any() || TableSelect.Any(), "Can't Order element without select statement!");
            ExpressionLimit = (skipCount, rowsPerPage);
        }

        internal void SelectAlias(Expression property, string columnAlias)
        {
            AssertException(ExpressionSelect.Any() || TableSelect.Any(), "Can't add alias without select statement!");
            ExpressionAlias.Add((property, columnAlias));
        }
    }
}

