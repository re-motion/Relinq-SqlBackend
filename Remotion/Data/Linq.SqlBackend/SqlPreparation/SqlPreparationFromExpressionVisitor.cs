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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
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
        MethodCallTransformerRegistry registry,
        ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("registry", registry);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new SqlPreparationFromExpressionVisitor (generator, stage, registry, context);
      visitor.VisitExpression (fromExpression);
      if (visitor._fromExpressionInfo != null)
        return visitor._fromExpressionInfo.Value;

      var message = string.Format (
          "Error parsing expression '{0}'. Expressions of type '{1}' cannot be used as the SqlTables of a from clause.",
          FormattingExpressionTreeVisitor.Format (fromExpression),
          fromExpression.Type.Name);
      throw new NotSupportedException (message);
    }

    // TODO 2880: Refactor - move to separate class, split into smaller methods
    public static FromExpressionInfo CreateSqlTableForSubStatement (
        SqlStatement sqlStatement,
        ISqlPreparationStage sqlPreparationStage,
        ISqlPreparationContext context,
        UniqueIdentifierGenerator generator,
        Func<ResolvedSubStatementTableInfo, SqlTableBase> tableCreator)
    {
      SqlTableBase sqlTable;
      Expression itemSelector;
      var extractedOrderings = new List<Ordering>();

      if (sqlStatement.Orderings.Count > 0)
      {
        Expression newSelectProjection = Expression.Constant (null);
        Type tupleType;

        for (var i = sqlStatement.Orderings.Count - 1; i >= 0; --i)
        {
          tupleType = typeof (KeyValuePair<,>).MakeGenericType (sqlStatement.Orderings[i].Expression.Type, newSelectProjection.Type);
          newSelectProjection =
              Expression.New (
                  tupleType.GetConstructors()[0],
                  new[] { sqlStatement.Orderings[i].Expression, newSelectProjection },
                  new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });
        }

        tupleType = typeof (KeyValuePair<,>).MakeGenericType (sqlStatement.SelectProjection.Type, newSelectProjection.Type);
        newSelectProjection = Expression.New (
            tupleType.GetConstructors()[0],
            new[] { sqlStatement.SelectProjection, newSelectProjection },
            new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });

        newSelectProjection = sqlPreparationStage.PrepareSelectExpression (newSelectProjection, context);

        var builder = new SqlStatementBuilder (sqlStatement) { SelectProjection = newSelectProjection };
        if (sqlStatement.TopExpression == null)
          builder.Orderings.Clear();
        builder.RecalculateDataInfo (sqlStatement.SelectProjection);
        var newSqlStatement = builder.GetSqlStatement();

        var tableInfo = new ResolvedSubStatementTableInfo (generator.GetUniqueIdentifier ("q"), newSqlStatement);
        sqlTable = tableCreator (tableInfo);
        itemSelector = Expression.MakeMemberAccess (new SqlTableReferenceExpression (sqlTable), newSelectProjection.Type.GetProperty ("Key"));

        var currentOrderingTuple = Expression.MakeMemberAccess (
            new SqlTableReferenceExpression (sqlTable), newSelectProjection.Type.GetProperty ("Value"));
        for (var i = 0; i < sqlStatement.Orderings.Count; ++i)
        {
          extractedOrderings.Add (
              new Ordering (
                  Expression.MakeMemberAccess (currentOrderingTuple, currentOrderingTuple.Type.GetProperty ("Key")),
                  sqlStatement.Orderings[i].OrderingDirection));
          currentOrderingTuple = Expression.MakeMemberAccess (currentOrderingTuple, currentOrderingTuple.Type.GetProperty ("Value"));
        }
      }
      else
      {
        var tableInfo = new ResolvedSubStatementTableInfo (generator.GetUniqueIdentifier ("q"), sqlStatement);
        sqlTable = tableCreator (tableInfo);
        itemSelector = new SqlTableReferenceExpression (sqlTable);
      }
      return new FromExpressionInfo (sqlTable, extractedOrderings.ToArray(), itemSelector, null, true);
    }

    private readonly UniqueIdentifierGenerator _generator;
    private readonly ISqlPreparationStage _stage;
    private readonly MethodCallTransformerRegistry _registry;
    private readonly ISqlPreparationContext _context;

    private FromExpressionInfo? _fromExpressionInfo;

    protected SqlPreparationFromExpressionVisitor (
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        MethodCallTransformerRegistry registry,
        ISqlPreparationContext context)
        : base (context, stage, registry)
    {
      ArgumentUtility.CheckNotNull ("generator", generator);

      _generator = generator;
      _stage = stage;
      _registry = registry;
      _context = context;
      _fromExpressionInfo = null;
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var itemType = ReflectionUtility.GetItemTypeOfIEnumerable (expression.Type, "from expression");
      var sqlTable = new SqlTable (new UnresolvedTableInfo (itemType));
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      _fromExpressionInfo = new FromExpressionInfo (sqlTable, new Ordering[0], sqlTableReferenceExpression, null, true);

      return sqlTableReferenceExpression;
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var preparedMemberExpression = (MemberExpression) TranslateExpression (expression, _context, _stage, _registry);

      var joinInfo = new UnresolvedCollectionJoinInfo (preparedMemberExpression.Expression, preparedMemberExpression.Member);
      var joinedTable = new SqlJoinedTable (joinInfo, JoinSemantics.Inner);
      var oldStyleJoinedTable = new SqlTable (joinedTable);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (oldStyleJoinedTable);
      _fromExpressionInfo = new FromExpressionInfo (
          oldStyleJoinedTable, new Ordering[0], sqlTableReferenceExpression, new JoinConditionExpression (joinedTable), true);

      return sqlTableReferenceExpression;
    }

    public override Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var sqlStatement = expression.SqlStatement;
      _fromExpressionInfo = CreateSqlTableForSubStatement (sqlStatement, Stage, Context, _generator, info => new SqlTable (info));
      Debug.Assert (_fromExpressionInfo.Value.WhereCondition == null);

      return new SqlTableReferenceExpression (_fromExpressionInfo.Value.SqlTable);
    }

    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _fromExpressionInfo = new FromExpressionInfo (expression.SqlTable, new Ordering[0], expression, null, false);

      return expression;
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      return base.VisitUnknownExpression (expression);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityConstantExpression (SqlEntityConstantExpression expression)
    {
      return base.VisitUnknownExpression (expression);
    }
  }
}