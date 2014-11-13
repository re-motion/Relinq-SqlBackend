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
    private readonly SqlTable _joinedTable;

    public UnresolvedJoinConditionExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo, SqlTable joinedTable)
        : base (typeof (bool))
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);
      _joinedTable = joinedTable;
    }


  // TODO RMLNQSQL-64: Implement.
    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      throw new NotImplementedException();
    }
  }
}