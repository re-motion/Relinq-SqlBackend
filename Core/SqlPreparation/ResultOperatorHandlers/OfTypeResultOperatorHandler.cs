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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Handles the <see cref="OfTypeResultOperator"/> by adding a type check to the <see cref="SqlStatementBuilder.WhereCondition"/>.
  /// </summary>
  public class OfTypeResultOperatorHandler : ResultOperatorHandler<OfTypeResultOperator>
  {
    public override void HandleResultOperator (OfTypeResultOperator resultOperator, SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage, ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull (nameof(resultOperator), resultOperator);
      ArgumentUtility.CheckNotNull (nameof(sqlStatementBuilder), sqlStatementBuilder);
      ArgumentUtility.CheckNotNull (nameof(generator), generator);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(context), context);

      EnsureNoGroupExpression (sqlStatementBuilder, generator, stage, context);
      EnsureNoSetOperations (sqlStatementBuilder, generator, stage, context);
      UpdateDataInfo (resultOperator, sqlStatementBuilder, sqlStatementBuilder.DataInfo);

      var typeCheckExpression = Expression.TypeIs (sqlStatementBuilder.SelectProjection, resultOperator.SearchedItemType);
      sqlStatementBuilder.AddWhereCondition (typeCheckExpression);
      sqlStatementBuilder.SelectProjection = Expression.Convert (sqlStatementBuilder.SelectProjection, resultOperator.SearchedItemType);
    }
  }
}