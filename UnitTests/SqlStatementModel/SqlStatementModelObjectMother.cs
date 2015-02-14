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
using System.Reflection;
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
      return CreateSqlStatement (selectProjection, sqlTables.Select (x => CreateSqlAppendedTable (x)).ToArray());
    }

    public static SqlStatement CreateSqlStatement (Expression selectProjection, params SqlAppendedTable[] appendedTables)
    {
      return new SqlStatement (
          new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (null, typeof (Cook))),
          selectProjection,
          appendedTables,
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
          new[] { CreateSqlAppendedTable (sqlTable) },
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
          new[] { CreateSqlAppendedTable (sqlTable) },
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
          new[] { CreateSqlAppendedTable (sqlTable) },
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
          new SqlAppendedTable[0],
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
      return new SqlTable (tableInfo);
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
      return new SqlTable (unresolvedTableInfo);
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
        string tableAlias)
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (string), tableName, tableAlias);
      return new SqlTable (resolvedTableInfo);
    }

    public static SqlTable CreateSqlTable_WithResolvedTableInfo (Type type, string tableName, string tableAlias)
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (type, tableName, tableAlias);
      return new SqlTable (resolvedTableInfo);
    }

    public static ITableInfo CreateTableInfo (Type type)
    {
      return CreateUnresolvedTableInfo (type);
    }

    public static UnresolvedTableInfo CreateUnresolvedTableInfo ()
    {
      return CreateUnresolvedTableInfo (typeof (Cook));
    }

    public static UnresolvedTableInfo CreateUnresolvedTableInfo (Type type)
    {
      return new UnresolvedTableInfo (type);
    }

    public static UnresolvedJoinTableInfo CreateUnresolvedJoinTableInfo ()
    {
      return CreateUnresolvedJoinTableInfo_KitchenCook();
    }

    public static UnresolvedJoinTableInfo CreateUnresolvedJoinTableInfo_KitchenCook ()
    {
      var entityExpression = CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      return new UnresolvedJoinTableInfo (entityExpression, GetKitchenCookMemberInfo(), JoinCardinality.One);
    }

    public static UnresolvedJoinTableInfo CreateUnresolvedJoinTableInfo_CookSubstitution ()
    {
      var entityExpression = CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      return new UnresolvedJoinTableInfo (entityExpression, GetCookSubstitutionMemberInfo(), JoinCardinality.One);
    }

    public static UnresolvedCollectionJoinTableInfo CreateUnresolvedCollectionJoinTableInfo ()
    {
      return CreateUnresolvedCollectionJoinTableInfo_RestaurantCooks();
    }

    public static UnresolvedCollectionJoinTableInfo CreateUnresolvedCollectionJoinTableInfo_RestaurantCooks ()
    {
      return new UnresolvedCollectionJoinTableInfo (Expression.Constant (new Restaurant()), typeof (Restaurant).GetProperty ("Cooks"));
    }

    public static ResolvedSimpleTableInfo CreateResolvedTableInfo ()
    {
      return CreateResolvedTableInfo (typeof (Cook));
    }

    public static ResolvedSimpleTableInfo CreateResolvedTableInfo (Type type)
    {
      return new ResolvedSimpleTableInfo (type, "table", "t");
    }

    public static SqlColumnExpression CreateSqlColumn (Type type = null, string owningTableAlias = null, string column = null, bool isPrimaryKey = false)
    {
      return new SqlColumnDefinitionExpression (type ?? typeof (int), owningTableAlias ?? "t0", column ?? "column", isPrimaryKey);
    }

    public static SqlEntityExpression CreateSqlEntityExpression ()
    {
      return CreateSqlEntityDefinitionExpression();
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
      var memberInfo = GetKitchenCookMemberInfo();
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

    public static MemberInfo GetCookSubstitutionMemberInfo ()
    {
      return typeof (Cook).GetProperty ("Substitution");
    }

    public static SqlJoin CreateSqlJoin ()
    {
      return new SqlJoin (CreateSqlTable(), JoinSemantics.Inner, ExpressionHelper.CreateExpression (typeof (bool)));
    }

    public static SqlAppendedTable CreateSqlAppendedTable (SqlTable sqlTable = null, JoinSemantics semantics = JoinSemantics.Inner)
    {
      return new SqlAppendedTable (sqlTable ?? CreateSqlTable(), semantics);
    }

    public static SqlAppendedTable CreateSqlAppendedTable (ITableInfo tableInfo, JoinSemantics semantics = JoinSemantics.Inner)
    {
      return new SqlAppendedTable (CreateSqlTable(tableInfo), semantics);
    }

    public static SqlTable.LeftJoinData CreateLeftJoinData ()
    {
      return new SqlTable.LeftJoinData (CreateSqlTable(), ExpressionHelper.CreateExpression (typeof (bool)));
    }
  }
}