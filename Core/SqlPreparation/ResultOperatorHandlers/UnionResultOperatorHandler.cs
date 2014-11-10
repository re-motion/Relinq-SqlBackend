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
using System.Diagnostics;
using System.Linq;
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

      UpdateDataInfo (resultOperator, sqlStatementBuilder, sqlStatementBuilder.DataInfo);

      var preparedSubStatement = stage.PrepareResultOperatorItemExpression (resultOperator.Source2, context) as SqlSubStatementExpression;
      if (preparedSubStatement == null)
      {
        var message = string.Format (
            "The Union result operator is only supported for combining two query results, but a '{0}' was supplied as the second sequence: {1}",
            resultOperator.Source2.GetType().Name,
            FormattingExpressionTreeVisitor.Format (resultOperator.Source2));
        throw new NotSupportedException (message);
      }

      var combinedStatement = new SetOperationCombinedStatement (preparedSubStatement.SqlStatement, SetOperation.Union);
      sqlStatementBuilder.SetOperationCombinedStatements.Add (combinedStatement);

      // The UnionResultOperator acts as an IQuerySource, i.e., subsequent result operators can refer to its output.
      // When a result operator references the UnionResultOperator's output, it should simply refer to the outer statement's select projection instead. 
      AddMappingForItemExpression(context, sqlStatementBuilder.DataInfo, sqlStatementBuilder.SelectProjection);

      // In SQL, the set operators does not allow the input sequences to contain an "ORDER BY". Therefore, we'll remove them, if any, unless a TOP 
      // expression is specified.
      if (sqlStatementBuilder.Orderings.Any() && sqlStatementBuilder.TopExpression == null)
        sqlStatementBuilder.Orderings.Clear();
      
      // For the second source, removal of unneeded orderings is already performed by PrepareResultOperatorItemExpression.
      Debug.Assert (!combinedStatement.SqlStatement.Orderings.Any() || combinedStatement.SqlStatement.TopExpression != null);

      // However, if an ORDER BY _is_ included together with a TOP, then the ORDER BY is allowed again as long as the whole set-combined statement is 
      // moved to a substatement.
      // I.e., this is invalid:
      //   SELECT  [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = 'Hugo') 
      //   UNION (SELECT TOP (2) [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = 'Boss') ORDER BY [t1].[ID] ASC)
      // but this is valid:
      // SELECT * FROM
      // (
      //   SELECT  [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] = 'Hugo') 
      //   UNION (SELECT TOP (2) [t1].[ID] AS [value] FROM [CookTable] AS [t1] WHERE ([t1].[Name] = 'Boss') ORDER BY [t1].[ID] ASC)
      // ) AS q0

      if (sqlStatementBuilder.Orderings.Any() || combinedStatement.SqlStatement.Orderings.Any())
      {
        MoveCurrentStatementToSqlTable (
            sqlStatementBuilder,
            context,
            ti => new SqlTable (ti, JoinSemantics.Inner),
            stage,
            OrderingExtractionPolicy.DoNotExtractOrderings);
      }
    }
  }
}