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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved
{
  public class SqlEntityReferenceExpression : SqlEntityExpression
  {
    private readonly SqlColumnExpression _primaryKeyColumn;
    private readonly ReadOnlyCollection<SqlColumnExpression> _columns;
    private readonly SqlEntityExpression _referencedEntity;

    public SqlEntityReferenceExpression (Type itemType, string tableAlias, SqlEntityExpression referencedEntity)
        : base(itemType, tableAlias)
    {
      ArgumentUtility.CheckNotNull ("referencedEntity", referencedEntity);

      _referencedEntity = referencedEntity;
      _columns = Array.AsReadOnly(referencedEntity.Columns.Select (col => GetColumn (col.Type, col.ColumnName, col.IsPrimaryKey)).ToArray ());
      _primaryKeyColumn = GetColumn (referencedEntity.PrimaryKeyColumn.Type, referencedEntity.PrimaryKeyColumn.ColumnName, true);
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override SqlColumnExpression PrimaryKeyColumn
    {
      get { return _primaryKeyColumn; }
    }

    public override ReadOnlyCollection<SqlColumnExpression> Columns
    {
      get { return _columns;  }
    }

    // Returns a column from this entity. The column will be represented as: TableAlias.ReferencedEntityName_ColumnBaseName.
    // For example, for an entity referencing another entity "e0" from a substatement "q0", the column "ID" will be represented as: q0.e0_ID
    public override SqlColumnExpression GetColumn (Type type, string columnName, bool isPrimaryKeyColumn)
    {
      return new SqlColumnReferenceExpression (type, TableAlias, columnName, isPrimaryKeyColumn, _referencedEntity);
    }

    public override SqlEntityExpression CreateReference (string newTableAlias)
    {
      return new SqlEntityReferenceExpression(Type, newTableAlias, _referencedEntity);
    }
  }
}