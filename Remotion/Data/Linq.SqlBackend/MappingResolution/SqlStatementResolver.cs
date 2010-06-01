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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
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

    protected Expression ResolveSelectProjection (Expression selectProjection) 
    {
      ArgumentUtility.CheckNotNull ("selectProjection", selectProjection);

      return _stage.ResolveSelectExpression (selectProjection, _context);
    }

    protected void ResolveSqlTable (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      
      sqlTable.TableInfo = _stage.ResolveTableInfo (sqlTable.TableInfo, _context);
      ResolveJoins (sqlTable);
    }

    protected void ResolveJoinedTable (SqlJoinedTable joinedTable)
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);

      joinedTable.JoinInfo = _stage.ResolveJoinInfo (joinedTable.JoinInfo, _context);

      foreach (var table in joinedTable.JoinedTables)
        ResolveJoinedTable (table);
    }

    protected Expression ResolveWhereCondition (Expression whereCondition)
    {
      ArgumentUtility.CheckNotNull ("whereCondition", whereCondition);

      return _stage.ResolveWhereExpression (whereCondition, _context);
    }

    protected Expression ResolveOrderingExpression (Expression orderByExpression)
    {
      ArgumentUtility.CheckNotNull ("orderByExpression", orderByExpression);

      return _stage.ResolveOrderingExpression (orderByExpression, _context);
    }

    protected Expression ResolveTopExpression (Expression topExpression)
    {
      ArgumentUtility.CheckNotNull ("topExpression", topExpression);

      return _stage.ResolveTopExpression (topExpression, _context);
    }

    protected SqlStatement ResolveSqlStatement (SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      foreach (var sqlTable in sqlStatement.SqlTables)
        sqlTable.Accept (this);

      var sqlStatementBuilder = new SqlStatementBuilder(sqlStatement);

      var previousSelectProjection = sqlStatementBuilder.SelectProjection;
      sqlStatementBuilder.SelectProjection = _stage.ResolveSelectExpression (sqlStatementBuilder.SelectProjection, _context);
      sqlStatementBuilder.RecalculateDataInfo (previousSelectProjection);

      if (sqlStatementBuilder.WhereCondition != null)
        sqlStatementBuilder.WhereCondition = _stage.ResolveWhereExpression (sqlStatementBuilder.WhereCondition, _context);

      if (sqlStatementBuilder.TopExpression != null)
        sqlStatementBuilder.TopExpression = _stage.ResolveTopExpression (sqlStatementBuilder.TopExpression, _context);

      if (sqlStatementBuilder.Orderings.Count > 0)
      {
        // TODO Review 2821: Refactor this to avoid modifying the original orderings; write a test showing that the original SqlStatement contains the original expression even if the resolved statement contains a new expression
        // implement as follows:
        // for (int i = 0; i < sqlStatementBuilder.Orderings.Count; ++i)
        // {
        //   var newOrderingExpression = _stage.ResolveOrderingExpression (sqlStatementBuilder.Orderings[i].Expression, _context);
        //   if (newOrderingExpression != sqlStatementBuilder.Orderings[i].Expression)
        //     sqlStatementBuilder.Orderings[i] = new Ordering (sqlStatementBuilder.Orderings[i].Direction, newOrderingExpression);
        foreach (var orderByClause in sqlStatementBuilder.Orderings)
          orderByClause.Expression = _stage.ResolveOrderingExpression (orderByClause.Expression, _context);
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