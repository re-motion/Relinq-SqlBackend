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

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// <see cref="SqlLengthExpression"></see> emits SQL that deals with spaces when calculating lengths.
  /// </summary>
  public class SqlLengthExpression : Expression
  {
    private readonly Expression _expression;

    public SqlLengthExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (expression.Type != typeof (string) && expression.Type != typeof (char))
      {
        throw new ArgumentException (
            "SqlLengthExpression can only be used on values of type 'System.String' or 'System.Char', not on '" + expression.Type
            + "'. (Add a conversion if you need to get the string length of a non-string value.)",
            nameof(expression));
      }

      _expression = expression;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return typeof(int); }
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var newExpression = visitor.Visit (_expression);

      if (newExpression != _expression)
        return new SqlLengthExpression (newExpression);
      else
        return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlLength (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("LEN({0})", _expression);
    }
  }
}