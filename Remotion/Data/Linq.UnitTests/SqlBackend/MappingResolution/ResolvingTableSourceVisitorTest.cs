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
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingTableSourceVisitorTest
  {
    [Test]
    public void ResolveConstantTableSource ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTableWithConstantTableSource();
      var resolver = MockRepository.GenerateMock<ISqlStatementResolver>();

      var tableSource = new SqlTableSource (typeof (int), "Table", "t");
      resolver.Expect (mock => mock.ResolveConstantTableSource ((ConstantTableSource) sqlTable.TableSource)).Return (tableSource);

      ResolvingTableSourceVisitor.ResolveTableSource (sqlTable, resolver);

      Assert.That (sqlTable.TableSource, Is.TypeOf (typeof (SqlTableSource)));
    }

    [Test]
    [ExpectedException (typeof (NotImplementedException))]
    public void ResolveConstantTableSource_WithUnknownTableSource ()
    {
      var sqlTable = new SqlTable();
      sqlTable.TableSource = new UnknownTableSource();
      var resolver = MockRepository.GenerateMock<ISqlStatementResolver>();
      ResolvingTableSourceVisitor.ResolveTableSource (sqlTable, resolver);
    }

    [Test]
    public void ResolveJoinedTableSource ()
    {
      var joinedTable = SqlStatementModelObjectMother.CreateSqlTableWithJoinedTableSource();
      var resolver = MockRepository.GenerateMock<ISqlStatementResolver>();

      var kitchenSource = new SqlTableSource (typeof (Kitchen), "Kitchen", "k");
      var cookSource = new SqlTableSource(typeof(Cook), "Cook", "c");
      var sqlJoinedTableSource = new SqlJoinedTableSource (kitchenSource, cookSource, "ID", "KitchenID", typeof (Cook));

      resolver.Expect (mock => mock.ResolveJoinedTableSource (Arg<SqlTable>.Is.Anything, Arg<SqlTable>.Is.Anything)).Return (sqlJoinedTableSource);

      ResolvingTableSourceVisitor.ResolveTableSource (joinedTable, resolver);

      Assert.That (joinedTable.TableSource, Is.TypeOf (typeof (SqlJoinedTableSource)));

    }

    private class UnknownTableSource : AbstractTableSource
    {
      public override Type Type
      {
        get { return typeof (string); }
      }

      public override AbstractTableSource Accept (ITableSourceVisitor visitor)
      {
        throw new NotImplementedException();
      }
    }
  }
}