namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal class OrderingComparer<TElement> : ExpressionVisitor, IComparer<TElement>
    {
        private readonly Expression _expression;
        private readonly Dictionary<ParameterExpression, ParameterExpression> _parameters;
        private bool _inOrderExpression;
        private Stack<Ordering<TElement, TElement>> _orderings;

        public OrderingComparer(Expression expression)
        {
            this._parameters = new Dictionary<ParameterExpression, ParameterExpression>();
            this._expression = expression;
        }

        public int Compare(TElement x, TElement y)
        {
            int num2;
            if (!this.CanCompare)
            {
                throw new InvalidOperationException(NuGetResources.AggregateQueriesRequireOrder);
            }
            int num = 0;
            using (Stack<Ordering<TElement, TElement>>.Enumerator enumerator = this._orderings.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        Ordering<TElement, TElement> current = enumerator.Current;
                        IComparable comparable = current.Extractor(x);
                        IComparable comparable2 = current.Extractor(y);
                        if ((comparable == null) && (comparable2 == null))
                        {
                            continue;
                        }
                        num = comparable.CompareTo(comparable2);
                        if (num == 0)
                        {
                            continue;
                        }
                        num2 = !current.Descending ? num : -num;
                    }
                    else
                    {
                        return num;
                    }
                    break;
                }
            }
            return num2;
        }

        private void EnsureOrderings()
        {
            if (this._orderings == null)
            {
                this._orderings = new Stack<Ordering<TElement, TElement>>();
                this.Visit(this._expression);
            }
        }

        protected override Expression VisitLambda<T>(Expression<T> node) => 
            (!this._inOrderExpression ? base.VisitLambda<T>(node) : Expression.Lambda<Func<TElement, IComparable>>(Expression.Convert(this.Visit(node.Body), typeof(IComparable)), Enumerable.Select<ParameterExpression, Expression>(node.Parameters, new Func<ParameterExpression, Expression>(this.Visit)).Cast<ParameterExpression>().ToArray<ParameterExpression>()));

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (QueryableUtility.IsOrderingMethod(node))
            {
                this._inOrderExpression = true;
                Expression<Func<TElement, IComparable>> operand = (Expression<Func<TElement, IComparable>>) ((UnaryExpression) this.Visit(node.Arguments[1])).Operand;
                Ordering<TElement, TElement> item = new Ordering<TElement, TElement>();
                item.Descending = node.Method.Name.EndsWith("Descending", StringComparison.OrdinalIgnoreCase);
                item.Extractor = operand.Compile();
                this._orderings.Push(item);
                this._inOrderExpression = false;
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            ParameterExpression expression;
            if (!this._inOrderExpression)
            {
                return base.VisitParameter(node);
            }
            if (!this._parameters.TryGetValue(node, out expression))
            {
                expression = Expression.Parameter(node.Type);
                this._parameters[node] = expression;
            }
            return expression;
        }

        public bool CanCompare
        {
            get
            {
                this.EnsureOrderings();
                return (this._orderings.Count > 0);
            }
        }

        private class Ordering<T>
        {
            public Func<T, IComparable> Extractor { get; set; }

            public bool Descending { get; set; }
        }
    }
}

