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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationExpressionVisitor"/> transforms the expressions stored by <see cref="SqlStatement"/> to a SQL-specific
  /// format.
  /// </summary>
  public class SqlPreparationExpressionVisitor : ExpressionTreeVisitor, ISqlSubStatementVisitor
  {
    private readonly ISqlPreparationContext _context;
    private readonly ISqlPreparationStage _stage;
    private readonly IMethodCallTransformerRegistry _registry;

    public static Expression TranslateExpression (
        Expression expression,
        ISqlPreparationContext context,
        ISqlPreparationStage stage,
        IMethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("registry", registry);

      var visitor = new SqlPreparationExpressionVisitor (context, stage, registry);
      var result = visitor.VisitExpression (expression);
      return result;
    }

    protected SqlPreparationExpressionVisitor (
        ISqlPreparationContext context, ISqlPreparationStage stage, IMethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("registry", registry);

      _context = context;
      _stage = stage;
      _registry = registry;
    }

    protected ISqlPreparationContext Context
    {
      get { return _context; }
    }

    protected ISqlPreparationStage Stage
    {
      get { return _stage; }
    }

    protected IMethodCallTransformerRegistry Registry
    {
      get { return _registry; }
    }

    public override Expression VisitExpression (Expression expression)
    {
      if (expression != null)
      {
        var replacementExpression = _context.GetExpressionMapping (expression);
        if (replacementExpression != null)
          expression = replacementExpression;
      }

      return base.VisitExpression (expression);
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.ReferencedQuerySource is GroupJoinClause)
      {
        var message = string.Format (
            "The results of a GroupJoin ('{0}') can only be used as a query source, for example, in a from expression. Expression: {1}",
            expression.ReferencedQuerySource.ItemName, FormattingExpressionTreeVisitor.Format(expression));
        throw new NotSupportedException (message);
      }
      else
      {
        var message = string.Format (
            "The expression '{0}' could not be found in the list of processed expressions. Probably, the feature declaring '{0}' isn't "
            + "supported yet. Expression: {1}",
            expression.Type.Name, FormattingExpressionTreeVisitor.Format(expression));
        throw new KeyNotFoundException (message);
      }
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = _stage.PrepareSqlStatement (expression.QueryModel, _context).CreateExpression();

      return VisitExpression (newExpression);
    }

    public virtual Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.SqlStatement.Orderings.Count > 0 && expression.SqlStatement.TopExpression == null)
      {
        var builder = new SqlStatementBuilder (expression.SqlStatement);
        builder.Orderings.Clear();
        return new SqlSubStatementExpression (builder.GetSqlStatement());
      }
      return expression;
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newInnerExpression = VisitExpression (expression.Expression);

      var innerExpressionAsConditionalExpression = newInnerExpression as ConditionalExpression;
      if (innerExpressionAsConditionalExpression != null)
      {
        var newConditionalExpression = Expression.Condition (
            innerExpressionAsConditionalExpression.Test, 
            Expression.MakeMemberAccess (innerExpressionAsConditionalExpression.IfTrue, expression.Member), 
            Expression.MakeMemberAccess (innerExpressionAsConditionalExpression.IfFalse, expression.Member));
        return VisitExpression (newConditionalExpression);
      }

      var innerExpressionAsBinaryExpression = newInnerExpression as BinaryExpression;
      if (innerExpressionAsBinaryExpression != null && innerExpressionAsBinaryExpression.NodeType == ExpressionType.Coalesce)
      {
        var newConditionalExpression = Expression.Condition (
            new SqlIsNotNullExpression (innerExpressionAsBinaryExpression.Left), 
            Expression.MakeMemberAccess (innerExpressionAsBinaryExpression.Left, expression.Member), 
            Expression.MakeMemberAccess (innerExpressionAsBinaryExpression.Right, expression.Member));
        return VisitExpression (newConditionalExpression);
      }

      var innerExpressionAsSqlSubStatementExpression = newInnerExpression as SqlSubStatementExpression;
      if (innerExpressionAsSqlSubStatementExpression != null)
      {
        var sqlStatementBuilder = new SqlStatementBuilder (innerExpressionAsSqlSubStatementExpression.SqlStatement);
        var namedExpression = (NamedExpression) sqlStatementBuilder.SelectProjection;
        sqlStatementBuilder.SelectProjection = new NamedExpression (
            namedExpression.Name, VisitExpression (Expression.MakeMemberAccess (namedExpression.Expression, expression.Member)));
        sqlStatementBuilder.RecalculateDataInfo (innerExpressionAsSqlSubStatementExpression.SqlStatement.SelectProjection);
        return new SqlSubStatementExpression (sqlStatementBuilder.GetSqlStatement());
      }

      var memberAsPropertyInfo = expression.Member as PropertyInfo;
      if (memberAsPropertyInfo != null)
      {
        var methodInfo = expression.Member.DeclaringType.GetMethod ("get_Length");
        if (methodInfo != null)
        {
          var methodCallExpression = Expression.Call (expression.Expression, methodInfo);
          var tranformer = _registry.GetTransformer(methodCallExpression);
          if (tranformer != null)
          {
            var tranformedExpression = tranformer.Transform (methodCallExpression);
            return VisitExpression (tranformedExpression);
          }
        }
      }
      return base.VisitMemberExpression (expression);
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

      var transformer = _registry.GetTransformer(expression);
      if (transformer != null)
      {
        var transformedExpression = transformer.Transform (expression);
        return VisitExpression (transformedExpression);
      }

      string message = string.Format (
          "The method '{0}.{1}' is not supported by this code generator, and no custom transformer has been registered. Expression: {2}",
          expression.Method.DeclaringType.FullName,
          expression.Method.Name,
          FormattingExpressionTreeVisitor.Format(expression));
      throw new NotSupportedException (message);
    }

    protected override Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return Expression.Condition (VisitExpression (expression.Test), VisitExpression (expression.IfTrue), VisitExpression (expression.IfFalse));
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return NamedExpression.CreateNewExpressionWithNamedArguments (expression, expression.Arguments.Select (e => VisitExpression (e)));
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