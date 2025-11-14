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

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="AggregationExpression"/> holds an aggregation modifier for a warapped expression.
  /// </summary>
  public class AggregationExpression : Expression
  {
    private readonly Type _type;
    private readonly Expression _expression;
    private readonly AggregationModifier _aggregationModifier;

    public AggregationExpression (Type type, Expression expression, AggregationModifier aggregationModifier)
    {
      ArgumentUtility.CheckNotNull (nameof(type), type);
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _type = type;
      _expression = expression;
      _aggregationModifier = aggregationModifier;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return _type; }
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    public AggregationModifier AggregationModifier
    {
      get { return _aggregationModifier; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var newExpression = visitor.Visit (_expression);
      if (newExpression != _expression)
        return new AggregationExpression(Type, newExpression,  _aggregationModifier);
      else
        return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var specificVisitor = visitor as IAggregationExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitAggregation (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("{0}({1})", _aggregationModifier, _expression);
    }
  }
}