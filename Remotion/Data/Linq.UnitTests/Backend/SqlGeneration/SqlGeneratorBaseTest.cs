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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;
using Remotion.Data.Linq.UnitTests.TestUtilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratorBaseTest
  {
    private MockRepository _mockRepository;
    private ISelectBuilder _selectBuilderMock;
    private IFromBuilder _fromBuilderMock;
    private IWhereBuilder _whereBuilderMock;
    private IOrderByBuilder _orderByBuilderMock;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _selectBuilderMock = _mockRepository.StrictMock<ISelectBuilder> ();
      _fromBuilderMock = _mockRepository.StrictMock<IFromBuilder> ();
      _whereBuilderMock = _mockRepository.StrictMock<IWhereBuilder> ();
      _orderByBuilderMock = _mockRepository.StrictMock<IOrderByBuilder> ();
    }

    [Test]
    public void BuildCommand_CallsPartBuilders()
    {
      var query = ExpressionHelper.CreateQueryModel_Student ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilderMock, _fromBuilderMock, _whereBuilderMock, _orderByBuilderMock, ParseMode.TopLevelQuery);

      using (_mockRepository.Ordered ())
      {
        _selectBuilderMock.Expect (mock => mock.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData));
        _fromBuilderMock.Expect (mock => mock.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData));
        _whereBuilderMock.Expect (mock => mock.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData));
        _orderByBuilderMock.Expect (mock => mock.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData));
      }

      _mockRepository.ReplayAll();

      CommandData result = generator.BuildCommand (query);
      Assert.IsNotNull (result);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void BuildCommand_ReturnsCommandAndParameters ()
    {
      var query = ExpressionHelper.CreateQueryModel_Student ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilderMock, _fromBuilderMock, _whereBuilderMock, _orderByBuilderMock, ParseMode.TopLevelQuery);

      _selectBuilderMock
          .Expect (mock => mock.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData))
          .WhenCalled (mi =>generator.Context.CommandText.Append ("Select"));

      _fromBuilderMock
          .Expect (mock => mock.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData))
          .WhenCalled (mi => generator.Context.CommandText.Append ("From"));

      var parameter = new CommandParameter ("fritz", "foo");
      _whereBuilderMock
          .Expect (mock => mock.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData))
          .WhenCalled (mi => {
              generator.Context.CommandText.Append ("Where");
              generator.Context.CommandParameters.Add (parameter);
          });

      _orderByBuilderMock
          .Expect (mock => mock.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData))
          .WhenCalled (mi => generator.Context.CommandText.Append ("OrderBy"));

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
      var query = ExpressionHelper.CreateQueryModel_Student ();
      var generatorPartialMock = _mockRepository.PartialMock<SqlGeneratorMock> (query, StubDatabaseInfo.Instance, _selectBuilderMock, _fromBuilderMock, _whereBuilderMock, 
          _orderByBuilderMock, ParseMode.TopLevelQuery);

      generatorPartialMock
          .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "ProcessQuery", null))
          .IgnoreArguments ()
          .Return (new SqlGenerationData ());
      _mockRepository.ReplayAll ();

      generatorPartialMock.BuildCommand (query);
    }

    [Test]
    public void ProcessQuery_PassesQueryToVisitor()
    {
      var query = ExpressionHelper.CreateQueryModel_Student ();
      var generator = new SqlGeneratorMock (
          query, 
          StubDatabaseInfo.Instance, 
          _selectBuilderMock, 
          _fromBuilderMock, 
          _whereBuilderMock, 
          _orderByBuilderMock, 
          ParseMode.TopLevelQuery);

      SetupDefaultBuilderExpectations (generator);

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommand (query);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void ProcessQuery_WithDifferentParseContext ()
    {
      var query = ExpressionHelper.CreateQueryModel_Student ();
      var generator = new SqlGeneratorMock (
          query, 
          StubDatabaseInfo.Instance, 
          _selectBuilderMock, 
          _fromBuilderMock, 
          _whereBuilderMock, 
          _orderByBuilderMock, 
          ParseMode.SubQueryInWhere);

      SetupDefaultBuilderExpectations (generator);

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommand (query);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void ProcessQuery_CreatesAliases()
    {
      IQueryable<Kitchen> source = ExpressionHelper.CreateKitchenQueryable();
      QueryModel query = ExpressionHelper.ParseQuery (JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (source));
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilderMock, _fromBuilderMock, _whereBuilderMock, _orderByBuilderMock, ParseMode.TopLevelQuery);

      SetupDefaultBuilderExpectations(generator);
      
      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommand (query);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void ProcessQuery_ReturnsSqlGenerationData ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateCookQueryable ();
      QueryModel query = ExpressionHelper.ParseQuery (DistinctTestQueryGenerator.CreateSimpleDistinctQuery (source));
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilderMock, _fromBuilderMock, _whereBuilderMock, _orderByBuilderMock, ParseMode.TopLevelQuery);

      SetupDefaultBuilderExpectations (generator);

      generator.CheckBaseProcessQueryMethod = true;

      var command = generator.BuildCommand (query);
      
      Assert.That (command.SqlGenerationData, Is.Not.Null);
      Assert.That (command.SqlGenerationData, Is.SameAs (generator.ReferenceVisitor.SqlGenerationData));
    }

    private void SetupDefaultBuilderExpectations (SqlGeneratorMock generator)
    {
      _selectBuilderMock.Expect (mock => mock.BuildSelectPart (generator.ReferenceVisitor.SqlGenerationData));
      _fromBuilderMock.Expect (mock => mock.BuildFromPart (generator.ReferenceVisitor.SqlGenerationData));
      _whereBuilderMock.Expect (mock => mock.BuildWherePart (generator.ReferenceVisitor.SqlGenerationData));
      _orderByBuilderMock.Expect (mock => mock.BuildOrderByPart (generator.ReferenceVisitor.SqlGenerationData));

      _selectBuilderMock.Replay ();
      _fromBuilderMock.Replay ();
      _whereBuilderMock.Replay ();
      _orderByBuilderMock.Replay ();
    }
  }
}
