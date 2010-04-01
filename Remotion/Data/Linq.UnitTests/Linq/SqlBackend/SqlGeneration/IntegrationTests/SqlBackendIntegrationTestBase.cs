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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  public class SqlBackendIntegrationTestBase
  {
    private IQueryable<Cook> _cooks;
    private IQueryable<Kitchen> _kitchens;
    private IQueryable<Restaurant> _restaurants;

    public IQueryable<Cook> Cooks
    {
      get { return _cooks; }
    }

    public IQueryable<Kitchen> Kitchens
    {
      get { return _kitchens; }
    }

    public IQueryable<Restaurant> Restaurants
    {
      get { return _restaurants; }
    }

    [SetUp]
    public virtual void SetUp ()
    {
      _cooks = ExpressionHelper.CreateCookQueryable();
      _kitchens = ExpressionHelper.CreateKitchenQueryable();
      _restaurants = ExpressionHelper.CreateRestaurantQueryable();
    }

    protected SqlCommand GenerateSql (Expression expression)
    {
      var queryModel = ExpressionHelper.ParseQuery (expression);

      var preparationContext = new SqlPreparationContext();
      var uniqueIdentifierGenerator = new UniqueIdentifierGenerator();
      var sqlStatement = SqlPreparationQueryModelVisitor.TransformQueryModel (
          queryModel,
          preparationContext,
          new DefaultSqlPreparationStage (MethodCallTransformerRegistry.CreateDefault2(), preparationContext, uniqueIdentifierGenerator));

      var resolver = new MappingResolverStub();
      var mappingResolutionStage = new DefaultMappingResolutionStage (resolver, uniqueIdentifierGenerator);
      mappingResolutionStage.ResolveSqlStatement (sqlStatement);

      var commandBuilder = new SqlCommandBuilder();
      var sqlGenerationStage = new DefaultSqlGenerationStage(MethodCallTransformerRegistry.CreateDefault());
      sqlGenerationStage.GenerateTextForSqlStatement (commandBuilder, sqlStatement, SqlExpressionContext.ValueRequired);

      return commandBuilder.GetCommand();
    }

    protected void CheckQuery<T> (IQueryable<T> queryable, string expectedStatement, params CommandParameter[] expectedParameters)
    {
      var result = GenerateSql (queryable.Expression);

      Assert.That (result.CommandText, Is.EqualTo (expectedStatement));
      Assert.That (result.Parameters, Is.EqualTo (expectedParameters));
    }

    protected void CheckQuery<T> (Expression<Func<T>> queryLambda, string expectedStatement, params CommandParameter[] expectedParameters)
    {
      var result = GenerateSql (queryLambda.Body);

      Assert.That (result.CommandText, Is.EqualTo (expectedStatement));
      Assert.That (result.Parameters, Is.EqualTo (expectedParameters));
    }
  }
}