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
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="SqlStatementResolver"/> provides methods to visit sql-statement classes.
  /// </summary>
  public class SqlStatementResolver
  {
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    public static SqlStatement ResolveExpressions (IMappingResolutionStage stage, SqlStatement statement, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("statement", statement);
      ArgumentUtility.CheckNotNull ("context", context);
      
      var resolver = new SqlStatementResolver (stage, context);
      return resolver.ResolveSqlStatement (statement);
    }

    protected SqlStatementResolver (IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);
      
      _stage = stage;
      _context = context;
    }

    protected virtual Expression ResolveSelectProjection (Expression selectProjection, SqlStatementBuilder sqlStatementBuilder) 
    {
      ArgumentUtility.CheckNotNull ("selectProjection", selectProjection);

      return _stage.ResolveSelectExpression (selectProjection, sqlStatementBuilder, _context);
    }

    protected virtual void ResolveSqlTable (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      
      sqlTable.TableInfo = _stage.ResolveTableInfo (sqlTable.TableInfo, _context);

      ResolveJoins (sqlTable);
    }

    protected virtual Expression ResolveWhereCondition (Expression whereCondition)
    {
      ArgumentUtility.CheckNotNull ("whereCondition", whereCondition);

      return _stage.ResolveWhereExpression (whereCondition, _context);
    }

    protected virtual Expression ResolveGroupByExpression (Expression groupByExpression)
    {
      ArgumentUtility.CheckNotNull ("groupByExpression", groupByExpression);

      return _stage.ResolveGroupByExpression (groupByExpression, _context);
    }
    
    protected virtual Expression ResolveOrderingExpression (Expression orderByExpression)
    {
      ArgumentUtility.CheckNotNull ("orderByExpression", orderByExpression);

      return _stage.ResolveOrderingExpression (orderByExpression, _context);
    }

    protected virtual Expression ResolveTopExpression (Expression topExpression)
    {
      ArgumentUtility.CheckNotNull ("topExpression", topExpression);

      return _stage.ResolveTopExpression (topExpression, _context);
    }

    protected virtual SqlStatement ResolveSqlStatement (SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      foreach (var appendedTable in sqlStatement.SqlTables)
        ResolveSqlTable (appendedTable.SqlTable);

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

    private void ResolveJoins (SqlTable sqlTable)
    {
      Dictionary<SqlJoin, SqlJoin> modifiedJoins = null;
      foreach (var join in sqlTable.OrderedJoins)
      {
        ResolveSqlTable (join.JoinedTable);

        var resolvedJoinCondition = _stage.ResolveJoinCondition (join.JoinCondition, _context);
        if (resolvedJoinCondition != join.JoinCondition)
        {
          modifiedJoins = new Dictionary<SqlJoin, SqlJoin>();
          modifiedJoins.Add (join, new SqlJoin (join.JoinedTable, join.JoinSemantics, resolvedJoinCondition));
        }
      }

      if (modifiedJoins != null)
        sqlTable.SubstituteJoins (modifiedJoins);
    }
  }
}