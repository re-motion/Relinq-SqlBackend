using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Represents a join.
  /// </summary>
  public class SqlJoin
  {
    private readonly SqlTable _joinedTable;
    private readonly JoinSemantics _joinSemantics;
    private Expression _joinCondition;

    public SqlJoin (SqlTable joinedTable, JoinSemantics joinSemantics, Expression joinCondition)
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);
      ArgumentUtility.CheckNotNull ("joinCondition", joinCondition);

      _joinedTable = joinedTable;
      _joinSemantics = joinSemantics;
      _joinCondition = joinCondition;
    }

    public SqlTable JoinedTable
    {
      get { return _joinedTable; }
    }

    public JoinSemantics JoinSemantics
    {
      get { return _joinSemantics; }
    }

    public Expression JoinCondition
    {
      get { return _joinCondition; }
      set { _joinCondition = value; }
    }
  }
}