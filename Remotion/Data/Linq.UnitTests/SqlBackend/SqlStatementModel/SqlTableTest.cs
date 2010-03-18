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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
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
      var oldTableInfo = new ResolvedSimpleTableInfo (typeof (int), "table1", "t");
      var sqlTable = new SqlTable (oldTableInfo);
      var newTableInfo = new ResolvedSimpleTableInfo (typeof (int), "table2", "s");

      sqlTable.TableInfo = newTableInfo;

      Assert.That (sqlTable.TableInfo.ItemType, Is.EqualTo (newTableInfo.ItemType));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void DifferentType ()
    {
      var oldTableInfo = new ResolvedSimpleTableInfo (typeof (int), "table1", "t");
      var sqlTable = new SqlTable (oldTableInfo);
      var newTableInfo = new ResolvedSimpleTableInfo (typeof (string), "table2", "s");

      sqlTable.TableInfo = newTableInfo;
    }
  }
}