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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlEntityDefinitionExpressionTest
  {
    private SqlEntityExpression _entityExpression;
    private SqlColumnExpression _columnExpression1;
    private SqlColumnExpression _columnExpression2;
    private SqlColumnExpression _columnExpression3;
    private SqlColumnExpression[] _orginalColumns;
    private ReadOnlyCollection<SqlColumnExpression> _originalColumnsReadonly;

    [SetUp]
    public void SetUp ()
    {
      _columnExpression1 = new SqlColumnDefinitionExpression (typeof (int), "t", "ID", true);
      _columnExpression2 = new SqlColumnDefinitionExpression (typeof (int), "t", "Name", false);
      _columnExpression3 = new SqlColumnDefinitionExpression (typeof (int), "t", "City", false);
      _orginalColumns = new[] { _columnExpression1, _columnExpression2, _columnExpression3 };
      _entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "t", null, e => e.GetColumn (typeof (int), "ID", true), _orginalColumns);
      _originalColumnsReadonly = _entityExpression.Columns;
    }

    [Test]
    public void GetColumn ()
    {
      var column = _entityExpression.GetColumn (typeof (int), "Test", false);

      Assert.That (column.OwningTableAlias, Is.EqualTo ("t"));
      Assert.That (column.ColumnName, Is.EqualTo("Test"));
      Assert.That (column.IsPrimaryKey, Is.False);
      Assert.That (column.Type, Is.EqualTo(typeof (int)));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlEntityExpression, IResolvedSqlExpressionVisitor> (
          _entityExpression,
          mock => mock.VisitSqlEntity (_entityExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_entityExpression);
    }

    [Test]
    public void VisitChildren_NoColumnChanged ()
    {
      var visitorMock = new Mock<ExpressionVisitor>();
      visitorMock.Setup (mock => mock.Visit (_originalColumnsReadonly[0])).Returns (_originalColumnsReadonly[0]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (_originalColumnsReadonly[1])).Returns (_originalColumnsReadonly[1]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (_originalColumnsReadonly[2])).Returns (_originalColumnsReadonly[2]).Verifiable();

      var expression = ExtensionExpressionTestHelper.CallVisitChildren (_entityExpression, visitorMock.Object);

      visitorMock.Verify();
      Assert.That (expression, Is.SameAs (_entityExpression));
    }

    [Test]
    public void VisitChildren_ChangeColumn ()
    {
      var newColumnExpression = new SqlColumnDefinitionExpression (typeof (string), "o", "Test", false);

      var visitorMock = new Mock<ExpressionVisitor>();
      var expectedColumns = new[] { _columnExpression1, newColumnExpression, _columnExpression3 };

      visitorMock.Setup (mock => mock.Visit (_originalColumnsReadonly[0])).Returns (expectedColumns[0]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (_originalColumnsReadonly[1])).Returns (expectedColumns[1]).Verifiable();
      visitorMock.Setup (mock => mock.Visit (_originalColumnsReadonly[2])).Returns (expectedColumns[2]).Verifiable();

      var expression = (SqlEntityExpression) ExtensionExpressionTestHelper.CallVisitChildren (_entityExpression, visitorMock.Object);

      Assert.That (expression, Is.Not.SameAs (_entityExpression));
      Assert.That (expression.Type, Is.SameAs (_entityExpression.Type));
      Assert.That (expression.Columns, Is.EqualTo (expectedColumns));
    }

    [Test]
    public void CreateReference ()
    {
      var columns = new[]
                   {
                       new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false),
                       new SqlColumnDefinitionExpression (typeof (string), "c", "FirstName", false)
                   };

      var entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, e => e.GetColumn (typeof (int), "ID", true), columns);

      var result = entityExpression.CreateReference("c1", typeof(Cook));

      var expectedResult = new SqlEntityReferenceExpression (typeof (Cook), "c1", null, entityExpression);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Update ()
    {
      var columns = new[]
                   {
                       new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false),
                       new SqlColumnDefinitionExpression (typeof (string), "c", "FirstName", false)
                   };

      var entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, e => e.GetColumn (typeof (int), "ID", true), columns);

      var result = entityExpression.Update (typeof (Kitchen), "f", null);

      var expectedResult = new SqlEntityDefinitionExpression (typeof (Kitchen), "f", null, entityExpression.IdentityExpressionGenerator, columns);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ToString_UnnamedEntity ()
    {
      var result = _entityExpression.ToString();

      Assert.That (result, Is.EqualTo ("[t]"));
    }

    [Test]
    public void ToString_NamedEntity ()
    {
      var namedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), "e1", "t");
      var result = namedEntity.ToString ();

      Assert.That (result, Is.EqualTo ("[t] AS [e1]"));
    }
  }
}