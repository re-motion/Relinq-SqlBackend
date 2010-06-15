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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlGroupingSelectExpression"/> represents a group-by expression.
  /// </summary>
  public class SqlGroupingSelectExpression : ExtensionExpression
  {
    private readonly Expression _keyExpression;
    private readonly Expression _elementExpression;
    private readonly List<Expression> _aggregationExpressions;

    public SqlGroupingSelectExpression (Expression keyExpression, Expression elementExpression)
      : base (typeof (IGrouping<,>).MakeGenericType (keyExpression.Type, elementExpression.Type))
    {
      ArgumentUtility.CheckNotNull ("keyExpression", keyExpression);
      ArgumentUtility.CheckNotNull ("elementExpression", elementExpression);

      _keyExpression = keyExpression;
      _elementExpression = elementExpression;
      _aggregationExpressions = new List<Expression>();
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

    public void AddAggregationExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _aggregationExpressions.Add (expression);
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var originalAggregationExpressions = AggregationExpressions;
      var newKeyExpression = visitor.VisitExpression (KeyExpression);
      var newElementExpression = visitor.VisitExpression (ElementExpression);
      var newAggregationExpressions = visitor.VisitAndConvert (originalAggregationExpressions, "VisitChildren");
      if (newKeyExpression != KeyExpression || newElementExpression != ElementExpression || newAggregationExpressions != originalAggregationExpressions)
      {
        var newSqlGroupingSelectExpression = new SqlGroupingSelectExpression (newKeyExpression, newElementExpression);
        foreach (var newAggregationExpression in newAggregationExpressions)
          newSqlGroupingSelectExpression.AddAggregationExpression (newAggregationExpression);
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
      return string.Format (
          "GROUP BY {0}.{1}", FormattingExpressionTreeVisitor.Format (KeyExpression), FormattingExpressionTreeVisitor.Format (ElementExpression));
    }
  }
}