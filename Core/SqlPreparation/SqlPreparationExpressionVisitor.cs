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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationExpressionVisitor"/> transforms the expressions stored by <see cref="SqlStatement"/> to a SQL-specific
  /// format.
  /// </summary>
  public class SqlPreparationExpressionVisitor : RelinqExpressionVisitor, ISqlSubStatementVisitor, IPartialEvaluationExceptionExpressionVisitor
  {
    private readonly ISqlPreparationContext _context;
    private readonly ISqlPreparationStage _stage;
    private readonly IMethodCallTransformerProvider _methodCallTransformerProvider;

    public static Expression TranslateExpression (
        Expression expression,
        ISqlPreparationContext context,
        ISqlPreparationStage stage,
        IMethodCallTransformerProvider provider)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);
      ArgumentUtility.CheckNotNull (nameof(context), context);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(provider), provider);

      var visitor = new SqlPreparationExpressionVisitor (context, stage, provider);
      var result = visitor.Visit (expression);
      return result;
    }

    protected SqlPreparationExpressionVisitor (
        ISqlPreparationContext context, ISqlPreparationStage stage, IMethodCallTransformerProvider provider)
    {
      ArgumentUtility.CheckNotNull (nameof(context), context);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);
      ArgumentUtility.CheckNotNull (nameof(provider), provider);

      _context = context;
      _stage = stage;
      _methodCallTransformerProvider = provider;
    }

    protected ISqlPreparationContext Context
    {
      get { return _context; }
    }

    protected ISqlPreparationStage Stage
    {
      get { return _stage; }
    }

    protected IMethodCallTransformerProvider MethodCallTransformerProvider
    {
      get { return _methodCallTransformerProvider; }
    }

    public override Expression Visit (Expression expression)
    {
      if (expression != null)
      {
        var replacementExpression = _context.GetExpressionMapping (expression);
        if (replacementExpression != null)
          expression = replacementExpression;
      }

      return base.Visit (expression);
    }

    protected override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

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
            "The expression declaring identifier '{0}' could not be found in the list of processed expressions. Probably, the feature declaring '{0}' "
            + "isn't supported yet.",
            expression.ReferencedQuerySource.ItemName);
        throw new KeyNotFoundException (message);
      }
    }

    protected override Expression VisitSubQuery (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var newExpression = _stage.PrepareSqlStatement (expression.QueryModel, _context).CreateExpression();

      return Visit (newExpression);
    }

    public virtual Expression VisitSqlSubStatement (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (expression.SqlStatement.Orderings.Count > 0 && expression.SqlStatement.TopExpression == null)
      {
        var builder = new SqlStatementBuilder (expression.SqlStatement);
        builder.Orderings.Clear();
        return new SqlSubStatementExpression (builder.GetSqlStatement());
      }
      return expression;
    }

    protected override Expression VisitMember (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var newInnerExpression = Visit (expression.Expression);

      var innerExpressionAsSqlCaseExpression = newInnerExpression as SqlCaseExpression;
      if (innerExpressionAsSqlCaseExpression != null)
      {
        var originalCases = innerExpressionAsSqlCaseExpression.Cases;
        var originalElseCase = innerExpressionAsSqlCaseExpression.ElseCase;
        var newCases = originalCases.Select (c => new SqlCaseExpression.CaseWhenPair (c.When, Expression.MakeMemberAccess (c.Then, expression.Member)));
        var newElseCase = originalElseCase != null ? Expression.MakeMemberAccess (originalElseCase, expression.Member) : null;
        // If there is no else case, ensure that the resulting type is nullable
        var caseExpressionType =
            newElseCase == null && expression.Type.IsValueType && Nullable.GetUnderlyingType (expression.Type) == null
                ? typeof (Nullable<>).MakeGenericType (expression.Type)
                : expression.Type;
        var newSqlCaseExpression = new SqlCaseExpression (caseExpressionType, newCases, newElseCase);
        return Visit (newSqlCaseExpression);
      }

      if (newInnerExpression.NodeType == ExpressionType.Coalesce)
      {
        var innerExpressionAsBinaryExpression = (BinaryExpression) newInnerExpression;
        var newConditionalExpression = Expression.Condition (
            new SqlIsNotNullExpression (innerExpressionAsBinaryExpression.Left), 
            Expression.MakeMemberAccess (innerExpressionAsBinaryExpression.Left, expression.Member), 
            Expression.MakeMemberAccess (innerExpressionAsBinaryExpression.Right, expression.Member));
        return Visit (newConditionalExpression);
      }

      var innerExpressionAsSqlSubStatementExpression = newInnerExpression as SqlSubStatementExpression;
      if (innerExpressionAsSqlSubStatementExpression != null)
      {
        var sqlStatementBuilder = new SqlStatementBuilder (innerExpressionAsSqlSubStatementExpression.SqlStatement);
        var namedExpression = (NamedExpression) sqlStatementBuilder.SelectProjection;
        sqlStatementBuilder.SelectProjection = new NamedExpression (
            namedExpression.Name, Visit (Expression.MakeMemberAccess (namedExpression.Expression, expression.Member)));
        sqlStatementBuilder.RecalculateDataInfo (innerExpressionAsSqlSubStatementExpression.SqlStatement.SelectProjection);
        return new SqlSubStatementExpression (sqlStatementBuilder.GetSqlStatement());
      }

      var memberAsPropertyInfo = expression.Member as PropertyInfo;
      if (memberAsPropertyInfo != null)
      {
        var methodInfo = memberAsPropertyInfo.GetGetMethod();
        if (methodInfo != null)
        {
          var methodCallExpression = Expression.Call (expression.Expression, methodInfo);
          var tranformer = _methodCallTransformerProvider.GetTransformer(methodCallExpression);
          if (tranformer != null)
          {
            var tranformedExpression = tranformer.Transform (methodCallExpression);
            return Visit (tranformedExpression);
          }
        }
      }
      return base.VisitMember (expression);
    }

    protected override Expression VisitBinary (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (IsNullConstant (expression.Left))
      {
        if (expression.NodeType == ExpressionType.Equal)
          return Visit (new SqlIsNullExpression (expression.Right));
        else if (expression.NodeType == ExpressionType.NotEqual)
          return Visit (new SqlIsNotNullExpression (expression.Right));
        else
          return base.VisitBinary (expression);
      }
      else if (IsNullConstant (expression.Right))
      {
        if (expression.NodeType == ExpressionType.Equal)
          return Visit (new SqlIsNullExpression (expression.Left));
        else if (expression.NodeType == ExpressionType.NotEqual)
          return Visit (new SqlIsNotNullExpression (expression.Left));
        else
          return base.VisitBinary (expression);
      }
      else
        return base.VisitBinary (expression);
    }

    protected override Expression VisitMethodCall (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var transformer = _methodCallTransformerProvider.GetTransformer(expression);
      if (transformer != null)
      {
        var transformedExpression = transformer.Transform (expression);
        return Visit (transformedExpression);
      }

      var namedInstance = expression.Object != null ? NamedExpression.CreateFromMemberName ("Object", Visit (expression.Object)) : null;
      var namedArguments = expression.Arguments.Select ((a, i) => (Expression) NamedExpression.CreateFromMemberName ("Arg" + i, Visit (a)));
      return Expression.Call (namedInstance, expression.Method, namedArguments);
    }

    protected override Expression VisitConditional (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      return SqlCaseExpression.CreateIfThenElse (
          expression.Type, Visit (expression.Test), Visit (expression.IfTrue), Visit (expression.IfFalse));
    }

    protected override Expression VisitNew (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      return NamedExpression.CreateNewExpressionWithNamedArguments (expression, expression.Arguments.Select (Visit));
    }

    public virtual Expression VisitPartialEvaluationException (PartialEvaluationExceptionExpression partialEvaluationExceptionExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(partialEvaluationExceptionExpression), partialEvaluationExceptionExpression);
      
      return Visit (partialEvaluationExceptionExpression.EvaluatedExpression);
    }

    protected override Expression VisitConstant (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (IsSingleValue(expression.Value))
        return base.VisitConstant (expression);

      if (expression.Value is ICollection || IsGenericCollection(expression.Value))
      {
        return new ConstantCollectionExpression ((IEnumerable)expression.Value);
      }

      // expression.Value is an unevaluated IEnumerable
      throw new NotSupportedException (
          "Only collection can be used as a parameter. "
          + "Use an object that implements the ICollection, ICollection<T>, or IReadOnlyCollection<T> interface "
          + "instead of a sequence based on IEnumerable.");

      bool IsSingleValue (object value)
      {
        return value is not IEnumerable || value is string or char[] or byte[];
      }

      bool IsGenericCollection (object value)
      {
        return value
            .GetType()
            .GetInterfaces()
            .Select (i => i.IsGenericType ? i.GetGenericTypeDefinition() : null)
            .Any (t => t == typeof (IReadOnlyCollection<>) || t == typeof (ICollection<>));
      }
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
