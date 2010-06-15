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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlGroupingSelectExpressionTest
  {
    private SqlGroupingSelectExpression _sqlGroupingExpression;
    private ConstantExpression _aggregateExpression1;
    private ConstantExpression _aggregateExpression2;
    private ReadOnlyCollection<Expression> _originalAggregationExpressions;
    private ConstantExpression _keyExpression;
    private ConstantExpression _elementExpression;

    [SetUp]
    public void SetUp ()
    {
      _keyExpression = Expression.Constant ("key");
      _elementExpression = Expression.Constant ("element");
      _sqlGroupingExpression = new SqlGroupingSelectExpression (_keyExpression, _elementExpression);
      _aggregateExpression1 = Expression.Constant ("agg1");
      _aggregateExpression2 = Expression.Constant ("agg2");
      _sqlGroupingExpression.AddAggregationExpression (_aggregateExpression1);
      _sqlGroupingExpression.AddAggregationExpression (_aggregateExpression2);
      _originalAggregationExpressions = _sqlGroupingExpression.AggregationExpressions;
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_sqlGroupingExpression.KeyExpression, Is.SameAs (_keyExpression));
      Assert.That (_sqlGroupingExpression.ElementExpression, Is.SameAs (_elementExpression));
      Assert.That (_sqlGroupingExpression.AggregationExpressions.Count, Is.EqualTo (2));
      Assert.That (_sqlGroupingExpression.AggregationExpressions[0], Is.SameAs (_aggregateExpression1));
      Assert.That (_sqlGroupingExpression.AggregationExpressions[1], Is.SameAs (_aggregateExpression2));
    }

    [Test]
    public void VisitChildren_NoExpressionChanged ()
    {
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      visitorMock.Expect (mock => mock.VisitExpression (_keyExpression)).Return (_keyExpression);
      visitorMock.Expect (mock => mock.VisitExpression (_elementExpression)).Return (_elementExpression);
      visitorMock.Expect (mock => mock.VisitAndConvert (Arg<ReadOnlyCollection<Expression>>.Is.Anything, Arg.Is ("VisitChildren"))).Return (null).
          WhenCalled (invocation => invocation.ReturnValue = invocation.Arguments[0]);
      visitorMock.Replay();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_sqlGroupingExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (expression, Is.SameAs (_sqlGroupingExpression));
    }

    [Test]
    public void VisitChildren_ChangeAggregationExpression ()
    {
      var newAggregationExpression = Expression.Constant ("newAgg");
      var expectedAggregationExpressions = new Expression[] { _aggregateExpression1, newAggregationExpression };

      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      visitorMock.Expect (mock => mock.VisitExpression (_keyExpression)).Return (_keyExpression);
      visitorMock.Expect (mock => mock.VisitExpression (_elementExpression)).Return (_elementExpression);
      visitorMock.Expect (mock => mock.VisitAndConvert (Arg<ReadOnlyCollection<Expression>>.Is.Anything, Arg.Is ("VisitChildren"))).Return (
          Array.AsReadOnly (expectedAggregationExpressions));
      visitorMock.Replay();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_sqlGroupingExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (expression, Is.Not.SameAs(_sqlGroupingExpression));
      Assert.That (expression, Is.TypeOf(typeof(SqlGroupingSelectExpression)));
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
      visitorMock.Expect (mock => mock.VisitAndConvert (Arg<ReadOnlyCollection<Expression>>.Is.Anything, Arg.Is ("VisitChildren"))).Return (null).
          WhenCalled (invocation => invocation.ReturnValue = invocation.Arguments[0]);
      visitorMock.Replay ();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_sqlGroupingExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (expression, Is.Not.SameAs(_sqlGroupingExpression));
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
      visitorMock.Expect (mock => mock.VisitAndConvert (Arg<ReadOnlyCollection<Expression>>.Is.Anything, Arg.Is ("VisitChildren"))).Return (null).
          WhenCalled (invocation => invocation.ReturnValue = invocation.Arguments[0]);
      visitorMock.Replay ();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_sqlGroupingExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (expression, Is.Not.SameAs (_sqlGroupingExpression));
      Assert.That (((SqlGroupingSelectExpression) expression).KeyExpression, Is.SameAs (_keyExpression));
      Assert.That (((SqlGroupingSelectExpression) expression).ElementExpression, Is.SameAs (newElementExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlGroupingSelectExpression, ISqlGroupingSelectExpressionVisitor> (
          _sqlGroupingExpression,
          mock => mock.VisitSqlGroupingSelectExpression (_sqlGroupingExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_sqlGroupingExpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _sqlGroupingExpression.ToString();

      Assert.That (result, Is.EqualTo ("GROUP BY \"key\".\"element\""));
    }
  }
}