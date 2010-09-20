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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
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

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
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

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);  
    }

    [Test]
    public void VisitChildren_ReturnsSame ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();

      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.Left))
          .Return (_likeExpression.Left);
      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.Right))
          .Return (_likeExpression.Right);
      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.EscapeExpression))
          .Return (_likeExpression.EscapeExpression);
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_likeExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_likeExpression));
    }

    [Test]
    public void VisitChildren_ReturnsDifferentRightExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();
      var newRightExpression = Expression.Constant (3);

      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.Left))
          .Return (_likeExpression.Left);
      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.Right))
          .Return (newRightExpression);
      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.EscapeExpression))
          .Return (_likeExpression.EscapeExpression);
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_likeExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.SameAs (_likeExpression));
      Assert.That (((SqlLikeExpression) result).Left, Is.SameAs (_likeExpression.Left));
      Assert.That (((SqlLikeExpression) result).Right, Is.SameAs (newRightExpression));
      Assert.That (((SqlLikeExpression) result).EscapeExpression, Is.SameAs (_likeExpression.EscapeExpression));
    }

    [Test]
    public void VisitChildren_ReturnsDifferentLeftExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();
      var newLeftExpression = Expression.Constant (3);

      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.Left))
          .Return (newLeftExpression);
      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.Right))
          .Return (_likeExpression.Right);
      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.EscapeExpression))
          .Return (_likeExpression.EscapeExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_likeExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_likeExpression));
      Assert.That (((SqlLikeExpression) result).Left, Is.SameAs (newLeftExpression));
      Assert.That (((SqlLikeExpression) result).Right, Is.SameAs (_likeExpression.Right));
      Assert.That (((SqlLikeExpression) result).EscapeExpression, Is.SameAs (_likeExpression.EscapeExpression));
    }

    [Test]
    public void VisitChildren_ReturnsDifferentEscapeExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();
      var newEscapeExpression = Expression.Constant (3);

      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.Left))
          .Return (_likeExpression.Left);
      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.Right))
          .Return (_likeExpression.Right);
      visitorMock
          .Expect (mock => mock.VisitExpression (_likeExpression.EscapeExpression))
          .Return (newEscapeExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_likeExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
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
          mock => mock.VisitSqlLikeExpression (_likeExpression));
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