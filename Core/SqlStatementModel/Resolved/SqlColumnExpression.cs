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
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="SqlColumnExpression"/> represents a sql-specific column expression.
  /// </summary>
  public abstract class SqlColumnExpression : Expression
  {
    private readonly Type _type;
    private readonly string _owningTableAlias;
    private readonly string _columnName;
    private readonly bool _isPrimaryKey;

    protected SqlColumnExpression (Type type, string owningTableAlias, string columnName, bool isPrimaryKey)
    {
      ArgumentUtility.CheckNotNull (nameof(type), type);
      ArgumentUtility.CheckNotNull (nameof(owningTableAlias), owningTableAlias);
      ArgumentUtility.CheckNotNullOrEmpty (nameof(columnName), columnName);
      ArgumentUtility.CheckNotNull (nameof(isPrimaryKey), isPrimaryKey);

      _type = type;
      _owningTableAlias = owningTableAlias;
      _columnName = columnName;
      _isPrimaryKey = isPrimaryKey;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return _type; }
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

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      var specificVisitor = visitor as IResolvedSqlExpressionVisitor;
      if(specificVisitor!=null)
        return specificVisitor.VisitSqlColumn (this);
      else
        return base.Accept (visitor);
    }
  }
}