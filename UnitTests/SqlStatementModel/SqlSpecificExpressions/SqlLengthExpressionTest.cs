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
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlLengthExpressionTest
  {
    private Expression _innerExpression;
    private SqlLengthExpression _lengthExpression;

    [SetUp]
    public void SetUp ()
    {
      _innerExpression = Expression.Constant ("test");
      _lengthExpression = new SqlLengthExpression (_innerExpression);
    }

    [Test]
    public void Initialization_String ()
    {
      var stringExpression = Expression.Constant ("test");
      var lengthExpression = new SqlLengthExpression (stringExpression);

      Assert.That (lengthExpression.Expression, Is.SameAs (stringExpression));
      Assert.That (lengthExpression.Type, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void Initialization_Char ()
    {
      var charExpression = Expression.Constant ('t');
      var lengthExpression = new SqlLengthExpression (charExpression);

      Assert.That (lengthExpression.Expression, Is.SameAs (charExpression));
      Assert.That (lengthExpression.Type, Is.EqualTo (typeof (int)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "SqlLengthExpression can only be used on values of type 'System.String' or 'System.Char', not on 'System.Int32'. (Add a conversion if you need "
        + "to get the string length of a non-string value.)\r\nParameter name: expression")]
    public void Initialization_OtherType ()
    {
      var intExpression = Expression.Constant (0);
      new SqlLengthExpression (intExpression);
    }

    [Test]
    public void VisitChildren ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();
      var expression = Expression.Constant ("test2");

      visitorMock
          .Expect (mock => mock.Visit (_innerExpression))
          .Return (expression);

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_lengthExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_lengthExpression));
      Assert.That (((SqlLengthExpression) result).Expression, Is.SameAs (expression));
    }

    [Test]
    public void VisitChildren_ReturnsSame ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_innerExpression))
          .Return (_lengthExpression.Expression);

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_lengthExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (_lengthExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlLengthExpression, ISqlSpecificExpressionVisitor> (
          _lengthExpression,
          mock => mock.VisitSqlLengthExpression (_lengthExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_lengthExpression);
    }

    [Test]
    public void To_String ()
    {
      Assert.That (_lengthExpression.ToString (), Is.EqualTo ("LEN(\"test\")"));
    }
    
  }
}