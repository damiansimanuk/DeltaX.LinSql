namespace DeltaX.LinSql.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class QueryHelper
    {
        public static object GetValueFromExpression(Expression expression)
        {
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        internal static bool IsVariable(Expression expr)
        {
            return (expr is MemberExpression) && (((MemberExpression)expr).Expression is ConstantExpression);
        }


        internal static bool IsHasValue(MemberExpression expr)
        {
            return  expr.Member.Name == "HasValue";
        }

        internal static bool IsBoolean(MemberExpression member)
        {
            var type = (member.Member as PropertyInfo)?.PropertyType;
            return type == typeof(bool) || type == typeof(bool?);
        }

        internal static bool IsString(MemberExpression member)
        {
            var type = (member.Member as PropertyInfo)?.PropertyType;
            return type == typeof(string);
        }

        internal static bool IsBoolean(Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        internal static Expression GetFirstMemberExpression(Expression expression, IEnumerable<Type> filterType)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                return GetFirstMemberExpression(binaryExpression.Left, filterType)
                    ?? GetFirstMemberExpression(binaryExpression.Right, filterType);
            }
            if (expression is UnaryExpression unaryExpression)
            {
                return GetFirstMemberExpression(unaryExpression.Operand, filterType);
            }
            if (expression is LambdaExpression lambdaExpression)
            {
                return GetFirstMemberExpression(lambdaExpression.Body, filterType);
            }
            if (expression is NewArrayExpression newArrayExpression)
            {
                return newArrayExpression.Expressions
                    .Select(a => GetFirstMemberExpression(a, filterType))
                    .FirstOrDefault(e => e != null);
            }
            if (expression is MethodCallExpression callExpression)
            {
                var res = GetFirstMemberExpression(callExpression.Object, filterType);
                res ??= callExpression.Arguments
                      .Select(a => GetFirstMemberExpression(a, filterType))
                      .FirstOrDefault(e => e != null);
                return res;
            }
            if (expression is MemberExpression memberExpression)
            {
                return filterType == null || filterType.Contains(memberExpression.Expression?.Type) ? expression : null;
            }
            return null;
        }


        internal static string GetOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            { 
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return (IsBoolean(b.Left.Type)) ? "AND" : "&";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return (IsBoolean(b.Left.Type) ? "OR" : "|");
                default:
                    return GetOperator(b.NodeType);
            }
        }

        internal static bool IsLogicalOperation(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Not: 
                case ExpressionType.And:
                case ExpressionType.AndAlso: 
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return true;
                default:
                    return false;
            }
        }

        internal static string GetOperator(ExpressionType exprType)
        {
            switch (exprType)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.RightShift:
                    return ">>";
                default:
                    return "";
            }
        }

    }
}
