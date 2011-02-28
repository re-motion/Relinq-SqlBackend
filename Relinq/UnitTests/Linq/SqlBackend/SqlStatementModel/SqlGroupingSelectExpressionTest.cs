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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlGroupingSelectExpressionTest
  {
    private SqlGroupingSelectExpression _sqlGroupingSelectExpression;
    private ConstantExpression _aggregateExpression1;
    private ConstantExpression _aggregateExpression2;
    private ConstantExpression _keyExpression;
    private ConstantExpression _elementExpression;

    [SetUp]
    public void SetUp ()
    {
      _keyExpression = Expression.Constant ("key");
      _elementExpression = Expression.Constant ("element");

      _aggregateExpression1 = Expression.Constant ("agg1");
      _aggregateExpression2 = Expression.Constant ("agg2");

      _sqlGroupingSelectExpression = new SqlGroupingSelectExpression (
          _keyExpression, 
          _elementExpression, 
          new [] { _aggregateExpression1 });
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_sqlGroupingSelectExpression.KeyExpression, Is.SameAs (_keyExpression));
      Assert.That (_sqlGroupingSelectExpression.ElementExpression, Is.SameAs (_elementExpression));
      Assert.That (_sqlGroupingSelectExpression.AggregationExpressions.Count, Is.EqualTo (1));
      Assert.That (_sqlGroupingSelectExpression.AggregationExpressions[0], Is.SameAs (_aggregateExpression1));
    }

    [Test]
    public void CreateWithNames ()
    {
      var result = SqlGroupingSelectExpression.CreateWithNames (_keyExpression, _elementExpression);

      var expectedKeyExpression = new NamedExpression ("key", _keyExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedKeyExpression, result.KeyExpression);

      var expectedElementExpression = new NamedExpression ("element", _elementExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedElementExpression, result.ElementExpression);
    }

    [Test]
    public void AddAggregationExpressionWithName ()
    {
      var name = _sqlGroupingSelectExpression.AddAggregationExpressionWithName (_aggregateExpression2);

      Assert.That (_sqlGroupingSelectExpression.AggregationExpressions.Count, Is.EqualTo (2));

      Assert.That (name, Is.EqualTo ("a1"));
      var expectedNamedExpression = new NamedExpression ("a1", _aggregateExpression2);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNamedExpression, _sqlGroupingSelectExpression.AggregationExpressions[1]);
    }

    [Test]
    public void VisitChildren_NoExpressionChanged ()
    {
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      visitorMock.Expect (mock => mock.VisitExpression (_keyExpression)).Return (_keyExpression);
      visitorMock.Expect (mock => mock.VisitExpression (_elementExpression)).Return (_elementExpression);
      visitorMock
          .Expect (mock => mock.VisitAndConvert (Arg<ReadOnlyCollection<Expression>>.Is.Anything, Arg.Is ("VisitChildren")))
          .Return (null)
          .WhenCalled (invocation => invocation.ReturnValue = invocation.Arguments[0]);
      visitorMock.Replay();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_sqlGroupingSelectExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (expression, Is.SameAs (_sqlGroupingSelectExpression));
    }

    [Test]
    public void VisitChildren_ChangeAggregationExpression ()
    {
      var newAggregationExpression = Expression.Constant ("newAgg");
      var expectedAggregationExpressions = new Expression[] { _aggregateExpression1, newAggregationExpression };

      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      visitorMock.Expect (mock => mock.VisitExpression (_keyExpression)).Return (_keyExpression);
      visitorMock.Expect (mock => mock.VisitExpression (_elementExpression)).Return (_elementExpression);
      visitorMock
          .Expect (mock => mock.VisitAndConvert (Arg<ReadOnlyCollection<Expression>>.Is.Anything, Arg.Is ("VisitChildren")))
          .Return (Array.AsReadOnly (expectedAggregationExpressions));
      visitorMock.Replay();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_sqlGroupingSelectExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (expression, Is.Not.SameAs(_sqlGroupingSelectExpression));
      Assert.That (expression, Is.TypeOf (typeof(SqlGroupingSelectExpression)));
      Assert.That (((SqlGroupingSelectExpression) expression).AggregationExpressions[0], Is.SameAs(_aggregateExpression1));
      Assert.That (((SqlGroupingSelectExpression) expression).AggregationExpressions[1], Is.SameAs (newAggregationExpression));
    }

    [Test]
    public void VisitChildren_KeyExpressionChanged ()
    {
      var newKeyExpression = Expression.Constant ("newKey");
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();
      visitorMock.Expect (mock => mock.VisitExpression (_keyExpression)).Return (newKeyExpression);
      visitorMock.Expect (mock => mock.VisitExpression (_elementExpression)).Return (_elementExpression);
      visitorMock
          .Expect (mock => mock.VisitAndConvert (Arg<ReadOnlyCollection<Expression>>.Is.Anything, Arg.Is ("VisitChildren")))
          .Return (null).WhenCalled (invocation => invocation.ReturnValue = invocation.Arguments[0]);
      visitorMock.Replay ();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_sqlGroupingSelectExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (expression, Is.Not.SameAs(_sqlGroupingSelectExpression));
      Assert.That (((SqlGroupingSelectExpression) expression).KeyExpression, Is.SameAs (newKeyExpression));
      Assert.That (((SqlGroupingSelectExpression) expression).ElementExpression, Is.SameAs (_elementExpression));
    }

    [Test]
    public void VisitChildren_ElementExpressionChanged ()
    {
      var newElementExpression = Expression.Constant ("newElement");
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();
      visitorMock.Expect (mock => mock.VisitExpression (_keyExpression)).Return (_keyExpression);
      visitorMock.Expect (mock => mock.VisitExpression (_elementExpression)).Return (newElementExpression);
      visitorMock
          .Expect (mock => mock.VisitAndConvert (Arg<ReadOnlyCollection<Expression>>.Is.Anything, Arg.Is ("VisitChildren")))
          .Return (null)
          .WhenCalled (invocation => invocation.ReturnValue = invocation.Arguments[0]);
      visitorMock.Replay ();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_sqlGroupingSelectExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (expression, Is.Not.SameAs (_sqlGroupingSelectExpression));
      Assert.That (((SqlGroupingSelectExpression) expression).KeyExpression, Is.SameAs (_keyExpression));
      Assert.That (((SqlGroupingSelectExpression) expression).ElementExpression, Is.SameAs (newElementExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlGroupingSelectExpression, ISqlGroupingSelectExpressionVisitor> (
          _sqlGroupingSelectExpression,
          mock => mock.VisitSqlGroupingSelectExpression (_sqlGroupingSelectExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_sqlGroupingSelectExpression);
    }

    [Test]
    public void Update ()
    {
      var newKeyExpression = Expression.Constant ("newKey");
      var newElementExpression = Expression.Constant ("newElement");
      var aggregations = new[] { Expression.Constant ("agg1"), Expression.Constant ("agg2") };

      var result = _sqlGroupingSelectExpression.Update (newKeyExpression, newElementExpression, aggregations);

      Assert.That (result.KeyExpression, Is.SameAs (newKeyExpression));
      Assert.That (result.ElementExpression, Is.SameAs (newElementExpression));
      Assert.That (result.AggregationExpressions.SequenceEqual (aggregations), Is.True);
    }

    [Test]
    public void To_String ()
    {
      _sqlGroupingSelectExpression.AddAggregationExpressionWithName (_aggregateExpression2);
      var result = _sqlGroupingSelectExpression.ToString();

      Assert.That (result, Is.EqualTo ("GROUPING (KEY: \"key\", ELEMENT: \"element\", AGGREGATIONS: (\"agg1\", \"agg2\" AS a1))"));
    }
  }
}