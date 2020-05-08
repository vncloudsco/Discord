namespace NuGet
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class QueryableUtility
    {
        private static readonly string[] _orderMethods = new string[] { "OrderBy", "ThenBy", "OrderByDescending", "ThenByDescending" };
        private static readonly MethodInfo[] _methods = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static);

        public static Type FindGenericType(Type definition, Type type)
        {
            while ((type != null) && (type != typeof(object)))
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == definition))
                {
                    return type;
                }
                if (definition.IsInterface)
                {
                    foreach (Type type2 in type.GetInterfaces())
                    {
                        Type type3 = FindGenericType(definition, type2);
                        if (type3 != null)
                        {
                            return type3;
                        }
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        private static MethodInfo GetQueryableMethod(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Call)
            {
                MethodCallExpression expression2 = (MethodCallExpression) expression;
                if (expression2.Method.IsStatic && (expression2.Method.DeclaringType == typeof(Queryable)))
                {
                    return expression2.Method.GetGenericMethodDefinition();
                }
            }
            return null;
        }

        public static bool IsOrderingMethod(Expression expression) => 
            Enumerable.Any<string>(_orderMethods, method => IsQueryableMethod(expression, method));

        public static bool IsQueryableMethod(Expression expression, string method) => 
            (from m in _methods
                where m.Name == method
                select m).Contains<MethodInfo>(GetQueryableMethod(expression));

        public static Expression ReplaceQueryableExpression(IQueryable query, Expression expression) => 
            new ExpressionRewriter(query).Visit(expression);

        private class ExpressionRewriter : ExpressionVisitor
        {
            private readonly IQueryable _query;

            public ExpressionRewriter(IQueryable query)
            {
                this._query = query;
            }

            protected override Expression VisitConstant(ConstantExpression node) => 
                (!typeof(IQueryable).IsAssignableFrom(node.Type) ? base.VisitConstant(node) : this._query.Expression);
        }
    }
}

