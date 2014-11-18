using System;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  // TODO RMLNQSQL-64: Test
  /// <summary>
  /// Represents a yet unresolved join condition.
  /// </summary>
  public class UnresolvedJoinConditionExpression : ExtensionExpression
  {
    private readonly SqlEntityExpression _originatingEntity;
    private readonly MemberInfo _memberInfo;
    private readonly SqlTable _joinedTable;

    public UnresolvedJoinConditionExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo, SqlTable joinedTable)
        : base (typeof (bool))
    {
      ArgumentUtility.CheckNotNull ("originatingEntity", originatingEntity);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);

      _originatingEntity = originatingEntity;
      _memberInfo = memberInfo;
      _joinedTable = joinedTable;
    }

    public SqlEntityExpression OriginatingEntity
    {
      get { return _originatingEntity; }
    }

    public MemberInfo MemberInfo
    {
      get { return _memberInfo; }
    }

    public SqlTable JoinedTable
    {
      get { return _joinedTable; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as IUnresolvedJoinConditionExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitUnresolvedJoinConditionExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      Assertion.DebugAssert (_memberInfo.DeclaringType != null, "_memberInfo.DeclaringType != null");
      return string.Format ("JoinCondition({0}.{1})", _memberInfo.DeclaringType.Name, _memberInfo.Name);
    }
  }
}