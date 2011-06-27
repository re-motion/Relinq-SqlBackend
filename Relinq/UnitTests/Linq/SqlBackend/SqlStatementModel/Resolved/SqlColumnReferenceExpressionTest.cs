// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlColumnReferenceExpressionTest
  {
    private SqlEntityDefinitionExpression _entityExpression;
    private SqlColumnReferenceExpression _columnExpression;

    [SetUp]
    public void SetUp ()
    {
      _entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", true));
      
      _columnExpression = new SqlColumnReferenceExpression (typeof (string), "c", "columnName", false, _entityExpression);
    }

    [Test]
    public void Initialize_SetReferenceEntity ()
    {
      Assert.That (_columnExpression.ReferencedEntity, Is.SameAs (_entityExpression));
    }

    [Test]
    public void Update ()
    {
      var result = _columnExpression.Update (typeof (char), "f", "test", false);

      var expectedResult = new SqlColumnReferenceExpression (typeof (char), "f", "test", false, _entityExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (result, expectedResult);
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlColumnReferenceExpression, ISqlColumnExpressionVisitor> (
          _columnExpression,
          mock => mock.VisitSqlColumnReferenceExpression (_columnExpression));
    }


    [Test]
    public void Accept_VisitorSupportingExpressionType_IResolvedSqlExpressionVisitor ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlColumnReferenceExpression, IResolvedSqlExpressionVisitor> (
          _columnExpression,
          mock => mock.VisitSqlColumnExpression(_columnExpression));
    }

    [Test]
    public void ToString_NoEntityName ()
    {
      var referencedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null);
      var columnExpression = new SqlColumnReferenceExpression (typeof (int), "t0", "ID", true, referencedEntity);
      var result = columnExpression.ToString ();

      Assert.That (result, Is.EqualTo ("[t0].[ID] (REF)"));
    }

    [Test]
    public void ToString_WithEntityName ()
    {
      var referencedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), "e1");
      var columnExpression = new SqlColumnReferenceExpression (typeof (int), "t0", "ID", true, referencedEntity);
      var result = columnExpression.ToString ();

      Assert.That (result, Is.EqualTo ("[t0].[e1_ID] (REF)"));
    }
  }
}