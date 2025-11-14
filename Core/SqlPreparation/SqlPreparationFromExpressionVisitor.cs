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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Analyzes the <see cref="FromClauseBase.FromExpression"/> of a <see cref="FromClauseBase"/> and returns a <see cref="SqlTableBase"/> that 
  /// represents the data source of the <see cref="FromClauseBase"/>.
  /// </summary>
  public class SqlPreparationFromExpressionVisitor : SqlPreparationExpressionVisitor, IUnresolvedSqlExpressionVisitor
  {
    public static FromExpressionInfo AnalyzeFromExpression (
        Expression fromExpression,
        ISqlPreparationStage stage,
        UniqueIdentifierGenerator generator,
        IMethodCallTransformerProvider provider,
        ISqlPreparationContext context,
        Func<ITableInfo, SqlTable> tableGenerator,
        OrderingExtractionPolicy orderingExtractionPolicy)
    {
      ArgumentUtility.CheckNotNull (nameof(fromExpression), fromExpression);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(generator), generator);
      ArgumentUtility.CheckNotNull (nameof(provider), provider);
      ArgumentUtility.CheckNotNull (nameof(context), context);

      var visitor = new SqlPreparationFromExpressionVisitor (generator, stage, provider, context, tableGenerator, orderingExtractionPolicy);
      visitor.Visit (fromExpression);
      if (visitor.FromExpressionInfo != null)
        return visitor.FromExpressionInfo.Value;

      var message = string.Format (
          "Error parsing expression '{0}'. Expressions of type '{1}' cannot be used as the SqlTables of a from clause.",
          fromExpression,
          fromExpression.Type.Name);
      throw new NotSupportedException (message);
    }

    private readonly UniqueIdentifierGenerator _generator;
    private readonly Func<ITableInfo, SqlTable> _tableGenerator;
    private readonly OrderingExtractionPolicy _orderingExtractionPolicy;

    protected SqlPreparationFromExpressionVisitor (
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        IMethodCallTransformerProvider provider,
        ISqlPreparationContext context,
        Func<ITableInfo, SqlTable> tableGenerator, 
        OrderingExtractionPolicy orderingExtractionPolicy)
        : base (context, stage, provider)
    {
      ArgumentUtility.CheckNotNull (nameof(generator), generator);
      ArgumentUtility.CheckNotNull (nameof(tableGenerator), tableGenerator);

      _generator = generator;
      _tableGenerator = tableGenerator;
      _orderingExtractionPolicy = orderingExtractionPolicy;

      FromExpressionInfo = null;
    }

    protected FromExpressionInfo? FromExpressionInfo { get; set; }

    protected UniqueIdentifierGenerator Generator
    {
      get { return _generator; }
    }

    public Func<ITableInfo, SqlTable> TableGenerator
    {
      get { return _tableGenerator; }
    }

    protected override Expression VisitConstant (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var itemType = ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (expression.Type, "from expression");
      var sqlTable = _tableGenerator (new UnresolvedTableInfo (itemType));
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      FromExpressionInfo = new FromExpressionInfo (sqlTable, new Ordering[0], sqlTableReferenceExpression, null);

      return sqlTableReferenceExpression;
    }

    protected override Expression VisitMember (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var preparedMemberExpression = (MemberExpression) TranslateExpression (expression, Context, Stage, MethodCallTransformerProvider);

      var joinInfo = new UnresolvedCollectionJoinInfo (preparedMemberExpression.Expression, preparedMemberExpression.Member);
      var joinedTable = new SqlJoinedTable (joinInfo, JoinSemantics.Inner);
      var oldStyleJoinedTable = _tableGenerator (joinedTable);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (oldStyleJoinedTable);
      FromExpressionInfo = new FromExpressionInfo (
          oldStyleJoinedTable, new Ordering[0], sqlTableReferenceExpression, new JoinConditionExpression (joinedTable));

      return sqlTableReferenceExpression;
    }

    public override Expression VisitSqlSubStatement (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var sqlStatement = expression.SqlStatement;

      var factory = new SqlPreparationSubStatementTableFactory (Stage, Context, _generator);
      FromExpressionInfo = factory.CreateSqlTableForStatement (sqlStatement, _tableGenerator, _orderingExtractionPolicy);
      Assertion.DebugAssert (FromExpressionInfo.Value.WhereCondition == null);

      return new SqlTableReferenceExpression (FromExpressionInfo.Value.SqlTable);
    }

    public Expression VisitSqlTableReference (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var tableInfo = new UnresolvedGroupReferenceTableInfo (expression.SqlTable);
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Inner);
      FromExpressionInfo = new FromExpressionInfo (sqlTable, new Ordering[0], new SqlTableReferenceExpression (sqlTable), null);

      return expression;
    }

    protected override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      var groupJoinClause = expression.ReferencedQuerySource as GroupJoinClause;
      if (groupJoinClause != null)
      {
        var fromExpressionInfo = AnalyzeFromExpression (
            groupJoinClause.JoinClause.InnerSequence,
            Stage,
            _generator,
            MethodCallTransformerProvider,
            Context,
            _tableGenerator,
            _orderingExtractionPolicy);

        Context.AddExpressionMapping (new QuerySourceReferenceExpression (groupJoinClause.JoinClause), fromExpressionInfo.ItemSelector);

        var whereCondition =
            Stage.PrepareWhereExpression (
                Expression.Equal (groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause.JoinClause.InnerKeySelector), Context);

        if (fromExpressionInfo.WhereCondition != null)
          whereCondition = Expression.AndAlso (fromExpressionInfo.WhereCondition, whereCondition);

        FromExpressionInfo = new FromExpressionInfo (
            fromExpressionInfo.SqlTable,
            fromExpressionInfo.ExtractedOrderings.ToArray(),
            fromExpressionInfo.ItemSelector,
            whereCondition);

        return new SqlTableReferenceExpression (fromExpressionInfo.SqlTable);
      }

      return base.VisitQuerySourceReference (expression);
    }

    Expression ISqlEntityRefMemberExpressionVisitor.VisitSqlEntityRefMember (SqlEntityRefMemberExpression expression)
    {
      return VisitExtension (expression);
    }
  }
}