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
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlLiteralExpressionTest
  {
    private SqlLiteralExpression _literalExpression;

    [SetUp]
    public void SetUp ()
    {
      _literalExpression = new SqlLiteralExpression (10);
    }

    [Test]
    public void Initialization_Typed ()
    {
      var intExpression = new SqlLiteralExpression (10);
      Assert.That (intExpression.Value, Is.EqualTo (10));
      Assert.That (intExpression.Type, Is.EqualTo (typeof (int)));

      var nullableIntExpression = new SqlLiteralExpression (10, true);
      Assert.That (nullableIntExpression.Value, Is.EqualTo (10));
      Assert.That (nullableIntExpression.Type, Is.EqualTo (typeof (int?)));

      var longExpression = new SqlLiteralExpression (10L);
      Assert.That (longExpression.Value, Is.EqualTo (10L));
      Assert.That (longExpression.Type, Is.EqualTo (typeof (long)));

      var nullableLongExpression = new SqlLiteralExpression (10L, true);
      Assert.That (nullableLongExpression.Value, Is.EqualTo (10L));
      Assert.That (nullableLongExpression.Type, Is.EqualTo (typeof (long?)));

      var stringExpression = new SqlLiteralExpression ("a");
      Assert.That (stringExpression.Value, Is.EqualTo ("a"));
      Assert.That (stringExpression.Type, Is.EqualTo (typeof (string)));

      Assert.That (() => new SqlLiteralExpression (null), Throws.TypeOf<ArgumentNullException>());

      var doubleExpression = new SqlLiteralExpression (10.12);
      Assert.That (doubleExpression.Value, Is.EqualTo (10.12));
      Assert.That (doubleExpression.Type, Is.EqualTo (typeof (double)));

      var nullableDoubleExpression = new SqlLiteralExpression (10.12, true);
      Assert.That (nullableDoubleExpression.Value, Is.EqualTo (10.12));
      Assert.That (nullableDoubleExpression.Type, Is.EqualTo (typeof (double?)));
    }

    [Test]
    public void VisitChildren_ReturnsThis ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionVisitor>();
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_literalExpression, visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_literalExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlLiteralExpression, ISqlSpecificExpressionVisitor> (
          _literalExpression,
          mock => mock.VisitSqlLiteral (_literalExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_literalExpression);
    }

    [Test]
    public void ToString_Int ()
    {
      Assert.That (_literalExpression.ToString(), Is.EqualTo ("10"));
    }

    [Test]
    public void ToString_String ()
    {
      var literalExpression = new SqlLiteralExpression ("test");
      Assert.That (literalExpression.ToString (), Is.EqualTo ("\"test\""));
    }
  }
}