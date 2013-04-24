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
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlJoinedTableTest
  {
    [Test]
    public void SameType ()
    {
      var oldJoinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo, JoinSemantics.Left);

      var newJoinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_CookSubstitution ();
      sqlJoinedTable.JoinInfo = newJoinInfo;

      Assert.That (sqlJoinedTable.JoinInfo.ItemType, Is.EqualTo (newJoinInfo.ItemType));
      Assert.That (sqlJoinedTable.JoinSemantics, Is.EqualTo (JoinSemantics.Left));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void DifferentType ()
    {
      var oldJoinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook ();
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo, JoinSemantics.Left);
      var newJoinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenRestaurant ();

      sqlJoinedTable.JoinInfo = newJoinInfo;
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      var oldJoinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook ();
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo, JoinSemantics.Left);

      var visitorMock = MockRepository.GenerateMock<ISqlTableBaseVisitor> ();
      visitorMock.Expect (mock => mock.VisitSqlJoinedTable (sqlJoinedTable));
      visitorMock.Replay ();

      sqlJoinedTable.Accept (visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (sqlJoinedTable.JoinSemantics, Is.EqualTo (JoinSemantics.Left));
    }

    [Test]
    public void Accept_ITableInfoVisitor ()
    {
      var oldJoinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo, JoinSemantics.Left);
      var fakeResult = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");

      var visitorMock = MockRepository.GenerateMock<ITableInfoVisitor>();
      visitorMock
          .Expect (mock => mock.VisitSqlJoinedTable (sqlJoinedTable))
          .Return (fakeResult);

      var result = ((ITableInfo) sqlJoinedTable).Accept (visitorMock);

      visitorMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public new void ToString ()
    {
      var joinedTable = new SqlJoinedTable (SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook(), JoinSemantics.Left);

      var result = joinedTable.ToString ();

      Assert.That (result, Is.EqualTo ("LEFT JOIN Kitchen.Cook"));
    }

    [Test]
    public void ToString_WithJoins ()
    {
      var joinedTable = new SqlJoinedTable (SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook(), JoinSemantics.Left);
      var joinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook ();
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      joinedTable.GetOrAddLeftJoin (joinInfo, memberInfo);

      var result = joinedTable.ToString ();
      Assert.That (result, Is.EqualTo ("LEFT JOIN Kitchen.Cook LEFT JOIN Kitchen.Cook"));
    }
  }
}