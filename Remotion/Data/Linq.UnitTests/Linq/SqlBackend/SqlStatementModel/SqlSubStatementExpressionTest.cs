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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlSubStatementExpressionTest
  {
    private SqlSubStatementExpression _expression;
    private SqlStatement _sqlStatement;

    [SetUp]
    public void SetUp ()
    {
      _sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      _expression = new SqlSubStatementExpression (_sqlStatement);
    }

    [Test]
    public void Initialization_ItemType ()
    {
      Assert.That (_expression.Type, Is.EqualTo (_sqlStatement.DataInfo.DataType));
    }

    [Test]
    public void VisitChildren_ReturnsThis_WithoutCallingVisitMethods ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      Assert.That (result, Is.SameAs (_expression));
      visitorMock.VerifyAllExpectations();
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlSubStatementExpression, ISqlSubStatementVisitor> (
          _expression,
          mock => mock.VisitSqlSubStatementExpression (_expression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_expression);
    }

    [Test]
    public void To_String ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var expression = new SqlSubStatementExpression (sqlStatement);
      
      var result = expression.ToString();

      Assert.That (result, Is.EqualTo ("SELECT [t].[Cook] FROM Cook"));
    }
  }
}