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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class ResolvedJoinedGroupingTableInfoTest
  {
    private ResolvedJoinedGroupingTableInfo _tableInfo;

    [SetUp]
    public void SetUp ()
    {
      _tableInfo = new ResolvedJoinedGroupingTableInfo (
          "q0",
          SqlStatementModelObjectMother.CreateSqlStatement(),
          SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression(),
          "q1");
    }

    [Test]
    public void To_String ()
    {
      Assert.That (_tableInfo.ToString(), Is.EqualTo ("JOINED-GROUPING([q1], (" + _tableInfo.SqlStatement + ") [q0])"));
    }

    [Test]
    public void Accept ()
    {
      var tableInfoVisitorMock = MockRepository.GenerateMock<ITableInfoVisitor>();
      tableInfoVisitorMock.Expect (mock => mock.VisitJoinedGroupingTableInfo (_tableInfo));

      tableInfoVisitorMock.Replay();
      _tableInfo.Accept (tableInfoVisitorMock);

      tableInfoVisitorMock.VerifyAllExpectations();
    }

    [Test]
    public void GetResolvedTableInfo ()
    {
      var result = _tableInfo.GetResolvedTableInfo();

      Assert.That (result, Is.SameAs (_tableInfo));
    }
  }
}