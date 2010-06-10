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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationExpressionVisitor"/> transforms the expressions stored by <see cref="SqlStatement.SelectProjection"/> to a SQL-specific
  /// format.
  /// </summary>
  public class SqlPreparationExpressionVisitor : ExpressionTreeVisitor, ISqlSubStatementVisitor
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
        var replacementExpression = _context.TryGetExpressionMapping (expression);
        if (replacementExpression != null)
          expression = replacementExpression;
      }

      return base.VisitExpression (expression);
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var message = string.Format (
          "The expression '{0}' could not be found in the list of processed expressions. Probably, the feature declaring '{0}' isn't "
          + "supported yet.",
          expression.Type.Name);
      throw new KeyNotFoundException (message);
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
        builder.Orderings.Clear ();
        return new SqlSubStatementExpression (builder.GetSqlStatement ());
      }
      return expression;
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
      var transformedExpression = _registry.GetItem (expression.Method).Transform ((MethodCallExpression) newExpression);
      return VisitExpression (transformedExpression);
    }

    protected override Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return new SqlCaseExpression (VisitExpression (expression.Test), VisitExpression (expression.IfTrue), VisitExpression (expression.IfFalse));
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // TODO Review 2744: Refactor this to make the operation idempotent (ie, calling the visitor on an expression that has already been prepared should not perform useless changes)
      // TODO Review 2744: In this case, we'll keep adding unnecessary NamedExpressions. Add a unit test showing that this is not the case (should fail), then change by checking whether the result of VisitExpression(e) is already a NamedExpression with the right name before wrapping it. Should probably be extracted into a separate method.
      var sqlSelectNewExpression = Expression.New (
          expression.Constructor,
          expression.Arguments.Select ((e, i) => new NamedExpression (expression.Members[i].Name, VisitExpression (e))).ToArray(),
          expression.Members);
      return sqlSelectNewExpression;
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