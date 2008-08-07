/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlGeneration;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using Remotion.Development.UnitTesting;

namespace Remotion.Data.UnitTests.Linq.SqlGenerationTest
{
  [TestFixture]
  public class SqlGeneratorBaseTest
  {
    private MockRepository _mockRepository;
    private ISelectBuilder _selectBuilder;
    private IFromBuilder _fromBuilder;
    private IWhereBuilder _whereBuilder;
    private IOrderByBuilder _orderByBuilder;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _selectBuilder = _mockRepository.CreateMock<ISelectBuilder> ();
      _fromBuilder = _mockRepository.CreateMock<IFromBuilder> ();
      _whereBuilder = _mockRepository.CreateMock<IWhereBuilder> ();
      _orderByBuilder = _mockRepository.CreateMock<IOrderByBuilder> ();
      
    }

    [Test]
    public void BuildCommand_CallsPartBuilders()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      SqlGeneratorMock generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseMode.TopLevelQuery);
      
      // expected
      _selectBuilder.BuildSelectPart (generator.Visitor.SqlGenerationData.SelectEvaluation, null);
      _fromBuilder.BuildFromPart (generator.Visitor.SqlGenerationData.FromSources, generator.Visitor.SqlGenerationData.Joins);
      _fromBuilder.BuildLetPart (generator.Visitor.SqlGenerationData.LetEvaluations);
      _whereBuilder.BuildWherePart (generator.Visitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.SqlGenerationData.OrderingFields);

      _mockRepository.ReplayAll();

      CommandData result = generator.BuildCommand (query);
      Assert.IsNotNull (result);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void BuildCommand_ReturnsCommandAndParameters ()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseMode.TopLevelQuery);

      // expected
      _selectBuilder.BuildSelectPart (generator.Visitor.SqlGenerationData.SelectEvaluation, null);
      LastCall.Do ((Action<IEvaluation, List<MethodCall>>) delegate
      {
        generator.Context.CommandText.Append ("Select");
      });

      _fromBuilder.BuildFromPart (generator.Visitor.SqlGenerationData.FromSources, generator.Visitor.SqlGenerationData.Joins);
      LastCall.Do ((Action<List<IColumnSource>, JoinCollection>) delegate
      {
        generator.Context.CommandText.Append ("From");
      });

      _fromBuilder.BuildLetPart (generator.Visitor.SqlGenerationData.LetEvaluations);
      LastCall.Do ((Action<List<LetData>>) delegate
      {
         generator.Context.CommandText.Append ("Let");
       });

      var parameter = new CommandParameter("fritz", "foo");
      _whereBuilder.BuildWherePart (generator.Visitor.SqlGenerationData.Criterion);
      LastCall.Do ((Action<ICriterion>) delegate
      {
        generator.Context.CommandText.Append ("Where");
        generator.Context.CommandParameters.Add (parameter);
      });

      _orderByBuilder.BuildOrderByPart (generator.Visitor.SqlGenerationData.OrderingFields);
      LastCall.Do ((Action<List<OrderingField>>) delegate
      {
        generator.Context.CommandText.Append ("OrderBy");
      });

      _mockRepository.ReplayAll ();

      CommandData result = generator.BuildCommand (query);
      Assert.AreEqual ("SelectFromLetWhereOrderBy", result.Statement);
      Assert.That (result.Parameters, Is.EqualTo (new object[] {parameter}));

      _mockRepository.VerifyAll ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The concrete subclass did not set a select evaluation.")]
    public void BuildCommand_ThrowsOnNullSelect ()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      var generator = _mockRepository.PartialMock<SqlGeneratorMock> (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, 
          _orderByBuilder, ParseMode.TopLevelQuery);

      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (generator, "ProcessQuery", null)).IgnoreArguments ().Return (new SqlGenerationData ());
      _mockRepository.ReplayAll ();

      generator.BuildCommand (query);
    }

    [Test]
    public void ProcessQuery_PassesQueryToVisitor()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseMode.TopLevelQuery);

      // expected
      _selectBuilder.BuildSelectPart (generator.Visitor.SqlGenerationData.SelectEvaluation, null);
      _fromBuilder.BuildFromPart (generator.Visitor.SqlGenerationData.FromSources, generator.Visitor.SqlGenerationData.Joins);
      _fromBuilder.BuildLetPart (generator.Visitor.SqlGenerationData.LetEvaluations);
      _whereBuilder.BuildWherePart (generator.Visitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.SqlGenerationData.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommand (query);
    }

    [Test]
    public void ProcessQuery_WithDifferentParseContext ()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseMode.SubQueryInWhere);

      // expected
      _selectBuilder.BuildSelectPart (generator.Visitor.SqlGenerationData.SelectEvaluation, null);
      _fromBuilder.BuildFromPart (generator.Visitor.SqlGenerationData.FromSources, generator.Visitor.SqlGenerationData.Joins);
      _fromBuilder.BuildLetPart (generator.Visitor.SqlGenerationData.LetEvaluations);
      _whereBuilder.BuildWherePart (generator.Visitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.SqlGenerationData.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommand (query);
    }

    [Test]
    public void ProcessQuery_CreatesAliases()
    {
      IQueryable<Student_Detail> source = ExpressionHelper.CreateQuerySource_Detail();
      QueryModel query = ExpressionHelper.ParseQuery (JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (source));
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseMode.TopLevelQuery);

      // expected
      _selectBuilder.BuildSelectPart (generator.Visitor.SqlGenerationData.SelectEvaluation, null);
      _fromBuilder.BuildFromPart (generator.Visitor.SqlGenerationData.FromSources, generator.Visitor.SqlGenerationData.Joins);
      _fromBuilder.BuildLetPart (generator.Visitor.SqlGenerationData.LetEvaluations);
      _whereBuilder.BuildWherePart (generator.Visitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.SqlGenerationData.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommand (query);
    }

    [Test]
    public void CreateDistinct ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      QueryModel query = ExpressionHelper.ParseQuery (DistinctTestQueryGenerator.CreateSimpleDistinctQuery (source));
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseMode.TopLevelQuery);

      // expected
      _selectBuilder.BuildSelectPart (generator.Visitor.SqlGenerationData.SelectEvaluation, generator.Visitor.SqlGenerationData.ResultModifiers);
      _fromBuilder.BuildFromPart (generator.Visitor.SqlGenerationData.FromSources, generator.Visitor.SqlGenerationData.Joins);
      _fromBuilder.BuildLetPart (generator.Visitor.SqlGenerationData.LetEvaluations);
      _whereBuilder.BuildWherePart (generator.Visitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.SqlGenerationData.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommand (query);
    }

    [Test]
    public void ProcessQuery_ReturnsSqlGenerationData ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      QueryModel query = ExpressionHelper.ParseQuery (DistinctTestQueryGenerator.CreateSimpleDistinctQuery (source));
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseMode.TopLevelQuery);

      // expected
      _selectBuilder.BuildSelectPart (generator.Visitor.SqlGenerationData.SelectEvaluation, generator.Visitor.SqlGenerationData.ResultModifiers);
      _fromBuilder.BuildFromPart (generator.Visitor.SqlGenerationData.FromSources, generator.Visitor.SqlGenerationData.Joins);
      _fromBuilder.BuildLetPart (generator.Visitor.SqlGenerationData.LetEvaluations);
      _whereBuilder.BuildWherePart (generator.Visitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.SqlGenerationData.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      CommandData command = generator.BuildCommand (query);
      Assert.That (command.SqlGenerationData, Is.Not.Null);
      Assert.That (command.SqlGenerationData, Is.SameAs (generator.Visitor.SqlGenerationData));
    }
  }
}
