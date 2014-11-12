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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests
{
  public class SqlBackendIntegrationTestBase
  {
    private IQueryable<Cook> _cooks;
    private IQueryable<Knife> _knives;
    private IQueryable<Kitchen> _kitchens;
    private IQueryable<Restaurant> _restaurants;
    private IQueryable<Company> _companies;
    private IQueryable<Chef> _chefs;

    public IQueryable<Cook> Cooks
    {
      get { return _cooks; }
    }

    public IQueryable<Knife> Knives
    {
      get { return _knives; }
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
      _cooks = ExpressionHelper.CreateQueryable<Cook>();
      _knives = ExpressionHelper.CreateQueryable<Knife> ();
      _kitchens = ExpressionHelper.CreateQueryable<Kitchen>();
      _restaurants = ExpressionHelper.CreateQueryable<Restaurant>();
      _chefs = ExpressionHelper.CreateQueryable<Chef>();
      _companies = ExpressionHelper.CreateQueryable<Company>();
    }

    protected SqlCommandData GenerateSql (QueryModel queryModel)
    {
      var generator = new UniqueIdentifierGenerator();

      var resultOperatorHandlerRegistry = ResultOperatorHandlerRegistry.CreateDefault();
      var defaultSqlPreparationStage = new DefaultSqlPreparationStage (CompoundMethodCallTransformerProvider.CreateDefault(), resultOperatorHandlerRegistry, generator);
      var sqlStatement = defaultSqlPreparationStage.PrepareSqlStatement (queryModel, null);

      var resolver = new MappingResolverStub();
      var mappingResolutionStage = new DefaultMappingResolutionStage (resolver, generator);
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

    protected void CheckQuery<T> (
        IQueryable<T> queryable,
        string expectedStatement,
        Expression<Func<IDatabaseResultRow, object>> expectedInMemoryProjection,
        bool simplifyInMemoryProjection,
        params CommandParameter[] expectedParameters)
    {
      CheckQuery (queryable.Expression, expectedStatement, expectedInMemoryProjection, simplifyInMemoryProjection, expectedParameters);
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

    protected void CheckQuery (
        Expression queryExpression,
        string expectedStatement,
        Expression<Func<IDatabaseResultRow, object>> expectedInMemoryProjection,
        bool simplify,
        params CommandParameter[] expectedParameters)
    {
      var queryModel = ExpressionHelper.ParseQuery (queryExpression);
      CheckQuery (queryModel, expectedStatement, expectedInMemoryProjection, simplify, expectedParameters);
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
      CheckQuery (queryModel, expectedStatement, expectedInMemoryProjection, true, expectedParameters);
    }

    protected void CheckQuery (
        QueryModel queryModel,
        string expectedStatement,
        Expression<Func<IDatabaseResultRow, object>> expectedInMemoryProjection,
        bool simplifyInMemoryProjection,
        params CommandParameter[] expectedParameters)
    {
      var result = GenerateSql (queryModel);

      try
      {
        Assert.That (result.CommandText, Is.EqualTo (expectedStatement));
      }
      catch
      {
        Console.WriteLine ("Full generated statement: " + result.CommandText);
        Console.WriteLine ("Full expected statement:  " + expectedStatement);
        throw;
      }

      Assert.That (result.Parameters, Is.EqualTo (expectedParameters));

      if (expectedInMemoryProjection != null)
      {
        Expression checkedInMemoryProjection = expectedInMemoryProjection;
        if (simplifyInMemoryProjection)
        {
          checkedInMemoryProjection = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (checkedInMemoryProjection);
          checkedInMemoryProjection = ReplaceConvertExpressionMarker (checkedInMemoryProjection);
        }
        SqlExpressionTreeComparer.CheckAreEqualTrees (checkedInMemoryProjection, result.GetInMemoryProjection<object> ());
      }
    }

    /// <summary>
    /// Denotes that the <paramref name="methodCallResult"/> should be represented as a 
    /// <see cref="Expression.Convert(System.Linq.Expressions.Expression,System.Type,System.Reflection.MethodInfo)"/> expression.
    /// </summary>
    protected T ConvertExpressionMarker<T> (T methodCallResult)
    {
      return methodCallResult;
    }

    private Expression ReplaceConvertExpressionMarker (Expression simplifiedExpectedInMemoryProjection)
    {
      return AdHocExpressionTreeVisitor.Transform(simplifiedExpectedInMemoryProjection, expr =>
      {
        var methodCallExpression = expr as MethodCallExpression;
        if (methodCallExpression != null && methodCallExpression.Method.Name == "ConvertExpressionMarker")
        {
          var conversionMethodCall = methodCallExpression.Arguments.Single() as MethodCallExpression;
          if (conversionMethodCall == null || conversionMethodCall.Object != null || conversionMethodCall.Arguments.Count != 1)
          {
            throw new InvalidOperationException (
                "The argument to ConvertExpressionMarker must be a MethodCallExpression to a static method with a single argument.");
          }
          return Expression.Convert (conversionMethodCall.Arguments.Single(), conversionMethodCall.Type, conversionMethodCall.Method);
        }

        return expr;
      });
    }

    protected class AdHocExpressionTreeVisitor : ExpressionTreeVisitor
    {
      public static Expression Transform (Expression expression, Func<Expression, Expression> transformation)
      {
        return new AdHocExpressionTreeVisitor (transformation).VisitExpression (expression);
      }

      public static T TransformAndRetainType<T> (T expression, Func<Expression, Expression> transformation)
        where T : Expression
      {
        return (T) new AdHocExpressionTreeVisitor (transformation).VisitExpression (expression);
      }

      private readonly Func<Expression, Expression> _transformation;

      public AdHocExpressionTreeVisitor (Func<Expression, Expression> transformation)
      {
        _transformation = transformation;
      }

      public override Expression VisitExpression (Expression expression)
      {
        return _transformation (base.VisitExpression (expression));
      }
    }
  }
}