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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// Implementation of <see cref="SqlEntityExpression"/> for entity definitions, i.e., entities that are directly defined by a table.
  /// </summary>
  public class SqlEntityDefinitionExpression : SqlEntityExpression
  {
    private readonly ReadOnlyCollection<SqlColumnExpression> _columns;

    public SqlEntityDefinitionExpression (
        Type entityType, 
        string tableAlias, 
        string entityName, 
        Func<SqlEntityExpression, Expression> identityExpressionGenerator, 
        params SqlColumnExpression[] projectionColumns)
      : base (entityType, tableAlias, entityName, identityExpressionGenerator)
    {
      ArgumentUtility.CheckNotNull (nameof(projectionColumns), projectionColumns);

      _columns = Array.AsReadOnly (projectionColumns);
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      var newColumns = visitor.VisitAndConvert (Columns, "VisitChildren");
      if (newColumns != Columns)
        return new SqlEntityDefinitionExpression (Type, TableAlias, null, IdentityExpressionGenerator, newColumns.ToArray ());
      else
        return this;
    }

    public override ReadOnlyCollection<SqlColumnExpression> Columns
    {
      get { return _columns; }
    }

    public override SqlColumnExpression GetColumn (Type type, string columnName, bool isPrimaryKeyColumn)
    {
      return new SqlColumnDefinitionExpression (type, TableAlias, columnName, isPrimaryKeyColumn);
    }

    public override SqlEntityExpression Update (Type itemType, string tableAlias, string entityName)
    {
      return new SqlEntityDefinitionExpression (itemType, tableAlias, entityName, IdentityExpressionGenerator, Columns.ToArray ());
    }

    public override SqlEntityExpression CreateReference (string newTableAlias, Type newType)
    {
      return new SqlEntityReferenceExpression (newType, newTableAlias, null, this);
    }

    public override string ToString ()
    {
      var entityName = Name != null ? string.Format (" AS [{0}]", Name) : string.Empty;
      return string.Format ("[{0}]{1}", TableAlias, entityName);
    }
  }
}