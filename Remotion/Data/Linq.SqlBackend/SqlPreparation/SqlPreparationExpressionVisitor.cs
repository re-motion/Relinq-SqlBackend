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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        Expression expression, ISqlPreparationContext context, ISqlPreparationStage stage, MethodCallTransformerRegistry registry)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("registry", registry);

      var visitor = new SqlPreparationExpressionVisitor (context, stage, registry);
      var result = visitor.VisitExpression (expression);
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
        builder.Orderings.Clear();
        return new SqlSubStatementExpression (builder.GetSqlStatement());
      }
      return expression;
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // TODO Review 3005: Use an "as" cast to cast to PropertyInfo, then check for null (instead of "is PropertyInfo"); then use propertyInfo.GetGetMethod() to access the property getter; use that instead of GetMethod ("get_Length")
      if (expression.Member is PropertyInfo)
      {
        var methodInfo = expression.Member.DeclaringType.GetMethod ("get_Length");
        // TORO Review 3005: After the refactoring, avoid calling _registry.GetItem twice
        if (methodInfo != null && _registry.IsRegistered (methodInfo))
        {
          var tranformer = _registry.GetItem (methodInfo);
          var methodCallExpression = Expression.Call (expression.Expression, methodInfo);
          var tranformedExpression = tranformer.Transform (methodCallExpression);
          return VisitExpression (tranformedExpression);
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

      var transformedExpression = _registry.GetItem (expression.Method).Transform (expression);
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

      return CreateNewExpressionWithNamedArguments(expression, expression.Arguments.Select (e => VisitExpression (e)));
    }

    // TODO Review 2991: I refactored this method so that SubStatementReferenceResolver could use the same logic. Please move this to the NamedExpression class, then add unit tests for: calling this method with/without members, calling this method with arguments already named in the correct way (the resulting expression must be the same as the original one), also test the case where the members are property getters with/without already named arguments
    public static Expression CreateNewExpressionWithNamedArguments (NewExpression expression, IEnumerable<Expression> processedArguments)
    {
      var newArguments = processedArguments.Select ((e, i) => WrapIntoNamedExpression (GetMemberName (expression.Members, i), e)).ToArray ();
      if (!newArguments.SequenceEqual (expression.Arguments))
      {
        if (expression.Members != null)
          return Expression.New (expression.Constructor, newArguments, expression.Members);
        else
          return Expression.New (expression.Constructor, newArguments);
      }

      return expression;
    }

    private static Expression WrapIntoNamedExpression (string memberName, Expression argumentExpression)
    {
      // TODO Review 2991: This check doesn't work if memberName gets adjusted because its a property getter, see SqlPreparationExpressionVisitorTest.VisitNewExpression_PreventsNestedNamedExpressions_WhenAppliedTwice_WithGetterMethods - move this test to NamedExpressionTest when the method is moved)
      // TODO Review 2991: To fix this, change this as follows: First create the NamedExpression as below, then compare the name in the NamedExpression with the argument's name. If those are equal, return the original expression, otherwise the new one.
      var expressionAsNamedExpression = argumentExpression as NamedExpression;
      if (expressionAsNamedExpression != null && expressionAsNamedExpression.Name == memberName)
        return expressionAsNamedExpression;

      // TODO Review 2991: Change back to CreateFromMemberInfo, this should work now after my refactoring
      return NamedExpression.CreateFromMemberName (memberName, argumentExpression);
    }

    private static string GetMemberName (ReadOnlyCollection<MemberInfo> members, int index)
    {
      if (members == null || members.Count <= index)
        return "m" + index;
      return members[index].Name;
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