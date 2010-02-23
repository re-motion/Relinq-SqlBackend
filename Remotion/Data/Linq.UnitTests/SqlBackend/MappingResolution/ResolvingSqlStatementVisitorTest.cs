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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingSqlStatementVisitorTest
  {
    private SqlStatement _sqlStatement;
    private SqlStatementResolverStub _resolver;
    private ResolvingSqlStatementVisitor _resolvingSqlStatementVisitor;

    [SetUp]
    public void SetUp ()
    {
      _sqlStatement = new SqlStatement();
      var tableExpression = new SqlTableExpression (typeof (Student), new ConstantTableSource (Expression.Constant ("Student",typeof(string))));
      var tableReferenceExpression = new SqlTableReferenceExpression (typeof (Student), tableExpression);
      _sqlStatement.FromExpression = tableExpression;
      _sqlStatement.SelectProjection = tableReferenceExpression;
      _resolver = new SqlStatementResolverStub();
      _resolvingSqlStatementVisitor = new ResolvingSqlStatementVisitor (_resolver);
    }

    [Test]
    public void VisitFromExpression_ReplacesTableSource ()
    {
      _resolvingSqlStatementVisitor.VisitSqlStatement (_sqlStatement);

      Assert.That (_sqlStatement.FromExpression.TableSource, Is.InstanceOfType (typeof (SqlTableSource)));
      Assert.That (
          ((SqlTableSource) _sqlStatement.FromExpression.TableSource).TableName, Is.EqualTo ("Student"));
      Assert.That (
          ((SqlTableSource) _sqlStatement.FromExpression.TableSource).TableAlias, Is.EqualTo ("s"));
    }

    [Test]
    public void VisitSelectProjection_CreatesSqlColumnListExpression ()
    {
      _resolvingSqlStatementVisitor.VisitSqlStatement (_sqlStatement);
      List<string> studentColumns = new List<string> (typeof (Student).GetProperties ().Select (s => s.Name));

      Assert.That (((SqlColumnListExpression) _sqlStatement.SelectProjection).Columns.Count, Is.EqualTo (studentColumns.Count));
      Assert.That (((SqlColumnListExpression) _sqlStatement.SelectProjection).Columns.Select (n => n.ColumnName).ToList (), Is.EquivalentTo (studentColumns));
    }
  }
}