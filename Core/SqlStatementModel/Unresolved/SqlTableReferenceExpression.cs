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
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// <see cref="SqlTableReferenceExpression"/> represents a data row in a <see cref="SqlTable"/>.
  /// </summary>
  public class SqlTableReferenceExpression : Expression
  {
    private readonly SqlTableBase _sqlTableBase;

    public SqlTableReferenceExpression (SqlTableBase sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      _sqlTableBase = sqlTable;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return _sqlTableBase.ItemType; }
    }

    public SqlTableBase SqlTable
    {
      get { return _sqlTableBase; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as ISqlTableReferenceExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlTableReference (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      var sqlTableBaseAsSqlTable = _sqlTableBase as SqlTable;
      if (sqlTableBaseAsSqlTable!=null)
      {
        var resolvedTableInfo = sqlTableBaseAsSqlTable.TableInfo as IResolvedTableInfo;
        if (resolvedTableInfo != null)
          return "TABLE-REF(" + resolvedTableInfo.TableAlias + ")";
        else
          return "TABLE-REF(" + sqlTableBaseAsSqlTable.TableInfo.GetType ().Name + "(" + sqlTableBaseAsSqlTable.TableInfo.ItemType.Name + "))";
      }

      var sqlTableBaseAsSqlJoinedTable = _sqlTableBase as SqlJoinedTable;
      if (sqlTableBaseAsSqlJoinedTable != null)
      {
        if (sqlTableBaseAsSqlJoinedTable.JoinInfo is ResolvedJoinInfo)
          return "TABLE-REF(" + sqlTableBaseAsSqlJoinedTable.JoinInfo.GetResolvedJoinInfo ().ForeignTableInfo.TableAlias + ")"; //TODO RMLNQSQL-1: Drop TableAlias
        else
          return "TABLE-REF(" + sqlTableBaseAsSqlJoinedTable.JoinInfo.GetType().Name + "(" + sqlTableBaseAsSqlJoinedTable.JoinInfo.ItemType.Name + "))";
      }

      return "TABLE-REF (" + _sqlTableBase.GetType().Name + " (" + _sqlTableBase.ItemType.Name + "))";
    }
  }
}