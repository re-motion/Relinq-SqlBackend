﻿using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Holds a table appended to the list of <see cref="SqlStatement.SqlTables"/> via CROSS JOIN, CROSS APPLY, or OUTER APPLY.
  /// The concrete appended table semantics is calculated from the given <see cref="JoinSemantics"/> and the kind of <see cref="SqlStatementModel.SqlTable"/> that
  /// is appended.
  /// </summary>
  public class SqlAppendedTable
  {
    private readonly SqlTable _sqlTable;
    private readonly JoinSemantics _joinSemantics;

    public SqlAppendedTable (SqlTable sqlTable, JoinSemantics joinSemantics)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      _sqlTable = sqlTable;
      _joinSemantics = joinSemantics;
    }

    public SqlTable SqlTable
    {
      get { return _sqlTable; }
    }

    public JoinSemantics JoinSemantics
    {
      get { return _joinSemantics; }
    }

    public override string ToString ()
    {
      return GetApplySemanticsString() + SqlTable;
    }

    private string GetApplySemanticsString ()
    {
      if (_joinSemantics == JoinSemantics.Left)
        return "OUTER APPLY ";
      if (_sqlTable.TableInfo is ResolvedSimpleTableInfo)
        return "CROSS APPLY ";
      return "CROSS JOIN ";
    }
  }
}