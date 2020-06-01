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
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Moq;

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
    [ExpectedException (typeof (ArgumentException))]
    public void DifferentType ()
    {
      _sqlTable.TableInfo = _newTableInfo;
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
    public void Accept_VisitorSupportingExpressionType ()
    {
      var visitorMock = new Mock<ISqlTableBaseVisitor>();
      visitorMock
         .Setup (mock => mock.VisitSqlTable (_sqlTable)).Verifiable();

      _sqlTable.Accept (visitorMock.Object);

      visitorMock.Verify();
    }

    [Test]
    public void ToString_WithoutJoins ()
    {
      var result = _sqlTable.ToString ();
      Assert.That (result, Is.EqualTo ("[table1] [t]"));
    }

    [Test]
    public void ToString_WithJoins ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook ();
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      _sqlTable.GetOrAddLeftJoin (joinInfo, memberInfo);

      var result = _sqlTable.ToString ();
      Assert.That (result, Is.EqualTo ("[table1] [t] LEFT JOIN Kitchen.Cook"));
    }

  }
}