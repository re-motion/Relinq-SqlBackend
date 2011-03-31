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
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Unresolved
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
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = 
        "Expected a type implementing IEnumerable<T>, but found 'System.Int32'.\r\nParameter name: referencedGroupSource")]
    public void Initialization_ThrowsWhenNoSequenceType ()
    {
      var invalidGroupSource = SqlStatementModelObjectMother.CreateSqlTable (typeof (int));
      new UnresolvedGroupReferenceTableInfo (invalidGroupSource);
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
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "This table has not yet been resolved; call the resolution step first.")]
    public void GetResolvedTableInfo ()
    {
      _tableInfo.GetResolvedTableInfo();
    }

    [Test]
    public void To_String ()
    {
      var result = _tableInfo.ToString();

      Assert.That (result, Is.EqualTo ("GROUP-REF-TABLE(TABLE-REF(UnresolvedTableInfo(IGrouping`2)))"));
    }
  }
}