// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingExpressionVisitor"/> analyzes a prepared <see cref="Expression"/> for things that need to be analyzed by the 
  /// <see cref="IMappingResolver"/> and resolves member accesses and similar structures. Substatements are recursively resolved.
  /// </summary>
  public class ResolvingExpressionVisitor : 
      ExpressionTreeVisitor, 
      IUnresolvedSqlExpressionVisitor, 
      ISqlSubStatementVisitor, 
      IJoinConditionExpressionVisitor
  {
    private readonly IMappingResolver _resolver;
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;
    private readonly UniqueIdentifierGenerator _generator;

    public static Expression ResolveExpression (
        Expression expression,
        IMappingResolver resolver,
        IMappingResolutionStage stage,
        IMappingResolutionContext context,
        UniqueIdentifierGenerator generator
        )
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("generator", generator);

      var visitor = new ResolvingExpressionVisitor (resolver, stage, context, generator);
      var result = visitor.VisitExpression (expression);
      return result;
    }

    protected IMappingResolver Resolver
    {
      get { return _resolver; }
    }

    protected IMappingResolutionStage Stage
    {
      get { return _stage; }
    }

    protected IMappingResolutionContext Context
    {
      get { return _context; }
    }

    protected UniqueIdentifierGenerator Generator
    {
      get { return _generator; }
    }

    protected ResolvingExpressionVisitor (
        IMappingResolver resolver, IMappingResolutionStage stage, IMappingResolutionContext context, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("generator", generator);

      _resolver = resolver;
      _stage = stage;
      _context = context;
      _generator = generator;
    }

    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedExpression = _stage.ResolveTableReferenceExpression (expression, _context);
      return VisitExpression (resolvedExpression);
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return _resolver.ResolveConstantExpression (expression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // First process any nested expressions
      // E.g, for (kitchen.Cook).FirstName, first process kitchen => newExpression1 (SqlEntity)
      // then newExpression1.Cook => newExpression2 (SqlEntityRef/SqlEntity)
      // then newExpression2.FirstName => result (SqlColumn)

      var sourceExpression = VisitExpression (expression.Expression);
      return _stage.ResolveMemberAccess (sourceExpression, expression.Member, _resolver, _context);
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      var newBinaryExpression = (BinaryExpression) base.VisitBinaryExpression (expression);

      // NewExpressions are compared by comparing them member-wise
      var leftExpressionAsNewExpression = newBinaryExpression.Left as NewExpression;
      var rightExpressionAsNewExpression = newBinaryExpression.Right as NewExpression;

      if (leftExpressionAsNewExpression != null && rightExpressionAsNewExpression != null)
      {
        return GetBinaryExpressionForNewExpressionComparison (
            expression.NodeType, leftExpressionAsNewExpression, rightExpressionAsNewExpression);
      }

      if (leftExpressionAsNewExpression != null)
        return GetBinaryExpressionForMemberAccessComparison (expression.NodeType, leftExpressionAsNewExpression, newBinaryExpression.Right);

      if (rightExpressionAsNewExpression != null)
        return GetBinaryExpressionForMemberAccessComparison (expression.NodeType, rightExpressionAsNewExpression, newBinaryExpression.Left);

      return newBinaryExpression;
    }

    protected override Expression VisitTypeBinaryExpression (TypeBinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = VisitExpression (expression.Expression);
      var resolvedTypeExpression = _resolver.ResolveTypeCheck (newExpression, expression.TypeOperand);
      return VisitExpression (resolvedTypeExpression);
    }

    public virtual Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedSqlStatement = _stage.ResolveSqlStatement (expression.SqlStatement, _context);

      return GroupAggregateSimplifier.SimplifyIfPossible (resolvedSqlStatement, expression.SqlStatement.SelectProjection, _stage, _context);
    }

    public virtual Expression VisitJoinConditionExpression (JoinConditionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedLeftJoinInfo = expression.JoinedTable.JoinInfo.GetResolvedJoinInfo();
      var whereExpression = ConversionUtility.MakeBinaryWithOperandConversion (
          ExpressionType.Equal,
          resolvedLeftJoinInfo.LeftKey,
          resolvedLeftJoinInfo.RightKey,
          false,
          null);
      return VisitExpression (whereExpression);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return VisitExtensionExpression (expression);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityConstantExpression (SqlEntityConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return VisitExtensionExpression (expression);
    }

    private Expression GetBinaryExpressionForNewExpressionComparison (ExpressionType expressionType, NewExpression leftNewExpression, NewExpression rightNewExpression)
    {
      if (!leftNewExpression.Constructor.Equals (rightNewExpression.Constructor))
      {
        var message = string.Format (
            "The results of constructor invocations can only be compared if the same constructors are used for both invocations. "
            + "Expressions: '{0}', '{1}'", 
            FormattingExpressionTreeVisitor.Format (leftNewExpression),
            FormattingExpressionTreeVisitor.Format (rightNewExpression));
        throw new NotSupportedException (message);
      }

      Expression binaryExpression = null;
      for (int i = 0; i < leftNewExpression.Arguments.Count; i++)
      {
        var argumentComparisonExpression = Expression.MakeBinary (expressionType, leftNewExpression.Arguments[i], rightNewExpression.Arguments[i]);

        if (binaryExpression == null)
          binaryExpression = argumentComparisonExpression;
        else
          binaryExpression = Expression.AndAlso (binaryExpression, argumentComparisonExpression);
      }
      return binaryExpression;
    }

    private Expression GetBinaryExpressionForMemberAccessComparison (ExpressionType expressionType, NewExpression newExpression, Expression memberAccessExpression)
    {
      // The ReSharper warning is wrong - newExpression.Members can be null
      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      if (newExpression.Members == null || newExpression.Members.Count == 0)
      // ReSharper restore ConditionIsAlwaysTrueOrFalse
      {
        var message = string.Format (
            "Compound values can only be compared if the respective constructor invocation has members associated with it. Expressions: '{0}', '{1}'",
            FormattingExpressionTreeVisitor.Format (newExpression),
            FormattingExpressionTreeVisitor.Format (memberAccessExpression));
        throw new NotSupportedException (message);
      }

      Expression binaryExpression = null;
      for (int i = 0; i < newExpression.Members.Count; i++)
      {
        Expression memberExpression;
        if (newExpression.Members[i] is MethodInfo)
          memberExpression = Expression.Call (memberAccessExpression, (MethodInfo) newExpression.Members[i]);
        else
          memberExpression = Expression.MakeMemberAccess (memberAccessExpression, newExpression.Members[i]);
        var argumentComparisonExpression = Expression.MakeBinary (
            expressionType,
            newExpression.Arguments[i],
            memberExpression);

        if (binaryExpression == null)
          binaryExpression = argumentComparisonExpression;
        else
          binaryExpression = Expression.AndAlso (binaryExpression, argumentComparisonExpression);
      }
      return PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (binaryExpression);
    }
  }
}