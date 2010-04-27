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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="SqlContextStatementVisitor"/> applies <see cref="SqlExpressionContext"/> to a <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlContextStatementVisitor
  {
    private readonly IMappingResolutionStage _stage;

    public static SqlStatement ApplyContext (SqlStatement sqlStatement, SqlExpressionContext context, IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlContextStatementVisitor (stage);
      return visitor.VisitSqlStatement (sqlStatement, context);
    }

    private SqlContextStatementVisitor (IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);

      _stage = stage;
    }
    
    public SqlStatement VisitSqlStatement (SqlStatement sqlStatement, SqlExpressionContext context)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      if (context == SqlExpressionContext.PredicateRequired)
        throw new InvalidOperationException ("A SqlStatement cannot be used as a predicate.");

      var statementBuilder = new SqlStatementBuilder ();

      statementBuilder.IsCountQuery = sqlStatement.IsCountQuery;
      statementBuilder.IsDistinctQuery = sqlStatement.IsDistinctQuery;

      VisitSelectProjection(sqlStatement.SelectProjection, context, statementBuilder);
      VisitWhereCondition(sqlStatement.WhereCondition, statementBuilder);
      VisitOrderings (sqlStatement.Orderings, statementBuilder);
      VisitTopExpression(sqlStatement.TopExpression, statementBuilder);
      VisitSqlTables (sqlStatement.SqlTables, statementBuilder);

      if (statementBuilder.SelectProjection != sqlStatement.SelectProjection)
        statementBuilder.DataInfo = GetNewDataInfo (sqlStatement.DataInfo, statementBuilder.SelectProjection);
      else
        statementBuilder.DataInfo = sqlStatement.DataInfo;

      return statementBuilder.GetSqlStatement();
    }

    private void VisitSelectProjection (Expression selectProjection, SqlExpressionContext selectContext, SqlStatementBuilder statementBuilder)
    {
      var newSelectProjection = _stage.ApplyContext (selectProjection, selectContext);
      statementBuilder.SelectProjection = newSelectProjection;
    }

    private void VisitWhereCondition (Expression whereCondition, SqlStatementBuilder statementBuilder)
    {
      if (whereCondition != null)
        statementBuilder.WhereCondition = _stage.ApplyContext (whereCondition, SqlExpressionContext.PredicateRequired);
    }

    private void VisitOrderings (IEnumerable<Ordering> orderings, SqlStatementBuilder statementBuilder)
    {
      foreach (var ordering in orderings)
      {
        var newExpression = _stage.ApplyContext (ordering.Expression, SqlExpressionContext.SingleValueRequired);
        statementBuilder.Orderings.Add (new Ordering (newExpression, ordering.OrderingDirection));
      }
    }

    private void VisitTopExpression (Expression topExpression, SqlStatementBuilder statementBuilder)
    {
      if (topExpression != null)
        statementBuilder.TopExpression = _stage.ApplyContext (topExpression, SqlExpressionContext.SingleValueRequired);
    }

    private void VisitSqlTables (IEnumerable<SqlTableBase> tables, SqlStatementBuilder statementBuilder)
    {
      foreach (var table in tables)
      {
        _stage.ApplyContext (table, SqlExpressionContext.ValueRequired);
        statementBuilder.SqlTables.Add (table);
      }
    }

    private IStreamedDataInfo GetNewDataInfo (IStreamedDataInfo previousDataInfo, Expression newSelectProjection)
    {
      var previousStreamedSequenceInfo = previousDataInfo as StreamedSequenceInfo;
      if (previousStreamedSequenceInfo != null)
        return new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (newSelectProjection.Type), newSelectProjection);

      var previousSingleValueInfo = previousDataInfo as StreamedSingleValueInfo;
      if (previousSingleValueInfo != null)
        return new StreamedSingleValueInfo (newSelectProjection.Type, previousSingleValueInfo.ReturnDefaultWhenEmpty);

      return previousDataInfo;
    }
  }
}