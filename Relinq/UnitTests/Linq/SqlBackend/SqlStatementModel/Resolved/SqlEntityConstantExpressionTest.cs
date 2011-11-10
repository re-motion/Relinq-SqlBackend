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
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Rhino.Mocks;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlEntityConstantExpressionTest
  {
    private ConstantExpression _primaryKeyExpression;
    private SqlEntityConstantExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _primaryKeyExpression = Expression.Constant (5);
      _expression = new SqlEntityConstantExpression (typeof (Cook), new object(), _primaryKeyExpression);
    }

    [Test]
    public void VisitChildren_VisitsPrimaryKeyExpression_Unchanged ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();
      visitorMock.Expect (mock => mock.VisitExpression (_primaryKeyExpression)).Return (_primaryKeyExpression);
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_expression));
    }

    [Test]
    public void VisitChildren_VisitsPrimaryKeyExpression_Changed ()
    {
      var newPrimaryKeyExpression = Expression.Constant (6);
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();
      visitorMock.Expect (mock => mock.VisitExpression (_primaryKeyExpression)).Return (newPrimaryKeyExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_expression));
      Assert.That (result, Is.TypeOf<SqlEntityConstantExpression> ());

      var newSqlEntityConstantExpression = ((SqlEntityConstantExpression) result);
      Assert.That (newSqlEntityConstantExpression.PrimaryKeyExpression, Is.SameAs (newPrimaryKeyExpression));
      Assert.That (newSqlEntityConstantExpression.Value, Is.SameAs (_expression.Value));
      Assert.That (newSqlEntityConstantExpression.Type, Is.SameAs (_expression.Type));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlEntityConstantExpression, IUnresolvedSqlExpressionVisitor> (
          _expression,
          mock => mock.VisitSqlEntityConstantExpression(_expression));
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