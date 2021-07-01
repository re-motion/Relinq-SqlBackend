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
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.NUnit;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlCaseExpressionTest
  {
    private ConstantExpression _predicate1;
    private ConstantExpression _predicate2;

    private Expression _value1;
    private Expression _value2;
    private Expression _nullableValue1;
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
      _elseValue = Expression.Constant (42);

      _caseExpressionWithElse = new SqlCaseExpression (
          typeof (int),
          new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, _value1), new SqlCaseExpression.CaseWhenPair (_predicate2, _value2) },
          _elseValue);
      _caseExpressionWithoutElse = new SqlCaseExpression (
          typeof (int?),
          new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, _nullableValue1), new SqlCaseExpression.CaseWhenPair (_predicate2, _value2) },
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
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
              "When no ELSE case is given, the expression's result type must be nullable.", "type"));

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
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
              "The WHEN expression's type must be boolean.", "when"));
      Assert.That (
          () => new SqlCaseExpression (typeof (int), new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, Expression.Constant ("x")) }, _elseValue),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
              "The THEN expressions' types must match the expression type.", "cases"));
      Assert.That (
          () => new SqlCaseExpression (typeof (int), new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, _value1) }, Expression.Constant ("a")),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
              "The ELSE expression's type must match the expression type.", "elseCase"));

      Assert.That (
          () => new SqlCaseExpression (typeof (int?), new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, _value1) }, _elseValue), Throws.Nothing);
      Assert.That (
          () => new SqlCaseExpression (typeof (int?), new[] { new SqlCaseExpression.CaseWhenPair (_predicate1, _value1) }, null), Throws.Nothing);
    }

    [Test]
    public void CreateIfThenElse ()
    {
      var result = SqlCaseExpression.CreateIfThenElse (typeof (int), _predicate1, _value1, _value2);

      Assert.That (result.Cases, Has.Count.EqualTo (1));
      Assert.That (result.Cases[0].When, Is.SameAs (_predicate1));
      Assert.That (result.Cases[0].Then, Is.SameAs (_value1));
      Assert.That (result.ElseCase, Is.SameAs (_value2));
    }

    [Test]
    public void CreateIfThenElseNull ()
    {
      var result = SqlCaseExpression.CreateIfThenElseNull (typeof (int?), _predicate1, _value1, _value2);

      Assert.That (result.Cases, Has.Count.EqualTo (2));
      Assert.That (result.Cases[0].When, Is.SameAs (_predicate1));
      Assert.That (result.Cases[0].Then, Is.SameAs (_value1));
      SqlExpressionTreeComparer.CheckAreEqualTrees (result.Cases[1].When, Expression.Not (_predicate1));
      Assert.That (result.Cases[1].Then, Is.SameAs (_value2));
      SqlExpressionTreeComparer.CheckAreEqualTrees (result.ElseCase, Expression.Constant (null, typeof (int?)));
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_predicate1))
          .Return (_predicate1);
      visitorMock
          .Expect (mock => mock.Visit (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.Visit (_value1))
          .Return (_value1);
      visitorMock
        .Expect (mock => mock.Visit (_value2))
        .Return (_value2);
      visitorMock
        .Expect (mock => mock.Visit (_elseValue))
        .Return (_elseValue);
      
      var result = ExtensionExpressionTestHelper.CallVisitChildren (_caseExpressionWithElse, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_caseExpressionWithElse));
    }

    [Test]
    public void VisitChildren_NoElse ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_predicate1))
          .Return (_predicate1);
      visitorMock
          .Expect (mock => mock.Visit (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.Visit (_nullableValue1))
          .Return (_nullableValue1);
      visitorMock
        .Expect (mock => mock.Visit (_value2))
        .Return (_value2);

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_caseExpressionWithoutElse, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_caseExpressionWithoutElse));
    }

    [Test]
    public void VisitChildren_ChangedWhen ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();

      var newPredicate = Expression.Constant (true);

      visitorMock
          .Expect (mock => mock.Visit (_predicate1))
          .Return (newPredicate);
      visitorMock
          .Expect (mock => mock.Visit (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.Visit (_value1))
          .Return (_value1);
      visitorMock
        .Expect (mock => mock.Visit (_value2))
        .Return (_value2);
      visitorMock
        .Expect (mock => mock.Visit (_elseValue))
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
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();

      var newValue = Expression.Constant (17);

      visitorMock
          .Expect (mock => mock.Visit (_predicate1))
          .Return (_predicate1);
      visitorMock
          .Expect (mock => mock.Visit (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.Visit (_value1))
          .Return (newValue);
      visitorMock
        .Expect (mock => mock.Visit (_value2))
        .Return (_value2);
      visitorMock
        .Expect (mock => mock.Visit (_elseValue))
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
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();

      var newElseValue = Expression.Constant (17);

      visitorMock
          .Expect (mock => mock.Visit (_predicate1))
          .Return (_predicate1);
      visitorMock
          .Expect (mock => mock.Visit (_predicate2))
          .Return (_predicate2);
      visitorMock
          .Expect (mock => mock.Visit (_value1))
          .Return (_value1);
      visitorMock
        .Expect (mock => mock.Visit (_value2))
        .Return (_value2);
      visitorMock
        .Expect (mock => mock.Visit (_elseValue))
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
    public void Update_NoChanges ()
    {
      var resultWithElse = _caseExpressionWithElse.Update (_caseExpressionWithElse.Cases, _caseExpressionWithElse.ElseCase);
      var resultWithoutElse = _caseExpressionWithoutElse.Update (_caseExpressionWithoutElse.Cases, _caseExpressionWithoutElse.ElseCase);

      Assert.That (resultWithElse, Is.SameAs (_caseExpressionWithElse));
      Assert.That (resultWithoutElse, Is.SameAs (_caseExpressionWithoutElse));
    }

    [Test]
    public void Update_ChangedCases ()
    {
      var newCases = Array.AsReadOnly (new[] { new SqlCaseExpression.CaseWhenPair (Expression.Constant (true), Expression.Constant (1)) });
      var result = _caseExpressionWithElse.Update (newCases, _caseExpressionWithElse.ElseCase);

      Assert.That (result, Is.Not.SameAs (_caseExpressionWithElse));
      Assert.That (result.Cases, Is.EqualTo (newCases));
      Assert.That (result.ElseCase, Is.SameAs (_caseExpressionWithElse.ElseCase));
    }

    [Test]
    public void Update_ChangedElseCase ()
    {
      var newElseCase = Expression.Constant (1);
      var result = _caseExpressionWithElse.Update (_caseExpressionWithElse.Cases, newElseCase);

      Assert.That (result, Is.Not.SameAs (_caseExpressionWithElse));
      Assert.That (result.Cases, Is.EqualTo (_caseExpressionWithElse.Cases));
      Assert.That (result.ElseCase, Is.SameAs (newElseCase));
    }

    [Test]
    public void Update_CaseWhenPair_NoChanges ()
    {
      var caseWhenPair = new SqlCaseExpression.CaseWhenPair (_predicate1, _value1);

      var result = caseWhenPair.Update (_predicate1, _value1);

      Assert.That (result, Is.SameAs (caseWhenPair));
    }

    [Test]
    public void Update_CaseWhenPair_ChangeWhen ()
    {
      var caseWhenPair = new SqlCaseExpression.CaseWhenPair (_predicate1, _value1);

      var result = caseWhenPair.Update (_predicate2, _value1);

      Assert.That (result, Is.Not.SameAs (caseWhenPair));
      Assert.That (result.When, Is.SameAs (_predicate2));
      Assert.That (result.Then, Is.SameAs (_value1));
    }

    [Test]
    public void Update_CaseWhenPair_ChangeThen ()
    {
      var caseWhenPair = new SqlCaseExpression.CaseWhenPair (_predicate1, _value1);

      var result = caseWhenPair.Update (_predicate1, _value2);

      Assert.That (result, Is.Not.SameAs (caseWhenPair));
      Assert.That (result.When, Is.SameAs (_predicate1));
      Assert.That (result.Then, Is.SameAs (_value2));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlCaseExpression, ISqlSpecificExpressionVisitor> (
          _caseExpressionWithElse,
          mock => mock.VisitSqlCase (_caseExpressionWithElse));
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
      Assert.That (resultWithoutElse, Is.EqualTo ("CASE WHEN True THEN 47 WHEN False THEN 4 END"));
    }
  }
}