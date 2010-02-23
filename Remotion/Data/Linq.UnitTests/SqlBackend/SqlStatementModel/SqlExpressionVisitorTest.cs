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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;
using System.Linq;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlExpressionVisitorTest
  {
    private SqlStatementResolverStub _resolver;

    [SetUp]
    public void SetUp ()
    {
      _resolver = new SqlStatementResolverStub();
    }

    [Test]
    public void VisitSqlTableExpression_CreatesSqlTableSource ()
    {
      var tableExpression = new SqlTableExpression (typeof (Student), new ConstantTableSource (Expression.Constant ("Student",typeof(string))));
      var expectedTableExpression = SqlExpressionVisitor.TranslateSqlTableExpression (tableExpression, _resolver);

      Assert.That (((SqlTableExpression)expectedTableExpression).TableSource, Is.InstanceOfType (typeof (SqlTableSource)));
      Assert.That (((SqlTableSource) ((SqlTableExpression) expectedTableExpression).TableSource).TableName, Is.EqualTo ("Student"));
      Assert.That (((SqlTableSource) ((SqlTableExpression) expectedTableExpression).TableSource).TableAlias, Is.EqualTo ("s"));
    }

    [Test]
    public void VisitSqlTableReferenceExpression_CreatesSqlColumnListExpression ()
    {
      var tableExpression = new SqlTableExpression (typeof (Student), new ConstantTableSource (Expression.Constant ("Student", typeof (string))));
      var tableReferenceExpression = new SqlTableReferenceExpression (typeof (Student), tableExpression);

      var sqlColumnListExpression = SqlExpressionVisitor.TranslateSqlTableExpression (tableReferenceExpression, _resolver);
      List<string> studentColumns = new List<string>(typeof (Student).GetProperties().Select (s => s.Name));

      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns.Count, Is.EqualTo (studentColumns.Count));
      Assert.That (((SqlColumnListExpression) sqlColumnListExpression).Columns.Select (n => n.ColumnName).ToList(), Is.EquivalentTo (studentColumns));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), 
      ExpectedMessage = "The given expression type 'NotSupportedExpression' is not supported in from clauses. (Expression: '[2147483647]')")]
    public void UnknownExpression ()
    {
      var unknownExpression = new NotSupportedExpression (typeof (int));
      SqlExpressionVisitor.TranslateSqlTableExpression (unknownExpression, _resolver);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void NoConstantTableSource ()
    {
      var tableExpression = new SqlTableExpression (typeof (Student), new UnknownTableSource());
      SqlExpressionVisitor.TranslateSqlTableExpression (tableExpression, _resolver);
    }

    public class UnknownTableSource : AbstractTableSource
    { 
    }

  }
}