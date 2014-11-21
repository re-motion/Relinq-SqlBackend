using System;
using System.Linq.Expressions;
using Remotion.Linq.Parsing;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// Subclasses of <see cref="ExpressionTreeVisitor"/> implement <see cref="IUnresolvedCollectionJoinConditionExpressionVisitor"/> if they want to 
  /// explicitly deal with <see cref="UnresolvedCollectionJoinConditionExpression"/> instances.
  /// </summary>
  public interface IUnresolvedCollectionJoinConditionExpressionVisitor
  {
    Expression VisitUnresolvedCollectionJoinConditionExpression (UnresolvedCollectionJoinConditionExpression expression);
  }
}