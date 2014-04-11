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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlFunctionExpressionTest
  {
    private SqlFunctionExpression _sqlFunctionExpression;

    [SetUp]
    public void SetUp ()
    {
      _sqlFunctionExpression = new SqlFunctionExpression (typeof (string), "Test", Expression.Constant("test"), Expression.Constant (1), Expression.Constant (2));
    }

    [Test]
    public void VisitChildren_NewArgs ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();
      var newArgs = new ReadOnlyCollection<Expression> (new List<Expression> { Expression.Constant (1), Expression.Constant (2) });
      
      visitorMock
          .Expect (mock => mock.VisitAndConvert (_sqlFunctionExpression.Args, "SqlFunctionExpression.VisitChildren"))
          .Return (newArgs);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlFunctionExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.Not.SameAs (_sqlFunctionExpression));
      Assert.That (((SqlFunctionExpression) result).Args, Is.EqualTo (newArgs));
      Assert.That (((SqlFunctionExpression) result).SqlFunctioName, Is.EqualTo ("Test"));
    }

    [Test]
    public void VisitChildren_SameSqlFunctionExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();
      
      visitorMock
          .Expect (mock => mock.VisitAndConvert (_sqlFunctionExpression.Args, "SqlFunctionExpression.VisitChildren"))
          .Return (_sqlFunctionExpression.Args);
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlFunctionExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (_sqlFunctionExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlFunctionExpression, ISqlSpecificExpressionVisitor> (
          _sqlFunctionExpression,
          mock => mock.VisitSqlFunctionExpression (_sqlFunctionExpression));
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