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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Unresolved
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
    public new void ToString ()
    {
      var result = _tableReferenceExpression.ToString();

      Assert.That (result, Is.EqualTo ("TABLE-REF(TABLE(Cook))"));
    }
  }
}