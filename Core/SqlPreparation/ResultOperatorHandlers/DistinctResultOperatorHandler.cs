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

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Handles the <see cref="DistinctResultOperator"/> by setting the <see cref="SqlStatementBuilder.IsDistinctQuery"/> flag. When the 
  /// <see cref="DistinctResultOperator"/> occurs after a <see cref="SqlStatementBuilder.TopExpression"/> has been set, a sub-statement is created.
  /// </summary>
  public class DistinctResultOperatorHandler : ResultOperatorHandler<DistinctResultOperator>
  {
    public override void HandleResultOperator (DistinctResultOperator resultOperator, SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage, ISqlPreparationContext context)
    {
      EnsureNoTopExpression (sqlStatementBuilder, generator, stage, context);
      EnsureNoGroupExpression (sqlStatementBuilder, generator, stage, context);
      EnsureNoSetOperations (sqlStatementBuilder, generator, stage, context);
      UpdateDataInfo (resultOperator, sqlStatementBuilder, sqlStatementBuilder.DataInfo);

      sqlStatementBuilder.IsDistinctQuery = true;
      sqlStatementBuilder.Orderings.Clear (); //Distinct queries do not require ORDER BY clauses because LINQ's Distinct operator allows to reorder the result
    }
  }
}