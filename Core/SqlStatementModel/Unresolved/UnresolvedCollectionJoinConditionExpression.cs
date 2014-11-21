using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// Represents the yet unresolved join condition for an <see cref="UnresolvedCollectionJoinInfo"/>.
  /// </summary>
  public class UnresolvedCollectionJoinConditionExpression : ExtensionExpression
  {
    private readonly SqlTable _joinedTable;
    private readonly UnresolvedCollectionJoinTableInfo _unresolvedCollectionJoinTableInfo;

    public UnresolvedCollectionJoinConditionExpression (SqlTable joinedTable)
        : base (typeof (bool))
    {
      _joinedTable = joinedTable;
      _unresolvedCollectionJoinTableInfo = joinedTable.TableInfo as UnresolvedCollectionJoinTableInfo;
      if (_unresolvedCollectionJoinTableInfo == null)
        throw new ArgumentException ("The given SqlTable must be joined using an UnresolvedCollectionJoinTableInfo.", "joinedTable");
    }

    public SqlTable JoinedTable
    {
      get { return _joinedTable; }
    }

    public UnresolvedCollectionJoinTableInfo UnresolvedCollectionJoinTableInfo
    {
      get { return _unresolvedCollectionJoinTableInfo; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as IUnresolvedCollectionJoinConditionExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitUnresolvedCollectionJoinConditionExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      Assertion.DebugAssert (
          _unresolvedCollectionJoinTableInfo.MemberInfo.DeclaringType != null,
          "_unresolvedCollectionJoinTableInfo.MemberInfo.DeclaringType != null");
      return string.Format (
          "CollectionJoinCondition({0}.{1})",
          _unresolvedCollectionJoinTableInfo.MemberInfo.DeclaringType.Name,
          _unresolvedCollectionJoinTableInfo.MemberInfo.Name);
    }
  }
}