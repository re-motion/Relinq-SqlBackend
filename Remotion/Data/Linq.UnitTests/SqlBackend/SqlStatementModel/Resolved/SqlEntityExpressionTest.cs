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
using System.Collections.ObjectModel;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlEntityExpressionTest
  {
    private SqlEntityExpression _entityExpression;
    private SqlColumnExpression _columnExpression1;
    private SqlColumnExpression _columnExpression2;
    private SqlColumnExpression _columnExpression3;
    private SqlTableReferenceExpression _tableReferenceExpression;
    private SqlColumnExpression[] _orginalColumns;
    private ReadOnlyCollection<SqlColumnExpression> _originalColumnsReadonly;

    [SetUp]
    public void SetUp ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));

      _tableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      _columnExpression1 = new SqlColumnExpression (typeof (int), "t", "ID");
      _columnExpression2 = new SqlColumnExpression (typeof (int), "t", "Name");
      _columnExpression3 = new SqlColumnExpression (typeof (int), "t", "City");
      _orginalColumns = new[] { _columnExpression1, _columnExpression2, _columnExpression3 };
      _entityExpression = new SqlEntityExpression (_tableReferenceExpression.Type, _columnExpression1, _orginalColumns);
      _originalColumnsReadonly = _entityExpression.ProjectionColumns;
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlEntityExpression, IResolvedSqlExpressionVisitor> (
          _entityExpression, 
          mock => mock.VisitSqlEntityExpression (_entityExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_entityExpression);
    }

    [Test]
    public void VisitChildren_NoColumnChanged ()
    {
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor>();
      visitorMock.Expect (mock => mock.VisitAndConvert (_originalColumnsReadonly, "VisitChildren")).Return (_originalColumnsReadonly);
      visitorMock.Replay();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_entityExpression, visitorMock);
      
      visitorMock.VerifyAllExpectations();
      Assert.That (expression, Is.SameAs (_entityExpression));
    }

    [Test]
    public void VisitChildren_ChangeColumn ()
    {
      var newColumnExpression = new SqlColumnExpression (typeof (string), "o", "Test");

      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();
      var expectedColumns = new[] { _columnExpression1, newColumnExpression, _columnExpression3 };
      
      visitorMock.Expect (mock => mock.VisitAndConvert (_originalColumnsReadonly, "VisitChildren")).Return (Array.AsReadOnly(expectedColumns));
      visitorMock.Replay ();

      var expression = (SqlEntityExpression) ExtensionExpressionTestHelper.CallVisitChildren (_entityExpression, visitorMock);

      Assert.That (expression, Is.Not.SameAs(_entityExpression));
      Assert.That (expression.Type, Is.SameAs (_entityExpression.Type));
      Assert.That (expression.ProjectionColumns, Is.EqualTo (expectedColumns));
    }
  }
}