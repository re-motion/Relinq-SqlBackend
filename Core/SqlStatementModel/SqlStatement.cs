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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Utilities;

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
    private readonly SqlTable[] _sqlTables;
    private readonly Expression _groupByExpression;
    private readonly Ordering[] _orderings;
    private readonly Expression _whereCondition;
    private readonly Expression _topExpression;
    private readonly bool _isDistinctQuery;
    private readonly Expression _rowNumberSelector;
    private readonly Expression _currentRowNumberOffset;
    private readonly SetOperationCombinedStatement[] _setOperationCombinedStatements;

    public SqlStatement (
        IStreamedDataInfo dataInfo,
        Expression selectProjection,
        IEnumerable<SqlTable> sqlTables,
        Expression whereCondition,
        Expression groupByExpression,
        IEnumerable<Ordering> orderings,
        Expression topExpression,
        bool isDistinctQuery,
        Expression rowNumberSelector,
        Expression currentRowNumberOffset,
        IEnumerable<SetOperationCombinedStatement> setOperationCombinedStatements)
    {
      ArgumentUtility.CheckNotNull (nameof(dataInfo), dataInfo);
      ArgumentUtility.CheckNotNull (nameof(selectProjection), selectProjection);
      ArgumentUtility.CheckNotNull (nameof(sqlTables), sqlTables);
      ArgumentUtility.CheckNotNull (nameof(orderings), orderings);
      ArgumentUtility.CheckNotNull (nameof(setOperationCombinedStatements), setOperationCombinedStatements);
      
      if (whereCondition != null && whereCondition.Type != typeof (bool))
        throw ArgumentUtility.CreateArgumentTypeException (nameof(whereCondition), whereCondition.Type, typeof (bool));

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
      _setOperationCombinedStatements = setOperationCombinedStatements.ToArray();
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

    public ReadOnlyCollection<SqlTable> SqlTables
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

    // IDEA: When refactoring for full immutability, also change to no longer use Ordering here - it's not immutable!
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

    public ReadOnlyCollection<SetOperationCombinedStatement> SetOperationCombinedStatements
    {
      get { return Array.AsReadOnly (_setOperationCombinedStatements); }
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

      return (_dataInfo.Equals (statement._dataInfo))
             && (_selectProjection == statement._selectProjection)
             && (_whereCondition == statement._whereCondition)
             && (_topExpression == statement._topExpression)
             && (_isDistinctQuery == statement._isDistinctQuery)
             && (_rowNumberSelector == statement._rowNumberSelector)
             && (_currentRowNumberOffset == statement._currentRowNumberOffset)
             && (_groupByExpression == statement._groupByExpression)
             // Note: These items are all compared by reference, which is okay because the visitors take care to reuse the objects if their contents
             // don't change.
             && (_sqlTables.SequenceEqual (statement._sqlTables))
             && (_orderings.SequenceEqual (statement._orderings))
             && (_setOperationCombinedStatements.SequenceEqual(statement.SetOperationCombinedStatements));
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (
          _dataInfo,
          _selectProjection,
          _whereCondition,
          _topExpression,
          _isDistinctQuery,
          _rowNumberSelector,
          _currentRowNumberOffset,
          _groupByExpression)
             ^ EqualityUtility.GetRotatedHashCode (_sqlTables)
             ^ EqualityUtility.GetRotatedHashCode (_orderings)
             ^ EqualityUtility.GetRotatedHashCode (_setOperationCombinedStatements);
    }

    public override string ToString ()
    {
      var statementBuilder = new SqlStatementBuilder (this);
      return statementBuilder.ToString();
    }
  }
}