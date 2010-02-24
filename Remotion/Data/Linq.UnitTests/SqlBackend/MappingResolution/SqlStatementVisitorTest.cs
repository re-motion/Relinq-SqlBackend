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
using Remotion.Data.Linq.UnitTests.TestDomain;
using SqlStatementVisitor=Remotion.Data.Linq.SqlBackend.MappingResolution.SqlStatementVisitor;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  [TestFixture]
  public class SqlStatementVisitorTest
  {
    private SqlStatement _sqlStatement;
    private SqlStatementResolverStub _resolver;
    private SqlStatementVisitor _sqlStatementVisitor;

    [SetUp]
    public void SetUp ()
    {
      _sqlStatement = new SqlStatement();

      // TODO: Consider adding SqlExpressionObjectMother with CreateSqlTable.
      var source = new ConstantTableSource (Expression.Constant ("Student", typeof (string)));
      _sqlStatement.FromExpression.TableSource = source;

      var tableReferenceExpression = new SqlTableReferenceExpression (typeof (Student), _sqlStatement.FromExpression);
      _sqlStatement.SelectProjection = tableReferenceExpression;
      
      _resolver = new SqlStatementResolverStub();
      _sqlStatementVisitor = new SqlStatementVisitor (_resolver);
    }

    [Test]
    public void VisitFromExpression_ReplacesTableSource ()
    {
      _sqlStatementVisitor.VisitSqlStatement (_sqlStatement);

      Assert.That (_sqlStatement.FromExpression.TableSource, Is.InstanceOfType (typeof (SqlTableSource)));
      Assert.That (((SqlTableSource) _sqlStatement.FromExpression.TableSource).TableName, Is.EqualTo ("Student"));
      Assert.That (((SqlTableSource) _sqlStatement.FromExpression.TableSource).TableAlias, Is.EqualTo ("s"));
    }

    [Test]
    public void VisitSelectProjection_CreatesSqlColumnListExpression ()
    {
      _sqlStatementVisitor.VisitSqlStatement (_sqlStatement);
      
      Assert.That (((SqlColumnListExpression) _sqlStatement.SelectProjection).Columns.Count, Is.EqualTo (3));
      // TODO: Test Columns[x].SqlTable
      Assert.That (((SqlColumnListExpression) _sqlStatement.SelectProjection).Columns[0].ColumnName, Is.EqualTo ("ID"));
      Assert.That (((SqlColumnListExpression) _sqlStatement.SelectProjection).Columns[1].ColumnName, Is.EqualTo ("Name"));
      Assert.That (((SqlColumnListExpression) _sqlStatement.SelectProjection).Columns[2].ColumnName, Is.EqualTo ("City"));
    }
  }
}