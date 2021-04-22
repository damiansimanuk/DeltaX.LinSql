namespace DeltaX.LinSql.Query
{
    using DeltaX.LinSql.Table;
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ExpressionQueryParser : ExpressionVisitor
    {
        private QueryStream stream;
        private List<ExpressionType> operators;

        public ExpressionQueryParser(Expression expression, TableQueryFactory tableFactory = null, IEnumerable<Type> allowedTables = null)
        {
            this.operators = new List<ExpressionType>();
            this.stream = new QueryStream(tableFactory, allowedTables);
            Visit(expression);
        }

        public ExpressionQueryParser(QueryStream stream)
        {
            this.stream = stream;
            this.operators = new List<ExpressionType>();
        }

        public string GetSql()
        {
            return stream.GetSql();
        }

        public IDictionary<string, object> GetParameters()
        {
            return stream.GetParameters();
        }

        protected ExpressionType? LastOperator()
        {
            if (operators.Any())
                return operators.Last();
            return null;
        }

        private static ExpressionType[] noOpenBraceTypes = new[] {
            ExpressionType.Not,
            ExpressionType.Convert
        };

        private static ExpressionType[] notOperators = new[] {
            ExpressionType.Not,
            ExpressionType.NotEqual
        };

        protected bool IsNotLastOperator()
        {
            if (operators.Any())
            {
                return notOperators.Contains(LastOperator().Value);
            }
            return false;
        }

        protected Expression WithOperator(ExpressionType nodeType, Func<Expression> action)
        {
            operators.Add(nodeType);

            var openBrace = (operators.Count() > 1 && !noOpenBraceTypes.Contains(nodeType));

            if (openBrace) stream.OpenBrace();

            var res = action.Invoke();

            operators.RemoveAt(operators.Count() - 1);
            if (openBrace) stream.CloseBrace();

            return res;
        }

        public override Expression Visit(Expression node)
        {
            var member = QueryHelper.GetFirstMemberExpression(node, stream.AllowedTables);
            if (member == null && node.NodeType != ExpressionType.Parameter)
            {
                stream.AddExpression(node);
                return node;
            }
            else
            {
                return base.Visit(node);
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            
            return WithOperator(node.NodeType, () =>
            {
                var not = IsNotLastOperator();
                if (node.Left is ConstantExpression left && left.Value == null)
                {
                    Visit(node.Right);
                    stream.AddIsNull(not);
                }
                else if (node.Right is ConstantExpression right && right.Value == null)
                {
                    Visit(node.Left);
                    stream.AddIsNull(not);
                }
                else
                {
                    Visit(node.Left);
                    stream.AddOperator(QueryHelper.GetOperator(node));
                    Visit(node.Right);
                }
                return node;
            });
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return WithOperator(node.NodeType, () => base.VisitUnary(node));
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Parameter
                && stream.IsAllowed(node.Expression.Type)
                && stream.AddTableField(node.Expression.Type, node.Member.Name))
            {
                var lastOp = LastOperator();
                var not = IsNotLastOperator();

                if (lastOp == null || QueryHelper.IsLogicalOperation(lastOp.Value))
                { 
                    if (QueryHelper.IsBoolean(node))
                    {
                        stream.AddBoolean(not);
                    }
                    else
                    {
                        stream.AddIsNull(not);
                    }
                }

                return node;
            }

            // External Variable Value
            if (QueryHelper.IsVariable(node))
            {
                stream.AddExpression(node);
                return node;
            } 

            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var not = IsNotLastOperator();

            switch (node.Method.Name)
            {
                case MethodCall.Contains:
                    if (node.Method.DeclaringType == typeof(string))
                    {
                        return WithOperator(node.NodeType, () => StringLike(node, not, true, true));
                    }
                    else
                    {
                        return WithOperator(node.NodeType, () => ListIn(node, not));
                    }
                case MethodCall.EndsWith:
                    return WithOperator(node.NodeType, () => StringLike(node, not, true, false));
                case MethodCall.StartsWith:
                    return WithOperator(node.NodeType, () => StringLike(node, not, false, true));
                case MethodCall.IsNullOrEmpty:
                    if (node.Arguments[0] is MemberExpression member && QueryHelper.IsString(member))
                    {
                        StringNullOrEmpty(member, not);
                        return node;
                    }
                    break;
            }
            return base.VisitMethodCall(node);
        }


        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return WithOperator(node.NodeType, () => base.VisitNewArray(node));
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            stream.AddParameter(node.Value);
            return base.VisitConstant(node);
        }

        private Expression ListIn(MethodCallExpression node, bool not = false)
        {
            var values = QueryHelper.GetValueFromExpression(node.Object) as IList;

            Visit(node.Arguments[0]);

            var elements = new List<string>();
            foreach (var e in values)
            {
                if (e is string es)
                    elements.Add($"'{es}'");
                else
                    elements.Add($"{e}");
            }

            stream.AddIn(not, elements);

            return node;
        }

        private Expression StringLike(MethodCallExpression node, bool not = false, bool prefix = false, bool sufix = false)
        {
            Visit(node.Object);

            stream.AddLike(not);

            if (prefix) stream.AddLikePrefix();

            Visit(node.Arguments[0]);

            if (sufix) stream.AddLikeSuffix();

            return node;
        }

        private void StringNullOrEmpty(MemberExpression member, bool not = false)
        { 
            if (stream.IsAllowed(member.Expression.Type))
            {
                stream.AddIsNullOrEmpty(member.Expression.Type, member.Member.Name, not);
            } 
        }
    }

}
