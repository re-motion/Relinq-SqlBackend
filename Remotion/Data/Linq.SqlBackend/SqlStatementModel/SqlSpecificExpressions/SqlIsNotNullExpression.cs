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

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// Represents 'is not null' in a comparison.
  /// </summary>
  public class SqlIsNotNullExpression : ExtensionExpression
  {
    private readonly Expression _nullExpression;
    private readonly Expression _expression;

    // TODO Review 2528: IS NOT NULL only needs one operand
    public SqlIsNotNullExpression (Expression nullExpression, Expression expression)
        : base(typeof(bool))
    {
      ArgumentUtility.CheckNotNull ("nullExpression", nullExpression);
      ArgumentUtility.CheckNotNull ("expression", expression);

      _nullExpression = nullExpression;
      _expression = expression;      
    }

    public Expression NullExpression
    {
      get { return _nullExpression; }
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newExpression = visitor.VisitExpression (_expression);

      if (newExpression != _expression)
        return new SqlIsNotNullExpression (_nullExpression, newExpression);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlIsNotNullExpression (this);
      else
        return base.Accept (visitor);
    }
  }
}