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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="AggregationExpression"/> holds an aggregation modifier for a warapped expression.
  /// </summary>
  public class AggregationExpression : ExtensionExpression
  {
    private readonly Expression _expression;
    private readonly AggregationModifier _aggregationModifier;

    public AggregationExpression (Expression expression, AggregationModifier aggregationModifier)
        : base (expression.Type) // TODO Review 2760: expression type must be passed in - it is not always the type of the inner expression; for example, Count is always int, Average is decimal or double, etc. Use the data type of the DataInfo in the result operator handlers; don't forget to adapt the handler tests to check the expression type.
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _expression = expression;
      _aggregationModifier = aggregationModifier;
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    public AggregationModifier AggregationModifier
    {
      get { return _aggregationModifier; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newExpression = visitor.VisitExpression (_expression);
      if (newExpression != _expression)
        return new AggregationExpression(newExpression,  _aggregationModifier);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as IAggregationExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitAggregationExpression (this);
      else
        return base.Accept (visitor);
    }
  }
}