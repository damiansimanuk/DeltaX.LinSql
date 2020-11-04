namespace DeltaX.LinSql.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class TableConfig
    {
        public TableConfig(Type type)
        {
            TableType = type;
            ExpressionWhere = new List<Expression>();
            ExpressionSelect = new List<Expression>();
            ExpressionJoin = new List<Expression>();
        }

        public Type TableType { get; set; }
        public List<Expression> ExpressionWhere { get; private set; }
        public List<Expression> ExpressionSelect { get; private set; }
        public List<Expression> ExpressionJoin { get; private set; }
    }
}
