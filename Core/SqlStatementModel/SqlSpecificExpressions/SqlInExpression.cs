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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// Represents a SQL "a IN b" expression.
  /// </summary>
  public class SqlInExpression : ExtensionExpression
  {
    private readonly Expression _leftExpression;
    private readonly Expression _rightExpression;

    public SqlInExpression (Expression leftExpression, Expression rightExpression)
      : base (typeof (bool))
    {
      ArgumentUtility.CheckNotNull ("leftExpression", leftExpression);
      ArgumentUtility.CheckNotNull ("rightExpression", rightExpression);

      _leftExpression = leftExpression;
      _rightExpression = rightExpression;
    }

    public Expression LeftExpression
    {
      get { return _leftExpression; }
    }

    public Expression RightExpression
    {
      get { return _rightExpression; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      var newLeftExpression = visitor.Visit (_leftExpression);
      var newRightExpression = visitor.Visit (_rightExpression);

      if(newLeftExpression!=_leftExpression || newRightExpression!=_rightExpression)
        return new SqlInExpression (newLeftExpression, newRightExpression);
      else
        return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      var specificVisitor = visitor as ISqlInExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlInExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format (
          "{0} IN {1}",
          FormattingExpressionTreeVisitor.Format (_leftExpression),
          FormattingExpressionTreeVisitor.Format (_rightExpression));
    }
  }
}