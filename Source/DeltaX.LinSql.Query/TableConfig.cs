namespace DeltaX.LinSql.Query
{
    using System;
    using System.Linq.Expressions;

    public class TableConfig
    {
        public Type TableType { get; set; }
        public Expression ExpressionWhere { get; set; }
        public Expression ExpressionSelect { get; set; }
    }
}
