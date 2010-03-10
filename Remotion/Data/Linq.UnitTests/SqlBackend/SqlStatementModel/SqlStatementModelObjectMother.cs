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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel
{
  public class SqlStatementModelObjectMother
  {
    public static SqlStatement CreateSqlStatement ()
    {
      var sqlTable = CreateSqlTable_WithUnresolvedTableInfo ();
      return new SqlStatement (new SqlTableReferenceExpression (sqlTable), sqlTable);
    }

    public static SqlTable CreateSqlTable ()
    {
      return CreateSqlTable_TypeIsCook();
    }

    public static SqlTable CreateSqlTable (AbstractTableInfo tableInfo)
    {
      var sqlTable = new SqlTable (tableInfo);
      return sqlTable;
    }

    public static SqlTable CreateSqlTable_TypeIsCook ()
    {
      return new SqlTable (CreateUnresolvedTableInfo_TypeIsCook ());
    }

    public static SqlTable CreateSqlTable_WithUnresolvedTableInfo () 
    {
      var unresolvedTableInfo = new UnresolvedTableInfo (Expression.Constant (1, typeof (int)),typeof(int));
      var sqlTable = new SqlTable (unresolvedTableInfo);
      return sqlTable;
    }

    public static SqlTable CreateSqlTable_WithResolvedTableInfo ()
    {
      var resolvedTableInfo = new ResolvedTableInfo (typeof (string), "Table", "t");
      var sqlTable = new SqlTable (resolvedTableInfo);
      return sqlTable;
    }

    public static SqlJoinedTable CreateSqlJoinedTable_WithUnresolvedJoinInfo ()
    {
      var joinInfo = new UnresolvedJoinInfo (typeof (Cook).GetProperty ("FirstName"));
      return new SqlJoinedTable (joinInfo);
    }

    public static UnresolvedTableInfo CreateUnresolvedTableInfo ()
    {
      return CreateUnresolvedTableInfo_TypeIsCook();
    }

    public static UnresolvedTableInfo CreateUnresolvedTableInfo_TypeIsKitchen ()
    {
      return new UnresolvedTableInfo (Expression.Constant (new Kitchen[0]), typeof (Kitchen));
    }

    public static UnresolvedTableInfo CreateUnresolvedTableInfo_TypeIsCook ()
    {
      return new UnresolvedTableInfo (Expression.Constant (new Cook { FirstName = "Test" }, typeof (Cook)), typeof (Cook));
    }

    public static UnresolvedJoinInfo CreateUnresolvedJoinInfo_KitchenCook ()
    {
      return new UnresolvedJoinInfo (typeof (Kitchen).GetProperty ("Cook"));
    }

    public static ResolvedTableInfo CreateResolvedTableInfo ()
    {
      return CreateResolvedTableInfo_TypeIsCook();
    }

    public static ResolvedTableInfo CreateResolvedTableInfo_TypeIsCook ()
    {
      return new ResolvedTableInfo (typeof (Cook), "table", "t");
    }

    public static ResolvedJoinInfo CreateResolvedJoinInfo ()
    {
      return CreateResolvedJoinInfo_TypeIsCook();
    }

    public static ResolvedJoinInfo CreateResolvedJoinInfo (Type type)
    {
      var primaryColumn = new SqlColumnExpression (typeof (int), "k", "ID");
      var foreignColumn = new SqlColumnExpression (typeof (int), "s", "ID");
      var foreignTableInfo = new ResolvedTableInfo (type, "Table", "s");
      return new ResolvedJoinInfo (foreignTableInfo, primaryColumn, foreignColumn);
    }

    // TODO: Inline
    public static ResolvedJoinInfo CreateResolvedJoinInfo_TypeIsCook ()
    {
      return CreateResolvedJoinInfo (typeof (Cook));
    }
  }
}