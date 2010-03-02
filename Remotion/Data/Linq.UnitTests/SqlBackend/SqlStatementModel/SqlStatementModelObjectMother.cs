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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel
{
  public class SqlStatementModelObjectMother
  {
    public static SqlTable CreateSqlTable (AbstractTableSource tableSource)
    {
      var sqlTable = new SqlTable ();
      sqlTable.TableSource = tableSource;
      return sqlTable;
    }

    public static SqlTable CreateSqlTableWithConstantTableSource () 
    {
      var constantTableSource = new ConstantTableSource (Expression.Constant (1, typeof (int)));
      var sqlTable = new SqlTable ();
      sqlTable.TableSource = constantTableSource;
      return sqlTable;
    }

    public static SqlTable CreateSqlTableWithSqlTableSource ()
    {
      var sqlTableSource = new SqlTableSource (typeof (string), "Table", "t");
      var sqlTable = new SqlTable ();
      sqlTable.TableSource = sqlTableSource;
      return sqlTable;
    }

    public static SqlTable CreateSqlTableWithJoinedTableSource ()
    {
      var sqlTableSource = new JoinedTableSource (typeof (Cook).GetMember ("FirstName")[0]);
      var sqlTable = new SqlTable ();
      sqlTable.TableSource = sqlTableSource;
      return sqlTable;
    }
  }
}