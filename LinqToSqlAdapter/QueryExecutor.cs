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
using System.Linq;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Constitutes the bridge between re-linq and a Linq2Sql.
  /// </summary>
  public class QueryExecutor : IQueryExecutor
  {
    private readonly IMappingResolver _mappingResolver;
    private readonly IQueryResultRetriever _resultRetriever;
    
    private readonly CompoundMethodCallTransformerProvider _methodCallTransformerProvider;
    private readonly ResultOperatorHandlerRegistry _resultOperatorHandlerRegistry;

    private readonly bool _showQuery;

    public QueryExecutor (
        IMappingResolver mappingResolver,
        IQueryResultRetriever resultRetriever,
        ResultOperatorHandlerRegistry resultOperatorHandlerRegistry,
        CompoundMethodCallTransformerProvider methodCallTransformerProvider,
        bool showQuery)
    {
      ArgumentUtility.CheckNotNull (nameof(mappingResolver), mappingResolver);
      ArgumentUtility.CheckNotNull (nameof(resultRetriever), resultRetriever);
      ArgumentUtility.CheckNotNull (nameof(resultOperatorHandlerRegistry), resultOperatorHandlerRegistry);
      ArgumentUtility.CheckNotNull (nameof(methodCallTransformerProvider), methodCallTransformerProvider);

      _mappingResolver = mappingResolver;
      _resultRetriever = resultRetriever;

      _resultOperatorHandlerRegistry = resultOperatorHandlerRegistry;
      _methodCallTransformerProvider = methodCallTransformerProvider;

      _showQuery = showQuery;
    }

    public T ExecuteScalar<T> (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull (nameof(queryModel), queryModel);

      SqlCommandData commandData = GenerateSqlCommand (queryModel);
      var projection = commandData.GetInMemoryProjection<T> ().Compile ();
      return _resultRetriever.GetScalar (projection, commandData.CommandText, commandData.Parameters);
    }

    public T ExecuteSingle<T> (QueryModel queryModel, bool returnDefaultWhenEmpty)
    {
      ArgumentUtility.CheckNotNull (nameof(queryModel), queryModel);

      var sequence = ExecuteCollection<T> (queryModel);

      if (returnDefaultWhenEmpty)
        return sequence.SingleOrDefault ();
      else
        return sequence.Single ();
    }

    public IEnumerable<T> ExecuteCollection<T> (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull (nameof(queryModel), queryModel);

      SqlCommandData commandData = GenerateSqlCommand (queryModel);
      var projection = commandData.GetInMemoryProjection<T> ().Compile ();
      return _resultRetriever.GetResults (projection, commandData.CommandText, commandData.Parameters);
    }

    private SqlCommandData GenerateSqlCommand(QueryModel queryModel)
    {
      ISqlPreparationContext preparationContext = new SqlPreparationContext (new SqlStatementBuilder());
      IMappingResolutionContext mappingResolutionContext = new MappingResolutionContext ();

      var generator = new UniqueIdentifierGenerator ();
      var preparationStage = new DefaultSqlPreparationStage (_methodCallTransformerProvider, _resultOperatorHandlerRegistry, generator);
      var preparedStatement = preparationStage.PrepareSqlStatement (queryModel, preparationContext);

      var resolutionStage = new DefaultMappingResolutionStage (_mappingResolver, generator);
      var resolvedStatement = resolutionStage.ResolveSqlStatement (preparedStatement, mappingResolutionContext);

      var builder = new SqlCommandBuilder ();
      var generationStage = new DefaultSqlGenerationStage ();
      generationStage.GenerateTextForOuterSqlStatement (builder, resolvedStatement);

      var command = builder.GetCommand ();
      if (_showQuery)
      {
        Console.WriteLine (command.CommandText);
        Console.WriteLine (string.Join (", ", command.Parameters.Select (p => p.Name + "=" + p.Value)));
        Console.WriteLine (command.GetInMemoryProjection<object>());
      }
      return command;
    }
  }
}