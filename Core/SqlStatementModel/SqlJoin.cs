using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Represents an INNER or LEFT join with a <see cref="JoinCondition"/>.
  /// </summary>
  public class SqlJoin
  {
    private readonly SqlTable _joinedTable;
    private readonly JoinSemantics _joinSemantics;
    private readonly Expression _joinCondition;

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
    }

    public override string ToString ()
    {
      return JoinSemantics.ToString().ToUpper() + " JOIN " + JoinedTable.TableInfo + " ON " + JoinCondition;
    }
  }
}