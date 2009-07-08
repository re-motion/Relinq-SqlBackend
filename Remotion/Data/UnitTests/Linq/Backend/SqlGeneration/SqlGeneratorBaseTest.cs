// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Clauses;
using Rhino.Mocks;
using Remotion.Data.Linq.Backend.DataObjectModel;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using Remotion.Development.UnitTesting;

namespace Remotion.Data.UnitTests.Linq.Backend.SqlGeneration
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
      _selectBuilder = _mockRepository.StrictMock<ISelectBuilder> ();
      _fromBuilder = _mockRepository.StrictMock<IFromBuilder> ();
      _whereBuilder = _mockRepository.StrictMock<IWhereBuilder> ();
      _orderByBuilder = _mockRepository.StrictMock<IOrderByBuilder> ();
      
    }

    [Test]
    public void BuildCommand_CallsPartBuilders()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseMode.TopLevelQuery);

      var operators = new List<ResultOperatorBase>();
      
      // expected
      _selectBuilder.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData.SelectEvaluation, operators);


      _fromBuilder.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData.FromSources, generator.ReferenceVisitor.SqlGenerationData.Joins);
      _whereBuilder.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData.OrderingFields);

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
      var operators = new List<ResultOperatorBase> ();
      // expected
      _selectBuilder.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData.SelectEvaluation, operators);
      LastCall.Do ((Action<IEvaluation, List<ResultOperatorBase>>) delegate
      {
        generator.Context.CommandText.Append ("Select");
      });

      _fromBuilder.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData.FromSources, generator.ReferenceVisitor.SqlGenerationData.Joins);
      LastCall.Do ((Action<List<IColumnSource>, JoinCollection>) delegate
      {
        generator.Context.CommandText.Append ("From");
      });

      var parameter = new CommandParameter("fritz", "foo");
      _whereBuilder.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData.Criterion);
      LastCall.Do ((Action<ICriterion>) delegate
      {
        generator.Context.CommandText.Append ("Where");
        generator.Context.CommandParameters.Add (parameter);
      });

      _orderByBuilder.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData.OrderingFields);
      LastCall.Do ((Action<List<OrderingField>>) delegate
      {
        generator.Context.CommandText.Append ("OrderBy");
      });

      _mockRepository.ReplayAll ();

      CommandData result = generator.BuildCommand (query);
      Assert.AreEqual ("SelectFromWhereOrderBy", result.Statement);
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
      var operators = new List<ResultOperatorBase> ();

      // expected
      _selectBuilder.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData.SelectEvaluation, operators);
      _fromBuilder.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData.FromSources, generator.ReferenceVisitor.SqlGenerationData.Joins);
      _whereBuilder.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommand (query);
    }

    [Test]
    public void ProcessQuery_WithDifferentParseContext ()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseMode.SubQueryInWhere);
      var operators = new List<ResultOperatorBase> ();

      // expected
      _selectBuilder.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData.SelectEvaluation, operators);
      _fromBuilder.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData.FromSources, generator.ReferenceVisitor.SqlGenerationData.Joins);
      _whereBuilder.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData.OrderingFields);

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
      var operators = new List<ResultOperatorBase> ();

      // expected
      _selectBuilder.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData.SelectEvaluation, operators);
      _fromBuilder.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData.FromSources, generator.ReferenceVisitor.SqlGenerationData.Joins);
      _whereBuilder.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData.OrderingFields);

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
      _selectBuilder.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData.SelectEvaluation, generator.ReferenceVisitor.SqlGenerationData.ResultOperators);
      _fromBuilder.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData.FromSources, generator.ReferenceVisitor.SqlGenerationData.Joins);
      _whereBuilder.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData.OrderingFields);

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
      _selectBuilder.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData.SelectEvaluation, generator.ReferenceVisitor.SqlGenerationData.ResultOperators);
      _fromBuilder.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData.FromSources, generator.ReferenceVisitor.SqlGenerationData.Joins);
      _whereBuilder.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      CommandData command = generator.BuildCommand (query);
      Assert.That (command.SqlGenerationData, Is.Not.Null);
      Assert.That (command.SqlGenerationData, Is.SameAs (generator.ReferenceVisitor.SqlGenerationData));
    }
  }
}
