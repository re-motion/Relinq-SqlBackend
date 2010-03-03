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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlTableTest
  {
    [Test]
    public void SameType ()
    {
      var sqlTable = new SqlTable();
      var oldTableSource = new SqlTableSource (typeof (int), "table1", "t");
      var newTableSource = new SqlTableSource (typeof (int), "table2", "s");

      sqlTable.TableSource = oldTableSource;
      sqlTable.TableSource = newTableSource;

      Assert.That (sqlTable.TableSource.Type, Is.EqualTo (newTableSource.Type));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void DifferentType ()
    {
      var sqlTable = new SqlTable ();
      var oldTableSource = new SqlTableSource (typeof (int), "table1", "t");
      var newTableSource = new SqlTableSource (typeof (string), "table2", "s");

      sqlTable.TableSource = oldTableSource;
      sqlTable.TableSource = newTableSource;
    }

    [Test]
    public void GetOrAddJoin_NewEntry ()
    {
      var sqlTable = new SqlTable();
      
      var memberInfo = typeof (Cook).GetMember ("FirstName")[0]; // TODO: Use GetProperty instead, doesn't require indexer
      var tableSource = new SqlTableSource (typeof (Cook), "table2", "s");

      sqlTable.GetOrAddJoin (memberInfo, tableSource);

      // TODO: Check return value: should be a SqlTable whose .TableSource, Is.SameAs (tableSource)
      
      // TODO: Remove these lines; instead, add new test checking that when GetOrAddJoin is called twice with the same member, the same SqlTable is returned
      var newTable = new SqlTable();
      newTable.TableSource = tableSource;
      Assert.That (sqlTable.GetOrAddJoin (memberInfo, tableSource).TableSource, Is.EqualTo (newTable.TableSource));
    }

    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Type mismatch between Cook and Int32.")]
    [Test]
    public void GetOrAddJoin_ThrowsException ()
    {
      var sqlTable = new SqlTable ();

      var memberInfo = typeof (Cook).GetMember ("FirstName")[0];
      var tableSource = new SqlTableSource (typeof (int), "table2", "s");

      sqlTable.GetOrAddJoin (memberInfo, tableSource);
    }

  }
}