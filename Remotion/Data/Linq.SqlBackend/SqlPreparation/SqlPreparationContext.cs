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
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
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
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);

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
      ArgumentUtility.CheckNotNull ("original", original);
      ArgumentUtility.CheckNotNull ("replacement", replacement);

      _mapping[original] = replacement;
    }

    public void AddSqlTable (SqlTableBase sqlTableBase)
    {
      ArgumentUtility.CheckNotNull ("sqlTableBase", sqlTableBase);

      _sqlStatementBuilder.SqlTables.Add (sqlTableBase);
    }

    public Expression GetExpressionMapping (Expression original)
    {
      ArgumentUtility.CheckNotNull ("original", original);

      Expression result;
      if (_mapping.TryGetValue (original, out result))
        return result;

      if (_parentContext != null)
        return _parentContext.GetExpressionMapping (original);

      return null;
    }
  }
}