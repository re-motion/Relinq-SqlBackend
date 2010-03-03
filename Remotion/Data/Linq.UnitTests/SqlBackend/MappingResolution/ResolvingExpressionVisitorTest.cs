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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingExpressionVisitorTest
  {
    private SqlStatementResolverStub _resolver;
    private ConstantTableSource _source;
    private SqlTable _sqlTable;
    private SqlTableSource _constraint;
    private UniqueIdentifierGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _resolver = new SqlStatementResolverStub();
      _source = new ConstantTableSource (Expression.Constant (new Cook { FirstName = "Test" }, typeof (Cook))); // TODO: Move to object mother
      _sqlTable = new SqlTable (_source);
      _constraint = new SqlTableSource (typeof (Cook), "Cook", "c");
      _generator = new UniqueIdentifierGenerator();
    }

    [Test]
    public void VisitSqlTableReferenceExpression_CreatesSqlColumnListExpression ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);

      var sqlColumnListExpression = ResolvingExpressionVisitor.ResolveExpressions (tableReferenceExpression, _resolver, _generator);

      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns.Count, Is.EqualTo (3));
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns[0].OwningTableAlias, Is.EqualTo (_constraint.TableAlias));
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns[0].OwningTableAlias, Is.EqualTo (_constraint.TableAlias));
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns[0].OwningTableAlias, Is.EqualTo (_constraint.TableAlias));
      
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns[0].ColumnName, Is.EqualTo ("ID"));
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns[1].ColumnName, Is.EqualTo ("Name"));
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns[2].ColumnName, Is.EqualTo ("City"));
    }

    [Test]
    public void VisitSqlMemberExpression_CreatesSqlColumnExpression() 
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      _sqlTable.TableSource = new JoinedTableSource (memberInfo);
      var memberExpression = new SqlMemberExpression (_sqlTable, memberInfo);
      var sqlColumnExpression = ResolvingExpressionVisitor.ResolveExpressions (memberExpression, _resolver, _generator);
      
      Assert.That (sqlColumnExpression, Is.TypeOf (typeof(SqlColumnExpression)));
      Assert.That (((SqlColumnExpression) sqlColumnExpression).OwningTableAlias, Is.EqualTo (_constraint.TableAlias));
      Assert.That (((SqlColumnExpression) sqlColumnExpression).ColumnName, Is.EqualTo ("FirstName"));
    }

    [Test]
    public void UnknownExpression ()
    {
      var unknownExpression = new NotSupportedExpression (typeof (int));
      var result = ResolvingExpressionVisitor.ResolveExpressions (unknownExpression, _resolver, _generator);

      Assert.That (result, Is.EqualTo (unknownExpression));
    }
  }
}