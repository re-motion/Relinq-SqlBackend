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
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlTableSourceVisitorTest
  {
    [Test]
    public void GenerateSql_ForSqlTableSource ()
    {
      var sb = new StringBuilder();
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable ();
      sqlTable.TableSource = new SqlTableSource (typeof (int), "Table", "t");
      SqlTableSourceVisitor.GenerateSql (sqlTable, sb);

      Assert.That (sb.ToString(), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    [ExpectedException (typeof (NotImplementedException))]
    public void TranslateTableSource_WithUnknownTableSource ()
    {
      var sb = new StringBuilder ();
      var sqlTable = new SqlTable ();
      sqlTable.TableSource = new UnknownTableSource ();
      SqlTableSourceVisitor.GenerateSql (sqlTable, sb);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "ConstantTableSource is not valid at this point.")]
    public void GenerateSql_WithConstantTableSource_RaisesException ()
    {
      var sb = new StringBuilder ();
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable ();
      SqlTableSourceVisitor.GenerateSql (sqlTable, sb);
    }

    private class UnknownTableSource : AbstractTableSource
    {

      public override Type Type
      {
        get { return typeof (string); }
      }

      public override AbstractTableSource Accept (ITableSourceVisitor visitor)
      {
        throw new NotImplementedException ();
      }
    }
  }
}