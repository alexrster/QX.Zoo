using System;
using System.Linq.Expressions;

namespace QX.Zoo.Hold
{
  public static class ExpressionExtensions
  {
    public static Expression<Func<T, bool>> Combine<T>(this Expression<Func<T, bool>> param, Expression<Func<T, bool>> expression)
    {
      return Expression.Lambda<Func<T, bool>>(
        Expression.AndAlso(new ReplaceVisitor(param.Parameters[0], expression.Parameters[0]).Visit(param.Body), expression.Body),
        expression.Parameters);
    }

    public static Expression<Func<TParam, TResult>> Convert<TParam, T, TResult>(this Expression<Func<T, TResult>> expression, Expression<Func<TParam, T>> outerMemberExpression)
    {
      var p = Expression.Parameter(typeof(TParam), "p");
      return Expression.Lambda<Func<TParam, TResult>>(new ReplaceVisitor(expression.Parameters[0], outerMemberExpression.Body).Visit(expression.Body), p);
    }

    private class ReplaceVisitor : ExpressionVisitor
    {
      private readonly Expression _from;
      private readonly Expression _to;

      public ReplaceVisitor(Expression from, Expression to)
      {
        _from = from;
        _to = to;
      }

      public override Expression Visit(Expression node)
      {
        return node == _from ? _to : base.Visit(node);
      }
    }
  }
}
