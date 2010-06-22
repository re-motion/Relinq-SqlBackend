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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// <see cref="AggregationResultOperatorHandler{T}"/> is the base class for all specific aggregation <see cref="ResultOperatorHandler{T}"/>s.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class AggregationResultOperatorHandler<T> : ResultOperatorHandler<T>
      where T: ResultOperatorBase
  {
    public abstract AggregationModifier AggregationModifier { get; }

    public override void HandleResultOperator (
        T resultOperator,
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

      sqlStatementBuilder.Orderings.Clear ();
      EnsureNoTopExpression (resultOperator, sqlStatementBuilder, generator, stage, context);
      EnsureNoGroupExpression (resultOperator, sqlStatementBuilder, generator, stage, context);
      EnsureNoDistinctQuery (resultOperator, sqlStatementBuilder, generator, stage, context);
      UpdateDataInfo (resultOperator, sqlStatementBuilder, sqlStatementBuilder.DataInfo);

      var namedExpression = sqlStatementBuilder.SelectProjection as NamedExpression;
      if (namedExpression == null)
        throw new InvalidOperationException ("Named expression expected at this point");

      sqlStatementBuilder.SelectProjection = new AggregationExpression (
          sqlStatementBuilder.DataInfo.DataType, new NamedExpression(namedExpression.Name, namedExpression.Expression), AggregationModifier);
    }
  }
}