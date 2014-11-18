using System;
using System.Linq.Expressions;
using Remotion.Linq.Parsing;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// Subclasses of <see cref="ExpressionTreeVisitor"/> implement <see cref="IUnresolvedJoinConditionExpressionVisitor"/> if they want to explicitly
  /// deal with <see cref="UnresolvedJoinConditionExpression"/> instances.
  /// </summary>
  public interface IUnresolvedJoinConditionExpressionVisitor
  {
    Expression VisitUnresolvedJoinConditionExpression (UnresolvedJoinConditionExpression expression);
  }
}