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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlJoinedTableTest
  {
    private SqlTable _sqlTable;

    [SetUp]
    public void SetUp ()
    {
      _sqlTable = new SqlTable (new UnresolvedTableInfo (typeof (Cook))); // TODO Review 2486: Use object mother
    }

    [Test]
    public void SameType ()
    {
      var oldJoinInfo = new UnresolvedJoinInfo (_sqlTable, typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One); // TODO Review 2486: use kitchen table here
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo);
      var newJoinInfo = new UnresolvedJoinInfo (_sqlTable, typeof (Cook).GetProperty ("Substitution"), JoinCardinality.One);

      sqlJoinedTable.JoinInfo = newJoinInfo;

      Assert.That (sqlJoinedTable.JoinInfo.ItemType, Is.EqualTo (newJoinInfo.ItemType));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void DifferentType ()
    {
      var oldJoinInfo = new UnresolvedJoinInfo (_sqlTable, typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One); // TODO Review 2486: use kitchen table here
      var sqlJoinedTable = new SqlJoinedTable (oldJoinInfo);
      var newJoinInfo = new UnresolvedJoinInfo (_sqlTable, typeof (Cook).GetProperty ("FirstName"), JoinCardinality.One);

      sqlJoinedTable.JoinInfo = newJoinInfo;
    }

    // TODO Review 2487: Test Accept
  }
}