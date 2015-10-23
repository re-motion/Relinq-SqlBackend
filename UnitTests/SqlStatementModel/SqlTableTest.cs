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
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class SqlTableTest
  {
    [Test]
    public void TableInfo_Setter_SameType ()
    {
      var oldTableInfo = SqlStatementModelObjectMother.CreateTableInfo (typeof (int));
      var sqlTable = new SqlTable (oldTableInfo);

      var newTableInfo = SqlStatementModelObjectMother.CreateTableInfo (typeof (int));
      sqlTable.TableInfo = newTableInfo;

      Assert.That (sqlTable.TableInfo.ItemType, Is.EqualTo (newTableInfo.ItemType));
    }

    [Test]
    public void TableInfo_Setter_DifferentType ()
    {
      var oldTableInfo = SqlStatementModelObjectMother.CreateTableInfo (typeof (int));
      var sqlTable = new SqlTable (oldTableInfo);

      var newTableInfo = SqlStatementModelObjectMother.CreateTableInfo (typeof (string));

      Assert.That (
          () => sqlTable.TableInfo = newTableInfo,
          Throws.ArgumentException);
    }

    [Test]
    public void GetOrAddMemberBasedLeftJoin_NewEntry_IsAddedToOrderedListOfJoins()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));
      var memberInfo = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();
      var leftJoinData = SqlStatementModelObjectMother.CreateLeftJoinData();

      var result = sqlTable.GetOrAddMemberBasedLeftJoin (memberInfo, () => leftJoinData);

      Assert.That (result.JoinedTable, Is.SameAs (leftJoinData.JoinedTable));
      Assert.That (result.JoinCondition, Is.SameAs (leftJoinData.JoinCondition));
      Assert.That (result.JoinSemantics, Is.EqualTo (JoinSemantics.Left));
      Assert.That (sqlTable.Joins, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetOrAddMemberBasedLeftJoin_MultipleNewEntries_AreAddedInOrder()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));

      var result1 = sqlTable.GetOrAddMemberBasedLeftJoin (
          SqlStatementModelObjectMother.GetKitchenCookMemberInfo(),
          SqlStatementModelObjectMother.CreateLeftJoinData);
      var result2 = sqlTable.GetOrAddMemberBasedLeftJoin (
          SqlStatementModelObjectMother.GetKitchenRestaurantMemberInfo(),
          SqlStatementModelObjectMother.CreateLeftJoinData);

      Assert.That (sqlTable.Joins, Is.EqualTo (new[] { result1, result2 }));
    }

    [Test]
    public void GetOrAddMemberBasedLeftJoin_ExistingEntry_RetrievesExistingEntry()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));
      var memberInfo = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();
      var existingEntry = sqlTable.GetOrAddMemberBasedLeftJoin (memberInfo, SqlStatementModelObjectMother.CreateLeftJoinData);

      var result = sqlTable.GetOrAddMemberBasedLeftJoin (memberInfo, () => { throw new InvalidOperationException ("Must not be called."); });

      Assert.That (result, Is.SameAs (existingEntry));
      Assert.That (sqlTable.Joins, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetJoinByMember_ExistingEntry ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));
      var memberInfo = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();
      var existingEntry = sqlTable.GetOrAddMemberBasedLeftJoin (memberInfo, SqlStatementModelObjectMother.CreateLeftJoinData);

      var result = sqlTable.GetJoinByMember (memberInfo);

      Assert.That (result, Is.SameAs (existingEntry));
    }

    [Test]
    public void GetJoinByMember_NonExistingEntry_Throws ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));
      var memberInfo = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();

      Assert.That(() => sqlTable.GetJoinByMember (memberInfo), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void AddJoinForExplicitQuerySource_AddsJoinInOrder ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));

      var existingEntry = SqlStatementModelObjectMother.CreateSqlJoin();
      sqlTable.AddJoinForExplicitQuerySource (existingEntry);

      var newEntry = SqlStatementModelObjectMother.CreateSqlJoin();
      sqlTable.AddJoinForExplicitQuerySource (newEntry);

      Assert.That (sqlTable.Joins, Is.EqualTo (new[] { existingEntry, newEntry }));
    }

    [Test]
    public void GetJoins_ReturnsMemberBasedJoinsInOrder_TheReturnsJoinsForExplictQuerySourceInOrder ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));

      var entry1 = SqlStatementModelObjectMother.CreateSqlJoin();
      sqlTable.AddJoinForExplicitQuerySource (entry1);

      var entry2 = sqlTable.GetOrAddMemberBasedLeftJoin (
          SqlStatementModelObjectMother.GetKitchenCookMemberInfo(),
          SqlStatementModelObjectMother.CreateLeftJoinData);

      var entry3 = SqlStatementModelObjectMother.CreateSqlJoin();
      sqlTable.AddJoinForExplicitQuerySource (entry3);

      var entry4 = sqlTable.GetOrAddMemberBasedLeftJoin (
          SqlStatementModelObjectMother.GetCookSubstitutionMemberInfo(),
          SqlStatementModelObjectMother.CreateLeftJoinData);

      Assert.That (sqlTable.Joins, Is.EqualTo (new[] { entry2, entry4, entry1, entry3 }));
    }

    [Test]
    public void SubstituteJoins_ReplacesJoin_OrderRemains ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Kitchen));
      
      var originalJoin1 = SqlStatementModelObjectMother.CreateSqlJoin();
      sqlTable.AddJoinForExplicitQuerySource (originalJoin1);
      
      var originalJoin2 = SqlStatementModelObjectMother.CreateSqlJoin();
      sqlTable.AddJoinForExplicitQuerySource (originalJoin2);
      
      var originalJoin3 = SqlStatementModelObjectMother.CreateSqlJoin();
      sqlTable.AddJoinForExplicitQuerySource (originalJoin3);

      var replacementJoin1 = SqlStatementModelObjectMother.CreateSqlJoin();
      var replacementJoin3 = SqlStatementModelObjectMother.CreateSqlJoin();
      var substitutions = new Dictionary<SqlJoin, SqlJoin> { { originalJoin1, replacementJoin1 }, { originalJoin3, replacementJoin3 } };

      sqlTable.SubstituteJoins (substitutions);

      Assert.That (sqlTable.Joins, Is.EqualTo (new[] { replacementJoin1, originalJoin2, replacementJoin3 }));
    }

    [Test]
    public void SubstituteJoins_ReplacesJoin_MemberLookupRemains ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Kitchen));
      
      var memberInfo1 = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();
      var originalJoin1 = sqlTable.GetOrAddMemberBasedLeftJoin (memberInfo1, SqlStatementModelObjectMother.CreateLeftJoinData);

      var memberInfo2 = SqlStatementModelObjectMother.GetKitchenRestaurantMemberInfo();
      var originalJoin2 = sqlTable.GetOrAddMemberBasedLeftJoin (memberInfo2, SqlStatementModelObjectMother.CreateLeftJoinData);

      var replacementJoin = SqlStatementModelObjectMother.CreateSqlJoin();
      var substitutions = new Dictionary<SqlJoin, SqlJoin> { { originalJoin1, replacementJoin } };

      sqlTable.SubstituteJoins (substitutions);

      Assert.That (sqlTable.GetJoinByMember (memberInfo1), Is.SameAs (replacementJoin));
      Assert.That (sqlTable.GetJoinByMember (memberInfo2), Is.SameAs (originalJoin2));
    }

    [Test]
    public void ToString_WithoutJoins ()
    {
      var oldTableInfo = new ResolvedSimpleTableInfo (typeof (int), "table1", "t");
      var sqlTable = new SqlTable (oldTableInfo);

      var result = sqlTable.ToString ();
      Assert.That (result, Is.EqualTo ("[table1] [t]"));
    }

    [Test]
    public void ToString_WithJoin ()
    {
      var joinedTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (int), "Cook", "c"));
      var joinCondition = Expression.Equal (new SqlLiteralExpression ("left"), new SqlLiteralExpression ("right"));

      var tableInfo = new ResolvedSimpleTableInfo (typeof (int), "table1", "t");
      var sqlTable = new SqlTable (tableInfo);
      sqlTable.AddJoinForExplicitQuerySource (new SqlJoin(joinedTable, JoinSemantics.Inner, joinCondition));

      var result = sqlTable.ToString ();

      Assert.That (result, Is.EqualTo ("[table1] [t] INNER JOIN [Cook] [c] ON (\"left\" == \"right\")"));
    }

    [Test]
    public void ToString_WithMultipleJoins ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (int), "table1", "t");
      var sqlTable = new SqlTable (tableInfo);

      var joinedTable1 = new SqlTable (new ResolvedSimpleTableInfo (typeof (int), "Cook", "c"));
      var joinCondition1 = Expression.Equal (new SqlLiteralExpression ("left"), new SqlLiteralExpression ("right"));
      var memberInfo1 = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();
      sqlTable.GetOrAddMemberBasedLeftJoin (memberInfo1, ()=> new SqlTable.LeftJoinData(joinedTable1, joinCondition1));

      var joinedTable2 = new SqlTable (new ResolvedSimpleTableInfo (typeof (int), "Restaurant", "r"));
      var joinCondition2 = Expression.Equal (new SqlLiteralExpression ("left2"), new SqlLiteralExpression ("right2"));
      sqlTable.AddJoinForExplicitQuerySource (new SqlJoin(joinedTable2, JoinSemantics.Inner, joinCondition2));

      var result = sqlTable.ToString ();

      Assert.That (
          result,
          Is.EqualTo ("[table1] [t] LEFT JOIN [Cook] [c] ON (\"left\" == \"right\") INNER JOIN [Restaurant] [r] ON (\"left2\" == \"right2\")"));
    }

  }
}