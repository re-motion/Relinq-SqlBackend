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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="SqlEntityExpression"/> holds a list of <see cref="SqlColumnExpression"/> instances.
  /// </summary>
  public class SqlEntityExpression : ExtensionExpression
  {
    private readonly string _tableAlias;
    private readonly SqlColumnExpression _primaryKeyColumn;
    private readonly ReadOnlyCollection<SqlColumnExpression> _projectionColumns;

    public SqlEntityExpression (Type itemType, string tableAlias, SqlColumnExpression primaryKeyColumn, params SqlColumnExpression[] projectionColumns)
        : base (ArgumentUtility.CheckNotNull ("itemType", itemType))
    {
      ArgumentUtility.CheckNotNull ("tableAlias", tableAlias);
      ArgumentUtility.CheckNotNull ("projectionColumns", projectionColumns);
      ArgumentUtility.CheckNotNull ("primaryKeyColumn", primaryKeyColumn);

      _tableAlias = tableAlias;
      _projectionColumns = Array.AsReadOnly (projectionColumns);
      _primaryKeyColumn = primaryKeyColumn;
    }

    public string TableAlias
    {
      get { return _tableAlias; }
    }

    public SqlColumnExpression PrimaryKeyColumn
    {
      get { return _primaryKeyColumn; }
    }

    public ReadOnlyCollection<SqlColumnExpression> ProjectionColumns
    {
      get { return _projectionColumns; }
    }

    public SqlColumnExpression GetColumn (Type type, string columnName, bool isPrimaryKeyColumn)
    {
      return new SqlColumnExpression(type, TableAlias, columnName, isPrimaryKeyColumn);
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newColumns = visitor.VisitAndConvert (ProjectionColumns, "VisitChildren");
      if (newColumns != ProjectionColumns)
        return new SqlEntityExpression (Type, TableAlias, PrimaryKeyColumn, newColumns.ToArray());
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as IResolvedSqlExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlEntityExpression (this);
      else
        return base.Accept (visitor);
    }

    public SqlEntityExpression Clone (SqlTableBase newSqlTable) // becomes CreateReference  (parameter newTableAlias instead of newSqltable)
    {
      var newAlias = newSqlTable.GetResolvedTableInfo().TableAlias;

      var primaryKeyColumn = CreateClonedColumn (PrimaryKeyColumn, newAlias);
      var projectionColumns = ProjectionColumns.Select ( columnExpression => CreateClonedColumn(columnExpression, newAlias)).ToArray();

      return new SqlEntityExpression (Type, newAlias, primaryKeyColumn, projectionColumns); // becomes SqlEntityReferenceExpression
    }

    private SqlColumnExpression CreateClonedColumn (SqlColumnExpression originalColumn, string newAlias)
    {
      return new SqlColumnExpression (originalColumn.Type, newAlias, originalColumn.ColumnName, originalColumn.IsPrimaryKey);
    }
  }
}