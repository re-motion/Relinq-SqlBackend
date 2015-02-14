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
using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Provides entry points for all transformations that occur during the SQL preparation phase.
  /// </summary>
  public interface ISqlPreparationStage
  {
    Expression PrepareSelectExpression (Expression expression, ISqlPreparationContext context);
    Expression PrepareWhereExpression (Expression expression, ISqlPreparationContext context);
    Expression PrepareTopExpression (Expression expression, ISqlPreparationContext context);
    Expression PrepareOrderByExpression (Expression expression, ISqlPreparationContext context);
    Expression PrepareResultOperatorItemExpression (Expression expression, ISqlPreparationContext context);

    FromExpressionInfo PrepareFromExpression (
        Expression fromExpression,
        ISqlPreparationContext context,
        OrderingExtractionPolicy orderingExtractionPolicy);

    SqlStatement PrepareSqlStatement (QueryModel queryModel, ISqlPreparationContext parentContext);
  }
}