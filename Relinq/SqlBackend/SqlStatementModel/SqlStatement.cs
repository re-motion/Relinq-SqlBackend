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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlStatement"/> represents a SQL database query. The <see cref="QueryModel"/> is translated to this model, and the 
  /// <see cref="SqlStatement"/> is transformed several times until it can easily be translated to SQL text.
  /// </summary>
  public class SqlStatement
  {
    private readonly IStreamedDataInfo _dataInfo;
    private readonly Expression _selectProjection;
    private readonly SqlTableBase[] _sqlTables;
    private readonly Expression _groupByExpression;
    private readonly Ordering[] _orderings;
    private readonly Expression _whereCondition;
    private readonly Expression _topExpression;
    private readonly bool _isDistinctQuery;
    private readonly Expression _rowNumberSelector;
    private readonly Expression _currentRowNumberOffset;

    public SqlStatement (
        IStreamedDataInfo dataInfo,
        Expression selectProjection,
        IEnumerable<SqlTableBase> sqlTables,
        Expression whereCondition,
        Expression groupByExpression,
        IEnumerable<Ordering> orderings,
        Expression topExpression,
        bool isDistinctQuery,
        Expression rowNumberSelector,
        Expression currentRowNumberOffset)
    {
      ArgumentUtility.CheckNotNull ("dataInfo", dataInfo);
      ArgumentUtility.CheckNotNull ("selectProjection", selectProjection);
      ArgumentUtility.CheckNotNull ("sqlTables", sqlTables);
      ArgumentUtility.CheckNotNull ("orderings", orderings);

      if (whereCondition != null && whereCondition.Type != typeof (bool))
        throw new ArgumentTypeException ("whereCondition", typeof (bool), whereCondition.Type);

      _dataInfo = dataInfo;
      _selectProjection = selectProjection;
      _sqlTables = sqlTables.ToArray();
      _orderings = orderings.ToArray();
      _whereCondition = whereCondition;
      _topExpression = topExpression;
      _isDistinctQuery = isDistinctQuery;
      _rowNumberSelector = rowNumberSelector;
      _currentRowNumberOffset = currentRowNumberOffset;
      _groupByExpression = groupByExpression;
    }

    public IStreamedDataInfo DataInfo
    {
      get { return _dataInfo; }
    }

    public bool IsDistinctQuery
    {
      get { return _isDistinctQuery; }
    }

    public Expression TopExpression
    {
      get { return _topExpression; }
    }

    public Expression SelectProjection
    {
      get { return _selectProjection; }
    }

    public ReadOnlyCollection<SqlTableBase> SqlTables
    {
      get { return Array.AsReadOnly (_sqlTables); }
    }

    public Expression WhereCondition
    {
      get { return _whereCondition; }
    }

    public Expression GroupByExpression
    {
      get { return _groupByExpression; }
    }

    public ReadOnlyCollection<Ordering> Orderings
    {
      get { return Array.AsReadOnly (_orderings); }
    }

    public Expression RowNumberSelector
    {
      get { return _rowNumberSelector; }
    }

    public Expression CurrentRowNumberOffset
    {
      get { return _currentRowNumberOffset; }
    }

    public Expression CreateExpression ()
    {
      return SqlTables.Count == 0 && !IsDistinctQuery ? SelectProjection : new SqlSubStatementExpression (this);
    }

    public override bool Equals (object obj)
    {
      var statement = obj as SqlStatement;
      if (statement == null)
        return false;

      // TODO 3303: Compare DataInfo by value when implemented
      return (_dataInfo.DataType.Equals (statement._dataInfo.DataType))
              && (_dataInfo.GetType().Equals (statement._dataInfo.GetType()))
              && (_selectProjection == statement._selectProjection)
              && (_sqlTables.SequenceEqual (statement._sqlTables))
              && (_orderings.SequenceEqual (statement._orderings))
              && (_whereCondition == statement._whereCondition)
              && (_topExpression == statement._topExpression)
              && (_isDistinctQuery == statement._isDistinctQuery)
              && (_rowNumberSelector == statement._rowNumberSelector)
              && (_currentRowNumberOffset == statement._currentRowNumberOffset)
              && (_groupByExpression == statement._groupByExpression);
    }

    public override int GetHashCode ()
    {
      return HashCodeUtility.GetHashCodeOrZero (_dataInfo.DataType)
             ^ HashCodeUtility.GetHashCodeOrZero (_dataInfo.GetType())
             ^ HashCodeUtility.GetHashCodeOrZero (_selectProjection)
             ^ HashCodeUtility.GetHashCodeForSequence (_sqlTables)
             ^ HashCodeUtility.GetHashCodeForSequence (_orderings)
             ^ HashCodeUtility.GetHashCodeOrZero (_whereCondition)
             ^ HashCodeUtility.GetHashCodeOrZero (_topExpression)
             ^ HashCodeUtility.GetHashCodeOrZero (_isDistinctQuery)
             ^ HashCodeUtility.GetHashCodeOrZero (_rowNumberSelector)
             ^ HashCodeUtility.GetHashCodeOrZero (_currentRowNumberOffset)
             ^ HashCodeUtility.GetHashCodeOrZero (_groupByExpression);
    }

    public override string ToString ()
    {
      var sb = new StringBuilder ("SELECT ");
      if (IsDistinctQuery)
        sb.Append ("DISTINCT ");
      if (TopExpression != null)
        sb.Append ("TOP (").Append (FormattingExpressionTreeVisitor.Format (TopExpression)).Append (") ");
      sb.Append (FormattingExpressionTreeVisitor.Format (SelectProjection));
      if (SqlTables.Count > 0)
      {
        sb.Append (" FROM ");
        SqlTables.Aggregate (sb, (builder, table) => builder.Append (table));
      }
      if (WhereCondition != null)
        sb.Append (" WHERE ").Append (FormattingExpressionTreeVisitor.Format (WhereCondition));
      if (GroupByExpression != null)
        sb.Append (" GROUP BY ").Append (FormattingExpressionTreeVisitor.Format (GroupByExpression));
      if (Orderings.Count > 0)
      {
        sb.Append (" ORDER BY ");
        Orderings.Aggregate (
            sb,
            (builder, ordering) => builder
                                       .Append (FormattingExpressionTreeVisitor.Format (ordering.Expression))
                                       .Append (" ")
                                       .Append (ordering.OrderingDirection.ToString().ToUpper()));
      }

      return sb.ToString();
    }
  }
}