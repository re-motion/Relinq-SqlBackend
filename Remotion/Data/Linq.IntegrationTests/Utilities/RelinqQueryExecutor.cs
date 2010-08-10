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
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.Utilities;
using SqlCommandBuilder = Remotion.Data.Linq.SqlBackend.SqlGeneration.SqlCommandBuilder;

namespace Remotion.Data.Linq.IntegrationTests.Utilities
{
  public class RelinqQueryExecutor:IQueryExecutor
  {
    private readonly IQueryResultRetriever _resultRetriever;
    private readonly IMappingResolver _mappingResolver;

    public RelinqQueryExecutor (IQueryResultRetriever resultRetriever, IMappingResolver mappingResolver)
    {
      ArgumentUtility.CheckNotNull ("resultRetriever", resultRetriever);
      ArgumentUtility.CheckNotNull ("mappingResolver", mappingResolver);

      _resultRetriever = resultRetriever;
      _mappingResolver = mappingResolver;
    }

    public T ExecuteScalar<T> (QueryModel queryModel)
    {
      throw new NotImplementedException();
    }

    public T ExecuteSingle<T> (QueryModel queryModel, bool returnDefaultWhenEmpty)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<T> ExecuteCollection<T> (QueryModel queryModel)
    {
      var methodCallTransformerRegistry = MethodCallTransformerRegistry.CreateDefault();
      var resultOperatorHandlerRegistry = ResultOperatorHandlerRegistry.CreateDefault();

      ISqlPreparationContext preparationContext = new SqlPreparationContext();
      IMappingResolutionContext mappingResolutionContext = new MappingResolutionContext();

      var generator = new UniqueIdentifierGenerator();
      var preparationStage = new DefaultSqlPreparationStage (methodCallTransformerRegistry, resultOperatorHandlerRegistry, generator);
      var preparedStatement = preparationStage.PrepareSqlStatement (queryModel, preparationContext);

      var resolutionStage = new DefaultMappingResolutionStage (_mappingResolver, generator);
      var resolvedStatement = resolutionStage.ResolveSqlStatement (preparedStatement, mappingResolutionContext);

      var builder = new SqlCommandBuilder();
      var generationStage = new DefaultSqlGenerationStage();
      generationStage.GenerateTextForOuterSqlStatement (builder, resolvedStatement);

      SqlCommandData commandData = builder.GetCommand();

      Func<IDatabaseResultRow, T> projection = commandData.GetInMemoryProjection<T>().Compile();
      return _resultRetriever.GetResults (projection, commandData.CommandText, commandData.Parameters);
    }
  }
}