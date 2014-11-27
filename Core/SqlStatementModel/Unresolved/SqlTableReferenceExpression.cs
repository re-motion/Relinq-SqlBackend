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
    private readonly SqlTable _sqlTable;

    public SqlTableReferenceExpression (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      _sqlTable = sqlTable;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return _sqlTable.ItemType; }
    }

    public SqlTable SqlTable
    {
      get { return _sqlTable; }
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
      var resolvedTableInfo = _sqlTable.TableInfo as IResolvedTableInfo;
      if (resolvedTableInfo != null)
        return "TABLE-REF(" + resolvedTableInfo.TableAlias + ")";
      else
        return "TABLE-REF(" + _sqlTable.TableInfo.GetType ().Name + "(" + _sqlTable.TableInfo.ItemType.Name + "))";
    }
  }
}