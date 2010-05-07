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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationExpressionVisitor"/> transforms the expressions stored by <see cref="SqlStatement.SelectProjection"/> to a SQL-specific
  /// format.
  /// </summary>
  public class SqlPreparationExpressionVisitor : ExpressionTreeVisitor
  {
    private readonly ISqlPreparationContext _context;
    private readonly ISqlPreparationStage _stage;
    private readonly MethodCallTransformerRegistry _registry;

    public static Expression TranslateExpression (
        Expression projection, ISqlPreparationContext context, ISqlPreparationStage stage, MethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("projection", projection);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("registry", registry);

      var visitor = new SqlPreparationExpressionVisitor (context, stage, registry);
      var result = visitor.VisitExpression (projection);
      return result;
    }

    protected SqlPreparationExpressionVisitor (ISqlPreparationContext context, ISqlPreparationStage stage, MethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("registry", registry);

      _context = context;
      _stage = stage;
      _registry = registry;
    }

    public override Expression VisitExpression (Expression expression)
    {
      // TODO Review 2691: Refactor as follows:
      // - move error message from GetContextMapping out to VisitQuerySourceReferenceExpression; change GetContextMapping to return null if none found
      // - use GetContextMapping here (not TryGetContextMappingFromHierarchy)
      // - in VisitQuerySourceReferenceExpression, remove the context lookup - it should already have happened here. Only leave the error message in - whenever the VisitQuerySourceReferenceExpression method is executed, that is an error
      if (expression != null)
      {
        var replacementExpression = _context.TryGetContextMappingFromHierarchy (expression);
        if (replacementExpression != null) // TODO Review 2691: Actually, the visitor is not required - replacementExpression is already a replacement for expression (if it is not null)
          expression = ReplacingExpressionTreeVisitor.Replace (expression, replacementExpression, expression);
      }

      return base.VisitExpression (expression);
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var referencedTable = (SqlTableReferenceExpression)_context.GetContextMapping (expression);
      return new SqlTableReferenceExpression (referencedTable.SqlTable);
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      var sqlStatement = _stage.PrepareSqlStatement (expression.QueryModel, _context);
      return sqlStatement.SqlTables.Count == 0 && sqlStatement.AggregationModifier == AggregationModifier.None && !sqlStatement.IsDistinctQuery
                 ? sqlStatement.SelectProjection
                 : new SqlSubStatementExpression (sqlStatement);
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (IsNullConstant (expression.Left))
      {
        if (expression.NodeType == ExpressionType.Equal)
          return VisitExpression (new SqlIsNullExpression (expression.Right));
        else if (expression.NodeType == ExpressionType.NotEqual)
          return VisitExpression (new SqlIsNotNullExpression (expression.Right));
        else
          return base.VisitBinaryExpression (expression);
      }
      else if (IsNullConstant (expression.Right))
      {
        if (expression.NodeType == ExpressionType.Equal)
          return VisitExpression (new SqlIsNullExpression (expression.Left));
        else if (expression.NodeType == ExpressionType.NotEqual)
          return VisitExpression (new SqlIsNotNullExpression (expression.Left));
        else
          return base.VisitBinaryExpression (expression);
      }
      else
        return base.VisitBinaryExpression (expression);
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = base.VisitMethodCallExpression (expression);
      var transformedExpression = _registry.GetItem(expression.Method).Transform ((MethodCallExpression) newExpression);
      return VisitExpression (transformedExpression);
    }

    protected override Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return new SqlCaseExpression (VisitExpression (expression.Test), VisitExpression (expression.IfTrue), VisitExpression (expression.IfFalse));
    }

    private bool IsNullConstant (Expression expression)
    {
      var constantExpression = expression as ConstantExpression;
      if (constantExpression != null)
      {
        if (constantExpression.Value == null)
          return true;
      }
      return false;
    }
  }
}