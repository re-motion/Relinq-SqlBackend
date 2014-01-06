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
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlInExpressionTest
  {
    private SqlInExpression _expression;
    private ConstantExpression _leftExpression;
    private ConstantExpression _rightExpression;
    
    [SetUp]
    public void SetUp ()
    {
      _leftExpression = Expression.Constant (1);
      _rightExpression = Expression.Constant (2);

      _expression = new SqlInExpression (_leftExpression, _rightExpression);
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
      Assert.That (((SqlInExpression) result).LeftExpression, Is.SameAs (newLeftExpression));
      Assert.That (((SqlInExpression) result).RightExpression, Is.SameAs (newRightExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlInExpression, ISqlInExpressionVisitor> (
          _expression,
          mock => mock.VisitSqlInExpression(_expression));
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

      Assert.That (result, Is.EqualTo ("1 IN 2"));
    }
  }
}