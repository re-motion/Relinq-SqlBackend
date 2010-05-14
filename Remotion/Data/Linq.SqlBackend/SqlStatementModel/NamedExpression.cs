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
  /// <see cref="NamedExpression"/> holds an expression and a name for it. If the name is null, a default name is used (or omitted if possible).
  /// When a <see cref="NamedExpression"/> holds an expression resolved to a 
  /// <see cref="Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved.SqlEntityExpression"/>, the entity's name is set to the 
  /// <see cref="NamedExpression"/>'s name. Otherwise, the <see cref="NamedExpression"/> is retained and used to emit "AS ..." clauses in SQL 
  /// generation. Therefore, <see cref="NamedExpression"/> must only be used in parts of a <see cref="SqlStatement"/> where "AS ..." clauses are 
  /// allowed.
  /// </summary>
  public class NamedExpression : ExtensionExpression
  {
    private readonly string _name;
    private readonly Expression _expression;

    public NamedExpression (string name, Expression expression)
        : base(expression.Type)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _name = name;
      _expression = expression;
    }

    public string Name
    {
      get { return _name; }
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
        return new NamedExpression(_name, newExpression);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as INamedExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitNamedExpression (this);
      else
        return base.Accept (visitor);
    }
  }

  
}