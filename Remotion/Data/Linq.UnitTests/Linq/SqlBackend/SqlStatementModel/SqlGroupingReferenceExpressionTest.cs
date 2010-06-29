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
using System.Runtime.InteropServices;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlGroupingReferenceExpressionTest
  {
    private SqlGroupingSelectExpression _referencedExpression;
    private SqlGroupingReferenceExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _referencedExpression = new SqlGroupingSelectExpression (Expression.Constant ("key"), Expression.Constant ("element"));
      _expression = new SqlGroupingReferenceExpression (_referencedExpression, "c");
    }

    [Test]
    public void Initialize ()
    {
      Assert.That (_expression.Type, Is.SameAs (_referencedExpression.Type));
    }

    [Test]
    public void VisitChildren_NoExpressionChanged ()
    {
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();
      visitorMock.Replay ();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (expression, Is.SameAs (_expression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlGroupingReferenceExpression, ISqlGroupingReferenceExpressionVisitor> (
          _expression,
          mock => mock.VisitSqlGroupingReferenceExpression (_expression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_expression);
    }

    [Test]
    public void To_String ()
    {
      Assert.That (_expression.ToString(), Is.EqualTo ("GROUPING-REF(c)"));
    }
  }
}