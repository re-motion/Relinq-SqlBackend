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
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationContext"/> holds context information required during SQL preparation stage.
  /// </summary>
  public class SqlPreparationContext : ISqlPreparationContext
  {
    private readonly ISqlPreparationContext _parentContext;
    private readonly SqlStatementBuilder _sqlStatementBuilder;
    private readonly Dictionary<Expression, Expression> _mapping;

    public SqlPreparationContext (SqlStatementBuilder sqlStatementBuilder)
        : this (null, sqlStatementBuilder)
    {
    }

    public SqlPreparationContext (ISqlPreparationContext parentContext, SqlStatementBuilder sqlStatementBuilder)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlStatementBuilder), sqlStatementBuilder);

      _parentContext = parentContext;
      _sqlStatementBuilder = sqlStatementBuilder;
      _mapping = new Dictionary<Expression, Expression>();
    }

    public bool IsOuterMostQuery
    {
      get { return _parentContext == null; }
    }

    public void AddExpressionMapping (Expression original, Expression replacement)
    {
      ArgumentUtility.CheckNotNull (nameof(original), original);
      ArgumentUtility.CheckNotNull (nameof(replacement), replacement);

      _mapping[original] = replacement;
    }

    public void AddSqlTable (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlTable), sqlTable);

      _sqlStatementBuilder.SqlTables.Add (sqlTable);
    }

    public Expression GetExpressionMapping (Expression original)
    {
      ArgumentUtility.CheckNotNull (nameof(original), original);

      Expression result;
      if (_mapping.TryGetValue (original, out result))
        return result;

      if (_parentContext != null)
        return _parentContext.GetExpressionMapping (original);

      return null;
    }
    
  }
}