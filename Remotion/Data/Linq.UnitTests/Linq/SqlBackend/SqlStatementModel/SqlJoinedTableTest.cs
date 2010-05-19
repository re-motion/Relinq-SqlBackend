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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlJoinedTableTest
  {
    private SqlTable _cookTable;
    private SqlTable _kitchenTable;

    [SetUp]
    public void SetUp ()
    {
      _cookTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo (typeof (Cook));
      _kitchenTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo (typeof (Kitchen));
    }

    [Test]
    public void SameType ()
    {
      var oldEntityExpression = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var oldJoinInfo = new UnresolvedJoinInfo (oldEntityExpression, typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One);
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo, JoinSemantics.Left);
      var newEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var newJoinInfo = new UnresolvedJoinInfo (newEntityExpression, typeof (Cook).GetProperty ("Substitution"), JoinCardinality.One);

      sqlJoinedTable.JoinInfo = newJoinInfo;

      Assert.That (sqlJoinedTable.JoinInfo.ItemType, Is.EqualTo (newJoinInfo.ItemType));
      Assert.That (sqlJoinedTable.JoinSemantics, Is.EqualTo (JoinSemantics.Left));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void DifferentType ()
    {
      var oldEntityExpression = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var oldJoinInfo = new UnresolvedJoinInfo (oldEntityExpression, typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One);
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo, JoinSemantics.Left);
      var newEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var newJoinInfo = new UnresolvedJoinInfo (newEntityExpression, typeof (Cook).GetProperty ("FirstName"), JoinCardinality.One);

      sqlJoinedTable.JoinInfo = newJoinInfo;
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      var oldEntityExpression = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var oldJoinInfo = new UnresolvedJoinInfo (oldEntityExpression, typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One);
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
      var oldEntityExpression = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var oldJoinInfo = new UnresolvedJoinInfo (oldEntityExpression, typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One);
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
  }
}