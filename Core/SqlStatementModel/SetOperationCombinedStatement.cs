using System;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Holds a statement combined with an outer statement using a SQL set operation.
  /// </summary>
  public class SetOperationCombinedStatement
  {
    private readonly SqlStatement _sqlStatement;
    private readonly SetOperation _setOperation;

    public SetOperationCombinedStatement (SqlStatement sqlStatement, SetOperation setOperation)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);

      _sqlStatement = sqlStatement;
      _setOperation = setOperation;
    }

    public SqlStatement SqlStatement
    {
      get { return _sqlStatement; }
    }

    public SetOperation SetOperation
    {
      get { return _setOperation; }
    }
  }
}