// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="SqlEntityConstantExpression"/> holds the primary key for a constant entity.
  /// </summary>
  public class SqlEntityConstantExpression : ExtensionExpression
  {
    private readonly object _value;
    private readonly Expression _primaryKeyExpression;

    public SqlEntityConstantExpression (Type type, object value, Expression primaryKeyExpression)
        : base(type)
    {
      ArgumentUtility.CheckNotNull ("value", value);
      ArgumentUtility.CheckNotNull ("primaryKeyExpression", primaryKeyExpression);

      _value = value;
      _primaryKeyExpression = primaryKeyExpression;
    }

    public object Value
    {
      get { return _value; }
    }

    public Expression PrimaryKeyExpression
    {
      get { return _primaryKeyExpression; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newPrimaryKeyExpression = visitor.VisitExpression (_primaryKeyExpression);
      if (newPrimaryKeyExpression != _primaryKeyExpression)
        return new SqlEntityConstantExpression (Type, _value, newPrimaryKeyExpression);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as IUnresolvedSqlExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlEntityConstantExpression(this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("ENTITY({0})", FormattingExpressionTreeVisitor.Format (_primaryKeyExpression));
    }
  }
}