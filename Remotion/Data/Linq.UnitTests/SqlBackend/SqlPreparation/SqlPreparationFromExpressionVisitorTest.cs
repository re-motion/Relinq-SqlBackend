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
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationFromExpressionVisitorTest
  {
    [Test]
    public void GetTableForFromExpression_ConstantExpression_ReturnsUnresolvedTable ()
    {
      var expression = Expression.Constant (new Cook[0]);

      var result = SqlPreparationFromExpressionVisitor.GetTableForFromExpression (expression, typeof (Cook));

      Assert.That (result, Is.TypeOf (typeof (SqlTable)));

      var tableInfo = ((SqlTable) result).TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (UnresolvedTableInfo)));

      Assert.That (((UnresolvedTableInfo) tableInfo).ConstantExpression, Is.SameAs (expression));
      Assert.That (tableInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void GetTableForFromExpression_SqlMemberExpression_ReturnsJoinedTable ()
    {
      // from r in Restaurant => sqlTable 
      // from c in r.Cooks => MemberExpression (QSRExpression (r), "Cooks") => SqlMemberExpression (sqlTable, "Cooks") => Join: sqlTable.Cooks

      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (memberInfo.DeclaringType);
      var sqlMemberExpression = new SqlMemberExpression (sqlTable, memberInfo);

      var result = SqlPreparationFromExpressionVisitor.GetTableForFromExpression (sqlMemberExpression, typeof (Cook));

      Assert.That (result, Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (result, Is.SameAs (sqlTable.GetJoin (memberInfo)));

      var joinInfo = ((SqlJoinedTable) result).JoinInfo;
      Assert.That (joinInfo, Is.TypeOf (typeof (UnresolvedJoinInfo)));

      Assert.That (((UnresolvedJoinInfo) joinInfo).MemberInfo, Is.EqualTo (memberInfo));
      Assert.That (((UnresolvedJoinInfo) joinInfo).Cardinality, Is.EqualTo (JoinCardinality.Many));
      Assert.That (joinInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "Expressions of type 'CustomExpression' cannot be used as the SqlTables of a from clause.")]
    public void GetTableForFromExpression_UnsupportedExpression_Throws ()
    {
      var customExpression = new CustomExpression (typeof (Cook[]));

      SqlPreparationFromExpressionVisitor.GetTableForFromExpression (customExpression, typeof (Cook));
    }
  }
}