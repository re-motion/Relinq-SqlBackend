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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
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
        IMethodCallTransformerRegistry registry,
        ISqlPreparationContext context,
        Func<ITableInfo, SqlTableBase> tableGenerator)
    {
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("registry", registry);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new SqlPreparationFromExpressionVisitor (generator, stage, registry, context, tableGenerator);
      visitor.VisitExpression (fromExpression);
      if (visitor._fromExpressionInfo != null)
        return visitor._fromExpressionInfo.Value;

      var message = string.Format (
          "Error parsing expression '{0}'. Expressions of type '{1}' cannot be used as the SqlTables of a from clause.",
          FormattingExpressionTreeVisitor.Format (fromExpression),
          fromExpression.Type.Name);
      throw new NotSupportedException (message);
    }

    private readonly UniqueIdentifierGenerator _generator;
    private readonly ISqlPreparationContext _context;

    private FromExpressionInfo? _fromExpressionInfo;
    private readonly Func<ITableInfo, SqlTableBase> _tableGenerator;

    protected FromExpressionInfo? FromExpressionInfo
    {
      get { return _fromExpressionInfo; }
    }

    protected SqlPreparationFromExpressionVisitor (
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        IMethodCallTransformerRegistry registry,
        ISqlPreparationContext context,
        Func<ITableInfo, SqlTableBase> tableGenerator)
        : base (context, stage, registry)
    {
      _generator = generator;
      _context = context;
      _fromExpressionInfo = null;
      _tableGenerator = tableGenerator;
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var itemType = ReflectionUtility.GetItemTypeOfIEnumerable (expression.Type, "from expression");
      var sqlTable = _tableGenerator (new UnresolvedTableInfo (itemType));
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      _fromExpressionInfo = new FromExpressionInfo (sqlTable, new Ordering[0], sqlTableReferenceExpression, null);

      return sqlTableReferenceExpression;
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var preparedMemberExpression = (MemberExpression) TranslateExpression (expression, _context, Stage, Registry);

      var joinInfo = new UnresolvedCollectionJoinInfo (preparedMemberExpression.Expression, preparedMemberExpression.Member);
      var joinedTable = new SqlJoinedTable (joinInfo, JoinSemantics.Inner);
      var oldStyleJoinedTable = _tableGenerator (joinedTable);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (oldStyleJoinedTable);
      _fromExpressionInfo = new FromExpressionInfo (
          oldStyleJoinedTable, new Ordering[0], sqlTableReferenceExpression, new JoinConditionExpression (joinedTable));

      return sqlTableReferenceExpression;
    }

    public override Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var sqlStatement = expression.SqlStatement;

      var selectProjectionAsSqlGroupingSelectExpression = ((NamedExpression) sqlStatement.SelectProjection).Expression as SqlGroupingSelectExpression;
      if (selectProjectionAsSqlGroupingSelectExpression != null
          && selectProjectionAsSqlGroupingSelectExpression.ElementExpression.Type == typeof (bool))
      {
        throw new NotSupportedException (
            string.Format (
                "It is not currently supported to use boolean values as a query source, eg., in the from clause of a query. Expression: {0}",
                FormattingExpressionTreeVisitor.Format (expression)));
      }

      var factory = new SqlPreparationSubStatementTableFactory (Stage, _context, _generator);
      _fromExpressionInfo = factory.CreateSqlTableForStatement (sqlStatement, _tableGenerator);
      Debug.Assert (_fromExpressionInfo.Value.WhereCondition == null);

      return new SqlTableReferenceExpression (_fromExpressionInfo.Value.SqlTable);
    }

    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var tableInfo = new UnresolvedGroupReferenceTableInfo (expression.SqlTable);
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Inner);
      _fromExpressionInfo = new FromExpressionInfo (sqlTable, new Ordering[0], new SqlTableReferenceExpression (sqlTable), null);

      return expression;
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      var groupJoinClause = expression.ReferencedQuerySource as GroupJoinClause;
      if (groupJoinClause != null)
      {
        var fromExpressionInfo = AnalyzeFromExpression (
            groupJoinClause.JoinClause.InnerSequence,
            Stage,
            _generator,
            Registry,
            _context,
            _tableGenerator);

        _context.AddExpressionMapping (new QuerySourceReferenceExpression (groupJoinClause.JoinClause), fromExpressionInfo.ItemSelector);

        var whereCondition =
            Stage.PrepareWhereExpression (
                Expression.Equal (groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause.JoinClause.InnerKeySelector), _context);

        if (fromExpressionInfo.WhereCondition != null)
          whereCondition = Expression.AndAlso (fromExpressionInfo.WhereCondition, whereCondition);

        _fromExpressionInfo = new FromExpressionInfo (
            fromExpressionInfo.SqlTable,
            fromExpressionInfo.ExtractedOrderings.ToArray(),
            fromExpressionInfo.ItemSelector,
            whereCondition);

        return new SqlTableReferenceExpression (fromExpressionInfo.SqlTable);
      }

      return base.VisitQuerySourceReferenceExpression (expression);
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