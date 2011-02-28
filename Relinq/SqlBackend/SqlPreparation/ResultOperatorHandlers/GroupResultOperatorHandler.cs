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
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Handles the <see cref="GroupResultOperator"/>.
  /// </summary>
  public class GroupResultOperatorHandler : ResultOperatorHandler<GroupResultOperator>
  {
    public override void HandleResultOperator (GroupResultOperator resultOperator, SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage, ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      UpdateDataInfo (resultOperator, sqlStatementBuilder, sqlStatementBuilder.DataInfo);
      EnsureNoTopExpression (sqlStatementBuilder, generator, stage, context);
      EnsureNoGroupExpression (sqlStatementBuilder, generator, stage, context);
      EnsureNoDistinctQuery (sqlStatementBuilder, generator, stage, context);

      var preparedKeySelector = stage.PrepareResultOperatorItemExpression (resultOperator.KeySelector, context);
      preparedKeySelector = HandlePotentialConstantExpression (preparedKeySelector);
      preparedKeySelector = HandlePotentialSubStatementExpression (preparedKeySelector, sqlStatementBuilder, generator);

      var preparedElementSelector = stage.PrepareResultOperatorItemExpression (resultOperator.ElementSelector, context);

      sqlStatementBuilder.GroupByExpression = preparedKeySelector;
      sqlStatementBuilder.SelectProjection = SqlGroupingSelectExpression.CreateWithNames (preparedKeySelector, preparedElementSelector);
    }

    private Expression HandlePotentialConstantExpression (Expression preparedKeySelector)
    {
      var constantExpression = preparedKeySelector as ConstantExpression;
      if (constantExpression == null)
        return preparedKeySelector;
      
      var subSqlStatement = new SqlStatementBuilder
                            {
                                DataInfo = new StreamedSingleValueInfo (constantExpression.Type, false),
                                SelectProjection = new NamedExpression (null, constantExpression)
                            }.GetSqlStatement();

      return new SqlSubStatementExpression (subSqlStatement);
    }

    private Expression HandlePotentialSubStatementExpression (
        Expression preparedKeySelector, 
        SqlStatementBuilder sqlStatementBuilder, 
        UniqueIdentifierGenerator generator)
    {
      var subStatementExpression = preparedKeySelector as SqlSubStatementExpression;
      if (subStatementExpression == null)
        return preparedKeySelector;

      var sqlTable = subStatementExpression.ConvertToSqlTable (generator.GetUniqueIdentifier ("t"));
      sqlStatementBuilder.SqlTables.Add (sqlTable);

      return new SqlTableReferenceExpression (sqlTable);
    }
  }
}