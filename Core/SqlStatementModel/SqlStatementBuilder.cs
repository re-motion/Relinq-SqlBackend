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
using System.Text;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlStatementBuilder"/> holds the specific SQL statement data and populates a build method.
  /// </summary>
  public sealed class SqlStatementBuilder
  {
    public IStreamedDataInfo DataInfo { get; set; }
    public bool IsDistinctQuery { get; set; }
    public Expression TopExpression { get; set; }
    public Expression SelectProjection { get; set; }
    public List<SqlAppendedTable> SqlTables { get; private set; }
    public Expression WhereCondition { get; set; }
    public Expression GroupByExpression { get; set; }
    public List<Ordering> Orderings { get; private set; }
    public Expression RowNumberSelector { get; set; }
    public Expression CurrentRowNumberOffset { get; set; }
    public List<SetOperationCombinedStatement> SetOperationCombinedStatements { get; private set; }

    public SqlStatementBuilder ()
    {
      Reset();
    }

    public SqlStatementBuilder (SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      DataInfo = sqlStatement.DataInfo;
      SelectProjection = sqlStatement.SelectProjection;
      WhereCondition = sqlStatement.WhereCondition;
      IsDistinctQuery = sqlStatement.IsDistinctQuery;
      TopExpression = sqlStatement.TopExpression;
      GroupByExpression = sqlStatement.GroupByExpression;
      RowNumberSelector = sqlStatement.RowNumberSelector;
      CurrentRowNumberOffset = sqlStatement.CurrentRowNumberOffset;

      SqlTables = new List<SqlAppendedTable> (sqlStatement.SqlTables);
      Orderings = new List<Ordering> (sqlStatement.Orderings);
      SetOperationCombinedStatements = new List<SetOperationCombinedStatement> (sqlStatement.SetOperationCombinedStatements);
    }

    public SqlStatement GetSqlStatement ()
    {
      if (DataInfo == null)
        throw new InvalidOperationException ("A DataInfo must be set before the SqlStatement can be retrieved.");

      if (SelectProjection == null)
        throw new InvalidOperationException ("A SelectProjection must be set before the SqlStatement can be retrieved.");
      
      return new SqlStatement (
          DataInfo,
          SelectProjection,
          SqlTables,
          WhereCondition,
          GroupByExpression,
          Orderings,
          TopExpression,
          IsDistinctQuery,
          RowNumberSelector,
          CurrentRowNumberOffset,
          SetOperationCombinedStatements);
    }

    public void AddWhereCondition (Expression translatedExpression)
    {
      if (WhereCondition != null)
        WhereCondition = Expression.AndAlso (WhereCondition, translatedExpression);
      else
        WhereCondition = translatedExpression;
    }

    public SqlStatement GetStatementAndResetBuilder ()
    {
      var sqlSubStatement = GetSqlStatement();

      Reset();

      return sqlSubStatement;
    }

    public void RecalculateDataInfo (Expression previousSelectProjection)
    {
      if (SelectProjection.Type != previousSelectProjection.Type) // TODO: Consider removing this check and the parameter
      {
        var sequenceInfo = DataInfo as StreamedSequenceInfo;
        if (sequenceInfo != null)
          DataInfo = new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (SelectProjection.Type), SelectProjection);

        var singleValueInfo = DataInfo as StreamedSingleValueInfo;
        if (singleValueInfo != null)
          DataInfo = new StreamedSingleValueInfo (SelectProjection.Type, singleValueInfo.ReturnDefaultWhenEmpty);

        // For scalar queries, the DataInfo never needs to be recalculated.
      }
    }

    public override string ToString ()
    {
      var sb = new StringBuilder ("SELECT ");
      if (IsDistinctQuery)
        sb.Append ("DISTINCT ");
      if (TopExpression != null)
        sb.Append ("TOP (").Append (TopExpression).Append (") ");
      if (SelectProjection != null)
        sb.Append (SelectProjection);
      if (SqlTables.Count > 0)
      {
        sb.Append (" FROM ");
        sb.Append (SqlTables.First());
        SqlTables.Skip(1).Aggregate (sb, (builder, table) => builder.Append (" ").Append (table));
      }
      if (WhereCondition != null)
        sb.Append (" WHERE ").Append (WhereCondition);
      if (GroupByExpression != null)
        sb.Append (" GROUP BY ").Append (GroupByExpression);
      if (Orderings.Count > 0)
      {
        sb.Append (" ORDER BY ");
        Orderings.Aggregate (
            sb,
            (builder, ordering) => builder
                                       .Append (ordering.Expression)
                                       .Append (" ")
                                       .Append (ordering.OrderingDirection.ToString().ToUpper()));
      }

      foreach (var combinedStatement in SetOperationCombinedStatements)
      {
        sb
            .Append (" ")
            .Append (combinedStatement.SetOperation.ToString().ToUpper())
            .Append (" (")
            .Append (combinedStatement.SqlStatement)
            .Append (")");
      }

      return sb.ToString();
    }

    private void Reset ()
    {
      DataInfo = null;
      SelectProjection = null;
      WhereCondition = null;
      IsDistinctQuery = false;
      TopExpression = null;
      GroupByExpression = null;
      RowNumberSelector = null;
      CurrentRowNumberOffset = null;

      SqlTables = new List<SqlAppendedTable>();
      Orderings = new List<Ordering>();
      SetOperationCombinedStatements = new List<SetOperationCombinedStatement>();
    }
  }
}