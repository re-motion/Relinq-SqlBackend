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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// Represents a SQL "a IN b" expression.
  /// </summary>
  public class SqlInExpression : ExtensionExpression
  {
    private readonly Expression _leftExpression;
    private readonly Expression _rightExpression;

    public SqlInExpression (Expression leftExpression, Expression rightExpression)
        : base(typeof(bool))
    {
      ArgumentUtility.CheckNotNull ("leftExpression", leftExpression);
      ArgumentUtility.CheckNotNull ("rightExpression", rightExpression);

      _leftExpression = leftExpression;
      _rightExpression = rightExpression;
    }

    public Expression LeftExpression
    {
      get { return _leftExpression; }
    }

    public Expression RightExpression
    {
      get { return _rightExpression; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newLeftExpression = visitor.VisitExpression (_leftExpression);
      var newRightExpression = visitor.VisitExpression (_rightExpression);

      // TODO Review 2493: Only create a new SqlInExpression if the newLeftExpression != _leftExpression or the _newRightExpression != _rightExpression.
      return new SqlInExpression (newLeftExpression, newRightExpression);
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlInExpression (this);
      else
        return base.Accept (visitor);
    }
  }
}