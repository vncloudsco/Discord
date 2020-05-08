namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ExpressionRewriter : ExpressionVisitor
    {
        private readonly IQueryable _rootQuery;
        private readonly IEnumerable<string> _methodsToExclude;

        public ExpressionRewriter(IQueryable rootQuery, IEnumerable<string> methodsToExclude)
        {
            this._methodsToExclude = methodsToExclude;
            this._rootQuery = rootQuery;
        }

        protected override Expression VisitConstant(ConstantExpression node) => 
            (!typeof(IQueryable).IsAssignableFrom(node.Type) ? base.VisitConstant(node) : this._rootQuery.Expression);

        protected override Expression VisitMethodCall(MethodCallExpression node) => 
            (!Enumerable.Any<string>(this._methodsToExclude, method => QueryableUtility.IsQueryableMethod(node, method)) ? base.VisitMethodCall(node) : this.Visit(node.Arguments[0]));
    }
}

