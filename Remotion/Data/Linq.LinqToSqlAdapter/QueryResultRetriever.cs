// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  public class QueryResultRetriever : IQueryResultRetriever
  {
    private readonly IConnectionManager _connectionManager;
    private readonly IReverseMappingResolver _resolver;

    public QueryResultRetriever (IConnectionManager connectionManager, IReverseMappingResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("connectionManager", connectionManager);
      ArgumentUtility.CheckNotNull ("resolver", resolver);

      _connectionManager = connectionManager;
      _resolver = resolver;
    }

    public IEnumerable<T> GetResults<T> (Func<IDatabaseResultRow, T> projection, string commandText, CommandParameter[] parameters)
    {
      using (var connection = _connectionManager.Open ())
      using (var command = connection.CreateCommand ())
      {
        command.CommandText = commandText;

        Array.ForEach (parameters, p => command.Parameters.Add(p));

        using (var reader = command.ExecuteReader())
        {
          while (reader.NextResult())
            yield return projection (new RowWrapper (reader, _resolver));
        }
      }
    }

    public T GetScalar<T> (string commandText, CommandParameter[] parameters)
    {
      using (var connection = _connectionManager.Open ())
      using (var command = connection.CreateCommand ())
      {
        command.CommandText = commandText;

        Array.ForEach (parameters, p => command.Parameters.Add (p));

        return (T) command.ExecuteScalar ();
      }
    }
  }
}