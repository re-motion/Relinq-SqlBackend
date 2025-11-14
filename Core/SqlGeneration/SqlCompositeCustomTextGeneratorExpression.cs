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
using System.Linq;
using System.Linq.Expressions;
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

    public override void Generate (ISqlCommandBuilder commandBuilder, ExpressionVisitor textGeneratingExpressionVisitor, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);
      ArgumentUtility.CheckNotNull (nameof(textGeneratingExpressionVisitor), textGeneratingExpressionVisitor);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      ExpressionVisitor.Visit (_expressions, textGeneratingExpressionVisitor.Visit);
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      var newExpressions = visitor.VisitAndConvert(_expressions, "VisitChildren");
      if (newExpressions != Expressions)
        return new SqlCompositeCustomTextGeneratorExpression (Type, newExpressions.ToArray());
      else
        return this;
    }

    public override string ToString ()
    {
      return string.Join (" ", _expressions.Select (expr => expr.ToString()).ToArray());
    }

  }
}