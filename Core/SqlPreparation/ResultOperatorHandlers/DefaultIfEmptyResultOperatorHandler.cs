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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.SqlBackend.SqlStatementModel;
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

      ////TODO RMLNQSQL-1: Implementation of join optimization
      //if (sqlStatementBuilder.SqlTables.Count == 1)
      //{
      //  // If there is exactly one top-level table in this statement, "DefaultIfEmpty" can be implemented simply by converting this table into the 
      //  // right part of a left join with a dummy table. It's important to convert the WHERE condition into a JOIN condition, otherwise it would
      //  // be applied _after_ the left join rather than _during_ the left join.
      //  //TODO RMLNQSQL-1: Check if there is an integration test with a WHERE condition as well as one without one.

      //  var nullIfEmptyStatementBuilder = new SqlStatementBuilder();
      //  nullIfEmptyStatementBuilder.SelectProjection = new NamedExpression ("Empty", new SqlCustomTextExpression("NULL", typeof (object)));
      //  nullIfEmptyStatementBuilder.DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<object>), nullIfEmptyStatementBuilder.SelectProjection);

      //  var nullIfEmptySqlTable = new SqlTable (new ResolvedSubStatementTableInfo("Empty", nullIfEmptyStatementBuilder.GetSqlStatement()), JoinSemantics.Inner);
      //  var originalSqlTable = sqlStatementBuilder.SqlTables[0];

      //  // TODO RMLNQSQL-1: Add AddLeftJoin for joins without a member.
      //  var newJoinedTable = nullIfEmptySqlTable.AddLeftJoin (
      //      // TODO RMLNQSQL-1: Rename ResolvedJoinInfo to TableBasedJoinInfo, rename other "Unresolved...JoinInfo" classes to "SingleItemMemberBasedJoinInfo" and "CollectionMemberBasedJoinInfo", move out of Unresolved/Resolved namespaces.
      //      // TODO RMLNQSQL-1: Change TableBasedJoinInfo to work with arbitrary ITableInfos, not only IResolvedTableInfos.
      //      new ResolvedJoinInfo (
      //          originalSqlTable.TableInfo,
      //          sqlStatementBuilder.WhereCondition ?? Expression.Equal (new SqlLiteralExpression (1), new SqlLiteralExpression (1))));
      //  sqlStatementBuilder.WhereCondition = null;

      //  sqlStatementBuilder.SqlTables.Clear();
      //  sqlStatementBuilder.SqlTables.Add (nullIfEmptySqlTable);

      //  // Further TODOs:
      //  // TODO: In SqlContextTableInfoVisitor, replace "!=" with !Equals checks for SqlStatements. Change SqlStatement.Equals to perform a ref check first for performance.
      //  // TODO: Rename ITableInfo.GetResolvedTableInfo and IJoinInfo.GetResolvedJoinInfo to ConvertTo...
      //  // TODO: When refactoring mutability of SqlTable/SqlJoinedTable/SqlTableBase, consider changing SqlTableReferenceExpression [and all other references to SqlTableBases] to no longer point to the SqlTableBase object, but instead needs to look up the associated table (in the SqlStatementBuilder or SqlStatement, exposed via context).
      //}
      //else
      //{
      //// Otherwise, we need to move the whole statement up to now into a subquery and put that into a left join.
      //  // TODO RMLNQSQL-1: Refactor to build exact join here as well, using nullIfEmptySqlTable; then eliminate SqlTable.JoinSemantics.
      MoveCurrentStatementToSqlTable (sqlStatementBuilder, context, info => new SqlTable (info, JoinSemantics.Left), stage);
      //}

      ////TODO RMLNQSQL-1: Also add integration tests with more than one table to see that this still works.
    }
  }
}