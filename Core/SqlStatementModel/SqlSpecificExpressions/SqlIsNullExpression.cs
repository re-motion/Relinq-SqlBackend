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
  /// Represents 'is null' in a comparison.
  /// </summary>
  public class SqlIsNullExpression : Expression
  {
    private readonly Expression _expression;

    public SqlIsNullExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      _expression = expression;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return typeof(bool); }
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      var newExpression = visitor.Visit (_expression);
     
      if (newExpression != _expression)
        return new SqlIsNullExpression (newExpression);
      else
        return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      var specificVisitor = visitor as ISqlNullCheckExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlIsNull (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("{0} IS NULL", _expression);
    }
  }
}