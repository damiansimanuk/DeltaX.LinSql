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
        public Dictionary<Type, List< Tuple<Expression, object>>> ExpressionSet { get; private set; }
        public bool MakeDelete { get; private set; }
        public bool MakeUpdate { get; private set; }
        public object TableUpdate { get; private set; }


        public TableQueryBuilder()
        {
            Tables = new HashSet<Type>();
            ExpressionWhere = new List<Expression>();
            ExpressionSelect = new List<Expression>();
            ExpressionJoin = new Dictionary<Type, Expression>();
            ExpressionSet = new Dictionary<Type, List<Tuple<Expression, object>>>();
            MakeDelete = false;
            MakeUpdate = false;
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
            AssertException(!MakeDelete, "Can't select element with delete statement!"); 

            ExpressionSelect.Add(properties);
        }

        internal void Join<T>(Expression properties)
        {
            var table = GetTable(typeof(T));
            ExpressionJoin[table] = properties;
        } 


        internal void Set<T>(Expression property, object value)
        {
            AssertException(!MakeDelete, "Can't update element with delete statement!");
            AssertException(!ExpressionSelect.Any(), "Can't update element with select statement!");
            AssertException(ExpressionWhere.Any(), "Can't update element without where statement!");

            var table = GetTable(typeof(T));
            if (!ExpressionSet.ContainsKey(table))
            {
                ExpressionSet[table] = new List<Tuple<Expression, object>>();
            }
            ExpressionSet[table].Add(new Tuple<Expression, object>(property, value));
            MakeUpdate = true;
        }

        internal void Update<T>(T table)
        {
            AssertException(TableUpdate == null, "Can't update multiple tables!");
            GetTable(typeof(T));

            TableUpdate = table;
            MakeUpdate = true;
        }

        internal void Delete()
        {
            AssertException(!ExpressionSelect.Any(), "Can't delete element with select statement!");
            AssertException(ExpressionWhere.Any(), "Can't delete element without where statement!");

            MakeDelete = true;
        }
    }
}

