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
    private readonly MethodCallTransformerRegistry _registry;

    public static Expression TranslateExpression (
        Expression expression,
        ISqlPreparationContext context,
        ISqlPreparationStage stage,
        MethodCallTransformerRegistry registry)
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
        ISqlPreparationContext context, ISqlPreparationStage stage, MethodCallTransformerRegistry registry)
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

    protected MethodCallTransformerRegistry Registry
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
            "The results of a GroupJoin ('{0}') can only be used as a query source, for example, in a from expression.",
            expression.ReferencedQuerySource.ItemName);
        throw new NotSupportedException (message);
      }
      else
      {
        var message = string.Format (
            "The expression '{0}' could not be found in the list of processed expressions. Probably, the feature declaring '{0}' isn't "
            + "supported yet.",
            expression.Type.Name);
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

    private Expression CreateAndRevisitConditionalExpression(Expression testPredicate, Expression thenValue, Expression elseValue)
    {
      var newConditionalExpression = Expression.Condition (testPredicate, thenValue, elseValue);
      return VisitExpression (newConditionalExpression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newInnerExpression = VisitExpression (expression.Expression);

      var innerExpressionAsConditionalExpression = newInnerExpression as ConditionalExpression;
      if (innerExpressionAsConditionalExpression!=null)
        return CreateAndRevisitConditionalExpression (
            innerExpressionAsConditionalExpression.Test,
            Expression.MakeMemberAccess (innerExpressionAsConditionalExpression.IfTrue, expression.Member),
            Expression.MakeMemberAccess (innerExpressionAsConditionalExpression.IfFalse, expression.Member));

      var innerExpressionAsBinaryExpression = newInnerExpression as BinaryExpression;
      if (innerExpressionAsBinaryExpression != null && innerExpressionAsBinaryExpression.NodeType == ExpressionType.Coalesce)
        return CreateAndRevisitConditionalExpression (
            new SqlIsNotNullExpression (innerExpressionAsBinaryExpression.Left),
            Expression.MakeMemberAccess (innerExpressionAsBinaryExpression.Left, expression.Member),
            Expression.MakeMemberAccess (innerExpressionAsBinaryExpression.Right, expression.Member));

      var memberAsPropertyInfo = expression.Member as PropertyInfo;
      if (memberAsPropertyInfo!=null)
      {
        var methodInfo = expression.Member.DeclaringType.GetMethod ("get_Length");
        if (methodInfo != null)
        {
          var tranformer = _registry.GetItem (methodInfo);
          if (tranformer != null)
          {
            var methodCallExpression = Expression.Call (expression.Expression, methodInfo);
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

      var transformer = _registry.GetItem (expression.Method);
      if (transformer != null)
      {
        var transformedExpression = transformer.Transform (expression);
        return VisitExpression (transformedExpression);
      }

      string message = string.Format (
          "The method '{0}.{1}' is not supported by this code generator, and no custom transformer has been registered.",
          expression.Method.DeclaringType.FullName,
          expression.Method.Name);
      throw new NotSupportedException (message);
    }

    protected override Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return Expression.Condition(VisitExpression (expression.Test), VisitExpression (expression.IfTrue), VisitExpression (expression.IfFalse));
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