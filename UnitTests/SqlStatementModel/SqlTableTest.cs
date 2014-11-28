// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Moq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class SqlTableTest
  {
    private ResolvedSimpleTableInfo _oldTableInfo;
    private ResolvedSimpleTableInfo _newTableInfo;
    private SqlTable _sqlTable;

    [SetUp]
    public void SetUp ()
    {
      _oldTableInfo = new ResolvedSimpleTableInfo (typeof (int), "table1", "t");
      _newTableInfo = new ResolvedSimpleTableInfo (typeof (string), "table2", "s");
      _sqlTable = new SqlTable (_oldTableInfo, JoinSemantics.Inner);
    }

    [Test]
    public void SameType ()
    {
      var newTableInfo = new ResolvedSimpleTableInfo (typeof (int), "table2", "s");
      _sqlTable.TableInfo = newTableInfo;

      Assert.That (_sqlTable.TableInfo.ItemType, Is.EqualTo (newTableInfo.ItemType));
    }

    [Test]
    public void DifferentType ()
    {
      Assert.That (
          () => _sqlTable.TableInfo = _newTableInfo,
          Throws.ArgumentException);
    }

    [Test]
    public void JoinSemantic ()
    {
      var sqlTableWithInnerJoinSemantic = new SqlTable (_oldTableInfo, JoinSemantics.Inner);
      var sqlTableWithLeftJoinSemantic = new SqlTable (_oldTableInfo, JoinSemantics.Left);

      Assert.That (sqlTableWithInnerJoinSemantic.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
      Assert.That (sqlTableWithLeftJoinSemantic.JoinSemantics, Is.EqualTo (JoinSemantics.Left));
    }

    [Test]
    public void ToString_WithoutJoins ()
    {
      var result = _sqlTable.ToString ();
      Assert.That (result, Is.EqualTo ("[table1] [t]"));
    }

    [Test]
    public void ToString_WithJoin ()
    {
      var joinedTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (int), "Cook", "c"), JoinSemantics.Inner);
      var joinCondition = Expression.Equal (new SqlLiteralExpression ("left"), new SqlLiteralExpression ("right"));

      _sqlTable.AddJoin (new SqlJoin(joinedTable, JoinSemantics.Inner, joinCondition));

      var result = _sqlTable.ToString ();

      Assert.That (result, Is.EqualTo ("[table1] [t] INNER JOIN [Cook] [c] ON (\"left\" == \"right\")"));
    }

    [Test]
    public void ToString_WithMultipleJoins ()
    {
      var joinedTable1 = new SqlTable (new ResolvedSimpleTableInfo (typeof (int), "Cook", "c"), JoinSemantics.Inner);
      var joinCondition1 = Expression.Equal (new SqlLiteralExpression ("left"), new SqlLiteralExpression ("right"));
      _sqlTable.AddJoin (new SqlJoin(joinedTable1, JoinSemantics.Left, joinCondition1));

      var joinedTable2 = new SqlTable (new ResolvedSimpleTableInfo (typeof (int), "Restaurant", "r"), JoinSemantics.Inner);
      var joinCondition2 = Expression.Equal (new SqlLiteralExpression ("left2"), new SqlLiteralExpression ("right2"));
      _sqlTable.AddJoin (new SqlJoin(joinedTable2, JoinSemantics.Inner, joinCondition2));

      var result = _sqlTable.ToString ();

      Assert.That (
          result,
          Is.EqualTo ("[table1] [t] LEFT JOIN [Cook] [c] ON (\"left\" == \"right\") INNER JOIN [Restaurant] [r] ON (\"left2\" == \"right2\")"));
    }

  }
}