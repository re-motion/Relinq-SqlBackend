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
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlEntityConstantExpressionTest
  {
    private ConstantExpression _identityExpression;
    private SqlEntityConstantExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _identityExpression = Expression.Constant (5);
      _expression = new SqlEntityConstantExpression (typeof (Cook), new object(), _identityExpression);
    }

    [Test]
    public void VisitChildren_VisitsIdentityExpression_Unchanged ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      visitorMock.Setup (mock => mock.Visit (_identityExpression)).Returns (_identityExpression).Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.SameAs (_expression));
    }

    [Test]
    public void VisitChildren_VisitsIdentityExpression_Changed ()
    {
      var newPrimaryKeyExpression = Expression.Constant (6);
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      visitorMock.Setup (mock => mock.Visit (_identityExpression)).Returns (newPrimaryKeyExpression).Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.Not.SameAs (_expression));
      Assert.That (result, Is.TypeOf<SqlEntityConstantExpression> ());

      var newSqlEntityConstantExpression = ((SqlEntityConstantExpression) result);
      Assert.That (newSqlEntityConstantExpression.IdentityExpression, Is.SameAs (newPrimaryKeyExpression));
      Assert.That (newSqlEntityConstantExpression.Value, Is.SameAs (_expression.Value));
      Assert.That (newSqlEntityConstantExpression.Type, Is.SameAs (_expression.Type));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlEntityConstantExpression, IResolvedSqlExpressionVisitor> (
          _expression,
          mock => mock.VisitSqlEntityConstant(_expression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_expression);
    }

    [Test]
    public new void ToString ()
    {
      var result = _expression.ToString();

      Assert.That (result, Is.EqualTo ("ENTITY(5)"));
    }
  }
}