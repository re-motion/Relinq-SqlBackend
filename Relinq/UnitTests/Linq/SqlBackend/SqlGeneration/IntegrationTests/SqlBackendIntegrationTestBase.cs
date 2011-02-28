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
using Remotion.Linq.UnitTests.Linq.Core;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlPreparation;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  public class SqlBackendIntegrationTestBase
  {
    private IQueryable<Cook> _cooks;
    private IQueryable<Kitchen> _kitchens;
    private IQueryable<Restaurant> _restaurants;
    private IQueryable<Company> _companies;
    private UniqueIdentifierGenerator _generator;
    private IQueryable<Chef> _chefs;

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

    public IQueryable<Company> Companies
    {
      get { return _companies; }
    }

    public IQueryable<Chef> Chefs
    {
      get { return _chefs; }
    }

    [SetUp]
    public virtual void SetUp ()
    {
      _cooks = ExpressionHelper.CreateCookQueryable();
      _kitchens = ExpressionHelper.CreateKitchenQueryable();
      _restaurants = ExpressionHelper.CreateRestaurantQueryable();
      _chefs = ExpressionHelper.CreateChefQueryable();
      _companies = ExpressionHelper.CreateCompanyQueryable ();

      _generator = new UniqueIdentifierGenerator();
    }

    protected SqlCommandData GenerateSql (QueryModel queryModel)
    {
      var preparationContext = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
      var uniqueIdentifierGenerator = new UniqueIdentifierGenerator();
      var resultOperatorHandlerRegistry = ResultOperatorHandlerRegistry.CreateDefault();
      var sqlStatement = SqlPreparationQueryModelVisitor.TransformQueryModel (
          queryModel,
          preparationContext,
          new DefaultSqlPreparationStage (CompoundMethodCallTransformerProvider.CreateDefault(), resultOperatorHandlerRegistry, uniqueIdentifierGenerator),
          _generator,
          resultOperatorHandlerRegistry);

      var resolver = new MappingResolverStub();
      var mappingResolutionStage = new DefaultMappingResolutionStage (resolver, uniqueIdentifierGenerator);
      var mappingResolutionContext = new MappingResolutionContext();
      var newSqlStatement = mappingResolutionStage.ResolveSqlStatement (sqlStatement, mappingResolutionContext);

      var commandBuilder = new SqlCommandBuilder();
      var sqlGenerationStage = new DefaultSqlGenerationStage();
      sqlGenerationStage.GenerateTextForOuterSqlStatement (commandBuilder, newSqlStatement);

      return commandBuilder.GetCommand();
    }

    protected void CheckQuery<T> (IQueryable<T> queryable, string expectedStatement, params CommandParameter[] expectedParameters)
    {
      CheckQuery (queryable, expectedStatement, null, expectedParameters);
    }

    protected void CheckQuery<T> (
        IQueryable<T> queryable,
        string expectedStatement,
        Expression<Func<IDatabaseResultRow, object>> expectedInMemoryProjection,
        params CommandParameter[] expectedParameters)
    {
      CheckQuery (queryable.Expression, expectedStatement, expectedInMemoryProjection, expectedParameters);
    }

    protected void CheckQuery<T> (Expression<Func<T>> queryLambda, string expectedStatement, params CommandParameter[] expectedParameters)
    {
      CheckQuery (queryLambda, expectedStatement, null, expectedParameters);
    }

    protected void CheckQuery<T> (
        Expression<Func<T>> queryLambda,
        string expectedStatement,
        Expression<Func<IDatabaseResultRow, object>> expectedInMemoryProjection,
        params CommandParameter[] expectedParameters)
    {
      CheckQuery (queryLambda.Body, expectedStatement, expectedInMemoryProjection, expectedParameters);
    }

    protected void CheckQuery (
        Expression queryExpression,
        string expectedStatement,
        Expression<Func<IDatabaseResultRow, object>> expectedInMemoryProjection,
        params CommandParameter[] expectedParameters)
    {
      var queryModel = ExpressionHelper.ParseQuery (queryExpression);
      CheckQuery (queryModel, expectedStatement, expectedInMemoryProjection, expectedParameters);
    }

    protected void CheckQuery (QueryModel queryModel, string expectedStatement, params CommandParameter[] expectedParameters)
    {
      CheckQuery (queryModel, expectedStatement, null, expectedParameters);
    }

    protected void CheckQuery (
        QueryModel queryModel,
        string expectedStatement,
        Expression<Func<IDatabaseResultRow, object>> expectedInMemoryProjection,
        params CommandParameter[] expectedParameters)
    {
      var result = GenerateSql (queryModel);

      Assert.That (result.CommandText, Is.EqualTo (expectedStatement), "Full generated statement: " + result.CommandText);
      Assert.That (result.Parameters, Is.EqualTo (expectedParameters));

      //TODO RM-3709: remove
      //if (result.Parameters.Count () > 0)
      //{
      //  Console.WriteLine (
      //    "command = connection.CreateCommand(); command.CommandText = \"{0}\"; foreach (var parameterItem in new[] {{ {1} }}) command.Parameters.Add (new NpgsqlParameter (parameterItem.Key, parameterItem.Value)); using (command.ExecuteReader()){{}}",
      //    result.CommandText.Replace ("[", "\\\"").Replace ("]", "\\\""),
      //    SeparatedStringBuilder.Build (", ", result.Parameters.Select (p => "new KeyValuePair<string, object> (\"" + p.Name + "\", " + GetValueConstant (p.Value) + ")")));
      //}
      //else
      //{
      //  Console.WriteLine (
      //    "command = connection.CreateCommand(); command.CommandText = \"{0}\"; using (command.ExecuteReader()){{}}",
      //    result.CommandText.Replace ("[", "\\\"").Replace ("]", "\\\""));
      //}
      
      if (expectedInMemoryProjection != null)
      {
        var simplifiedExpectedInMemoryProjection = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (expectedInMemoryProjection);
        ExpressionTreeComparer.CheckAreEqualTrees (simplifiedExpectedInMemoryProjection, result.GetInMemoryProjection<object>());
      }
    }

    private string GetValueConstant (object value)
    {
      if (value is string)
        return "\"" + value + "\"";
      else if (value is char)
        return "'" + value + "'";
      else if (value is double)
        return "(double)" + value;
      else
        return value.ToString ();
    }
  }
}