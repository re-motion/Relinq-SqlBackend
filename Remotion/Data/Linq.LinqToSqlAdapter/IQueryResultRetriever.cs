using System;
using System.Collections.Generic;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Provides functionality to get results from a database with a given command
  /// </summary>
  public interface IQueryResultRetriever
  {
    IEnumerable<T> GetResults<T> (Func<IDatabaseResultRow, T> projection, string commandText, CommandParameter[] parameters);
    T GetScalar<T> (string commandText, CommandParameter[] parameters);
  }
}