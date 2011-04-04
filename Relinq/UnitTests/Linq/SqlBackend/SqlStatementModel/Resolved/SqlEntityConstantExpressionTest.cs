// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
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
    private SqlEntityConstantExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _expression = new SqlEntityConstantExpression (typeof (Cook), new object(), "5");
    }

    [Test]
    public void VisitChildren_ReturnsThis_WithoutCallingVisitMethods ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      Assert.That (result, Is.SameAs (_expression));
      visitorMock.VerifyAllExpectations();
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