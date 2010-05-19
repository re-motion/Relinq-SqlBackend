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
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="SqlColumnReferenceExpression"/> represents a column that references another column defined by a SQL substatement.
  /// </summary>
  public class SqlColumnReferenceExpression : SqlColumnExpression
  {
    private readonly SqlEntityExpression _referencedEntity;

    public SqlColumnReferenceExpression (Type type, string tableAlias, string referencedColumnName, bool isPrimaryKey, SqlEntityExpression referencedEntity)
      : base (type, tableAlias, referencedColumnName, isPrimaryKey)
    {
      ArgumentUtility.CheckNotNull ("referencedEntity", referencedEntity);

      _referencedEntity = referencedEntity;
    }

    public SqlEntityExpression ReferencedEntity
    {
      get { return _referencedEntity; }
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlColumnExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlColumnReferenceExpression (this);
      else
        return base.Accept (visitor);
    }

    public override SqlColumnExpression Update (Type type, string tableAlias, string columnName, bool isPrimaryKey)
    {
      return new SqlColumnReferenceExpression (type, tableAlias, columnName, isPrimaryKey, _referencedEntity);
    }
  }
}