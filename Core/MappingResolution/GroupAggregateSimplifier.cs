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
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Provides functionality to simplify sub-statements that contain an <see cref="AggregateExpressionNode"/> aggregating over the
  /// elements of a grouping. The sub-statements must be resolved before they can be simplified.
  /// </summary>
  /// <remarks>
  /// For example, consider the following query (pseudo-code, mixing LINQ and SQL):
  /// <code>
  ///   from g in { SELECT ... FROM [OrderTable] [t0] GROUP BY [t0].[OrderDate] }
  ///   select new { g.Max(), g.Sum() }
  /// </code>
  /// In this code, g.Max() and g.Sum() are SubStatementExpressions because Max() and Sum() are parsed as result operators and thus as sub-statements.
  /// We want to simplify that query as follows:
  /// <code>
  ///   from g in { SELECT ..., MAX(...) AS m, SUM(...) AS s FROM [OrderTable] [t0] GROUP BY [t0].[OrderDate] }
  ///   select new { g.m, g.s }
  /// </code>
  /// </remarks>
  public class GroupAggregateSimplifier : IGroupAggregateSimplifier
  {
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    public GroupAggregateSimplifier (IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      _stage = stage;
      _context = context;
    }

    public bool IsSimplifiableGroupAggregate (SqlStatement resolvedSqlStatement)
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

    public Expression SimplifyIfPossible (
        SqlSubStatementExpression subStatementExpression,
        Expression unresolvedSelectProjection)
    {
      ArgumentUtility.CheckNotNull ("subStatementExpression", subStatementExpression);
      ArgumentUtility.CheckNotNull ("unresolvedSelectProjection", unresolvedSelectProjection);

      var resolvedSqlStatement = subStatementExpression.SqlStatement;
      if (IsSimplifiableGroupAggregate (resolvedSqlStatement))
      {
        var joinedGroupingTableInfo = (ResolvedJoinedGroupingTableInfo) resolvedSqlStatement.SqlTables[0].GetResolvedTableInfo ();

        // Strip surrounding names so that there won't be a named expression inside the new aggregation
        var elementExpression = _context.RemoveNamesAndUpdateMapping (joinedGroupingTableInfo.AssociatedGroupingSelectExpression.ElementExpression);
        var visitor = new SimplifyingVisitor (resolvedSqlStatement.SqlTables[0], elementExpression);

        var aggregationExpression = FindAggregationExpression (unresolvedSelectProjection);
        if (aggregationExpression == null)
        {
          throw new ArgumentException (
              "The unresolved projection doesn't match the resolved statement: it has no aggregation.",
              "unresolvedSelectProjection");
        }
        var newAggregation = visitor.Visit (aggregationExpression);

        if (visitor.CanBeTransferredToGroupingSource)
        {
          var resolvedNewAggregation = _stage.ResolveAggregationExpression (newAggregation, _context);

          var aggregationName = joinedGroupingTableInfo.AssociatedGroupingSelectExpression.AddAggregationExpressionWithName (resolvedNewAggregation);

          return new SqlColumnDefinitionExpression (
              resolvedSqlStatement.SelectProjection.Type,
              joinedGroupingTableInfo.GroupSourceTableAlias,
              aggregationName,
              false);
        }
      }

      return subStatementExpression;
    }

    public class SimplifyingVisitor : RelinqExpressionVisitor, IUnresolvedSqlExpressionVisitor
    {
      private readonly SqlTable _oldElementSource;
      private readonly Expression _newElementExpression;

      public SimplifyingVisitor (SqlTable oldElementSource, Expression newElementExpression)
      {
        ArgumentUtility.CheckNotNull ("oldElementSource", oldElementSource);
        ArgumentUtility.CheckNotNull ("newElementExpression", newElementExpression);

        _oldElementSource = oldElementSource;
        _newElementExpression = newElementExpression;

        CanBeTransferredToGroupingSource = true;
      }

      public bool CanBeTransferredToGroupingSource { get; protected set; }

      public Expression VisitSqlTableReference (SqlTableReferenceExpression expression)
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

      Expression ISqlEntityRefMemberExpressionVisitor.VisitSqlEntityRefMember (SqlEntityRefMemberExpression expression)
      {
        return VisitExtension (expression);
      }
    }
  }
}