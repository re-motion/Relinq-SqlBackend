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
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlRowNumberExpressionTest
  {
    private SqlRowNumberExpression _sqlRowNumberExpression;
    private ConstantExpression _orderingExpression1;
    private ConstantExpression _orderingExpression2;

    [SetUp]
    public void SetUp ()
    {
      _orderingExpression1 = Expression.Constant ("order1");
      _orderingExpression2 = Expression.Constant ("order2");
      _sqlRowNumberExpression =
          new SqlRowNumberExpression (
              new[]
              {
                  new Ordering (_orderingExpression1, OrderingDirection.Asc),
                  new Ordering (_orderingExpression2, OrderingDirection.Desc)
              });
    }

    [Test]
    public void VisitChildren_SameOrderingExpressions ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_orderingExpression1))
          .Return (_orderingExpression1);
      visitorMock
          .Expect (mock => mock.Visit (_orderingExpression2))
          .Return (_orderingExpression2);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlRowNumberExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (_sqlRowNumberExpression));
    }

    [Test]
    public void VisitChildren_NewOrderingExpressions ()
    {
      var visitorMock = MockRepository.GenerateMock<ExpressionVisitor> ();
      var fakeResult = Expression.Constant (3);

      visitorMock
          .Expect (mock => mock.Visit (_orderingExpression1))
          .Return (fakeResult);
      visitorMock
          .Expect (mock => mock.Visit (_orderingExpression2))
          .Return (_orderingExpression2);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlRowNumberExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.Not.SameAs (_sqlRowNumberExpression));
      Assert.That (((SqlRowNumberExpression) result).Orderings[0].Expression, Is.SameAs (fakeResult));
      Assert.That (((SqlRowNumberExpression) result).Orderings[0].OrderingDirection, Is.EqualTo(OrderingDirection.Asc));
      Assert.That (((SqlRowNumberExpression) result).Orderings[1].Expression, Is.SameAs (_orderingExpression2));
      Assert.That (((SqlRowNumberExpression) result).Orderings[1].OrderingDirection, Is.EqualTo (OrderingDirection.Desc));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlRowNumberExpression, ISqlSpecificExpressionVisitor> (
          _sqlRowNumberExpression,
          mock => mock.VisitSqlRowNumberExpression (_sqlRowNumberExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_sqlRowNumberExpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _sqlRowNumberExpression.ToString();

      Assert.That (result, Is.EqualTo ("ROW_NUMBER() OVER (ORDER BY \"order1\" asc,\"order2\" desc)"));
    }
  }
}