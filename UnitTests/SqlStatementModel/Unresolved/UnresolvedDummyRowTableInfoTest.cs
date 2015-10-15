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
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class UnresolvedDummyRowTableInfoTest
  {
    [Test]
    public void Initialization ()
    {
      Assert.That (UnresolvedDummyRowTableInfo.Instance.ItemType, Is.EqualTo (typeof (object)));
    }
    
    [Test]
    public void Singleton ()
    {
      Assert.That (UnresolvedDummyRowTableInfo.Instance, Is.SameAs (UnresolvedDummyRowTableInfo.Instance));
    }

    [Test]
    public void Accept ()
    {
      var tableInfoVisitorMock = new Mock<ITableInfoVisitor> ();
      tableInfoVisitorMock
          .Setup (mock => mock.VisitUnresolvedDummyRowTableInfo (UnresolvedDummyRowTableInfo.Instance))
          .Verifiable();

      UnresolvedDummyRowTableInfo.Instance.Accept (tableInfoVisitorMock.Object);
      tableInfoVisitorMock.Verify();
    }

    [Test]
    public void GetResolvedTableInfo ()
    {
      Assert.That (
          () => UnresolvedDummyRowTableInfo.Instance.GetResolvedTableInfo(),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "This table has not yet been resolved; call the resolution step first."));
    }

    [Test]
    public void To_String ()
    {
      var result = UnresolvedDummyRowTableInfo.Instance.ToString();

      Assert.That (result, Is.EqualTo ("TABLE(one row)"));
    } 
  }
}