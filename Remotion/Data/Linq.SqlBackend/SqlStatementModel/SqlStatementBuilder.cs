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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlStatementBuilder"/> holds the specific SQL statement data and populates a build method.
  /// </summary>
  public class SqlStatementBuilder
  {
    private ValueHolder _valueHolder;

    public SqlStatementBuilder ()
    {
      _valueHolder = new ValueHolder();
    }

    public SqlStatementBuilder (SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      _valueHolder = new ValueHolder (sqlStatement);
    }

    public IStreamedDataInfo DataInfo
    {
      get { return _valueHolder.DataInfo; }
      set { _valueHolder.DataInfo = value; }
    }

    public bool IsDistinctQuery
    {
      get { return _valueHolder.IsDistinctQuery; }
      set { _valueHolder.IsDistinctQuery = value; }
    }

    public Expression TopExpression
    {
      get { return _valueHolder.TopExpression; }
      set { _valueHolder.TopExpression = value; }
    }

    public Expression SelectProjection
    {
      get { return _valueHolder.SelectProjection; }
      set { _valueHolder.SelectProjection = value; }
    }

    public Expression WhereCondition
    {
      get { return _valueHolder.WhereCondition; }
      set { _valueHolder.WhereCondition = value; }
    }

    public List<SqlTableBase> SqlTables
    {
      get { return _valueHolder.SqlTables; }
    }

    public List<Ordering> Orderings
    {
      get { return _valueHolder.Orderings; }
    }

    public SqlStatement GetSqlStatement ()
    {
      if (DataInfo == null)
        throw new InvalidOperationException ("A DataInfo must be set before the SqlStatement can be retrieved.");
      return new SqlStatement (
          DataInfo, SelectProjection, SqlTables, Orderings, WhereCondition, TopExpression, IsDistinctQuery);
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
      _valueHolder = new ValueHolder();
      return sqlSubStatement;
    }

    public void RecalculateDataInfo (Expression previousSelectProjection)
    {
      if (SelectProjection.Type != previousSelectProjection.Type)
        DataInfo = GetNewDataInfo ();
    }

    private IStreamedDataInfo GetNewDataInfo ()
    {
      var sequenceInfo = DataInfo as StreamedSequenceInfo;
      if (sequenceInfo != null)
        return new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (SelectProjection.Type), SelectProjection);

      var singleValueInfo = DataInfo as StreamedSingleValueInfo;
      if (singleValueInfo != null)
        return new StreamedSingleValueInfo (SelectProjection.Type, singleValueInfo.ReturnDefaultWhenEmpty);

      return DataInfo;
    }

    private class ValueHolder
    {
      public ValueHolder ()
      {
        SqlTables = new List<SqlTableBase>();
        Orderings = new List<Ordering>();
      }

      public ValueHolder (SqlStatement sqlStatement)
      {
        DataInfo = sqlStatement.DataInfo;
        SelectProjection = sqlStatement.SelectProjection;
        WhereCondition = sqlStatement.WhereCondition;
        IsDistinctQuery = sqlStatement.IsDistinctQuery;
        TopExpression = sqlStatement.TopExpression;
        
        SqlTables = new List<SqlTableBase> (sqlStatement.SqlTables);
        Orderings = new List<Ordering> (sqlStatement.Orderings);
      }

      public IStreamedDataInfo DataInfo { get; set; }

      public bool IsDistinctQuery { get; set; }
      
      public Expression TopExpression { get; set; }

      public Expression SelectProjection { get; set; }
      public Expression WhereCondition { get; set; }

      public List<SqlTableBase> SqlTables { get; private set; }
      public List<Ordering> Orderings { get; private set; }
    }
  }
}