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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Linq;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlCompositeCustomTextGeneratorExpression"/> holds a list of expressions and generates sql text for each expression.
  /// </summary>
  public class SqlCompositeCustomTextGeneratorExpression : SqlCustomTextGeneratorExpressionBase
  {
    private readonly ReadOnlyCollection<Expression> _expressions;

    public SqlCompositeCustomTextGeneratorExpression (Type expressionType, params Expression[] expressions)
        : base (expressionType)
    {
      _expressions = Array.AsReadOnly (expressions);
    }

    public ReadOnlyCollection<Expression> Expressions
    {
      get { return _expressions; }
    }

    public override void Generate (ISqlCommandBuilder commandBuilder, ExpressionTreeVisitor textGeneratingExpressionVisitor, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("textGeneratingExpressionVisitor", textGeneratingExpressionVisitor);
      ArgumentUtility.CheckNotNull ("stage", stage);

      textGeneratingExpressionVisitor.VisitList (_expressions, textGeneratingExpressionVisitor.VisitExpression);
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newExpressions = visitor.VisitAndConvert(_expressions, "VisitChildren");
      if (newExpressions != Expressions)
        return new SqlCompositeCustomTextGeneratorExpression (Type, newExpressions.ToArray());
      else
        return this;
    }

    public override string ToString ()
    {
      return String.Join (" ", _expressions.Select (expr => FormattingExpressionTreeVisitor.Format (expr)).ToArray());
    }

  }
}