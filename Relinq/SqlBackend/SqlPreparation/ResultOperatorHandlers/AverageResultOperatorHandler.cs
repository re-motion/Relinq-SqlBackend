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
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Handles the <see cref="AverageResultOperator"/>. When the <see cref="AverageResultOperator"/> occurs after a 
  /// <see cref="SqlStatementBuilder.TopExpression"/> has been set, a sub-statement is created for 
  /// everything up to the <see cref="SqlStatementBuilder.TopExpression"/>.
  /// </summary>
  public class AverageResultOperatorHandler : AggregationResultOperatorHandler<AverageResultOperator>
  {
    public override AggregationModifier AggregationModifier
    {
      get { return AggregationModifier.Average; }
    }

    public override void HandleResultOperator (
        AverageResultOperator resultOperator,
        SqlStatementBuilder sqlStatementBuilder,
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        ISqlPreparationContext context)
    {
      base.HandleResultOperator (resultOperator, sqlStatementBuilder, generator, stage, context);

      // With the Average query operator, the result type determines the desired precision of the algorithm. For example, new[] { 1, 2 }.Average() 
      // returns a double and thus calculates an average with double precision (1.5).
      // With SQL, however, the argument type determines the precision of the algorithm. I.e., AVG (intColumn) will return an integer with truncated
      // average (1).
      // To simulate Average behavior, we'll add a conversion of the argument expression if the types don't match.
      var aggregationExpression = (AggregationExpression) sqlStatementBuilder.SelectProjection;
      if (aggregationExpression.Expression.Type != aggregationExpression.Type)
      {
        sqlStatementBuilder.SelectProjection = new AggregationExpression (
            aggregationExpression.Type,
            new SqlConvertExpression (aggregationExpression.Type, aggregationExpression.Expression),
            aggregationExpression.AggregationModifier);
      }
    }
  }
}