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
using Moq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlFunctionExpressionTest
  {
    private SqlFunctionExpression _sqlFunctionExpression;

    [SetUp]
    public void SetUp ()
    {
      Expression[] args = { Expression.Constant ("test"), Expression.Constant (1), Expression.Constant (2) };
      _sqlFunctionExpression = new SqlFunctionExpression (typeof (string), "Test", args);
    }

    [Test]
    public void VisitChildren_NewArgs ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      var newArgs = new[] { Expression.Constant (1), Expression.Constant (8), Expression.Constant (9) };
      
      visitorMock.Setup (mock => mock.Visit (_sqlFunctionExpression.Args[0])).Returns (newArgs[0]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (_sqlFunctionExpression.Args[1])).Returns (newArgs[1]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (_sqlFunctionExpression.Args[2])).Returns (newArgs[2]).Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlFunctionExpression, visitorMock.Object);

      visitorMock.Verify();

      Assert.That (result, Is.Not.SameAs (_sqlFunctionExpression));
      Assert.That (((SqlFunctionExpression) result).Args, Is.EqualTo (newArgs));
      Assert.That (((SqlFunctionExpression) result).SqlFunctioName, Is.EqualTo ("Test"));
    }

    [Test]
    public void VisitChildren_SameSqlFunctionExpression ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      
      visitorMock.Setup (mock => mock.Visit (_sqlFunctionExpression.Args[0])).Returns (_sqlFunctionExpression.Args[0]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (_sqlFunctionExpression.Args[1])).Returns (_sqlFunctionExpression.Args[1]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (_sqlFunctionExpression.Args[2])).Returns (_sqlFunctionExpression.Args[2]).Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlFunctionExpression, visitorMock.Object);

      visitorMock.Verify();

      Assert.That (result, Is.SameAs (_sqlFunctionExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlFunctionExpression, ISqlSpecificExpressionVisitor> (
          _sqlFunctionExpression,
          mock => mock.VisitSqlFunction (_sqlFunctionExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_sqlFunctionExpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _sqlFunctionExpression.ToString();

      Assert.That (result, Is.EqualTo ("Test(\"test\",1,2)"));
    }
  }
}