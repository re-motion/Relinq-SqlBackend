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
  /// Implementation of <see cref="SqlEntityExpression"/> for entity references, i.e., entities that stem from a substatement. Entity references
  /// know the <see cref="SqlEntityExpression"/> inside the substatement (the referenced entity), and their columns are of type
  /// <see cref="SqlColumnReferenceExpression"/>.
  /// </summary>
  public class SqlEntityReferenceExpression : SqlEntityExpression
  {
    private readonly ReadOnlyCollection<SqlColumnExpression> _columns;
    private readonly SqlEntityExpression _referencedEntity;

    public SqlEntityReferenceExpression (Type itemType, string tableAlias, string entityName, SqlEntityExpression referencedEntity)
        : base(itemType, tableAlias, entityName, referencedEntity.IdentityExpressionGenerator)
    {
      ArgumentUtility.CheckNotNull (nameof(referencedEntity), referencedEntity);

      _referencedEntity = referencedEntity;
      _columns = Array.AsReadOnly (referencedEntity.Columns.Select (col => GetColumn (col.Type, col.ColumnName, col.IsPrimaryKey)).ToArray ());
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      return this;
    }

    public override ReadOnlyCollection<SqlColumnExpression> Columns
    {
      get { return _columns;  }
    }

    public SqlEntityExpression ReferencedEntity
    {
      get { return _referencedEntity; }
    }

    // Returns a column from this entity. The column will be represented as: TableAlias.ReferencedEntityName_ColumnBaseName.
    // For example, for an entity referencing another entity "e0" from a substatement "q0", the column "ID" will be represented as: q0.e0_ID
    public override sealed SqlColumnExpression GetColumn (Type type, string columnName, bool isPrimaryKeyColumn)
    {
      return new SqlColumnReferenceExpression (type, TableAlias, columnName, isPrimaryKeyColumn, _referencedEntity);
    }

    public override SqlEntityExpression Update (Type itemType, string tableAlias, string entityName)
    {
      return new SqlEntityReferenceExpression (itemType, tableAlias, entityName, _referencedEntity);
    }

    public override SqlEntityExpression CreateReference (string newTableAlias, Type newType)
    {
      return new SqlEntityReferenceExpression (newType, newTableAlias, null, this);
    }

    public override string ToString ()
    {
      return string.Format ("[{0}]{1} (ENTITY-REF)", TableAlias, _referencedEntity.Name != null ? string.Format (".[{0}]", _referencedEntity.Name) : "");
    }
  }
}