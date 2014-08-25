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
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class JoinConditionExpressionTest
  {
    private JoinConditionExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlJoinedTable_WithUnresolvedJoinInfo();
      _expression = new JoinConditionExpression (sqlTable);
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
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<JoinConditionExpression, IJoinConditionExpressionVisitor> (
          _expression,
          mock => mock.VisitJoinConditionExpression (_expression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_expression);
    }

    [Test]
    public void ToString_Unresolved ()
    {
      var result = _expression.ToString();

      Assert.That (result, Is.EqualTo ("CONDITION(LEFT JOIN Cook.FirstName)"));
    }

    [Test]
    public void ToString_Resolved ()
    {
      var resolvedJoinInfo = new ResolvedJoinInfo (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), Expression.Constant (true));
      var expression = new JoinConditionExpression (new SqlJoinedTable (resolvedJoinInfo, JoinSemantics.Left));

      var result = expression.ToString();

      Assert.That (result, Is.EqualTo ("CONDITION(LEFT JOIN [CookTable] [c] ON True)"));
    }
  }
}