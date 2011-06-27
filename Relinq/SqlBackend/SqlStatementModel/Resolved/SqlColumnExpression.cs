// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="SqlColumnExpression"/> represents a sql-specific column expression.
  /// </summary>
  public abstract class SqlColumnExpression : ExtensionExpression
  {
    private readonly string _owningTableAlias;
    private readonly string _columnName;
    private readonly bool _isPrimaryKey;

    protected SqlColumnExpression (Type type, string owningTableAlias, string columnName, bool isPrimaryKey)
        : base(type)
    {
      ArgumentUtility.CheckNotNull ("owningTableAlias", owningTableAlias);
      ArgumentUtility.CheckNotNullOrEmpty ("columnName", columnName);
      ArgumentUtility.CheckNotNull ("isPrimaryKey", isPrimaryKey);

      _owningTableAlias = owningTableAlias;
      _columnName = columnName;
      _isPrimaryKey = isPrimaryKey;
    }

    public string OwningTableAlias
    {
      get { return _owningTableAlias; }
    }

    public string ColumnName
    {
      get { return _columnName; }
    }

    public bool IsPrimaryKey
    {
      get { return _isPrimaryKey; }
    }

    public abstract SqlColumnExpression Update (Type type, string owningTableAlias, string columnName, bool isPrimaryKey);

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as IResolvedSqlExpressionVisitor;
      if(specificVisitor!=null)
        return specificVisitor.VisitSqlColumnExpression (this);
      else
        return base.Accept (visitor);
    }
  }
}