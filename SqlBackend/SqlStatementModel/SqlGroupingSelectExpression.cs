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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlGroupingSelectExpression"/> represents the data returned by a Group-By query.
  /// </summary>
  public class SqlGroupingSelectExpression : ExtensionExpression
  {
    public static SqlGroupingSelectExpression CreateWithNames (Expression unnamedKeySelector, Expression unnamedElementSelector)
    {
      ArgumentUtility.CheckNotNull ("unnamedKeySelector", unnamedKeySelector);
      ArgumentUtility.CheckNotNull ("unnamedElementSelector", unnamedElementSelector);

      return new SqlGroupingSelectExpression (
          new NamedExpression ("key", unnamedKeySelector),
          new NamedExpression ("element", unnamedElementSelector));
    }

    private readonly Expression _keyExpression;
    private readonly Expression _elementExpression;
    private readonly List<Expression> _aggregationExpressions;

    public SqlGroupingSelectExpression (Expression keyExpression, Expression elementExpression) 
        : this (keyExpression, elementExpression, new List<Expression>())
    {
    }

    public SqlGroupingSelectExpression (Expression keyExpression, Expression elementExpression, IEnumerable<Expression> aggregationExpressions)
     : base (
          typeof (IGrouping<,>).MakeGenericType (
              ArgumentUtility.CheckNotNull ("keyExpression", keyExpression).Type, 
              ArgumentUtility.CheckNotNull ("elementExpression", elementExpression).Type))
    {
      _keyExpression = keyExpression;
      _elementExpression = elementExpression;
      _aggregationExpressions = aggregationExpressions.ToList();
    }

    public Expression KeyExpression
    {
      get { return _keyExpression; }
    }

    public Expression ElementExpression
    {
      get { return _elementExpression; }
    }

    public ReadOnlyCollection<Expression> AggregationExpressions
    {
      get { return _aggregationExpressions.AsReadOnly(); }
    }

    public string AddAggregationExpressionWithName (Expression unnamedExpression)
    {
      ArgumentUtility.CheckNotNull ("unnamedExpression", unnamedExpression);

      var name = "a" + _aggregationExpressions.Count;
      _aggregationExpressions.Add (new NamedExpression (name, unnamedExpression));
      return name;
    }

    public SqlGroupingSelectExpression Update (Expression newKeyEpression, Expression newElementExpression, IEnumerable<Expression> aggregations)
    {
      return new SqlGroupingSelectExpression (newKeyEpression, newElementExpression, aggregations);
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newKeyExpression = visitor.VisitExpression (KeyExpression);
      var newElementExpression = visitor.VisitExpression (ElementExpression);

      var originalAggregationExpressions = AggregationExpressions;
      var newAggregationExpressions = visitor.VisitAndConvert (originalAggregationExpressions, "VisitChildren");

      if (newKeyExpression != KeyExpression 
          || newElementExpression != ElementExpression 
          || newAggregationExpressions != originalAggregationExpressions)
      {
        var newSqlGroupingSelectExpression = new SqlGroupingSelectExpression (newKeyExpression, newElementExpression, newAggregationExpressions);
        return newSqlGroupingSelectExpression;
      }
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlGroupingSelectExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlGroupingSelectExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return String.Format (
          "GROUPING (KEY: {0}, ELEMENT: {1}, AGGREGATIONS: ({2}))", 
          FormattingExpressionTreeVisitor.Format (KeyExpression), 
          FormattingExpressionTreeVisitor.Format (ElementExpression),
          string.Join (", ", AggregationExpressions.Select (FormattingExpressionTreeVisitor.Format)));
    }
  }
}