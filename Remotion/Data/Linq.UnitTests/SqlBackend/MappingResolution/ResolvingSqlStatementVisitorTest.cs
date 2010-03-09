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
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingSqlStatementVisitorTest
  {
    private SqlStatement _sqlStatement;
    private SqlStatementResolverStub _resolver;
    private SqlMemberExpression _sqlMemberExpression;
    private UniqueIdentifierGenerator _uniqueIdentifierGenerator;

    [SetUp]
    public void SetUp ()
    {
      var tableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo_TypeIsCook();
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (tableInfo);

      _sqlMemberExpression = new SqlMemberExpression (sqlTable, typeof (Cook).GetProperty ("IsStarredCook"));
      _sqlStatement = new SqlStatement (_sqlMemberExpression, sqlTable);
      _resolver = new SqlStatementResolverStub();
      _uniqueIdentifierGenerator = new UniqueIdentifierGenerator ();
    }

    [Test]
    public void VisitFromExpression_ResolvesTableInfo ()
    {
      ResolvingSqlStatementVisitor.ResolveExpressions (_sqlStatement, _resolver, _uniqueIdentifierGenerator);

      Assert.That (_sqlStatement.FromExpression.TableInfo, Is.InstanceOfType (typeof (ResolvedTableInfo)));
    }

    [Test]
    public void VisitSelectProjection_CreatesSqlColumnListExpression ()
    {
      ResolvingSqlStatementVisitor.ResolveExpressions (_sqlStatement, _resolver, _uniqueIdentifierGenerator);

      Assert.That (_sqlStatement.SelectProjection, Is.TypeOf (typeof (SqlColumnExpression)));
    }

    [Test]
    public void VisitTopExpression_ResolvesExpression ()
    {
      _sqlStatement.TopExpression = _sqlMemberExpression;
      ResolvingSqlStatementVisitor.ResolveExpressions (_sqlStatement, _resolver, _uniqueIdentifierGenerator);

      Assert.That (_sqlStatement.TopExpression, Is.TypeOf (typeof (SqlColumnExpression)));
    }

    [Test]
    public void VisitWhereCondition_ResolvesExpression ()
    {
      _sqlStatement.WhereCondition = _sqlMemberExpression;
      ResolvingSqlStatementVisitor.ResolveExpressions (_sqlStatement, _resolver, _uniqueIdentifierGenerator);

      Assert.That (_sqlStatement.WhereCondition, Is.TypeOf (typeof (SqlColumnExpression)));
    }
  }
}