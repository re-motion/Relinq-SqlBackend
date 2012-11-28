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
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// Represents the conversion from a SQL predicate to a value. Since SQL does not allow the result of predicates to be passed around as values,
  /// the <see cref="SqlPredicateAsValueExpression"/> can be used to wrap a predicate, converting it into an integer value. (The SQL generated
  /// for this expression is a CASE WHEN expression, optionally with NULL support.)
  /// </summary>
  public class SqlPredicateAsValueExpression : ExtensionExpression
  {
    private static Type CheckPredicateAndGetValueType (Expression predicate)
    {
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      try
      {
        return BooleanUtility.GetMatchingIntType (predicate.Type);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentException ("The predicate must be an expression of type Boolean or Nullable<Boolean>.", "predicate", ex);
      }
    }

    private readonly Expression _predicate;

    public SqlPredicateAsValueExpression (Expression predicate)
        : base (CheckPredicateAndGetValueType(predicate))
    {
      _predicate = predicate;
    }

    public Expression Predicate
    {
      get { return _predicate; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newPredicate = visitor.VisitExpression (_predicate);
      if (newPredicate != _predicate)
        return new SqlPredicateAsValueExpression (newPredicate);
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlPredicateAsValueExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlPredicateAsValueExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("PredicateValue({0})", FormattingExpressionTreeVisitor.Format (_predicate));
    }
  }
}