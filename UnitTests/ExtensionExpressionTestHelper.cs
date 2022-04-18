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
using Moq.Protected;
using NUnit.Framework;
using Remotion.Development.UnitTesting;

namespace Remotion.Linq.SqlBackend.UnitTests
{
  public static class ExtensionExpressionTestHelper
  {
    public static void CheckAcceptForVisitorSupportingType<TExpression, TVisitorInterface> (
        TExpression expression,
        Expression<Func<TVisitorInterface, Expression>> visitMethodCall)
        where TExpression : Expression
        where TVisitorInterface : class
    {
      var baseMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      var visitorMock = baseMock.As<TVisitorInterface>();

      var returnedExpression = Expression.Constant (0);

      visitorMock
          .Setup (visitMethodCall)
          .Returns (returnedExpression)
          .Verifiable();

      var result = CallAccept (expression, baseMock.Object);

      visitorMock.Verify();

      Assert.That (result, Is.SameAs (returnedExpression));
    }

    public static void CheckAcceptForVisitorNotSupportingType<TExpression> (TExpression expression) where TExpression : Expression
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);

      var returnedExpression = Expression.Constant (0);

      visitorMock
          .Protected()
          .Setup<Expression> ("VisitExtension", ItExpr.Is<Expression> (_ => _ == expression))
          .Returns (returnedExpression)
          .Verifiable();

      var result = CallAccept (expression, visitorMock.Object);

      visitorMock.Verify();

      Assert.That (result, Is.SameAs (returnedExpression));
    }

    public static Expression CallAccept (Expression expression, ExpressionVisitor visitor)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (expression, "Accept", visitor);
    }

    public static Expression CallVisitChildren (Expression target, ExpressionVisitor visitor)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (target, "VisitChildren", visitor);
    }
  }
}