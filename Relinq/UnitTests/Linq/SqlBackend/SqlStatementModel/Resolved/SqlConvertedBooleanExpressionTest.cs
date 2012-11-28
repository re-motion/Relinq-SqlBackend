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
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlConvertedBooleanExpressionTest
  {
    private ConstantExpression _innerExpression;
    private SqlConvertedBooleanExpression _sqlConvertedBooleanExpression;

    [SetUp]
    public void SetUp ()
    {
      _innerExpression = Expression.Constant (1);
      _sqlConvertedBooleanExpression = new SqlConvertedBooleanExpression (_innerExpression);
    }

    [Test]
    public void Initialization_Boolean ()
    {
      var innerExpression = Expression.Constant (1);
      var convertedExpression = new SqlConvertedBooleanExpression (innerExpression);
      Assert.That (convertedExpression.Type, Is.SameAs (typeof (bool)));
      Assert.That (convertedExpression.Expression, Is.SameAs (innerExpression));
    }

    [Test]
    public void Initialization_NullableBoolean ()
    {
      var innerExpression = Expression.Constant (1, typeof (int?));
      var convertedExpression = new SqlConvertedBooleanExpression (innerExpression);
      Assert.That (convertedExpression.Type, Is.SameAs (typeof (bool?)));
      Assert.That (convertedExpression.Expression, Is.SameAs (innerExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "The inner expression must be an expression of type Int32 or Nullable<Int32>.\r\nParameter name: expression")]
    public void Initialization_InnerTypeMustBeInt32 ()
    {
      var innerExpression = Expression.Constant ("1");
      Dev.Null = new SqlConvertedBooleanExpression (innerExpression);
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_innerExpression))
          .Return (_innerExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlConvertedBooleanExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_sqlConvertedBooleanExpression));
    }

    [Test]
    public void VisitChildren_ReturnsNewExpression ()
    {
      var newExpression = Expression.Constant (5);
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_innerExpression))
          .Return (newExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlConvertedBooleanExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_sqlConvertedBooleanExpression));
      Assert.That (((SqlConvertedBooleanExpression) result).Expression, Is.SameAs (newExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlConvertedBooleanExpression, ISqlConvertedBooleanExpressionVisitor> (
          _sqlConvertedBooleanExpression,
          mock => mock.VisitSqlConvertedBooleanExpression (_sqlConvertedBooleanExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_sqlConvertedBooleanExpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _sqlConvertedBooleanExpression.ToString ();

      Assert.That (result, Is.EqualTo ("ConvertedBoolean(1)"));
    }
  }
}