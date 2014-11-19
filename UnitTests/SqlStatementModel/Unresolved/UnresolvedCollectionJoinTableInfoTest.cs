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
using Moq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class UnresolvedCollectionJoinTableInfoTest
  {
    private UnresolvedCollectionJoinTableInfo _tableInfo;

    [SetUp]
    public void SetUp ()
    {
      _tableInfo = SqlStatementModelObjectMother.CreateUnresolvedCollectionJoinTableInfo_RestaurantCooks ();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_tableInfo.ItemType, Is.EqualTo (typeof (Cook)));  
    }

    [Test]
    public void Accept ()
    {
      var tableInfo = SqlStatementModelObjectMother.CreateUnresolvedCollectionJoinTableInfo();
      var tableInfoVisitorMock = new Mock<ITableInfoVisitor>();

      tableInfo.Accept (tableInfoVisitorMock.Object);

      tableInfoVisitorMock.Verify(mock => mock.VisitUnresolvedCollectionJoinTableInfo (tableInfo));
    }

    [Test]
    public void GetResolvedTableInfo_Throws ()
    {
      var tableInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinTableInfo();

      Assert.That (
          () => tableInfo.GetResolvedTableInfo(),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "This table has not yet been resolved; call the resolution step first."));
    }

    [Test]
    public new void ToString ()
    {
      var result = _tableInfo.ToString ();
      Assert.That (result, Is.EqualTo ("TABLE(Restaurant.Cooks)"));
    }
  }
}