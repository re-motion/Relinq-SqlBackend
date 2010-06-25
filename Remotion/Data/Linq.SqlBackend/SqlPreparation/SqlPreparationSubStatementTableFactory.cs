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
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Creates a <see cref="SqlTableBase"/> object from a given <see cref="SqlStatement"/>, extracting all <see cref="SqlStatement.Orderings"/> in the
  /// statement as required by SQL.
  /// </summary>
  public class SqlPreparationSubStatementTableFactory
  {
    public static FromExpressionInfo CreateSqlTableForSubStatement (
        SqlStatement sqlStatement,
        ISqlPreparationStage sqlPreparationStage,
        ISqlPreparationContext context,
        UniqueIdentifierGenerator generator,
        Func<ITableInfo, SqlTableBase> tableCreator)
    {
      SqlTableBase sqlTable;
      Expression itemSelector;
      var extractedOrderings = new List<Ordering>();

      if (sqlStatement.Orderings.Count > 0)
      {
        Expression newSelectProjection = Expression.Constant (null);
        Type tupleType;

        for (var i = sqlStatement.Orderings.Count - 1; i >= 0; --i)
        {
          tupleType = typeof (KeyValuePair<,>).MakeGenericType (sqlStatement.Orderings[i].Expression.Type, newSelectProjection.Type);
          newSelectProjection =
              Expression.New (
                  tupleType.GetConstructors()[0],
                  new[] { sqlStatement.Orderings[i].Expression, newSelectProjection },
                  new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });
        }

        tupleType = typeof (KeyValuePair<,>).MakeGenericType (sqlStatement.SelectProjection.Type, newSelectProjection.Type);
        newSelectProjection = Expression.New (
            tupleType.GetConstructors()[0],
            new[] { sqlStatement.SelectProjection, newSelectProjection },
            new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });

        var preparedNewSelectProjection = sqlPreparationStage.PrepareSelectExpression (newSelectProjection, context);
        Debug.Assert (preparedNewSelectProjection.Type == newSelectProjection.Type);

        var builder = new SqlStatementBuilder (sqlStatement) { SelectProjection = preparedNewSelectProjection };
        if (sqlStatement.TopExpression == null)
          builder.Orderings.Clear();
        builder.RecalculateDataInfo (sqlStatement.SelectProjection);
        var newSqlStatement = builder.GetSqlStatement();

        var tableInfo = new ResolvedSubStatementTableInfo (generator.GetUniqueIdentifier ("q"), newSqlStatement);
        sqlTable = tableCreator (tableInfo);
        itemSelector = Expression.MakeMemberAccess (new SqlTableReferenceExpression (sqlTable), preparedNewSelectProjection.Type.GetProperty ("Key"));

        var currentOrderingTuple = Expression.MakeMemberAccess (
            new SqlTableReferenceExpression (sqlTable), preparedNewSelectProjection.Type.GetProperty ("Value"));
        for (var i = 0; i < sqlStatement.Orderings.Count; ++i)
        {
          extractedOrderings.Add (
              new Ordering (
                  Expression.MakeMemberAccess (currentOrderingTuple, currentOrderingTuple.Type.GetProperty ("Key")),
                  sqlStatement.Orderings[i].OrderingDirection));
          currentOrderingTuple = Expression.MakeMemberAccess (currentOrderingTuple, currentOrderingTuple.Type.GetProperty ("Value"));
        }
      }
      else
      {
        var tableInfo = new ResolvedSubStatementTableInfo (generator.GetUniqueIdentifier ("q"), sqlStatement);
        sqlTable = tableCreator (tableInfo);
        itemSelector = new SqlTableReferenceExpression (sqlTable);
      }
      return new FromExpressionInfo (sqlTable, extractedOrderings.ToArray(), itemSelector, null, true);
    }
  }
}