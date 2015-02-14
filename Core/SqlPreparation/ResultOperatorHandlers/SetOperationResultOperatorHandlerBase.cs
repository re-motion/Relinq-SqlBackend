using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Base class for for set operation result operator handlers that act as query sources.
  /// Only works if the unioned enumerable is a subquery. Also deals with ORDER BY in a SQL compatible way.
  /// </summary>
  public abstract class SetOperationResultOperatorHandlerBase<TResultOperator> : ResultOperatorHandler<TResultOperator>
      where TResultOperator : ResultOperatorBase, IQuerySource
  {
    private readonly SetOperation _setOperation;
    private readonly string _operationName;

    protected SetOperationResultOperatorHandlerBase (SetOperation setOperation, string operationName)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("operationName", operationName);

      _setOperation = setOperation;
      _operationName = operationName;
    }

    protected abstract Expression GetSource2 (TResultOperator resultOperator);

    public override void HandleResultOperator (
        TResultOperator resultOperator,
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

      var source2 = GetSource2 (resultOperator);
      var preparedSubStatement = stage.PrepareResultOperatorItemExpression (source2, context) as SqlSubStatementExpression;
      if (preparedSubStatement == null)
      {
        var message = string.Format (
            "The '" + _operationName + "' operation is only supported for combining two query results, but a '{0}' was supplied as the "
            + "second sequence: {1}",
            source2.GetType().Name,
            source2);
        throw new NotSupportedException (message);
      }

      var combinedStatement = new SetOperationCombinedStatement (preparedSubStatement.SqlStatement, _setOperation);
      sqlStatementBuilder.SetOperationCombinedStatements.Add (combinedStatement);

      // The set operators act as an IQuerySource, i.e., subsequent result operators can refer to its output.
      // When a result operator references the set operator's output, it should simply refer to the outer statement's select projection instead. 
      AddMappingForItemExpression(context, sqlStatementBuilder.DataInfo, sqlStatementBuilder.SelectProjection);

      // In SQL, the set operators does not allow the input sequences to contain an "ORDER BY". Therefore, we'll remove them, if any, unless a TOP 
      // expression is specified.
      if (sqlStatementBuilder.Orderings.Any() && sqlStatementBuilder.TopExpression == null)
        sqlStatementBuilder.Orderings.Clear();
      
      // For the second source, removal of unneeded orderings is already performed by PrepareResultOperatorItemExpression.
      Assertion.DebugAssert (!combinedStatement.SqlStatement.Orderings.Any() || combinedStatement.SqlStatement.TopExpression != null);

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
            stage,
            OrderingExtractionPolicy.DoNotExtractOrderings);
      }
    }
  }
}