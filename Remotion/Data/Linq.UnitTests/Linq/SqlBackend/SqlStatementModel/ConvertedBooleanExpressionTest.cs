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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class ConvertedBooleanExpressionTest
  {
    private ConstantExpression _innerExpression;
    private ConvertedBooleanExpression _convertedExpression;

    [SetUp]
    public void SetUp ()
    {
      _innerExpression = Expression.Constant (1);
      _convertedExpression = new ConvertedBooleanExpression (_innerExpression);
    }

    [Test]
    public void Initialization_TypeIsBoolean ()
    {
      Assert.That (_convertedExpression.Type, Is.SameAs (typeof (bool)));
      Assert.That (_convertedExpression.Expression, Is.SameAs (_innerExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "The inner expression must be an expression of type Int32.\r\nParameter name: expression")]
    public void Initialization_InnerTypeMustBeInt32 ()
    {
      var innerExpression = Expression.Constant ("1");
      new ConvertedBooleanExpression (innerExpression);
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_innerExpression))
          .Return (_innerExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_convertedExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_convertedExpression));
    }

    [Test]
    public void VisitChildren_ReturnsNewExpression ()
    {
      var newExpression = Expression.Constant (5);
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_innerExpression))
          .Return (newExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_convertedExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_convertedExpression));
      Assert.That (((ConvertedBooleanExpression) result).Expression, Is.SameAs (newExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<ConvertedBooleanExpression, IConvertedBooleanExpressionVisitor> (
          _convertedExpression,
          mock => mock.VisitConvertedBooleanExpression (_convertedExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_convertedExpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _convertedExpression.ToString ();

      Assert.That (result, Is.EqualTo ("ConvertedBoolean(1)"));
    }
  }
}