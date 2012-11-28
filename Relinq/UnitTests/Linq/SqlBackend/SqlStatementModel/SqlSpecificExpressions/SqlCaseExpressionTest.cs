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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlCaseExpressionTest
  {
    private ConstantExpression _predicate1;
    private ConstantExpression _predicate2;

    private Expression _value1;
    private Expression _value2;
    private Expression _nullableValue1;
    private Expression _nullableValue2;
    private Expression _elseValue;

    private SqlCaseExpression _caseExpressionWithElse;
    private SqlCaseExpression _caseExpressionWithoutElse;

    [SetUp]
    public void SetUp ()
    {
      _predicate1 = Expression.Constant (true);
      _predicate2 = Expression.Constant (false, typeof (bool?));
      _value1 = Expression.Constant (17);
      _value2 = Expression.Constant (4);
      _nullableValue1 = Expression.Constant (47, typeof (int?));
      _nullableValue2 = Expression.Constant (11, typeof (int?));
      _elseValue = Expression.Constant (42);

      _caseExpressionWithElse = new SqlCaseExpression (
          typeof (int),
          new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, _value1), new SqlCaseExpression.CaseWhenPair (_predicate2, _value2) },
          _elseValue);
      _caseExpressionWithoutElse = new SqlCaseExpression (
          typeof (int?),
          new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, _nullableValue1), new SqlCaseExpression.CaseWhenPair (_predicate2, _nullableValue2) },
          null);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_caseExpressionWithElse.Cases[0].When, Is.SameAs (_predicate1));
      Assert.That (_caseExpressionWithElse.Cases[0].Then, Is.SameAs (_value1));
      Assert.That (_caseExpressionWithElse.Cases[1].When, Is.SameAs (_predicate2));
      Assert.That (_caseExpressionWithElse.Cases[1].Then, Is.SameAs (_value2));
      Assert.That (_caseExpressionWithElse.ElseCase, Is.SameAs (_elseValue));
      Assert.That (_caseExpressionWithElse.Type, Is.SameAs (typeof (int)));

      Assert.That (_caseExpressionWithoutElse.ElseCase, Is.Null);
      Assert.That (_caseExpressionWithoutElse.Type, Is.SameAs (typeof (int?)));
    }

    [Test]
    public void Initialization_NoElseWithNonNullableType ()
    {
      Assert.That (
          () => new SqlCaseExpression (typeof (int), _caseExpressionWithElse.Cases, null),
          Throws.ArgumentException.With.Message.EqualTo (
              "When no ELSE case is given, the expression's result type must be nullable.\r\nParameter name: type"));

      Assert.That (() => new SqlCaseExpression (typeof (int?), _caseExpressionWithoutElse.Cases, null), Throws.Nothing);
      Assert.That (
          () => new SqlCaseExpression (typeof (string), new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, Expression.Constant ("x")) }, null),
          Throws.Nothing);
    }

    [Test]
    public void Initialization_NonMatchingTypes ()
    {
      Assert.That (
          () => new SqlCaseExpression.CaseWhenPair (_value1, _value1),
          Throws.ArgumentException.With.Message.EqualTo (
              "The WHEN expression's type must be boolean.\r\nParameter name: when"));
      Assert.That (
          () => new SqlCaseExpression (typeof (int), new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, Expression.Constant ("x")) }, _elseValue),
          Throws.ArgumentException.With.Message.EqualTo (
              "The THEN expressions' types must match the expression type.\r\nParameter name: cases"));
      Assert.That (
          () => new SqlCaseExpression (typeof (int), new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, _value1) }, Expression.Constant ("a")),
          Throws.ArgumentException.With.Message.EqualTo (
              "The ELSE expression's type must match the expression type.\r\nParameter name: elseCase"));
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate1))
          .Return (_predicate1);
      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.VisitExpression (_value1))
          .Return (_value1);
      visitorMock
        .Expect (mock => mock.VisitExpression (_value2))
        .Return (_value2);
      visitorMock
        .Expect (mock => mock.VisitExpression (_elseValue))
        .Return (_elseValue);
      
      var result = ExtensionExpressionTestHelper.CallVisitChildren (_caseExpressionWithElse, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_caseExpressionWithElse));
    }

    [Test]
    public void VisitChildren_NoElse ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate1))
          .Return (_predicate1);
      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.VisitExpression (_nullableValue1))
          .Return (_nullableValue1);
      visitorMock
        .Expect (mock => mock.VisitExpression (_nullableValue2))
        .Return (_nullableValue2);

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_caseExpressionWithoutElse, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_caseExpressionWithoutElse));
    }

    [Test]
    public void VisitChildren_ChangedWhen ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      var newPredicate = Expression.Constant (true);

      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate1))
          .Return (newPredicate);
      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.VisitExpression (_value1))
          .Return (_value1);
      visitorMock
        .Expect (mock => mock.VisitExpression (_value2))
        .Return (_value2);
      visitorMock
        .Expect (mock => mock.VisitExpression (_elseValue))
        .Return (_elseValue);

      var result = (SqlCaseExpression) ExtensionExpressionTestHelper.CallVisitChildren (_caseExpressionWithElse, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_caseExpressionWithElse));
      Assert.That (result.Type, Is.SameAs (_caseExpressionWithElse.Type));
      Assert.That (result.Cases[0].When, Is.SameAs (newPredicate));
      Assert.That (result.Cases[0].Then, Is.SameAs (_value1));
      Assert.That (result.Cases[1].When, Is.SameAs (_predicate2));
      Assert.That (result.Cases[1].Then, Is.SameAs (_value2));
      Assert.That (result.ElseCase, Is.SameAs (_elseValue));
    }

    [Test]
    public void VisitChildren_ChangedThen ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      var newValue = Expression.Constant (17);

      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate1))
          .Return (_predicate1);
      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.VisitExpression (_value1))
          .Return (newValue);
      visitorMock
        .Expect (mock => mock.VisitExpression (_value2))
        .Return (_value2);
      visitorMock
        .Expect (mock => mock.VisitExpression (_elseValue))
        .Return (_elseValue);

      var result = (SqlCaseExpression) ExtensionExpressionTestHelper.CallVisitChildren (_caseExpressionWithElse, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_caseExpressionWithElse));
      Assert.That (result.Type, Is.SameAs (_caseExpressionWithElse.Type));
      Assert.That (result.Cases[0].When, Is.SameAs (_predicate1));
      Assert.That (result.Cases[0].Then, Is.SameAs (newValue));
      Assert.That (result.Cases[1].When, Is.SameAs (_predicate2));
      Assert.That (result.Cases[1].Then, Is.SameAs (_value2));
      Assert.That (result.ElseCase, Is.SameAs (_elseValue));
    }

    [Test]
    public void VisitChildren_ChangedElse ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      var newElseValue = Expression.Constant (17);

      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate1))
          .Return (_predicate1);
      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.VisitExpression (_value1))
          .Return (_value1);
      visitorMock
        .Expect (mock => mock.VisitExpression (_value2))
        .Return (_value2);
      visitorMock
        .Expect (mock => mock.VisitExpression (_elseValue))
        .Return (newElseValue);

      var result = (SqlCaseExpression) ExtensionExpressionTestHelper.CallVisitChildren (_caseExpressionWithElse, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_caseExpressionWithElse));
      Assert.That (result.Type, Is.SameAs (_caseExpressionWithElse.Type));
      Assert.That (result.Cases[0].When, Is.SameAs (_predicate1));
      Assert.That (result.Cases[0].Then, Is.SameAs (_value1));
      Assert.That (result.Cases[1].When, Is.SameAs (_predicate2));
      Assert.That (result.Cases[1].Then, Is.SameAs (_value2));
      Assert.That (result.ElseCase, Is.SameAs (newElseValue));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlCaseExpression, ISqlCaseExpressionVisitor> (
          _caseExpressionWithElse,
          mock => mock.VisitSqlCaseExpression (_caseExpressionWithElse));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_caseExpressionWithElse);
    }

    [Test]
    public new void ToString ()
    {
      var resultWithElse = _caseExpressionWithElse.ToString ();
      Assert.That (resultWithElse, Is.EqualTo ("CASE WHEN True THEN 17 WHEN False THEN 4 ELSE 42 END"));

      var resultWithoutElse = _caseExpressionWithoutElse.ToString ();
      Assert.That (resultWithoutElse, Is.EqualTo ("CASE WHEN True THEN 47 WHEN False THEN 11 END"));
    }
  }
}