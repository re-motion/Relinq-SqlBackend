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
  /// Defines a SQL column as an expression with a given column name. The column is represented as "[expr] AS [name]".
  /// The value can either be a constant or a computed value.
  /// </summary>
  public class SqlComputedColumnExpression : SqlColumnExpression
  {
    public static SqlComputedColumnExpression CreateConstant (object value, Type type, string owningTableAlias, string columnName, bool isPrimaryKey)
    {
      ArgumentUtility.CheckNotNull (nameof(type), type);
      ArgumentUtility.CheckNotNull (nameof(owningTableAlias), owningTableAlias);
      ArgumentUtility.CheckNotNull (nameof(columnName), columnName);

      return new SqlComputedColumnExpression (
          Constant(value),
          type,
          owningTableAlias,
          columnName,
          isPrimaryKey);
    }

    public Expression Value { get; }

    private SqlComputedColumnExpression (Expression value, Type type, string owningTableAlias, string columnName, bool isPrimaryKey)
        : base (type, owningTableAlias, columnName, isPrimaryKey)
    {
      ArgumentUtility.CheckNotNull (nameof(value), value);

      Value = value;
    }

    public override SqlColumnExpression Update (Type type, string owningTableAlias, string columnName, bool isPrimaryKey)
    {
      ArgumentUtility.CheckNotNull (nameof(type), type);
      ArgumentUtility.CheckNotNull (nameof(owningTableAlias), owningTableAlias);
      ArgumentUtility.CheckNotNull (nameof(columnName), columnName);

      return new SqlComputedColumnExpression (Value, type, owningTableAlias, columnName, isPrimaryKey);
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var specificVisitor = visitor as ISqlColumnExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlComputedColumn (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("{0} AS [{1}]", Value, ColumnName);
    }
  }
}