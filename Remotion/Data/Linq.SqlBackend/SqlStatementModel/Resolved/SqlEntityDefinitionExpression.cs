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

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved
{
  public class SqlEntityDefinitionExpression : SqlEntityExpression
  {
    private readonly SqlColumnExpression _primaryKeyColumn;
    private readonly ReadOnlyCollection<SqlColumnExpression> _columns;

    public SqlEntityDefinitionExpression (Type itemType, string tableAlias, SqlColumnExpression primaryKeyColumn, params SqlColumnExpression[] projectionColumns)
        : base(itemType, tableAlias, null)
    {
      _columns = Array.AsReadOnly (projectionColumns);
      _primaryKeyColumn = primaryKeyColumn;
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newColumns = visitor.VisitAndConvert (Columns, "VisitChildren");
      if (newColumns != Columns)
        return new SqlEntityDefinitionExpression (Type, TableAlias, PrimaryKeyColumn, newColumns.ToArray ());
      else
        return this;
    }

    public override SqlColumnExpression PrimaryKeyColumn
    {
      get { return _primaryKeyColumn;  }
    }

    public override ReadOnlyCollection<SqlColumnExpression> Columns
    {
      get { return _columns; }
    }

    public override SqlColumnExpression GetColumn (Type type, string columnName, bool isPrimaryKeyColumn)
    {
      return new SqlColumnDefinitionExpression (type, TableAlias, columnName, isPrimaryKeyColumn);
    }

    public override SqlEntityExpression Update (Type itemType, string tableAlias)
    {
      return new SqlEntityDefinitionExpression (itemType, tableAlias, PrimaryKeyColumn, Columns.ToArray ());
    }

    public override SqlEntityExpression CreateReference (string newTableAlias)
    {
      //var primaryKeyColumn = CreateClonedColumn (PrimaryKeyColumn, newTableAlias); 
      //var projectionColumns = Columns.Select (columnExpression => CreateClonedColumn (columnExpression, newTableAlias)).ToArray ();

      //return new SqlEntityDefinitionExpression (Type, newTableAlias, primaryKeyColumn, projectionColumns); // becomes SqlEntityReferenceExpression

      return new SqlEntityReferenceExpression (Type, newTableAlias, this); //TODO 2779: integration test 'ExplicitJoinWithInto_DefaultIfEmptyOnGroupJoinVariable' failed!
    }

    private SqlColumnExpression CreateClonedColumn (SqlColumnExpression originalColumn, string newAlias)
    {
      return new SqlColumnDefinitionExpression (originalColumn.Type, newAlias, originalColumn.ColumnName, originalColumn.IsPrimaryKey);
    }
  }
}