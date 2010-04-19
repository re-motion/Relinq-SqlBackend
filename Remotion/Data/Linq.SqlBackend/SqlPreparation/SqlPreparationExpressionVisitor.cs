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
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing;
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
    private readonly SqlPreparationContext _context;
    private readonly ISqlPreparationStage _stage;
    private readonly MethodCallTransformerRegistry _registry;

    public static Expression TranslateExpression (
        Expression projection, SqlPreparationContext context, ISqlPreparationStage stage, MethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("projection", projection);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("registry", registry);

      var visitor = new SqlPreparationExpressionVisitor (context, stage, registry);
      var result = visitor.VisitExpression (projection);
      return result;
    }

    protected SqlPreparationExpressionVisitor (SqlPreparationContext context, ISqlPreparationStage stage, MethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("registry", registry);

      _context = context;
      _stage = stage;
      _registry = registry;
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var referencedTable = _context.GetSqlTableForQuerySource (expression.ReferencedQuerySource);
      return new SqlTableReferenceExpression (referencedTable);
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      var lastOperatorIndex = expression.QueryModel.ResultOperators.Count - 1;
      var containsOperator = lastOperatorIndex >= 0 ? expression.QueryModel.ResultOperators[lastOperatorIndex] as ContainsResultOperator : null;
      if (containsOperator != null)
      {
        var fromExpression = expression.QueryModel.MainFromClause.FromExpression as ConstantExpression;

        // Check whether the query applies Contains to a constant collection
        if (expression.QueryModel.IsIdentityQuery() && (fromExpression!=null) && typeof (ICollection).IsAssignableFrom (fromExpression.Type))
        {
          if (expression.QueryModel.ResultOperators.Count > 1)
            throw new NotSupportedException ("Expression with more than one results operators are not allowed when using contains.");

          // TODO Review 2582: Move this to the "then" part of the following if statement
          var preparedItemExpression = _stage.PrepareItemExpression (containsOperator.Item);
          
          if (((ICollection)fromExpression.Value).Count > 0)
            return new SqlBinaryOperatorExpression ("IN", preparedItemExpression, fromExpression);
          else
            return Expression.Constant (false);
        }

        var preparedSqlStatement = _stage.PrepareSqlStatement (expression.QueryModel);

        // PrepareSqlStatement will handle the contains operator by putting an "IN" expression into the select projection
        Debug.Assert (
            preparedSqlStatement.SqlTables.Count == 0
            && preparedSqlStatement.WhereCondition == null
            && preparedSqlStatement.Orderings.Count == 0
            && !preparedSqlStatement.IsCountQuery
            && !preparedSqlStatement.IsDistinctQuery
            && preparedSqlStatement.TopExpression == null);

        return preparedSqlStatement.SelectProjection;
      }

      var sqlStatement = _stage.PrepareSqlStatement (expression.QueryModel);
      return new SqlSubStatementExpression (sqlStatement, expression.Type);
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
      var transformedExpression = _registry.GetTransformer (expression.Method).Transform ((MethodCallExpression) newExpression);
      return VisitExpression (transformedExpression);
    }

    protected override Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return new SqlCaseExpression (VisitExpression(expression.Test), VisitExpression(expression.IfTrue), VisitExpression(expression.IfFalse));
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