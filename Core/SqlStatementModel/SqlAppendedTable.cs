using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Holds a table appended to the list of <see cref="SqlStatement.SqlTables"/> via CROSS JOIN, CROSS APPLY, or OUTER APPLY.
  /// The concrete appended table semantics is calculated from the given <see cref="JoinSemantics"/> and the kind of <see cref="SqlTable"/> that
  /// is appended.
  /// </summary>
  public class SqlAppendedTable
  {
    private readonly SqlTable _joinedTable;
    private readonly JoinSemantics _joinSemantics;

    public SqlAppendedTable (SqlTable joinedTable, JoinSemantics joinSemantics)
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);

      _joinedTable = joinedTable;
      _joinSemantics = joinSemantics;
    }

    public SqlTable JoinedTable
    {
      get { return _joinedTable; }
    }

    public JoinSemantics JoinSemantics
    {
      get { return _joinSemantics; }
    }

    public override string ToString ()
    {
      return GetApplySemanticsString() + JoinedTable.TableInfo;
    }

    private string GetApplySemanticsString ()
    {
      switch (_joinSemantics)
      {
        case JoinSemantics.Left:
          return "OUTER APPLY ";
        default:
          return "CROSS APPLY ";
      }
    }
  }
}