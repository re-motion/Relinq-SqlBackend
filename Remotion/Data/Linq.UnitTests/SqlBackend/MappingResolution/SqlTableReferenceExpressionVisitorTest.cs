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
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  [TestFixture]
  public class SqlTableReferenceExpressionVisitorTest
  {
    private SqlStatementResolverStub _resolver;

    [SetUp]
    public void SetUp ()
    {
      _resolver = new SqlStatementResolverStub();
    }

    [Test]
    public void VisitSqlTableReferenceExpression_CreatesSqlColumnListExpression ()
    {
      // TODO: Consider adding SqlExpressionObjectMother with CreateSqlTableReference.
      var sqlTable = new SqlTable (new ConstantTableSource (Expression.Constant ("Student", typeof (string))));
      var tableReferenceExpression = new SqlTableReferenceExpression (typeof (Student), sqlTable);

      var sqlColumnListExpression = SqlTableReferenceExpressionVisitor.TranslateSqlTableReferenceExpressions (tableReferenceExpression, _resolver);
      
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns.Count, Is.EqualTo (3));
      // TODO: Test Columns[x].SqlTable
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns[0].ColumnName, Is.EqualTo ("ID"));
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns[1].ColumnName, Is.EqualTo ("Name"));
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns[2].ColumnName, Is.EqualTo ("City"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), 
        ExpectedMessage = "The given expression type 'NotSupportedExpression' is not supported in from clauses. (Expression: '[2147483647]')")]
    public void UnknownExpression ()
    {
      var unknownExpression = new NotSupportedExpression (typeof (int));
      SqlTableReferenceExpressionVisitor.TranslateSqlTableReferenceExpressions (unknownExpression, _resolver);
    }
  }
}