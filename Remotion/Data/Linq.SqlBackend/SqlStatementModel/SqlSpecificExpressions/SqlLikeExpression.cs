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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// Represents a sql 'LIKE' command
  /// </summary>
  public class SqlLikeExpression : ExtensionExpression
  {
    private readonly Expression _left;
    private readonly Expression _right;
    private readonly Expression _escapeExpression;

    public SqlLikeExpression (Expression left, Expression right, Expression escapeExpression)
        : base (typeof (bool))
    {
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);
      ArgumentUtility.CheckNotNull ("escapeExpression", escapeExpression);

      _left = left;
      _right = right;
      _escapeExpression = escapeExpression;
    }

    public Expression Left
    {
      get { return _left; }
    }

    public Expression Right
    {
      get { return _right; }
    }

    public Expression EscapeExpression
    {
      get { return _escapeExpression; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newLeftExpression = visitor.VisitExpression (_left);
      var newRightExpression = visitor.VisitExpression (_right);
      var newEscapeExpression = visitor.VisitExpression (_escapeExpression);

      if (newLeftExpression != _left || newRightExpression != _right || newEscapeExpression != _escapeExpression)
        return new SqlLikeExpression (newLeftExpression, newRightExpression, newEscapeExpression);
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlLikeExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format (
          "{0} LIKE {1} ESCAPE {2}",
          FormattingExpressionTreeVisitor.Format (_left),
          FormattingExpressionTreeVisitor.Format (_right),
          FormattingExpressionTreeVisitor.Format (_escapeExpression));
    }
  }
}