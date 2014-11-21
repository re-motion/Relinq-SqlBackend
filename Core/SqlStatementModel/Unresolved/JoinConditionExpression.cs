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

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  // TODO RMLNQSQL-64: Remove.
  /// <summary>
  /// <see cref="JoinConditionExpression"/> represents the data source defined by a member access in the from part of a linq expression.
  /// </summary>
  public class JoinConditionExpression : Expression
  {
    private readonly SqlJoinedTable _joinedTable;

    public JoinConditionExpression (SqlJoinedTable joinedTable)
    {
      ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);

      _joinedTable = joinedTable;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return typeof(bool); }
    }

    public SqlJoinedTable JoinedTable
    {
      get { return _joinedTable; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as IJoinConditionExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitJoinCondition (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return "CONDITION(" + JoinedTable + ")";
    }
  }
}