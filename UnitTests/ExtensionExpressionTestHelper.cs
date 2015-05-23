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
using Remotion.Development.UnitTesting;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests
{
  public static class ExtensionExpressionTestHelper
  {
    public static void CheckAcceptForVisitorSupportingType<TExpression, TVisitorInterface> (
        TExpression expression,
        Func<TVisitorInterface, Expression> visitMethodCall) where TExpression : Expression
    {
      var mockRepository = new MockRepository ();
      var visitorMock = mockRepository.StrictMultiMock<ExpressionVisitor> (typeof (TVisitorInterface));

      var returnedExpression = Expression.Constant (0);

      visitorMock
          .Expect (mock => visitMethodCall ((TVisitorInterface) (object) mock))
          .Return (returnedExpression);
      visitorMock.Replay ();

      var result = CallAccept (expression, visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (returnedExpression));
    }

    public static void CheckAcceptForVisitorNotSupportingType<TExpression> (TExpression expression) where TExpression : Expression
    {
      var mockRepository = new MockRepository ();
      var visitorMock = mockRepository.StrictMock<ExpressionVisitor> ();

      var returnedExpression = Expression.Constant (0);

      visitorMock
          .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitExtension", expression))
          .Return (returnedExpression);
      visitorMock.Replay ();

      var result = CallAccept (expression, visitorMock);

      visitorMock.VerifyAllExpectations ();

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