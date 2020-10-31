namespace DeltaX.LinSql.Query
{
    using DeltaX.LinSql.Table;
    using System.Collections.Generic;
    using System.Linq.Expressions;


    public class QueryParser : ExpressionVisitor
    {
        QueryStream stream;

        public QueryParser(Expression expression = null, TableQueryFactory tableFactory = null)
        {
            stream = new QueryStream(tableFactory);
            base.Visit(expression);

            var sql = stream.GetSql(); 
        }

        public string GetSql()
        {
            return stream.GetSql();
        }

        public IDictionary<string, object> GetParameters()
        {
            return stream.GetParameters();
        }

        public IDictionary<ITableConfiguration, HashSet<ColumnConfiguration>> GetTableColumns()
        {
            return stream.GetTableColumns();
        }

        protected override Expression VisitNew(NewExpression node)
        {
            return base.VisitNew(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return base.VisitNewArray(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            base.Visit(node.Left);
            stream.AddOperator(QueryHelper.GetOperator(node));
            base.Visit(node.Right);

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return base.VisitUnary(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            return base.VisitBlock(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (stream.IsConfiguredTable(node.Expression.Type))
            {
                if (stream.AddColumn(node.Expression.Type, node.Member.Name))
                {
                    return node;
                }
            }

            if (QueryHelper.IsVariable(node))
            {
                stream.AddParameter(QueryHelper.GetValueFromExpression(node));
                return node;
            }

            return base.VisitMember(node);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            return base.VisitCatchBlock(node);
        }



        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // QueryHelper.GetValueFromExpression(node.Value);            
            stream.AddParameter(node.Value);

            return base.VisitConstant(node);
        }
    }

}
