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

      Expression rowNumberExpression;
      if(sqlStatementBuilder.Orderings.Count>0)
         rowNumberExpression =  new SqlRowNumberExpression (sqlStatementBuilder.Orderings.ToArray ());
      else
         rowNumberExpression =
            new SqlRowNumberExpression (
                new[]
                {
                    new Ordering (
                    new SqlSubStatementExpression (
                        new SqlStatement (
                            new StreamedScalarValueInfo (typeof (int)), Expression.Constant (1), new SqlTable[0], new Ordering[0], null, null, false)),
                    OrderingDirection.Asc)});

      var tupleType = typeof (KeyValuePair<,>).MakeGenericType (sqlStatementBuilder.SelectProjection.Type, rowNumberExpression.Type);
      Expression newSelectProjection = Expression.New (
          tupleType.GetConstructors ()[0],
          new[] { sqlStatementBuilder.SelectProjection, rowNumberExpression },
          new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });

      newSelectProjection = stage.PrepareSelectExpression (newSelectProjection, context);

      sqlStatementBuilder.SelectProjection = newSelectProjection;

      sqlStatementBuilder.RecalculateDataInfo (oldSqlStatement.SelectProjection);
      var newSqlStatement = sqlStatementBuilder.GetStatementAndResetBuilder ();
      
      var tableInfo = new ResolvedSubStatementTableInfo (generator.GetUniqueIdentifier ("q"), newSqlStatement);
      var sqlTable = new SqlTable (tableInfo);
      var keySelector = Expression.MakeMemberAccess (new SqlTableReferenceExpression (sqlTable), newSelectProjection.Type.GetProperty ("Key"));
      var valueSelector = Expression.MakeMemberAccess (new SqlTableReferenceExpression (sqlTable), newSelectProjection.Type.GetProperty ("Value"));
      
      sqlStatementBuilder.SelectProjection = keySelector;
      sqlStatementBuilder.SqlTables.Add (sqlTable);
      sqlStatementBuilder.WhereCondition = Expression.GreaterThan (valueSelector, resultOperator.Count);
      sqlStatementBuilder.Orderings.Add (new Ordering(valueSelector, OrderingDirection.Asc));
      sqlStatementBuilder.DataInfo = oldSqlStatement.DataInfo;

      context.AddExpressionMapping (resultOperator.Count, keySelector);
    }
  }
}