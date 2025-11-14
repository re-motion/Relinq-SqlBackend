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
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="SqlStatementResolver"/> provides methods to visit sql-statement classes.
  /// </summary>
  public class SqlStatementResolver : ISqlTableBaseVisitor
  {
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    public static SqlStatement ResolveExpressions (IMappingResolutionStage stage, SqlStatement statement, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(statement), statement);
      ArgumentUtility.CheckNotNull (nameof(context), context);
      
      var resolver = new SqlStatementResolver (stage, context);
      return resolver.ResolveSqlStatement (statement);
    }

    protected SqlStatementResolver (IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(context), context);
      
      _stage = stage;
      _context = context;
    }

    protected virtual Expression ResolveSelectProjection (Expression selectProjection, SqlStatementBuilder sqlStatementBuilder) 
    {
      ArgumentUtility.CheckNotNull (nameof(selectProjection), selectProjection);

      return _stage.ResolveSelectExpression (selectProjection, sqlStatementBuilder, _context);
    }

    protected virtual void ResolveSqlTable (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlTable), sqlTable);
      
      sqlTable.TableInfo = _stage.ResolveTableInfo (sqlTable.TableInfo, _context);
      ResolveJoins (sqlTable);
    }

    protected virtual void ResolveJoinedTable (SqlJoinedTable joinedTable)
    {
      ArgumentUtility.CheckNotNull (nameof(joinedTable), joinedTable);

      joinedTable.JoinInfo = _stage.ResolveJoinInfo (joinedTable.JoinInfo, _context);

      foreach (var table in joinedTable.JoinedTables)
        ResolveJoinedTable (table);
    }

    protected virtual Expression ResolveWhereCondition (Expression whereCondition)
    {
      ArgumentUtility.CheckNotNull (nameof(whereCondition), whereCondition);

      return _stage.ResolveWhereExpression (whereCondition, _context);
    }

    protected virtual Expression ResolveGroupByExpression (Expression groupByExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(groupByExpression), groupByExpression);

      return _stage.ResolveGroupByExpression (groupByExpression, _context);
    }
    
    protected virtual Expression ResolveOrderingExpression (Expression orderByExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(orderByExpression), orderByExpression);

      return _stage.ResolveOrderingExpression (orderByExpression, _context);
    }

    protected virtual Expression ResolveTopExpression (Expression topExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(topExpression), topExpression);

      return _stage.ResolveTopExpression (topExpression, _context);
    }

    protected virtual SqlStatement ResolveSqlStatement (SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatement), sqlStatement);

      foreach (var sqlTable in sqlStatement.SqlTables)
        sqlTable.Accept (this);

      var sqlStatementBuilder = new SqlStatementBuilder(sqlStatement);

      var previousSelectProjection = sqlStatementBuilder.SelectProjection;
      sqlStatementBuilder.SelectProjection = ResolveSelectProjection(sqlStatementBuilder.SelectProjection, sqlStatementBuilder);
      sqlStatementBuilder.RecalculateDataInfo (previousSelectProjection);

      if (sqlStatementBuilder.GroupByExpression != null)
        sqlStatementBuilder.GroupByExpression = ResolveGroupByExpression(sqlStatementBuilder.GroupByExpression);

      if (sqlStatementBuilder.WhereCondition != null)
        sqlStatementBuilder.WhereCondition = ResolveWhereCondition(sqlStatementBuilder.WhereCondition);

      if (sqlStatementBuilder.TopExpression != null)
        sqlStatementBuilder.TopExpression = ResolveTopExpression(sqlStatementBuilder.TopExpression);

      if (sqlStatementBuilder.Orderings.Count > 0)
      {
        for (int i = 0; i < sqlStatementBuilder.Orderings.Count; ++i)
        {
          var resolvedOrderingExpression = _stage.ResolveOrderingExpression (sqlStatementBuilder.Orderings[i].Expression, _context);
          if (resolvedOrderingExpression != sqlStatementBuilder.Orderings[i].Expression)
            sqlStatementBuilder.Orderings[i] = new Ordering (resolvedOrderingExpression, sqlStatementBuilder.Orderings[i].OrderingDirection);
        }
      }

      for (int i = 0; i < sqlStatement.SetOperationCombinedStatements.Count; i++)
      {
        var combinedStatement = sqlStatement.SetOperationCombinedStatements[i];
        var resolvedSqlStatement = _stage.ResolveSqlStatement (combinedStatement.SqlStatement, _context);
        if (!Equals (resolvedSqlStatement, combinedStatement.SqlStatement))
        {
          sqlStatementBuilder.SetOperationCombinedStatements[i] = new SetOperationCombinedStatement (
              resolvedSqlStatement,
              combinedStatement.SetOperation);
        }
      }

      return sqlStatementBuilder.GetSqlStatement();
    }

    void ISqlTableBaseVisitor.VisitSqlTable (SqlTable sqlTable)
    {
      ResolveSqlTable (sqlTable);
    }

    void ISqlTableBaseVisitor.VisitSqlJoinedTable (SqlJoinedTable joinedTable)
    {
      ResolveJoinedTable (joinedTable);
    }

    private void ResolveJoins (SqlTableBase sqlTable)
    {
      foreach (var joinedTable in sqlTable.JoinedTables)
      {
        ResolveJoinedTable (joinedTable);
      }
    }
  }
}