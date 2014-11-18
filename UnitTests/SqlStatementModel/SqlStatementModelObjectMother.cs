// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
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
          sqlTables,
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);
    }

    public static SqlStatement CreateSqlStatementWithCook ()
    {
      var sqlTable = CreateSqlTable_WithUnresolvedTableInfo (typeof (Cook));
      return new SqlStatement (
          new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (null, typeof (Cook))),
          new SqlTableReferenceExpression (sqlTable),
          new[] { sqlTable },
          null,
          null,
          new Ordering[] { },
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);
    }

    public static SqlStatement CreateSqlStatement ()
    {
      var sqlTable = CreateSqlTable_WithUnresolvedTableInfo(typeof (int));
      return new SqlStatement (
          new StreamedSequenceInfo (typeof (IQueryable<int>), Expression.Constant (0, typeof (int))),
          new SqlTableReferenceExpression (sqlTable),
          new[] { sqlTable },
          null,
          null,
          new Ordering[] { },
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);
    }

    public static SqlStatement CreateSqlStatement_Resolved (Type type)
    {
      var sqlTable = CreateSqlTable_WithResolvedTableInfo (type);
      return new SqlStatement (
          new StreamedSequenceInfo (
              typeof (IQueryable<>).MakeGenericType (type),
              Expression.Constant (type.IsValueType ? Activator.CreateInstance (type) : null, type)),
          CreateSqlEntityDefinitionExpression (type),
          new[] { sqlTable },
          null,
          null,
          new Ordering[] { },
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);
    }

    public static SqlStatement CreateSqlStatement_Single ()
    {
      return new SqlStatement (
          new StreamedSingleValueInfo (typeof (int), false),
          Expression.Constant (0),
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          new SqlLiteralExpression (1),
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);
    }

    public static SqlStatement CreateSqlStatement_Scalar ()
    {
      return CreateSqlStatement_Scalar(Expression.Constant (0));
    }

    public static SqlStatement CreateSqlStatement_Scalar (Expression selectProjection)
    {
      return new SqlStatementBuilder
      {
        DataInfo = new StreamedScalarValueInfo (selectProjection.Type),
        SelectProjection = selectProjection,
        TopExpression = new SqlLiteralExpression (1)
      }.GetSqlStatement ();
    }

    public static SqlStatement CreateMinimalSqlStatement (SqlStatementBuilder builder)
    {
      if (builder.SelectProjection == null)
        builder.SelectProjection = ExpressionHelper.CreateExpression();
      if (builder.DataInfo == null)
        builder.DataInfo = new TestStreamedValueInfo (builder.SelectProjection.Type);
      return builder.GetSqlStatement();
    }

    public static SqlTable CreateSqlTable ()
    {
      return CreateSqlTable (typeof (Cook));
    }

    public static SqlTable CreateSqlTable (ITableInfo tableInfo)
    {
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Inner);
      return sqlTable;
    }

    public static SqlTable CreateSqlTable (Type type)
    {
      return new SqlTable (CreateUnresolvedTableInfo (type), JoinSemantics.Inner);
    }

    public static SqlTable CreateSqlTable_WithUnresolvedTableInfo ()
    {
      return CreateSqlTable_WithUnresolvedTableInfo (typeof (int));
    }

    public static SqlTable CreateSqlTable_WithUnresolvedTableInfo (Type type)
    {
      var unresolvedTableInfo = new UnresolvedTableInfo (type);
      var sqlTable = new SqlTable (unresolvedTableInfo, JoinSemantics.Inner);
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

    public static SqlTable CreateSqlTable_WithResolvedTableInfo (
        string tableName,
        string tableAlias,
        JoinSemantics joinSemantics = JoinSemantics.Inner)
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (string), tableName, tableAlias);
      var sqlTable = new SqlTable (resolvedTableInfo, joinSemantics);
      return sqlTable;
    }

    public static SqlTable CreateSqlTable_WithResolvedTableInfo (Type type, string tableName, string tableAlias)
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (type, tableName, tableAlias);
      var sqlTable = new SqlTable (resolvedTableInfo, JoinSemantics.Inner);
      return sqlTable;
    }

    public static SqlJoinedTable CreateSqlJoinedTable_WithUnresolvedJoinInfo ()
    {
      var entityExpression = CreateSqlEntityDefinitionExpression (typeof (Cook));
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
      var entityExpression = CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      return new UnresolvedJoinInfo (entityExpression, typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One);
    }

    public static UnresolvedJoinTableInfo CreateUnresolvedJoinTableInfo ()
    {
      return CreateUnresolvedJoinTableInfo_KitchenCook();
    }

    public static UnresolvedJoinTableInfo CreateUnresolvedJoinTableInfo_KitchenCook ()
    {
      var entityExpression = CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      return new UnresolvedJoinTableInfo (entityExpression, typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One);
    }

    public static UnresolvedJoinInfo CreateUnresolvedJoinInfo_KitchenRestaurant ()
    {
      var entityExpression = CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      return new UnresolvedJoinInfo (entityExpression, typeof (Kitchen).GetProperty ("Restaurant"), JoinCardinality.One);
    }

    public static UnresolvedJoinInfo CreateUnresolvedJoinInfo_CookSubstitution ()
    {
      var entityExpression = CreateSqlEntityDefinitionExpression (typeof (Cook));
      return new UnresolvedJoinInfo (entityExpression, typeof (Cook).GetProperty ("Substitution"), JoinCardinality.One);
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

    public static ResolvedJoinInfo CreateResolvedJoinInfo (Type type, Expression joinCondition = null)
    {
      if (joinCondition == null)
      {
        var primaryColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", false);
        var foreignColumn = new SqlColumnDefinitionExpression (typeof (int), "s", "ID", false);
        joinCondition = Expression.Equal (primaryColumn, foreignColumn);
      }

      var foreignTableInfo = new ResolvedSimpleTableInfo (type, "Table", "s");
      return new ResolvedJoinInfo (foreignTableInfo, joinCondition);
    }

    public static SqlColumnExpression CreateSqlColumn (Type type = null, string owningTableAlias = null, string column = null, bool isPrimaryKey = false)
    {
      return new SqlColumnDefinitionExpression (type ?? typeof (int), owningTableAlias ?? "t0", column ?? "column", isPrimaryKey);
    }

    public static SqlEntityDefinitionExpression CreateSqlEntityDefinitionExpression (Type type = null, string name = null, string owningTableAlias = null, Type primaryKeyType = null)
    {
      type = type ?? typeof (Cook);
      owningTableAlias = owningTableAlias ?? "t0";
      primaryKeyType = primaryKeyType ?? typeof (int);

      return new SqlEntityDefinitionExpression (
          type,
          owningTableAlias, 
          name,
          e => e.GetColumn (primaryKeyType, "ID", true),
          new[]
          {
              new SqlColumnDefinitionExpression (primaryKeyType, owningTableAlias, "ID", true),
              new SqlColumnDefinitionExpression (typeof (int), owningTableAlias, "Name", false),
              new SqlColumnDefinitionExpression (typeof (int), owningTableAlias, "City", false)
          });
    }

    public static UnresolvedGroupReferenceTableInfo CreateUnresolvedGroupReferenceTableInfo ()
    {
      return new UnresolvedGroupReferenceTableInfo (CreateSqlTable (typeof (IGrouping<int, string>)));
    }

    public static SqlGroupingSelectExpression CreateSqlGroupingSelectExpression ()
    {
      return new SqlGroupingSelectExpression (Expression.Constant ("key"), Expression.Constant ("element"));
    }

    public static ResolvedJoinedGroupingTableInfo CreateResolvedJoinedGroupingTableInfo (SqlStatement sqlStatement)
    {
      return new ResolvedJoinedGroupingTableInfo (
          "cook", 
          sqlStatement, 
          CreateSqlGroupingSelectExpression(), 
          "q1");
    }

    public static ISqlPreparationContext CreateSqlPreparationContext ()
    {
      return new SqlPreparationContext (null, new SqlStatementBuilder());
    }

    public static SqlEntityRefMemberExpression CreateSqlEntityRefMemberExpression ()
    {
      var originatingEntity = CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
    }

    public static SetOperationCombinedStatement CreateSetOperationCombinedStatement ()
    {
      return new SetOperationCombinedStatement (
          CreateSqlStatement(),
          SetOperation.Union);
    }

    public static Ordering CreateOrdering ()
    {
      return new Ordering (ExpressionHelper.CreateExpression(), OrderingDirection.Asc);
    }

    public static MemberInfo GetSomeMemberInfo ()
    {
      return GetKitchenCookMemberInfo();
    }

    public static MemberInfo GetKitchenCookMemberInfo ()
    {
      return typeof (Kitchen).GetProperty ("Cook");
    }

    public static PropertyInfo GetKitchenRestaurantMemberInfo ()
    {
      return typeof (Kitchen).GetProperty ("Restaurant");
    }

    public static SqlJoin CreateSqlJoin ()
    {
      return new SqlJoin (CreateSqlTable(), JoinSemantics.Inner, ExpressionHelper.CreateExpression (typeof (bool)));
    }
  }
}