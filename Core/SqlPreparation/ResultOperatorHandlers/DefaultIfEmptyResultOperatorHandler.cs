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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// <see cref="DefaultIfEmptyResultOperatorHandler"/> handles the <see cref="DefaultIfEmptyResultOperator"/>. It wraps the SQL statement into
  /// a subquery and puts that subquery into a left join.
  /// </summary>
  public class DefaultIfEmptyResultOperatorHandler : ResultOperatorHandler<DefaultIfEmptyResultOperator>
  {
    public override void HandleResultOperator (
        DefaultIfEmptyResultOperator resultOperator,
        SqlStatementBuilder sqlStatementBuilder,
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      if (sqlStatementBuilder.SqlTables.Count == 1 && !sqlStatementBuilder.SetOperationCombinedStatements.Any())
      {
        // If there is exactly one top-level table in this statement (and no UNIONS etc.), "DefaultIfEmpty" can be implemented simply by converting 
        // this table into the right part of a left join with a dummy table.
        // It's important to convert the WHERE condition into a JOIN condition, otherwise it would be applied _after_ the left join rather than 
        // _during_ the left join.

        // Create a new dummy table: (SELECT NULL AS [Empty]) AS [Empty]
        var nullIfEmptyStatementBuilder = new SqlStatementBuilder();
        var selectProjection = new NamedExpression ("Empty", SqlLiteralExpression.Null (typeof (object)));
        nullIfEmptyStatementBuilder.SelectProjection = selectProjection;
        nullIfEmptyStatementBuilder.DataInfo = new StreamedSequenceInfo (
            typeof (IEnumerable<>).MakeGenericType (selectProjection.Type),
            selectProjection);

        var nullIfEmptySqlTable = new SqlTable (
            new ResolvedSubStatementTableInfo ("Empty", nullIfEmptyStatementBuilder.GetSqlStatement()),
            JoinSemantics.Inner);

        // Add the original table to the dummy table as a LEFT JOIN, use the WHERE condition as the JOIN condition (if any; otherwise use (1 = 1)):
        var originalSqlTable = sqlStatementBuilder.SqlTables[0];
        var joinCondition = sqlStatementBuilder.WhereCondition ?? Expression.Equal (new SqlLiteralExpression (1), new SqlLiteralExpression (1));
        var join = new SqlJoin(originalSqlTable, JoinSemantics.Left,  joinCondition);
        nullIfEmptySqlTable.AddJoin (join);

        // Replace original table with dummy table:
        sqlStatementBuilder.SqlTables.Clear();
        sqlStatementBuilder.SqlTables.Add (nullIfEmptySqlTable);
        
        // WHERE condition was moved to JOIN condition => no longer needed.
        sqlStatementBuilder.WhereCondition = null;

        // Further TODOs:
        // TODO RMLNQSQL-1: In SqlContextTableInfoVisitor, replace "!=" with !Equals checks for SqlStatements. Change SqlStatement.Equals to perform a ref check first for performance.
        // TODO RMLNQSQL-1: Rename ITableInfo.GetResolvedTableInfo and IJoinInfo.GetResolvedJoinInfo to ConvertTo...
      }
      else
      {
        // Otherwise, we need to move the whole statement up to now into a subquery and put that into a left join.
        // TODO RMLNQSQL-1: Refactor to build exact join here as well, using nullIfEmptySqlTable; then eliminate SqlTable.JoinSemantics.
        MoveCurrentStatementToSqlTable (sqlStatementBuilder, context, info => new SqlTable (info, JoinSemantics.Left), stage);
      }
    }
  }
}