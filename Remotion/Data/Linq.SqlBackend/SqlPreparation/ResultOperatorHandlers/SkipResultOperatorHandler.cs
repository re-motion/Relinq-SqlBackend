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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  public class SkipResultOperatorHandler : ResultOperatorHandler<SkipResultOperator>
  {
    public override void HandleResultOperator (
        SkipResultOperator resultOperator,
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

      var oldSqlStatement = sqlStatementBuilder.GetSqlStatement();

      var orderings = GetOrderingsForRowNumber(sqlStatementBuilder);
      Expression rowNumberExpression = new SqlRowNumberExpression (orderings);

      var tupleType = typeof (KeyValuePair<,>).MakeGenericType (sqlStatementBuilder.SelectProjection.Type, rowNumberExpression.Type);
      Expression newSelectProjection = Expression.New (
          tupleType.GetConstructors ()[0],
          new[] { sqlStatementBuilder.SelectProjection, rowNumberExpression },
          new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });

      newSelectProjection = stage.PrepareSelectExpression (newSelectProjection, context);

      sqlStatementBuilder.SelectProjection = newSelectProjection;
      if (sqlStatementBuilder.TopExpression == null) 
        sqlStatementBuilder.Orderings.Clear();

      sqlStatementBuilder.RecalculateDataInfo (oldSqlStatement.SelectProjection);
      var newSqlStatement = sqlStatementBuilder.GetStatementAndResetBuilder ();
      
      var tableInfo = new ResolvedSubStatementTableInfo (generator.GetUniqueIdentifier ("q"), newSqlStatement);
      var sqlTable = new SqlTable (tableInfo);
      
      var originalProjectionSelector = Expression.MakeMemberAccess (new SqlTableReferenceExpression (sqlTable), newSelectProjection.Type.GetProperty ("Key"));
      var rowNumberSelector = Expression.MakeMemberAccess (new SqlTableReferenceExpression (sqlTable), newSelectProjection.Type.GetProperty ("Value"));
      
      sqlStatementBuilder.SelectProjection = originalProjectionSelector;
      sqlStatementBuilder.SqlTables.Add (sqlTable);
      sqlStatementBuilder.AddWhereCondition(Expression.GreaterThan (rowNumberSelector, resultOperator.Count));
      sqlStatementBuilder.Orderings.Add (new Ordering (rowNumberSelector, OrderingDirection.Asc));
      sqlStatementBuilder.DataInfo = oldSqlStatement.DataInfo;
      sqlStatementBuilder.RowNumberSelector = rowNumberSelector;
      sqlStatementBuilder.CurrentRowNumberOffset = resultOperator.Count;
      
      context.AddExpressionMapping (resultOperator.Count, originalProjectionSelector);
    }

    private Ordering[] GetOrderingsForRowNumber (SqlStatementBuilder sqlStatementBuilder)
    {
      var orderings = sqlStatementBuilder.Orderings.ToArray();
      if (orderings.Length == 0)
      {
        // Create a trivial substatement selecting an integer as the ordering expression if the statement doesn't contain one.
        // This will cause SQL Server to assign the row number according to its internal row order.
        var trivialSubStatement = new SqlStatement (
            new StreamedScalarValueInfo (typeof (int)),
            Expression.Constant (1),
            new SqlTable[0],
            new Ordering[0],
            null,
            null,
            false, null, null);
        orderings = new[] { new Ordering (new SqlSubStatementExpression (trivialSubStatement), OrderingDirection.Asc) };
      }

      return orderings;
    }
  }
}