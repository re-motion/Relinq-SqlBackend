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
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlPredicateAsValueExpressionTest
  {
    private ConstantExpression _predicate;
    private SqlPredicateAsValueExpression _predicateAsValueExpression;

    [SetUp]
    public void SetUp ()
    {
      _predicate = Expression.Constant (true);
      _predicateAsValueExpression = new SqlPredicateAsValueExpression (_predicate);
    }

    [Test]
    public void Initialization_Boolean ()
    {
      var predicate = Expression.Constant (true);
      var predicateAsValueExpression = new SqlPredicateAsValueExpression (predicate);

      Assert.That (predicateAsValueExpression.Predicate, Is.SameAs (predicate));
      Assert.That (predicateAsValueExpression.Type, Is.SameAs (typeof (int)));
    }

    [Test]
    public void Initialization_NullableBoolean ()
    {
      var predicate = Expression.Constant (true, typeof (bool?));
      var predicateAsValueExpression = new SqlPredicateAsValueExpression (predicate);

      Assert.That (predicateAsValueExpression.Predicate, Is.SameAs (predicate));
      Assert.That (predicateAsValueExpression.Type, Is.SameAs (typeof (int?)));
    }

    [Test]
    public void Initialization_NonBoolean ()
    {
      Assert.That (
          () => new SqlPredicateAsValueExpression (Expression.Constant (1)), 
          Throws.TypeOf<ArgumentException>().With.Message.EqualTo (
            "The predicate must be an expression of type Boolean or Nullable<Boolean>.\r\nParameter name: predicate"));
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate))
          .Return (_predicate);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_predicateAsValueExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_predicateAsValueExpression));
    }

    [Test]
    public void VisitChildren_ReturnsNewExpression ()
    {
      var newPredicate = Expression.Constant (false);
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_predicate))
          .Return (newPredicate);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_predicateAsValueExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_predicateAsValueExpression));
      Assert.That (((SqlPredicateAsValueExpression) result).Predicate, Is.SameAs (newPredicate));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlPredicateAsValueExpression, ISqlPredicateAsValueExpressionVisitor> (
          _predicateAsValueExpression,
          mock => mock.VisitSqlPredicateAsValueExpression (_predicateAsValueExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_predicateAsValueExpression);
    }

    [Test]
    public new void ToString ()
    {
      var result = _predicateAsValueExpression.ToString ();

      Assert.That (result, Is.EqualTo ("PredicateValue(True)"));
    }
  }
}