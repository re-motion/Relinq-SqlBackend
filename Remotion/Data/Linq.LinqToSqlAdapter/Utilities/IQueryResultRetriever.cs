using System;
using System.Collections.Generic;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.LinqToSqlAdapter.Utilities
{
  public interface IQueryResultRetriever
  {
    IEnumerable<T> GetResults<T> (Func<IDatabaseResultRow, T> projection, string commandText, CommandParameter[] parameters);
    T GetScalar<T> (string commandText, CommandParameter[] parameters);
  }
}