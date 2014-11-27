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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class SqlTableBaseTest
  {

    [Test]
    public void GetOrAddLeftJoinByMember_NewEntry_IsAddedToOrderedListOfJoins()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));
      var memberInfo = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();
      var createdSqlJoin = SqlStatementModelObjectMother.CreateSqlJoin();

      var result = sqlTable.GetOrAddLeftJoinByMember (memberInfo, () => createdSqlJoin);

      Assert.That (result, Is.SameAs (createdSqlJoin));
      Assert.That (sqlTable.OrderedJoins, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetOrAddLeftJoinByMember_MultipleNewEntries_AreAddedInOrder()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));

      var result1 = sqlTable.GetOrAddLeftJoinByMember (
          SqlStatementModelObjectMother.GetKitchenCookMemberInfo(),
          SqlStatementModelObjectMother.CreateSqlJoin);
      var result2 = sqlTable.GetOrAddLeftJoinByMember (
          SqlStatementModelObjectMother.GetKitchenRestaurantMemberInfo(),
          SqlStatementModelObjectMother.CreateSqlJoin);

      Assert.That (sqlTable.OrderedJoins, Is.EqualTo (new[] { result1, result2 }));
    }

    [Test]
    public void GetOrAddLeftJoinByMember_ExistingEntry_RetrievesExistingEntry()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));
      var memberInfo = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();
      var existingEntry = sqlTable.GetOrAddLeftJoinByMember (memberInfo, SqlStatementModelObjectMother.CreateSqlJoin);

      var result = sqlTable.GetOrAddLeftJoinByMember (memberInfo, () => { throw new InvalidOperationException ("Must not be called."); });

      Assert.That (result, Is.SameAs (existingEntry));
      Assert.That (sqlTable.OrderedJoins, Is.EqualTo (new[] { result }));
    }

    [Test]
    public void GetJoinByMember_ExistingEntry ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));
      var memberInfo = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();
      var existingEntry = sqlTable.GetOrAddLeftJoinByMember (memberInfo, SqlStatementModelObjectMother.CreateSqlJoin);

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
    public void AddJoin_AddsJoinInOrder ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof (Kitchen));
      var memberInfo = SqlStatementModelObjectMother.GetKitchenCookMemberInfo();
      var existingEntry = sqlTable.GetOrAddLeftJoinByMember (memberInfo, SqlStatementModelObjectMother.CreateSqlJoin);

      var newEntry = SqlStatementModelObjectMother.CreateSqlJoin();
      sqlTable.AddJoin (newEntry);

      Assert.That (sqlTable.OrderedJoins, Is.EqualTo (new[] { existingEntry, newEntry }));
    }
  }
}