// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class JoinedTableSourceTest
  {
    [Test]
    public void Accept ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTableWithJoinedTableSource ();

      var tableSourceVisitorMock = MockRepository.GenerateMock<ITableSourceVisitor> ();
      tableSourceVisitorMock.Expect (mock => mock.VisitJoinedTableSource ((JoinedTableSource) sqlTable.TableSource));
      sqlTable.TableSource.Accept (tableSourceVisitorMock);
      tableSourceVisitorMock.Replay ();
      tableSourceVisitorMock.VerifyAllExpectations ();
    }
  }
}