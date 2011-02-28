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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// <see cref="SqlLengthExpression"></see> emits SQL that deals with spaces when calculating lengths.
  /// </summary>
  public class SqlLengthExpression : ExtensionExpression
  {
    private readonly Expression _expression;

    public SqlLengthExpression (Expression expression)
        : base(typeof(int))
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (string) && expression.Type != typeof (char))
      {
        throw new ArgumentException (
            "SqlLengthExpression can only be used on values of type 'System.String' or 'System.Char', not on '" + expression.Type
            + "'. (Add a conversion if you need to get the string length of a non-string value.)",
            "expression");
      }

      _expression = expression;
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newExpression = visitor.VisitExpression (_expression);

      if (newExpression != _expression)
        return new SqlLengthExpression (newExpression);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlLengthExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("LEN({0})", FormattingExpressionTreeVisitor.Format (_expression));
    }
  }
}