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
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  public class UnionResultOperatorHandler : ResultOperatorHandler<UnionResultOperator>
  {
    public override void HandleResultOperator (
        UnionResultOperator resultOperator,
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

      var preparedSubStatement = stage.PrepareResultOperatorItemExpression (resultOperator.Source2, context) as SqlSubStatementExpression;
      if (preparedSubStatement == null)
      {
        var message = string.Format (
            "The Union result operator is only supported for combining two query results, but a '{0}' was supplied as the second sequence: {1}",
            resultOperator.Source2.GetType().Name,
            FormattingExpressionTreeVisitor.Format (resultOperator.Source2));
        throw new NotSupportedException (message);
      }

      sqlStatementBuilder.SetOperationCombinedStatements.Add (
          new SetOperationCombinedStatement (preparedSubStatement.SqlStatement, SetOperation.Union));
    }
  }
}