// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class SqlTableReferenceExpressionTest
  {
    private SqlTableReferenceExpression _tableReferenceExpression;

    [SetUp]
    public void SetUp ()
    {
      _tableReferenceExpression = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable(typeof(Cook)));
    }

    [Test]
    public void Initialize ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo();
      Assert.That (new SqlTableReferenceExpression (sqlTable).Type, Is.EqualTo (sqlTable.TableInfo.ItemType));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlTableReferenceExpression, IUnresolvedSqlExpressionVisitor> (
          _tableReferenceExpression,
          mock => mock.VisitSqlTableReferenceExpression (_tableReferenceExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_tableReferenceExpression);
    }

    [Test]
    public void ToString_SqlTableWithResolvedTableInfo ()
    {
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Left);
      var expression = new SqlTableReferenceExpression (sqlTable);
      var result = expression.ToString();

      Assert.That (result, Is.EqualTo ("TABLE-REF(c)"));
    }

    [Test]
    public void ToString_SqlTableWithUnresolvedTableInfo ()
    {
      var sqlTable = new SqlTable (new UnresolvedTableInfo (typeof (Cook)), JoinSemantics.Left);
      var expression = new SqlTableReferenceExpression (sqlTable);
      var result = expression.ToString();

      Assert.That (result, Is.EqualTo ("TABLE-REF(UnresolvedTableInfo(Cook))"));
    }

    [Test]
    public void ToString_SqlJoinedTableWithResolvedJoinInfo ()
    {
      var sqlTable =
          new SqlJoinedTable (
              new ResolvedJoinInfo (
                  new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), Expression.Constant ("leftKey"), Expression.Constant ("rightKey")),
              JoinSemantics.Left);
      var expression = new SqlTableReferenceExpression (sqlTable);
      var result = expression.ToString();

      Assert.That (result, Is.EqualTo ("TABLE-REF(c)"));
    }

    [Test]
    public void ToString_SqlJoinedTableWithUnresolvedJoinInfo ()
    {
      var sqlTable =
          new SqlJoinedTable (
              new UnresolvedJoinInfo (
                  new SqlEntityDefinitionExpression (typeof (Cook), "c", "Test", new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true)),
                  typeof (Cook).GetProperty ("Substitution"),
                  JoinCardinality.One), JoinSemantics.Left);
      var expression = new SqlTableReferenceExpression (sqlTable);
      var result = expression.ToString();

      Assert.That (result, Is.EqualTo ("TABLE-REF(UnresolvedJoinInfo(Cook))"));
    }

    [Test]
    public void ToString_OtherSqlTable ()
    {
      var sqlTable = new OtherSqlTable (typeof (Cook), JoinSemantics.Left);
      var expression = new SqlTableReferenceExpression (sqlTable);
      var result = expression.ToString();

      Assert.That (result, Is.EqualTo ("TABLE-REF (OtherSqlTable (Cook))"));
    }
  }

  internal class OtherSqlTable : SqlTableBase
  {
    public OtherSqlTable (Type itemType, JoinSemantics joinSemantics)
        : base(itemType, joinSemantics)
    {
    }

    public override void Accept (ISqlTableBaseVisitor visitor)
    {
      throw new NotImplementedException();
    }

    public override IResolvedTableInfo GetResolvedTableInfo ()
    {
      throw new NotImplementedException();
    }
  }
}