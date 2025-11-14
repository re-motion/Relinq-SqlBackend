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
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Handles the <see cref="SkipResultOperator"/> by transforming the query analyzed so far into a sub-statement construct including a 
  /// <see cref="SqlRowNumberExpression"/>. The outer query selects from that sub-statement, orders the result correctly, and filters on the 
  /// row number.
  /// </summary>
  public class SkipResultOperatorHandler : ResultOperatorHandler<SkipResultOperator>
  {
    struct SubStatementWithRowNumber
    {
      public readonly SqlTable SubStatementTable;
      public readonly Expression OriginalProjectionSelector;
      public readonly Expression RowNumberSelector;

      public SubStatementWithRowNumber (SqlTable subStatement, Expression originalProjectionSelector, Expression rowNumberSelector)
      {
        SubStatementTable = subStatement;
        OriginalProjectionSelector = originalProjectionSelector;
        RowNumberSelector = rowNumberSelector;
      }
    }

    public override void HandleResultOperator (
        SkipResultOperator resultOperator,
        SqlStatementBuilder sqlStatementBuilder,
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull (nameof(resultOperator), resultOperator);
      ArgumentUtility.CheckNotNull (nameof(sqlStatementBuilder), sqlStatementBuilder);
      ArgumentUtility.CheckNotNull (nameof(generator), generator);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(context), context);

      EnsureNoSetOperations (sqlStatementBuilder, generator, stage, context);

      // We move the statement into a subquery and change it to return the row number in addition to the original projection. Then, we use that
      // row number from the outer statement to skip the desired amount of rows. We also add an order by clause to the outer statement to ensure
      // that the rows come in the correct order.
      // E.g., (from c in Cooks orderby c.FirstName select c.LastName).Skip (20)
      // becomes
      // from x in 
      //   (from c in Cooks select new { Key = c.LastName, Value = ROW_NUMBER() OVER (c.FirstName) })
      // where x.Value > 20
      // orderby x.Value
      // select x.Key

      var originalDataInfo = sqlStatementBuilder.DataInfo;

      var subStatementWithRowNumber = CreateSubStatementWithRowNumber (sqlStatementBuilder, generator, stage, context);

      sqlStatementBuilder.SelectProjection = subStatementWithRowNumber.OriginalProjectionSelector;
      sqlStatementBuilder.SqlTables.Add (subStatementWithRowNumber.SubStatementTable);
      sqlStatementBuilder.AddWhereCondition (Expression.GreaterThan (subStatementWithRowNumber.RowNumberSelector, resultOperator.Count));
      sqlStatementBuilder.Orderings.Add (new Ordering (subStatementWithRowNumber.RowNumberSelector, OrderingDirection.Asc));
      sqlStatementBuilder.DataInfo = originalDataInfo;
      sqlStatementBuilder.RowNumberSelector = subStatementWithRowNumber.RowNumberSelector;
      sqlStatementBuilder.CurrentRowNumberOffset = resultOperator.Count;

      AddMappingForItemExpression (context, originalDataInfo, subStatementWithRowNumber.OriginalProjectionSelector);
    }

    private SubStatementWithRowNumber CreateSubStatementWithRowNumber (
        SqlStatementBuilder sqlStatementBuilder, 
        UniqueIdentifierGenerator generator, 
        ISqlPreparationStage stage, 
        ISqlPreparationContext context)
    {
      var originalSelectProjection = sqlStatementBuilder.SelectProjection;

      IncludeRowNumberInSelectProjection (sqlStatementBuilder, stage, context);

      // Orderings are not allowed in SQL substatements unless a TOP expression is present
      if (sqlStatementBuilder.TopExpression == null) 
        sqlStatementBuilder.Orderings.Clear();

      sqlStatementBuilder.RecalculateDataInfo (originalSelectProjection);
      // No NamedExpression required here because the row number tuple's items already have named members
      var newSqlStatement = sqlStatementBuilder.GetStatementAndResetBuilder ();

      var tableInfo = new ResolvedSubStatementTableInfo (generator.GetUniqueIdentifier ("q"), newSqlStatement);
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Inner);

      var originalProjectionSelector = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (sqlTable), 
          newSqlStatement.SelectProjection.Type.GetProperty ("Key"));
      var rowNumberSelector = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (sqlTable), 
          newSqlStatement.SelectProjection.Type.GetProperty ("Value"));

      return new SubStatementWithRowNumber (sqlTable, originalProjectionSelector, rowNumberSelector);
    }

    private void IncludeRowNumberInSelectProjection (SqlStatementBuilder sqlStatementBuilder, ISqlPreparationStage stage, ISqlPreparationContext context)
    {
      var rowNumberExpression = CreateRowNumberExpression(sqlStatementBuilder);

      var tupleType = typeof (KeyValuePair<,>).MakeGenericType (sqlStatementBuilder.SelectProjection.Type, rowNumberExpression.Type);
      Expression newSelectProjection = Expression.New (
          tupleType.GetConstructors ()[0],
          new[] { sqlStatementBuilder.SelectProjection, rowNumberExpression },
          new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });

      sqlStatementBuilder.SelectProjection = stage.PrepareSelectExpression (newSelectProjection, context);
    }

    private Expression CreateRowNumberExpression (SqlStatementBuilder sqlStatementBuilder)
    {
      var orderings = sqlStatementBuilder.Orderings.ToArray();
      if (orderings.Length == 0)
        orderings = new[] { new Ordering (Expression.Constant (1), OrderingDirection.Asc) };
      
      return new SqlRowNumberExpression (orderings);
    }
  }
}