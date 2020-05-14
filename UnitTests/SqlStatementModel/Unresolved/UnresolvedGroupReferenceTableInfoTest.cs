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
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class UnresolvedGroupReferenceTableInfoTest
  {
    private UnresolvedGroupReferenceTableInfo _tableInfo;
    private SqlTable _referencedGroupSource;

    [SetUp]
    public void SetUp ()
    {
      _referencedGroupSource = SqlStatementModelObjectMother.CreateSqlTable (typeof (IGrouping<int, string>));
      _tableInfo = new UnresolvedGroupReferenceTableInfo (_referencedGroupSource);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_tableInfo.ItemType, Is.EqualTo (typeof (string)));
    }

    [Test]
    public void Initialization_ThrowsWhenNoSequenceType ()
    {
      var invalidGroupSource = SqlStatementModelObjectMother.CreateSqlTable (typeof (int));
      Assert.That (
          () => new UnresolvedGroupReferenceTableInfo (invalidGroupSource),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Expected a closed generic type implementing IEnumerable<T>, but found 'System.Int32'.\r\nParameter name: referencedGroupSource"));
    }

    [Test]
    public void Accept ()
    {
      var tableInfoVisitorMock = MockRepository.GenerateMock<ITableInfoVisitor> ();
      tableInfoVisitorMock.Expect (mock => mock.VisitUnresolvedGroupReferenceTableInfo (_tableInfo));

      tableInfoVisitorMock.Replay ();
      _tableInfo.Accept (tableInfoVisitorMock);
      tableInfoVisitorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Initialize ()
    {
      Assert.That (_tableInfo.ItemType, Is.SameAs (typeof (string)));
    }

    [Test]
    public void GetResolvedTableInfo ()
    {
      Assert.That (
          () => _tableInfo.GetResolvedTableInfo(),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "This table has not yet been resolved; call the resolution step first."));
    }

    [Test]
    public void To_String ()
    {
      var result = _tableInfo.ToString();

      Assert.That (result, Is.EqualTo ("GROUP-REF-TABLE(TABLE-REF(UnresolvedTableInfo(IGrouping`2)))"));
    }
  }
}