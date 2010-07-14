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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Provides functionality to simplify sub-statements that contain an <see cref="AggregateExpressionNode"/> aggregating over the
  /// elements of a grouping. The sub-statements must be resolved before they can be simplified.
  /// </summary>
  public class GroupAggregateSimplifier : ExpressionTreeVisitor, IUnresolvedSqlExpressionVisitor
  {
    public static bool IsSimplifiableGroupAggregate (SqlStatement resolvedSqlStatement)
    {
      ArgumentUtility.CheckNotNull ("resolvedSqlStatement", resolvedSqlStatement);

      return FindAggregationExpression (resolvedSqlStatement.SelectProjection) != null
             && resolvedSqlStatement.WhereCondition == null
             && resolvedSqlStatement.Orderings.Count == 0
             && resolvedSqlStatement.GroupByExpression == null
             && resolvedSqlStatement.SqlTables.Count == 1
             && resolvedSqlStatement.SqlTables[0].GetResolvedTableInfo() is ResolvedJoinedGroupingTableInfo 
             && resolvedSqlStatement.TopExpression == null
             && !resolvedSqlStatement.IsDistinctQuery;
    }

    private static AggregationExpression FindAggregationExpression (Expression expression)
    {
      while (expression is NamedExpression)
        expression = ((NamedExpression) expression).Expression;
      return expression as AggregationExpression;
    }

    public static Expression SimplifyIfPossible (SqlStatement resolvedSqlStatement, Expression unresolvedSelectProjection, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("resolvedSqlStatement", resolvedSqlStatement);
      ArgumentUtility.CheckNotNull ("unresolvedSelectProjection", unresolvedSelectProjection);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      if (IsSimplifiableGroupAggregate (resolvedSqlStatement))
      {
        var joinedGroupingTableInfo = (ResolvedJoinedGroupingTableInfo) resolvedSqlStatement.SqlTables[0].GetResolvedTableInfo();

        // Strip surrounding names so that there won't be a named expression inside the new aggregation
        var elementExpression = NamedExpression.StripSurroundingNames (joinedGroupingTableInfo.AssociatedGroupingSelectExpression.ElementExpression);
        var visitor = new GroupAggregateSimplifier (
            resolvedSqlStatement.SqlTables[0], 
            elementExpression);
        
        var aggregationExpression = FindAggregationExpression (unresolvedSelectProjection);
        if (aggregationExpression == null)
        {
          throw new ArgumentException (
              "The unresolved projection doesn't match the resolved statement: it has no aggregation.",
              "unresolvedSelectProjection");
        }
        var newAggregation = visitor.VisitExpression (aggregationExpression);

        if (visitor.CanBeTransferredToGroupingSource)
        {
          var resolvedNewAggregation = stage.ResolveSelectExpression (newAggregation, context);
          var aggregationName = joinedGroupingTableInfo.AssociatedGroupingSelectExpression.AddAggregationExpressionWithName (resolvedNewAggregation);

          return new SqlColumnDefinitionExpression (
              resolvedSqlStatement.SelectProjection.Type,
              joinedGroupingTableInfo.GroupSourceTableAlias,
              aggregationName,
              false);
        }
      }

      return new SqlSubStatementExpression (resolvedSqlStatement);
    }

    private readonly SqlTableBase _oldElementSource;
    private readonly Expression _newElementExpression;

    protected GroupAggregateSimplifier (SqlTableBase oldElementSource, Expression newElementExpression)
    {
      ArgumentUtility.CheckNotNull ("oldElementSource", oldElementSource);
      ArgumentUtility.CheckNotNull ("newElementExpression", newElementExpression);

      _oldElementSource = oldElementSource;
      _newElementExpression = newElementExpression;

      CanBeTransferredToGroupingSource = true;
    }

    public bool CanBeTransferredToGroupingSource { get; protected set; }
    
    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      if (expression.SqlTable == _oldElementSource)
      {
        return _newElementExpression;
      }
      else
      {
        CanBeTransferredToGroupingSource = false;
        return expression;
      }
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityConstantExpression (SqlEntityConstantExpression expression)
    {
      return VisitUnknownExpression (expression);
    }
  }
}