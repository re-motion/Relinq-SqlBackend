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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlStatement"/> represents a SQL database query. The <see cref="QueryModel"/> is translated to this model, and the 
  /// <see cref="SqlStatement"/> is transformed several times until it can easily be translated to SQL text.
  /// </summary>
  public class SqlStatement
  {
    private readonly IStreamedDataInfo _dataInfo;
    private readonly SqlTableBase[] _sqlTables;
    private readonly Ordering[] _orderings;
    private readonly Expression _selectProjection;
    private readonly Expression _whereCondition;
    private readonly Expression _topExpression;
    private readonly bool _isCountQuery;
    private readonly bool _isDistinctQuery;

    public SqlStatement (IStreamedDataInfo dataInfo, Expression selectProjection, IEnumerable<SqlTableBase> sqlTables, IEnumerable<Ordering> orderings, Expression whereCondition, Expression topExpression, bool isCountQuery, bool isDistinctQuery)
    {
      ArgumentUtility.CheckNotNull ("dataInfo", dataInfo);
      ArgumentUtility.CheckNotNull ("selectProjection", selectProjection);
      ArgumentUtility.CheckNotNull ("sqlTables", sqlTables);
      ArgumentUtility.CheckNotNull ("orderings", orderings);

      if (whereCondition != null && whereCondition.Type != typeof (bool))
        throw new ArgumentTypeException ("whereCondition", typeof (bool), whereCondition.Type);

      if ((isCountQuery && topExpression != null) || (isCountQuery && isDistinctQuery))
        throw new NotSupportedException ("A SqlStatement cannot contain both Count and Top or Count and Distinct.");

      if (selectProjection.Type != typeof (string) && typeof (IEnumerable).IsAssignableFrom (selectProjection.Type))
        throw new NotSupportedException ("Subquery selects a collection where a single value is expected.");

      _dataInfo = dataInfo;
      _selectProjection = selectProjection;
      _sqlTables = sqlTables.ToArray();
      _orderings = orderings.ToArray();
      _whereCondition = whereCondition;
      _topExpression = topExpression;
      _isCountQuery = isCountQuery;
      _isDistinctQuery = isDistinctQuery;
    }

    public IStreamedDataInfo DataInfo
    {
      get { return _dataInfo; }
    }

    public bool IsCountQuery
    {
      get { return _isCountQuery; }
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

    public ReadOnlyCollection<Ordering> Orderings
    {
      get { return Array.AsReadOnly (_orderings); }
    }

    public override bool Equals (object obj)
    {
      if (obj == null)
        return false;

      var statement = obj as SqlStatement;
      if (statement == null)
        return false;

      return (_dataInfo == statement._dataInfo) &&
             (_selectProjection == statement._selectProjection) &&
             (_sqlTables.SequenceEqual (statement._sqlTables)) &&
             (_orderings.SequenceEqual(statement._orderings)) &&
             (_whereCondition == statement._whereCondition) &&
             (_topExpression == statement._topExpression) &&
             (_isCountQuery == statement._isCountQuery) &&
             (_isDistinctQuery == statement._isDistinctQuery);
    }

    public override int GetHashCode ()
    {
      return base.GetHashCode ();
    }
  }
}