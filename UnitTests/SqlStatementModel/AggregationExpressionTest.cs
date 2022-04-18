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
using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class AggregationExpressionTest
  {
    private AggregationExpression _aggregationEpression;
    private ConstantExpression _wrappedExpression;

    [SetUp]
    public void SetUp ()
    {
      _wrappedExpression = Expression.Constant (1);
      _aggregationEpression = new AggregationExpression(typeof(int), _wrappedExpression, AggregationModifier.Max);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_aggregationEpression.Type, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);

      visitorMock
          .Setup (mock => mock.Visit (_wrappedExpression))
          .Returns (_wrappedExpression)
          .Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_aggregationEpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.SameAs (_aggregationEpression));
    }

    [Test]
    public void VisitChildren_ReturnsNewExpression ()
    {
      var newExpression = Expression.Constant (5);
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);

      visitorMock
          .Setup (mock => mock.Visit (_wrappedExpression))
          .Returns (newExpression)
          .Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_aggregationEpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.Not.SameAs (_aggregationEpression));
      Assert.That (((AggregationExpression) result).Expression, Is.SameAs (newExpression));
      Assert.That (((AggregationExpression) result).AggregationModifier, Is.EqualTo (AggregationModifier.Max));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<AggregationExpression, IAggregationExpressionVisitor> (
          _aggregationEpression,
          mock => mock.VisitAggregation(_aggregationEpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_aggregationEpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _aggregationEpression.ToString();

      Assert.That (result, Is.EqualTo ("Max(1)"));
    }
  }
}