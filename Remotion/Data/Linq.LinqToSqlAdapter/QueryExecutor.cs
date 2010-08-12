// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  public class QueryExecutor : IQueryExecutor
  {
    private readonly IQueryResultRetriever _resultRetriever;
    private readonly IMappingResolver _mappingResolver;

    public QueryExecutor (IQueryResultRetriever resultRetriever, IMappingResolver mappingResolver)
    {
      ArgumentUtility.CheckNotNull ("resultRetriever", resultRetriever);
      ArgumentUtility.CheckNotNull ("mappingResolver", mappingResolver);

      _resultRetriever = resultRetriever;
      _mappingResolver = mappingResolver;
    }

    public T ExecuteScalar<T> (QueryModel queryModel)
    {
      SqlCommandData commandData = GenerateSqlCommand (queryModel);
      return _resultRetriever.GetScalar<T> (commandData.CommandText, commandData.Parameters);
    }

    public T ExecuteSingle<T> (QueryModel queryModel, bool returnDefaultWhenEmpty)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var sequence = ExecuteCollection<T> (queryModel);

      if (returnDefaultWhenEmpty)
        return sequence.SingleOrDefault ();
      else
        return sequence.Single ();
    }

    public IEnumerable<T> ExecuteCollection<T> (QueryModel queryModel)
    {
      SqlCommandData commandData = GenerateSqlCommand (queryModel);
      Func<IDatabaseResultRow, T> projection = commandData.GetInMemoryProjection<T> ().Compile ();
      return _resultRetriever.GetResults (projection, commandData.CommandText, commandData.Parameters);
    }

    private SqlCommandData GenerateSqlCommand(QueryModel queryModel)
    {
      var methodCallTransformerRegistry = MethodCallTransformerRegistry.CreateDefault ();
      var resultOperatorHandlerRegistry = ResultOperatorHandlerRegistry.CreateDefault ();

      ISqlPreparationContext preparationContext = new SqlPreparationContext ();
      IMappingResolutionContext mappingResolutionContext = new MappingResolutionContext ();

      var generator = new UniqueIdentifierGenerator ();
      var preparationStage = new DefaultSqlPreparationStage (methodCallTransformerRegistry, resultOperatorHandlerRegistry, generator);
      var preparedStatement = preparationStage.PrepareSqlStatement (queryModel, preparationContext);

      var resolutionStage = new DefaultMappingResolutionStage (_mappingResolver, generator);
      var resolvedStatement = resolutionStage.ResolveSqlStatement (preparedStatement, mappingResolutionContext);

      var builder = new SqlCommandBuilder ();
      var generationStage = new DefaultSqlGenerationStage ();
      generationStage.GenerateTextForOuterSqlStatement (builder, resolvedStatement);

      var command = builder.GetCommand ();
      // TODO: Add a flag to QueryExecutor's ctor allowing to dump the SQL statement.
      //Console.WriteLine (command.CommandText);
      //Console.WriteLine (SeparatedStringBuilder.Build (", ", command.Parameters.Select (p => p.Name + "=" + p.Value)));
      return command;
    }
  }
}