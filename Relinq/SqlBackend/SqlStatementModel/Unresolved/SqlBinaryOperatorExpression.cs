// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// Represents a SQL "a OPERATOR b" expression.
  /// </summary>
  public class SqlBinaryOperatorExpression : ExtensionExpression
  {
    private readonly string _binaryOperator;
    private readonly Expression _leftExpression;
    private readonly Expression _rightExpression;

    public SqlBinaryOperatorExpression (Type type, string binaryOperator, Expression leftExpression, Expression rightExpression)
        : base(ArgumentUtility.CheckNotNull ("type", type))
    {
      ArgumentUtility.CheckNotNull ("binaryOperator", binaryOperator);
      ArgumentUtility.CheckNotNull ("leftExpression", leftExpression);
      ArgumentUtility.CheckNotNull ("rightExpression", rightExpression);

      _binaryOperator = binaryOperator;
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

    public string BinaryOperator
    {
      get { return _binaryOperator; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newLeftExpression = visitor.VisitExpression (_leftExpression);
      var newRightExpression = visitor.VisitExpression (_rightExpression);

      if(newLeftExpression!=_leftExpression || newRightExpression!=_rightExpression)
        return new SqlBinaryOperatorExpression (typeof(bool), _binaryOperator, newLeftExpression, newRightExpression);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlBinaryOperatorExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format (
          "{0} {1} {2}",
          FormattingExpressionTreeVisitor.Format (_leftExpression),
          _binaryOperator,
          FormattingExpressionTreeVisitor.Format (_rightExpression));
    }
  }
}