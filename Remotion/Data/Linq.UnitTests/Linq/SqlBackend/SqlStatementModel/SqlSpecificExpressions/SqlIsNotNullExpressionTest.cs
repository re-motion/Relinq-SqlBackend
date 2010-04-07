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
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlIsNotNullExpressionTest
  {
    private SqlIsNotNullExpression _sqlIsNotNullExpression;

    [SetUp]
    public void SetUp ()
    {
      _sqlIsNotNullExpression = new SqlIsNotNullExpression(Expression.Constant(2));
    }

    [Test]
    public void VisitChildren ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();
      var expression = Expression.Constant (3);
      
      visitorMock
          .Expect (mock => mock.VisitExpression (_sqlIsNotNullExpression.Expression))
          .Return (expression);
      
      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlIsNotNullExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_sqlIsNotNullExpression));
      Assert.That (((SqlIsNotNullExpression) result).Expression, Is.SameAs (expression));
    }

    // TODO Review 2528: Also add a test for VisitChildren that shows that the returned expression is the same if the inner expression is not changed

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlIsNotNullExpression, ISqlSpecificExpressionVisitor> (
          _sqlIsNotNullExpression,
          mock => mock.VisitSqlIsNotNullExpression (_sqlIsNotNullExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_sqlIsNotNullExpression);
    }
  }
}