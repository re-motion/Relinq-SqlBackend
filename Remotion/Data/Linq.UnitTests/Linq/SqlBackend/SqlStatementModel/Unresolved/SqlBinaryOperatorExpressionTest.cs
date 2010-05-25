// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Rhino.Mocks;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class SqlBinaryOperatorExpressionTest
  {
    private SqlBinaryOperatorExpression _expression;
    private ConstantExpression _leftExpression;
    private ConstantExpression _rightExpression;


    [SetUp]
    public void SetUp ()
    {
      _leftExpression = Expression.Constant (1);
      _rightExpression = Expression.Constant (2);

      _expression = new SqlBinaryOperatorExpression ("Operator", _leftExpression, _rightExpression);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expression.Type, Is.EqualTo (typeof (bool)));
    }

    [Test]
    public void VisitChildren_ReturnsNewSqlInExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();
      var newLeftExpression = Expression.Constant (3);
      var newRightExpression = Expression.Constant (4);

      visitorMock
          .Expect (mock => mock.VisitExpression (_leftExpression))
          .Return (newLeftExpression);
      visitorMock
          .Expect (mock => mock.VisitExpression (_rightExpression))
          .Return (newRightExpression);
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.SameAs (_expression));
      Assert.That (((SqlBinaryOperatorExpression) result).LeftExpression, Is.SameAs (newLeftExpression));
      Assert.That (((SqlBinaryOperatorExpression) result).RightExpression, Is.SameAs (newRightExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlBinaryOperatorExpression, ISqlSpecificExpressionVisitor> (
          _expression,
          mock => mock.VisitSqlBinaryOperatorExpression(_expression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_expression);
    }

    [Test]
    public void To_String ()
    {
      var result = _expression.ToString();

      Assert.That (result, Is.EqualTo ("1 Operator 2"));
    }
  }
}