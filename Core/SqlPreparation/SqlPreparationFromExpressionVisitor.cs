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
  /// Analyzes the <see cref="FromClauseBase.FromExpression"/> of a <see cref="FromClauseBase"/> and returns a <see cref="SqlTable"/> that 
  /// represents the data source of the <see cref="FromClauseBase"/>, together with other data held by a <see cref="SqlPreparation.FromExpressionInfo"/>.
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
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("provider", provider);
      ArgumentUtility.CheckNotNull ("context", context);

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
    // TODO RMLNQSQL-78: Remove.
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
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("tableGenerator", tableGenerator);

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
      ArgumentUtility.CheckNotNull ("expression", expression);

      var itemType = ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (expression.Type, "from expression");
      var sqlTable = _tableGenerator (new UnresolvedTableInfo (itemType));
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      
      FromExpressionInfo = new FromExpressionInfo (
          new SqlAppendedTable (sqlTable, JoinSemantics.Inner),
          new Ordering[0],
          sqlTableReferenceExpression,
          null);

      return sqlTableReferenceExpression;
    }

    protected override Expression VisitMember (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // This is the scenario "from oi in o.OrderItems" or "from oi in p.Order.OrderItems" or something like this. Note that we haven't resolved the
      // left side of the member expression yet - it could be a SqlTableReferenceExpression, a MemberExpression, a SubStatementExpression. Those
      // things will be moved into a SqlTable only in the resolution stage, so we can't really add a join to anything yet.
      // We'll therefore generate an old-style join instead: add a cross-joined table corresponding to the right side of the join and a where 
      // condition that constitutes the join condition.

      // IDEA: To generate a real inner join instead, refactor as follows:
      //   SqlTable GetSqlTable (MemberExpression expression)
      //   1 - Prepare the left side of the expression. 
      //   2a - If it is a SqlTableReferenceExpression, take that SqlTable. Otherwise, it must be a MemberExpression or a a SubStatementExpression.
      //       (Throw on anything else.) Since that table already existed, "null" will be returned in the FromExpressionInfo.
      //   2b - If a SubStatementExpression, put the sub-statement into a new SqlTable and take that. Remember this new SqlTable: It must be returned 
      //       in the FromExpressionInfo so that it is added to the list of SqlTables in the statement.
      //       (Or throw because we don't support sub-statements on the left side of a MemberExpression in the from expression.)
      //   2c - If a MemberExpression, recurse: GetSqlTable((MemberExpression) left side). Take the returned table.
      //   3 - After step 2, you have one table corresponding to the left side. Add the inner join to it. Note that this is not necessarily a 
      //       "collection join"; due to the recursion in 2c, it could also be a "single join". Therefore, refactor and rename 
      //       UnresolvedCollectionJoinTableInfo and its resolution to cope with that and hold a join cardinality or something. Can probably be 
      //       deduced from enumerability of the member result, or it can be made a parameter to the GetSqlTable function.
      //       That table is only returned via the FromExpressionInfo if it was newly created (in step 2b if you support it).
      //       Otherwise, null is returned. Ensure all callers can deal with null tables.
      //       Remove the FromExpressionInfo.WhereCondition, it's no longer needed.
      
      var preparedMemberExpression = (MemberExpression) TranslateExpression (expression, Context, Stage, MethodCallTransformerProvider);

      var joinTableInfo = new UnresolvedCollectionJoinTableInfo (preparedMemberExpression.Expression, preparedMemberExpression.Member);
      var oldStyleJoinedTable = _tableGenerator (joinTableInfo);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (oldStyleJoinedTable);

      FromExpressionInfo = new FromExpressionInfo (
          appendedTable: new SqlAppendedTable (oldStyleJoinedTable, JoinSemantics.Inner),
          extractedOrderings: new Ordering[0],
          itemSelector: sqlTableReferenceExpression,
          whereCondition: new UnresolvedCollectionJoinConditionExpression (oldStyleJoinedTable));

      return sqlTableReferenceExpression;
    }

    public override Expression VisitSqlSubStatement (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var sqlStatement = expression.SqlStatement;

      var factory = new SqlPreparationSubStatementTableFactory (Stage, Context, _generator);
      FromExpressionInfo = factory.CreateSqlTableForStatement (sqlStatement, _tableGenerator, _orderingExtractionPolicy);
      Assertion.IsTrue (FromExpressionInfo.Value.WhereCondition == null);

      return new SqlTableReferenceExpression (FromExpressionInfo.Value.AppendedTable.SqlTable);
    }

    public Expression VisitSqlTableReference (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var tableInfo = new UnresolvedGroupReferenceTableInfo (expression.SqlTable);
      var sqlTable = new SqlTable (tableInfo);
      FromExpressionInfo = new FromExpressionInfo (
          new SqlAppendedTable (sqlTable, JoinSemantics.Inner),
          new Ordering[0],
          new SqlTableReferenceExpression (sqlTable),
          null);

      return expression;
    }

    protected override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      // TODO RMLNQSQL-77: Actually, wouldn't it be really easy to simply detect DefaultIfEmpty here and return a SqlAppendedTable with outer ...
      // semantics _instead_ of adding a null if empty table? Consider this...
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
            fromExpressionInfo.AppendedTable,
            fromExpressionInfo.ExtractedOrderings.ToArray(),
            fromExpressionInfo.ItemSelector,
            whereCondition);

        return new SqlTableReferenceExpression (fromExpressionInfo.AppendedTable.SqlTable);
      }

      return base.VisitQuerySourceReference (expression);
    }

    Expression ISqlEntityRefMemberExpressionVisitor.VisitSqlEntityRefMember (SqlEntityRefMemberExpression expression)
    {
      return VisitExtension (expression);
    }
  }
}