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
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlLikeExpressionTest
  {
    private SqlLikeExpression _likeExpression;

    [SetUp]
    public void SetUp ()
    {
      var leftExpression = Expression.Constant ("left");
      var rightExpression = Expression.Constant ("right");
      var escapeExpression = new SqlLiteralExpression (@"\");
      _likeExpression = new SqlLikeExpression (leftExpression, rightExpression, escapeExpression);
    }

    [Test]
    public void Create_ArgumentIsNotNull ()
    {
      var objectExpression = Expression.Constant ("Test");
      var argument1 = Expression.Constant ("test");
      
      var result = SqlLikeExpression.Create (objectExpression, argument1, "%", "%");
      
      var rightExpression = Expression.Constant (string.Format ("%{0}%", argument1.Value));
      var expectedResult = new SqlLikeExpression (objectExpression, rightExpression, new SqlLiteralExpression (@"\"));

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Create_ArgumentIsNotNullAndNoConstantExpression ()
    {
      var method = typeof (string).GetMethod ("Contains", new[] { typeof (string) });
      var objectExpression = Expression.Constant ("Test");
      var argument1 = Expression.MakeMemberAccess (Expression.Constant (new Cook ()), typeof (Cook).GetProperty ("Name"));
      var expression = Expression.Call (objectExpression, method, argument1);
      
      var result = SqlLikeExpression.Create(expression.Object, expression.Arguments[0], "%", "%");

      Expression rightExpression = new SqlFunctionExpression (
          typeof (string),
          "REPLACE",
          new SqlFunctionExpression (
              typeof (string),
              "REPLACE",
                 new SqlFunctionExpression (
                      typeof (string),
                      "REPLACE",
                      new SqlFunctionExpression (
                          typeof (string),
                          "REPLACE",
                          argument1,
                          new SqlLiteralExpression (@"\"),
                          new SqlLiteralExpression (@"\\")),
                      new SqlLiteralExpression (@"%"),
                      new SqlLiteralExpression (@"\%")),
                  new SqlLiteralExpression (@"_"),
                  new SqlLiteralExpression (@"\_")),
              new SqlLiteralExpression (@"["),
              new SqlLiteralExpression (@"\["));
      rightExpression = Expression.Add (
          new SqlLiteralExpression ("%"), rightExpression, typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) }));
      rightExpression = Expression.Add (
          rightExpression, new SqlLiteralExpression ("%"), typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) }));
      var expectedResult = new SqlLikeExpression (objectExpression, rightExpression, new SqlLiteralExpression (@"\"));

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);  
    }

    [Test]
    public void Escape_Bracket ()
    {
      var result = SqlLikeExpression.Escape ("test[test", @"\");

      Assert.That (result, Is.EqualTo (@"test\[test"));
    }

    [Test]
    public void Escape_Percent ()
    {
      var result = SqlLikeExpression.Escape ("test%test", @"\");

      Assert.That (result, Is.EqualTo (@"test\%test"));
    }

    [Test]
    public void Escape_Underline ()
    {
      var result = SqlLikeExpression.Escape ("test_test", @"\");

      Assert.That (result, Is.EqualTo (@"test\_test"));
    }

    [Test]
    public void Escape_EscapeSequence ()
    {
      var result = SqlLikeExpression.Escape (@"test\test", @"\");

      Assert.That (result, Is.EqualTo (@"test\\test"));
    }

    [Test]
    public void Escape_Expression ()
    {
      var expression = Expression.Constant ("test[test");

      var result = SqlLikeExpression.Escape (expression, @"\");

      var expectedResult =
        new SqlFunctionExpression (typeof (string), "REPLACE",
          new SqlFunctionExpression (typeof (string), "REPLACE",
            new SqlFunctionExpression (typeof (string), "REPLACE",
              new SqlFunctionExpression (typeof (string), "REPLACE", expression,
                new SqlLiteralExpression (@"\"), new SqlLiteralExpression (@"\\")),
                  new SqlLiteralExpression ("%"), new SqlLiteralExpression (@"\%")),
                    new SqlLiteralExpression ("_"), new SqlLiteralExpression (@"\_")),
                      new SqlLiteralExpression ("["), new SqlLiteralExpression (@"\["));

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitChildren_ReturnsSame ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);

      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.Left))
          .Returns (_likeExpression.Left)
          .Verifiable();
      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.Right))
          .Returns (_likeExpression.Right)
          .Verifiable();
      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.EscapeExpression))
          .Returns (_likeExpression.EscapeExpression)
          .Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_likeExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.SameAs (_likeExpression));
    }

    [Test]
    public void VisitChildren_ReturnsDifferentRightExpression ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      var newRightExpression = Expression.Constant (3);

      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.Left))
          .Returns (_likeExpression.Left)
          .Verifiable();
      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.Right))
          .Returns (newRightExpression)
          .Verifiable();
      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.EscapeExpression))
          .Returns (_likeExpression.EscapeExpression)
          .Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_likeExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.Not.SameAs (_likeExpression));
      Assert.That (((SqlLikeExpression) result).Left, Is.SameAs (_likeExpression.Left));
      Assert.That (((SqlLikeExpression) result).Right, Is.SameAs (newRightExpression));
      Assert.That (((SqlLikeExpression) result).EscapeExpression, Is.SameAs (_likeExpression.EscapeExpression));
    }

    [Test]
    public void VisitChildren_ReturnsDifferentLeftExpression ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      var newLeftExpression = Expression.Constant (3);

      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.Left))
          .Returns (newLeftExpression)
          .Verifiable();
      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.Right))
          .Returns (_likeExpression.Right)
          .Verifiable();
      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.EscapeExpression))
          .Returns (_likeExpression.EscapeExpression)
          .Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_likeExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.Not.SameAs (_likeExpression));
      Assert.That (((SqlLikeExpression) result).Left, Is.SameAs (newLeftExpression));
      Assert.That (((SqlLikeExpression) result).Right, Is.SameAs (_likeExpression.Right));
      Assert.That (((SqlLikeExpression) result).EscapeExpression, Is.SameAs (_likeExpression.EscapeExpression));
    }

    [Test]
    public void VisitChildren_ReturnsDifferentEscapeExpression ()
    {
      var visitorMock = new Mock<ExpressionVisitor> (MockBehavior.Strict);
      var newEscapeExpression = Expression.Constant (3);

      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.Left))
          .Returns (_likeExpression.Left)
          .Verifiable();
      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.Right))
          .Returns (_likeExpression.Right)
          .Verifiable();
      visitorMock
          .Setup (mock => mock.Visit (_likeExpression.EscapeExpression))
          .Returns (newEscapeExpression)
          .Verifiable();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_likeExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (result, Is.Not.SameAs (_likeExpression));
      Assert.That (((SqlLikeExpression) result).Left, Is.SameAs (_likeExpression.Left));
      Assert.That (((SqlLikeExpression) result).Right, Is.SameAs (_likeExpression.Right));
      Assert.That (((SqlLikeExpression) result).EscapeExpression, Is.SameAs (newEscapeExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlLikeExpression, ISqlSpecificExpressionVisitor> (
          _likeExpression,
          mock => mock.VisitSqlLike (_likeExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_likeExpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _likeExpression.ToString();

      Assert.That (result, Is.EqualTo ("\"left\" LIKE \"right\" ESCAPE \"\\\""));
    }
  }
}