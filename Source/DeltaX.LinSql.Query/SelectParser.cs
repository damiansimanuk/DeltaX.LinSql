namespace DeltaX.LinSql.Query
{
    using DeltaX.LinSql.Table;
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class SelectParser : ExpressionVisitor
    {
        private QueryStream stream;  
        private bool openBrace;

        public SelectParser(Expression expression, TableQueryFactory tableFactory = null, IEnumerable<Type> allowedTables = null,
            int? paramGeneratorOffset = null, bool openBrace = false)
        {
            this.openBrace = openBrace;
            this.stream = new QueryStream(tableFactory, allowedTables, paramGeneratorOffset);
            Visit(expression);
        }

        public SelectParser(QueryStream stream)
        {
            this.stream = stream; 
        }

        public string GetSql()
        {
            return stream.GetSql();
        }

        public IDictionary<string, object> GetParameters()
        {
            return stream.GetParameters();
        } 

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {   
            if (stream.IsAllowed(node.Expression.Type)
                && stream.AddColumnSelector(node.Expression.Type, node.Member.Name))
            {  
                return node;
            } 

            return base.VisitMember(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            if (openBrace) stream.OpenBrace();
            var res = base.VisitNewArray(node);
            if (openBrace) stream.CloseBrace();
            return res;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            stream.AddColumnSelector(node.Value);
            return node;
        }  
    }

}
