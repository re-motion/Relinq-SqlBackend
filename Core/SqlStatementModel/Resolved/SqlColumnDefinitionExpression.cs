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

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// Defines a SQL column as an item with a given name coming from a given table alias. The column is represented as [alias].[name].
  /// </summary>
  public class SqlColumnDefinitionExpression : SqlColumnExpression
  {
    public SqlColumnDefinitionExpression (Type type, string owningTableAlias, string columnName, bool isPrimaryKey)
        : base(type, owningTableAlias, columnName, isPrimaryKey)
    {
    }

    public override SqlColumnExpression Update (Type type, string owningTableAlias, string columnName, bool isPrimaryKey)
    {
      return new SqlColumnDefinitionExpression (type, owningTableAlias, columnName, isPrimaryKey);
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      var specificVisitor = visitor as ISqlColumnExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlColumnDefinition (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("[{0}].[{1}]", OwningTableAlias, ColumnName);
    }
  }
}