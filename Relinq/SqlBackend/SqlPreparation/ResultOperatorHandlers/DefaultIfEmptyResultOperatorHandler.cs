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

using System.Linq.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// <see cref="DefaultIfEmptyResultOperatorHandler"/> handles the <see cref="DefaultIfEmptyResultOperator"/>. It wraps the SQL statement into
  /// a subquery and puts that subquery into a left join.
  /// </summary>
  public class DefaultIfEmptyResultOperatorHandler : ResultOperatorHandler<DefaultIfEmptyResultOperator>
  {
    public override void HandleResultOperator (
        DefaultIfEmptyResultOperator resultOperator,
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

      MoveCurrentStatementToSqlTable (
          sqlStatementBuilder,
          generator,
          context,
          info =>
          new SqlJoinedTable (
              new ResolvedJoinInfo ((IResolvedTableInfo) info, Expression.Equal (new SqlLiteralExpression (1), new SqlLiteralExpression (1))),
              JoinSemantics.Left),
          stage);
    }
  }
}