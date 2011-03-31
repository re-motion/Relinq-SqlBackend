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
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
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
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_ChecksType ()
    {
      new SqlLiteralExpression (true);
    }

    [Test]
    public void VisitChildren_ReturnsThis ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();
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
          mock => mock.VisitSqlLiteralExpression (_literalExpression));
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