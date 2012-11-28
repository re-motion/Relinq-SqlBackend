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
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// Holds an <see cref="Expression"/> that originally had <see cref="bool"/> type, but was converted to <see cref="int"/> because SQL doesn't know
  /// a boolean data type.
  /// </summary>
  // TODO 3335: Maybe use simple ConvertExpression instead?
  public class SqlConvertedBooleanExpression : ExtensionExpression
  {
    private static Type GetMatchingBoolType (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      try
      {
        return BooleanUtility.GetMatchingBoolType (ArgumentUtility.CheckNotNull ("expression", expression).Type);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentException ("The inner expression must be an expression of type Int32 or Nullable<Int32>.", "expression", ex);
      }
    }

    private readonly Expression _expression;

    public SqlConvertedBooleanExpression (Expression expression)
      : base (GetMatchingBoolType (expression))
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _expression = expression;
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newExpression = visitor.VisitExpression (_expression);
      if (newExpression != _expression)
        return new SqlConvertedBooleanExpression (newExpression);

      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlConvertedBooleanExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlConvertedBooleanExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("ConvertedBoolean({0})", FormattingExpressionTreeVisitor.Format (_expression));
    }
  }
}