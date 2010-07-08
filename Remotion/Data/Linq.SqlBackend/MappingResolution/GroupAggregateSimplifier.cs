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
  /// Provides utility methods to detect simplifiable sub-statements that contain an <see cref="AggregateExpressionNode"/> aggregating over the
  /// elements of a grouping. The sub-statements must be checked after they are resolved.
  /// </summary>
  public class GroupAggregateSimplifier : ExpressionTreeVisitor, IResolvedSqlExpressionVisitor
  {
    public static bool IsSimplifiableGroupAggregate (SqlStatement resolvedSqlStatement)
    {
      ArgumentUtility.CheckNotNull ("resolvedSqlStatement", resolvedSqlStatement);

      return resolvedSqlStatement.SelectProjection is AggregationExpression
             && resolvedSqlStatement.WhereCondition == null
             && resolvedSqlStatement.Orderings.Count == 0
             && resolvedSqlStatement.GroupByExpression == null
             && resolvedSqlStatement.SqlTables.Count == 1
             && resolvedSqlStatement.SqlTables[0].GetResolvedTableInfo() is ResolvedJoinedGroupingTableInfo 
             && resolvedSqlStatement.TopExpression == null
             && !resolvedSqlStatement.IsDistinctQuery;
    }

    public static Expression SimplifyIfPossible (SqlStatement resolvedSqlStatement, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("resolvedSqlStatement", resolvedSqlStatement);

      if (IsSimplifiableGroupAggregate (resolvedSqlStatement))
      {
        var joinedGroupingTableInfo = (ResolvedJoinedGroupingTableInfo) resolvedSqlStatement.SqlTables[0].GetResolvedTableInfo();
        var elementReference = stage.ResolveTableReferenceExpression (new SqlTableReferenceExpression (resolvedSqlStatement.SqlTables[0]), context);

        var visitor = new GroupAggregateSimplifier (elementReference, joinedGroupingTableInfo.AssociatedGroupingSelectExpression.ElementExpression);
        var newAggregation = visitor.VisitExpression (resolvedSqlStatement.SelectProjection);

        if (visitor.CanBeTransferredToGroupingSource)
        {
          var aggregationName = joinedGroupingTableInfo.AssociatedGroupingSelectExpression.AddAggregationExpressionWithName (newAggregation);

          return new SqlColumnDefinitionExpression (
              resolvedSqlStatement.SelectProjection.Type,
              joinedGroupingTableInfo.GroupSourceTableAlias,
              aggregationName,
              false);
        }
      }

      return new SqlSubStatementExpression (resolvedSqlStatement);
    }

    private readonly Expression _elementReference;
    private readonly Expression _elementExpressionToBeUsed;

    protected GroupAggregateSimplifier (Expression elementReference, Expression elementExpressionToBeUsed)
    {
      _elementReference = elementReference;
      _elementExpressionToBeUsed = elementExpressionToBeUsed;

      CanBeTransferredToGroupingSource = true;
    }

    public bool CanBeTransferredToGroupingSource { get; protected set; }
    
    public Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      var elementReferenceAsSqlEntityExpression = _elementReference as SqlEntityExpression;
      if (elementReferenceAsSqlEntityExpression != null
          && elementReferenceAsSqlEntityExpression.TableAlias == expression.TableAlias)
        return _elementExpressionToBeUsed;

      CanBeTransferredToGroupingSource = false;
      return expression;
    }

    public Expression VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      var elementReferenceAsSqlColumnExpression = _elementReference as SqlColumnExpression;
      if (elementReferenceAsSqlColumnExpression != null
          && elementReferenceAsSqlColumnExpression.OwningTableAlias == expression.OwningTableAlias)
        return _elementExpressionToBeUsed;

      CanBeTransferredToGroupingSource = false;
      return expression;
    }
  }
}