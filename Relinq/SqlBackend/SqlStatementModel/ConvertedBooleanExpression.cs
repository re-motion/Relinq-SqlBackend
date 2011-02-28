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

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Holds an <see cref="Expression"/> that originally had <see cref="bool"/> type but was converted to <see cref="int"/> because SQL doesn't know
  /// a boolean data type.
  /// </summary>
  public class ConvertedBooleanExpression : ExtensionExpression
  {
    private readonly Expression _expression;

    public ConvertedBooleanExpression (Expression expression)
        : base (typeof (bool))
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      if (expression.Type != typeof (int))
        throw new ArgumentException ("The inner expression must be an expression of type Int32.", "expression");

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
        return new ConvertedBooleanExpression (newExpression);

      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as IConvertedBooleanExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitConvertedBooleanExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("ConvertedBoolean({0})", FormattingExpressionTreeVisitor.Format (_expression));
    }
  }
}