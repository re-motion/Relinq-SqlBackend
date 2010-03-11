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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Clauses.Expressions;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlCaseExpressionTest
  {
    private SqlCaseExpression _caseExpression;

    [SetUp]
    public void SetUp ()
    {
      _caseExpression = new SqlCaseExpression (Expression.Constant (true), Expression.Constant (1), Expression.Constant (0));
    }

    [Test]
    public void Initialization_TypeComesFromThen ()
    {
      Assert.That (_caseExpression.Type, Is.SameAs (typeof (int)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "The test predicate must have boolean type.\r\nParameter name: testPredicate")]
    public void Initialization_ChecksPredicateType ()
    {
      new SqlCaseExpression (Expression.Constant (0), Expression.Constant (1), Expression.Constant (0));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "'Then' value and 'Else' value must have the same type.\r\nParameter name: thenValue")]
    public void Initialization_ChecksValueConsistency ()
    {
      new SqlCaseExpression (Expression.Constant (true), Expression.Constant (1), Expression.Constant ("0"));
    }

    [Test]
    public void VisitChildren_VisitsTest_AndThenValue_AndElseValue_WithoutChanges ()
    {
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.TestPredicate)).Return (_caseExpression.TestPredicate);
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.ThenValue)).Return (_caseExpression.ThenValue);
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.ElseValue)).Return (_caseExpression.ElseValue);
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_caseExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_caseExpression));
    }

    [Test]
    public void VisitChildren_TestPredicateChanged ()
    {
      var newTestPredicate = Expression.Constant (false);

      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.TestPredicate)).Return (newTestPredicate);
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.ThenValue)).Return (_caseExpression.ThenValue);
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.ElseValue)).Return (_caseExpression.ElseValue);
      visitorMock.Replay ();

      var result = (SqlCaseExpression) ExtensionExpressionTestHelper.CallVisitChildren (_caseExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_caseExpression));
      Assert.That (result.TestPredicate, Is.SameAs (newTestPredicate));
      Assert.That (result.ThenValue, Is.SameAs (_caseExpression.ThenValue));
      Assert.That (result.ElseValue, Is.SameAs (_caseExpression.ElseValue));
    }

    [Test]
    public void VisitChildren_ThenValueChanged ()
    {
      var newThenValue = Expression.Constant (2);

      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.TestPredicate)).Return (_caseExpression.TestPredicate);
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.ThenValue)).Return (newThenValue);
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.ElseValue)).Return (_caseExpression.ElseValue);
      visitorMock.Replay ();

      var result = (SqlCaseExpression) ExtensionExpressionTestHelper.CallVisitChildren (_caseExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_caseExpression));
      Assert.That (result.TestPredicate, Is.SameAs (_caseExpression.TestPredicate));
      Assert.That (result.ThenValue, Is.SameAs (newThenValue));
      Assert.That (result.ElseValue, Is.SameAs (_caseExpression.ElseValue));
    }

    [Test]
    public void VisitChildren_ElseValueChanged ()
    {
      var newElseValue = Expression.Constant (2);

      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.TestPredicate)).Return (_caseExpression.TestPredicate);
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.ThenValue)).Return (_caseExpression.ThenValue);
      visitorMock.Expect (mock => mock.VisitExpression (_caseExpression.ElseValue)).Return (newElseValue);
      visitorMock.Replay ();

      var result = (SqlCaseExpression) ExtensionExpressionTestHelper.CallVisitChildren (_caseExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_caseExpression));
      Assert.That (result.TestPredicate, Is.SameAs (_caseExpression.TestPredicate));
      Assert.That (result.ThenValue, Is.SameAs (_caseExpression.ThenValue));
      Assert.That (result.ElseValue, Is.SameAs (newElseValue));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlCaseExpression, ISqlSpecificExpressionVisitor> (
          _caseExpression,
          mock => mock.VisitSqlCaseExpressionExpression (_caseExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_caseExpression);
    }
  }
}