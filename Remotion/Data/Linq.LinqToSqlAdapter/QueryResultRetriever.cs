﻿// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Data;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Implements IQueryResultRetriever for a certain DBConnection defined by the ConnectionManager
  /// </summary>
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
      ArgumentUtility.CheckNotNull ("projection", projection);
      ArgumentUtility.CheckNotNullOrEmpty ("commandText", commandText);
      ArgumentUtility.CheckNotNull ("parameters", parameters);

      using (var connection = _connectionManager.Open ())
      using (var command = connection.CreateCommand ())
      {
        command.CommandText = commandText;
        AddParametersToCommand (command, parameters);

        using (var reader = command.ExecuteReader ())
        {
          while (reader.Read ())
          {
            yield return projection (new RowWrapper (reader, _resolver));
          }
        }
      }
    }

    public T GetScalar<T> (Func<IDatabaseResultRow, T> projection, string commandText, CommandParameter[] parameters)
    {
      using (var connection = _connectionManager.Open ())
      using (var command = connection.CreateCommand ())
      {
        command.CommandText = commandText;
        AddParametersToCommand (command, parameters);


        using (var reader = command.ExecuteReader ())
        {
          while (reader.Read ())
          {
            return (T) projection (new ScalarRowWrapper (reader, _resolver));
          }
        }
      }

      return default (T);
    }

    private static void AddParametersToCommand(IDbCommand command, CommandParameter[] parameters)
    {
      foreach (var commandParameter in parameters)
      {
        var dataParameter = command.CreateParameter ();
        dataParameter.ParameterName = commandParameter.Name;
        dataParameter.Value = commandParameter.Value;

        if (commandParameter.Value is decimal || commandParameter.Value is decimal?)
        {
          dataParameter.Precision = 33;
          dataParameter.Scale = 4;
        }

        command.Parameters.Add (dataParameter);
      }
    }
  }
}