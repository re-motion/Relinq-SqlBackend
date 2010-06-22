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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class UnresolvedGroupReferenceTableInfoTest
  {
    private UnresolvedGroupReferenceTableInfo _tableInfo;
    private ResolvedSubStatementTableInfo _resolvedSubStatmentTableInfo;

    [SetUp]
    public void SetUp ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();
      _resolvedSubStatmentTableInfo = new ResolvedSubStatementTableInfo ("cook", sqlStatement);
      _tableInfo = new UnresolvedGroupReferenceTableInfo (_resolvedSubStatmentTableInfo);
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
      Assert.That (_tableInfo.ReferencedTableInfo, Is.SameAs (_resolvedSubStatmentTableInfo));
      Assert.That (_tableInfo.ItemType, Is.SameAs (_resolvedSubStatmentTableInfo.ItemType));
    }
  }
}