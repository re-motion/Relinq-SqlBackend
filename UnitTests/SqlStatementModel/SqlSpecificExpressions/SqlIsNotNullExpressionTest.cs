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
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlIsNotNullExpressionTest
  {
    private SqlIsNotNullExpression _sqlIsNotNullExpression;

    [SetUp]
    public void SetUp ()
    {
      _sqlIsNotNullExpression = new SqlIsNotNullExpression(Expression.Constant(2));
    }

    [Test]
    public void VisitChildren ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      var expression = Expression.Constant (3);
      
      visitorMock
          .Setup (mock => mock.Visit (_sqlIsNotNullExpression.Expression))
          .Returns (expression)
          .Verifiable();
      
      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlIsNotNullExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.Not.SameAs (_sqlIsNotNullExpression));
      Assert.That (((SqlIsNotNullExpression) result).Expression, Is.SameAs (expression));
    }

    [Test]
    public void VisitChildren_ReturnsSame ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      var expression = Expression.Constant (3);

      visitorMock
          .Setup (mock => mock.Visit (_sqlIsNotNullExpression.Expression))
          .Returns (_sqlIsNotNullExpression.Expression)
          .Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlIsNotNullExpression, visitorMock.Object);

      visitorMock.Verify();

      Assert.That (result, Is.SameAs (_sqlIsNotNullExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlIsNotNullExpression, ISqlNullCheckExpressionVisitor> (
          _sqlIsNotNullExpression,
          mock => mock.VisitSqlIsNotNull (_sqlIsNotNullExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_sqlIsNotNullExpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _sqlIsNotNullExpression.ToString();

      Assert.That (result, Is.EqualTo ("2 IS NOT NULL"));
    }
  }
}