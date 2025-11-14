// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Utilities;

namespace Remotion.Linq.LinqToSqlAdapter
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
      ArgumentUtility.CheckNotNull (nameof(connectionManager), connectionManager);
      ArgumentUtility.CheckNotNull (nameof(resolver), resolver);

      _connectionManager = connectionManager;
      _resolver = resolver;
    }

    public IEnumerable<T> GetResults<T> (Func<IDatabaseResultRow, T> projection, string commandText, CommandParameter[] parameters)
    {
      ArgumentUtility.CheckNotNull (nameof(projection), projection);
      ArgumentUtility.CheckNotNullOrEmpty (nameof(commandText), commandText);
      ArgumentUtility.CheckNotNull (nameof(parameters), parameters);

      using (var connection = _connectionManager.Open())
      {
        using (var command = connection.CreateCommand())
        {
          command.CommandText = commandText;
          AddParametersToCommand (command, parameters);

          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
              yield return projection (new RowWrapper (reader, _resolver));
          }
        }
      }
    }

    public T GetScalar<T> (Func<IDatabaseResultRow, T> projection, string commandText, CommandParameter[] parameters)
    {
      using (var connection = _connectionManager.Open())
      {
        using (var command = connection.CreateCommand())
        {
          command.CommandText = commandText;
          AddParametersToCommand (command, parameters);


          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
              return projection (new ScalarRowWrapper (reader));
          }
        }
      }

      return default (T);
    }

    private static void AddParametersToCommand (DbCommand command, IEnumerable<CommandParameter> parameters)
    {
      foreach (var commandParameter in parameters)
      {
        var dataParameter = command.CreateParameter();
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