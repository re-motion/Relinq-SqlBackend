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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
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
      _sqlTable = new SqlTable (_oldTableInfo);
    }

    [Test]
    public void SameType ()
    {
      var newTableInfo = new ResolvedSimpleTableInfo (typeof (int), "table2", "s");
      _sqlTable.TableInfo = newTableInfo;

      Assert.That (_sqlTable.TableInfo.ItemType, Is.EqualTo (newTableInfo.ItemType));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void DifferentType ()
    {
      _sqlTable.TableInfo = _newTableInfo;
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      var visitorMock = MockRepository.GenerateMock<ISqlTableBaseVisitor>();
      visitorMock.Expect (mock => mock.VisitSqlTable (_sqlTable));
      visitorMock.Replay();

      _sqlTable.Accept (visitorMock);

      visitorMock.VerifyAllExpectations();
    }

  }
}