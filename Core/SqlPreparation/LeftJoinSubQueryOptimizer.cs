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

using System.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  // TODO RMLNQSQL-77: Interface IQuerySourceOptimizer?
  public class LeftJoinSubQueryOptimizer
  {
    private readonly ISqlPreparationContext _context;

    public LeftJoinSubQueryOptimizer (ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("context", context);

      _context = context;
    }

    // TODO RMLNQSQL-77: strategy or builder pattern?
    public SqlTable OptimizeQuerySource (IQuerySource source, FromExpressionInfo fromExpressionInfo, SqlStatementBuilder sqlStatementBuilder)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("fromExpressionInfo", fromExpressionInfo);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);

      // LEFT JOIN subquery optimization - convert subquery resulting from "DefaultIfEmpty" into a LEFT JOIN if possible
      // 
      // This optimizes _exactly_ the following situation:
      // ... CROSS APPLY (
      //       SELECT actualProjection
      //       FROM (SELECT NULL AS Empty) AS [Empty]
      //         LEFT OUTER JOIN joinedTable
      //         ON joinCondition
      //    ) [q1]
      //
      // and generates:
      // ... LEFT OUTER JOIN joinedTable ON joinCondition
      // (with references to [q1] being replaced by "actualProjection")

      var optimizedTable = GetOptimizedTableOrNull(source, fromExpressionInfo, sqlStatementBuilder);
      if (optimizedTable != null)
        return optimizedTable;

      AddPreparedFromExpression (fromExpressionInfo, sqlStatementBuilder);  
      
      // TODO RMLNQSQL-77: keep expression mapping inside this type or move out to callsite?
      _context.AddExpressionMapping (new QuerySourceReferenceExpression (source), fromExpressionInfo.ItemSelector);
      return fromExpressionInfo.AppendedTable.SqlTable;
    }

      // TODO RMLNQSQL-77: test, remove duplication with SqlPrepQueryModelVisitor, refactor, find a better design for passing sqlStatementBuilder
    public void AddPreparedFromExpression (FromExpressionInfo fromExpressionInfo, SqlStatementBuilder sqlStatementBuilder)
    {
      ArgumentUtility.CheckNotNull ("fromExpressionInfo", fromExpressionInfo);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);

      if (fromExpressionInfo.WhereCondition != null)
        sqlStatementBuilder.AddWhereCondition (fromExpressionInfo.WhereCondition);

      foreach (var ordering in fromExpressionInfo.ExtractedOrderings)
        sqlStatementBuilder.Orderings.Add (ordering);

      sqlStatementBuilder.SqlTables.Add (fromExpressionInfo.AppendedTable);
    }

    private SqlTable GetOptimizedTableOrNull (IQuerySource source, FromExpressionInfo fromExpressionInfo, SqlStatementBuilder sqlStatementBuilder)
    {
      // only possible if there is a table that can be the left side of the join
      var parentTableForLeftJoin = sqlStatementBuilder.SqlTables.LastOrDefault();
      if (parentTableForLeftJoin == null)
        return null;

      // don't want to deal with extracted orderings in a LEFT JOIN scenario
      // TODO RMLNQSQL-77: No integration test yet.
      if (fromExpressionInfo.ExtractedOrderings.Any())
        return null;

      // don't want to deal with extracted WHERE in a LEFT JOIN scenario
      // TODO RMLNQSQL-77: No integration test possible.
      if (fromExpressionInfo.WhereCondition != null)
        return null;

      // we only want to optimize LEFT JOIN (DefaultIfEmpty) embedded inside a simple CROSS APPLY (from clause)
      // TODO RMLNQSQL-77: No integration test yet.
      if (fromExpressionInfo.AppendedTable.JoinSemantics != JoinSemantics.Inner)
        return null;

      // don't want to deal with additional joins coming outside the DefaultIfEmpty  
      // TODO RMLNQSQL-77: Integration test possible?
      if (fromExpressionInfo.AppendedTable.SqlTable.Joins.Any())
        return null;

      // this is the subquery to be optimized
      if (!(fromExpressionInfo.AppendedTable.SqlTable.TableInfo is ResolvedSubStatementTableInfo))
        return null;
      
      var appendedSubStatement = ((ResolvedSubStatementTableInfo) fromExpressionInfo.AppendedTable.SqlTable.TableInfo).SqlStatement;

      // criteria for optimizable scenario

      // - substatement must not contain any complexities TODO RMLNQSQL-77: Integration tests for each of those
      // TODO RMLNQSQL-77: add Select check? should not contain aggregation (wouldn't work, but shouldn't be possible anyway) or subquery (might be no "optimization" any more), maybe whitelist NewExpressions, MemberExpressions, table references, constants, BinaryExpressions, UnaryExpressions, SqlIs/NotNull, SqlLike, SqlCase, etc.?

      if (appendedSubStatement.SetOperationCombinedStatements.Any())
        return null;

      if (appendedSubStatement.IsDistinctQuery)
        return null;

      // TODO RMLNQSQL-77: Probably not an issue because subqueries never have orderings if they have no TOP or ROW_NUMBER, added for completeness.
      if (appendedSubStatement.Orderings.Any())
        return null;

      if (appendedSubStatement.GroupByExpression != null)
        return null;

      if (appendedSubStatement.RowNumberSelector != null)
        return null;

      if (appendedSubStatement.TopExpression != null)
        return null;

      // - substatements must be a simple LEFT JOIN to a dummy table
      // TODO RMLNQSQL-77: Integration test possible?
      if (appendedSubStatement.SqlTables.Count != 1)
        return null;

      var appendedSubStatementTable = appendedSubStatement.SqlTables.Single().SqlTable;
      if (!(appendedSubStatementTable.TableInfo is UnresolvedDummyRowTableInfo))
        return null;

      // TODO RMLNQSQL-77: Probably no integration test possible, added for defensiveness.
      if (appendedSubStatementTable.Joins.Count() != 1)
        return null;

      // TODO RMLNQSQL-77: No integration test possible.
      var leftJoin = appendedSubStatementTable.Joins.Single();
      if (leftJoin.JoinSemantics != JoinSemantics.Left)
        return null;
      
      var leftJoinedTable = leftJoin.JoinedTable;

      // TODO RMLNQSQL-77: SqlStatementbuilder/SqlStatement: add flag IsDependentSugQuery, calculate when building SqlStatemnentBuilder. If it is dependent, it cannot be optimized.
      // Problem statement: If leftJoinedTable (or one of its joins) has a substatement that depends on one of the tables in 
      // SqlStatementBuilder.SqlTables, we must not perform this optimization because SQL does not allow the right side of a LEFT JOIN to depend on 
      // the left side of a LEFT JOIN.
      //
      // Solution:
      // - Build a blacklist of SqlTables potentially reachable from the leftJoinedTable's substatement (iterate SqlTables, recurse over their joins; 
      //   expression and substatement analysis not necessary because the leftJoin couldn't have accessed any tables in there due to LINQ rules).
      // - Then iterate leftJoinedTable and its recursively joined tables (join conditions are irrelevant as they may depend on outer variables in 
      //   SQL). Deeply visit any ResolvedSubStatementTableInfos, UnresolvedCollectionJoinTableInfos, UnresolvedGroupReferenceTableInfos, 
      //   ResolvedJoinedGroupingTableInfo found there and search for references to the black-listed tables (SqlTableReferenceExpression or direct 
      //   usages of SqlTable). "Deeply visiting" means:
      // -- Check every expression in the corresponding SqlStatements. If a SqlSubStatementExpression is found, recursively visit the nested 
      //    SqlStatement.
      // -- Check every SqlTable and the joined tables for the same ITableInfo kinds listed above.
      // -- Probably build some visitor stuff into ITableInfo so that every ITableInfo can be asked to help in identifying its table references.
      //
      // class TableDependencyFindingTableInfoVisitor : ITableInfoVisitor
      // {
      //   ctor (IReadOnlySet<SqlTable> sqlTables, ITableDependencyInNestedItemsFIndingVisitor nestedItemsVisitor) {... }
      //
      //   void VisitSimpleTableInfo(...) { /* nothing to do */ }
      //   void VisitResolvedSubStatementTableInfo(... ti) { nestedItemsVisitor.Visit (ti.SubStatement); }
      //   void VisitUnresolvedCollectionJoinTableInfo(... ti) { nestedItemsVisitor.Visit (ti.SourceExpression); }
      // }


      var projectionBeforeOptimization = appendedSubStatement.SelectProjection;
      var leftJoinCondition = leftJoin.JoinCondition;
      parentTableForLeftJoin.SqlTable.AddJoinForExplicitQuerySource (new SqlJoin (leftJoinedTable, JoinSemantics.Left, leftJoinCondition));

      _context.AddExpressionMapping (new QuerySourceReferenceExpression (source), projectionBeforeOptimization);

      return leftJoinedTable;
    } 
  }
}