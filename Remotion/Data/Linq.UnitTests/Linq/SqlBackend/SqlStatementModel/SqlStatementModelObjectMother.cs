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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  public class SqlStatementModelObjectMother
  {
    public static SqlStatement CreateSqlStatement (Expression selectProjection)
    {
      return new SqlStatementBuilder
             {
                 DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (selectProjection.Type), selectProjection),
                 SelectProjection = selectProjection
             }.GetSqlStatement();
    }

    public static SqlStatement CreateSqlStatement (Expression selectProjection, params SqlTable[] sqlTables)
    {
      return new SqlStatement (
          new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (null, typeof (Cook))),
          selectProjection,
          sqlTables, null, null, new Ordering[] { }, null, false, null, null);
    }

    public static SqlStatement CreateSqlStatementWithCook ()
    {
      var sqlTable = CreateSqlTable_WithUnresolvedTableInfo (typeof (Cook));
      return new SqlStatement (
          new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (null, typeof (Cook))),
          new SqlTableReferenceExpression (sqlTable),
          new[] { sqlTable }, null, null, new Ordering[] { }, null, false, null, null);
    }

    public static SqlStatement CreateSqlStatement ()
    {
      var sqlTable = CreateSqlTable_WithUnresolvedTableInfo(typeof (int));
      return new SqlStatement (
          new StreamedSequenceInfo (typeof (IQueryable<int>), Expression.Constant (0, typeof (int))),
          new SqlTableReferenceExpression (sqlTable),
          new[] { sqlTable }, null, null, new Ordering[] { }, null, false, null, null);
    }

    public static SqlStatement CreateSqlStatement_Resolved (Type type)
    {
      var sqlTable = CreateSqlTable_WithResolvedTableInfo (type);
      return new SqlStatement (
          new StreamedSequenceInfo (
              typeof (IQueryable<>).MakeGenericType (type), 
              Expression.Constant (type.IsValueType ? Activator.CreateInstance (type) : null, type)),
          CreateSqlEntityDefinitionExpression (type),
          new[] { sqlTable }, null, null, new Ordering[] { }, null, false, null, null);
    }

    public static SqlTable CreateSqlTable ()
    {
      return CreateSqlTable (typeof (Cook));
    }

    public static SqlTable CreateSqlTable (ITableInfo tableInfo)
    {
      var sqlTable = new SqlTable (tableInfo);
      return sqlTable;
    }

    public static SqlTable CreateSqlTable (Type type)
    {
      return new SqlTable (CreateUnresolvedTableInfo (type));
    }

    public static SqlTable CreateSqlTable_WithUnresolvedTableInfo ()
    {
      return CreateSqlTable_WithUnresolvedTableInfo (typeof (int));
    }

    public static SqlTable CreateSqlTable_WithUnresolvedTableInfo (Type type)
    {
      var unresolvedTableInfo = new UnresolvedTableInfo (type);
      var sqlTable = new SqlTable (unresolvedTableInfo);
      return sqlTable;
    }

    public static SqlTable CreateSqlTable_WithResolvedTableInfo ()
    {
      return CreateSqlTable_WithResolvedTableInfo ("Table", "t");
    }

    public static SqlTable CreateSqlTable_WithResolvedTableInfo (Type type)
    {
      return CreateSqlTable_WithResolvedTableInfo (type, "Table", "t");
    }

    public static SqlTable CreateSqlTable_WithResolvedTableInfo (string tableName, string tableAlias)
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (string), tableName, tableAlias);
      var sqlTable = new SqlTable (resolvedTableInfo);
      return sqlTable;
    }

    public static SqlTable CreateSqlTable_WithResolvedTableInfo (Type type, string tableName, string tableAlias)
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (type, tableName, tableAlias);
      var sqlTable = new SqlTable (resolvedTableInfo);
      return sqlTable;
    }

    public static SqlJoinedTable CreateSqlJoinedTable_WithUnresolvedJoinInfo ()
    {
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var joinInfo = new UnresolvedJoinInfo (entityExpression, typeof (Cook).GetProperty ("FirstName"), JoinCardinality.One);
      return new SqlJoinedTable (joinInfo, JoinSemantics.Left);
    }

    public static UnresolvedTableInfo CreateUnresolvedTableInfo ()
    {
      return CreateUnresolvedTableInfo (typeof (Cook));
    }

    public static UnresolvedTableInfo CreateUnresolvedTableInfo (Type type)
    {
      return new UnresolvedTableInfo (type);
    }

    public static UnresolvedJoinInfo CreateUnresolvedJoinInfo_KitchenCook ()
    {
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      return new UnresolvedJoinInfo (entityExpression, typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One);
    }

    public static UnresolvedCollectionJoinInfo CreateUnresolvedCollectionJoinInfo_RestaurantCooks ()
    {
      return new UnresolvedCollectionJoinInfo (Expression.Constant (new Restaurant()), typeof (Restaurant).GetProperty ("Cooks"));
    }

    public static ResolvedSimpleTableInfo CreateResolvedTableInfo ()
    {
      return CreateResolvedTableInfo (typeof (Cook));
    }

    public static ResolvedSimpleTableInfo CreateResolvedTableInfo (Type type)
    {
      return new ResolvedSimpleTableInfo (type, "table", "t");
    }

    public static ResolvedJoinInfo CreateResolvedJoinInfo ()
    {
      return CreateResolvedJoinInfo (typeof (Cook));
    }

    public static ResolvedJoinInfo CreateResolvedJoinInfo (Type type)
    {
      var primaryColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", false);
      var foreignColumn = new SqlColumnDefinitionExpression (typeof (int), "s", "ID", false);
      var foreignTableInfo = new ResolvedSimpleTableInfo (type, "Table", "s");
      return new ResolvedJoinInfo (foreignTableInfo, primaryColumn, foreignColumn);
    }

    public static SqlEntityDefinitionExpression CreateSqlEntityDefinitionExpression (Type type)
    {
      return CreateSqlEntityDefinitionExpression(type, null);
    }

    public static SqlEntityDefinitionExpression CreateSqlEntityDefinitionExpression (Type type, string name)
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "t", "ID", true);
      return new SqlEntityDefinitionExpression (
          type,
          "t", 
          name,
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnDefinitionExpression (typeof (int), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (int), "t", "City", false)
          });
    }

    public static UnresolvedGroupReferenceTableInfo CreateUnresolvedGroupReferenceTableInfo ()
    {
      return new UnresolvedGroupReferenceTableInfo (CreateSqlTable (typeof (IGrouping<int, string>)));
    }
  }
}