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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Default implementation of <see cref="IResultOperatorHandler"/> providing commonly needed functionality.
  /// </summary>
  /// <typeparam name="T">The result operator type handled by the concrete subclass of <see cref="ResultOperatorHandler{T}"/>.</typeparam>
  public abstract class ResultOperatorHandler<T> : IResultOperatorHandler
      where T: ResultOperatorBase
  {
    public Type SupportedResultOperatorType
    {
      get { return typeof (T); }
    }

    public abstract void HandleResultOperator (
        T resultOperator,
        SqlStatementBuilder sqlStatementBuilder,
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        ISqlPreparationContext context);

    protected void EnsureNoTopExpression (
        SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage, ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);

      if (sqlStatementBuilder.TopExpression != null)
        MoveCurrentStatementToSqlTable (sqlStatementBuilder, context, info => new SqlTable (info, JoinSemantics.Inner), stage);
    }

    protected void EnsureNoDistinctQuery (
        SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage, ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);

      if (sqlStatementBuilder.IsDistinctQuery)
        MoveCurrentStatementToSqlTable (sqlStatementBuilder, context, info => new SqlTable (info, JoinSemantics.Inner), stage);
    }

    protected void EnsureNoGroupExpression (
      SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage, ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      if (sqlStatementBuilder.GroupByExpression != null)
        MoveCurrentStatementToSqlTable (sqlStatementBuilder, context, info => new SqlTable (info, JoinSemantics.Inner), stage);
    }

    protected void EnsureNoSetOperations (
        SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage, ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);

      if (sqlStatementBuilder.SetOperationCombinedStatements.Any())
        MoveCurrentStatementToSqlTable (sqlStatementBuilder, context, info => new SqlTable (info, JoinSemantics.Inner), stage);
    }

    protected void MoveCurrentStatementToSqlTable (
        SqlStatementBuilder sqlStatementBuilder,
        ISqlPreparationContext context,
        // TODO RMLNQSQL-78: Replace with SqlTable
        Func<ITableInfo, SqlTable> tableGenerator,
        ISqlPreparationStage stage,
        OrderingExtractionPolicy orderingExtractionPolicy = OrderingExtractionPolicy.ExtractOrderingsIntoProjection)
    {
      // Ensure that select clause is named - usually SqlPreparationQueryModelVisitor would do this, but it hasn't done it yet
      sqlStatementBuilder.SelectProjection = new NamedExpression (null, sqlStatementBuilder.SelectProjection);

      var oldStatement = sqlStatementBuilder.GetStatementAndResetBuilder();
      var fromExpressionInfo = stage.PrepareFromExpression (
          new SqlSubStatementExpression (oldStatement),
          context,
          tableGenerator,
          orderingExtractionPolicy);

      sqlStatementBuilder.SqlTables.Add (fromExpressionInfo.AppendedTable);
      sqlStatementBuilder.SelectProjection = fromExpressionInfo.ItemSelector;
      sqlStatementBuilder.Orderings.AddRange (fromExpressionInfo.ExtractedOrderings);
      Assertion.DebugAssert (fromExpressionInfo.WhereCondition == null);

      // the new statement is an identity query that selects the result of its subquery, so it starts with the same data type
      sqlStatementBuilder.DataInfo = oldStatement.DataInfo;

      AddMappingForItemExpression(context, oldStatement.DataInfo, fromExpressionInfo.ItemSelector);
    }

    protected void AddMappingForItemExpression (ISqlPreparationContext context, IStreamedDataInfo dataInfo, Expression replacement)
    {
      Assertion.DebugAssert (dataInfo is StreamedSequenceInfo);

      // Later ResultOperatorHandlers might have expressions that access the value streaming out from this result operator. These expressions must 
      // be updated to get their input expression (the ItemExpression of sqlStatement.DataInfo) from the sub-statement table we just created.
      // Therefore, register an expression mapping from the ItemExpression to the new SqlTable.
      // (We cannot use the sqlStatement.SelectExpression for the mapping because that expression has already been transformed and therefore will 
      // not compare equal to the expressions of the later result operators as long as we can only compare expressions by reference. The 
      // ItemExpression, on the other hand, should compare fine because it is inserted by reference into the result operators' expressions during 
      // the front-end's lambda resolution process.)

      var itemExpressionNowInSqlTable = ((StreamedSequenceInfo) dataInfo).ItemExpression;
      context.AddExpressionMapping (itemExpressionNowInSqlTable, replacement);
    }

    /// <summary>
    /// Recalculates <see cref="SqlStatementBuilder.DataInfo"/> based on the <paramref name="resultOperator"/> and the 
    /// previous <paramref name="dataInfo"/>.
    /// </summary>
    protected void UpdateDataInfo (ResultOperatorBase resultOperator, SqlStatementBuilder sqlStatementBuilder, IStreamedDataInfo dataInfo)
    {
      sqlStatementBuilder.DataInfo = resultOperator.GetOutputDataInfo (dataInfo);
    }

    void IResultOperatorHandler.HandleResultOperator (
        ResultOperatorBase resultOperator,
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

      var castOperator = ArgumentUtility.CheckNotNullAndType<T> ("resultOperator", resultOperator);
      HandleResultOperator (castOperator, sqlStatementBuilder, generator, stage, context);
    }
  }
}