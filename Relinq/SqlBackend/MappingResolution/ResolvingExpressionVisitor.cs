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
using System.Linq.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingExpressionVisitor"/> analyzes a prepared <see cref="Expression"/> for things that need to be analyzed by the 
  /// <see cref="IMappingResolver"/> and resolves member accesses and similar structures. Substatements are recursively resolved.
  /// Calling <see cref="ResolveExpression"/> will automatically execute two passes in order to optimize away unnecessary left-outer joins.
  /// </summary>
  public class ResolvingExpressionVisitor : 
      ExpressionTreeVisitor, 
      IUnresolvedSqlExpressionVisitor, 
      ISqlSubStatementVisitor, 
      IJoinConditionExpressionVisitor,
      INamedExpressionVisitor,
      ISqlNullCheckExpressionVisitor,
      ISqlInExpressionVisitor,
      ISqlExistsExpressionVisitor
  {
    public static Expression ResolveExpression (
        Expression expression,
        IMappingResolver resolver,
        IMappingResolutionStage stage,
        IMappingResolutionContext context,
        UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("generator", generator);

      var entityIdentityResolver = new EntityIdentityResolver (stage, resolver, context);
      var comparisonSplitter = new CompoundExpressionComparisonSplitter();
      var namedExpressionCombiner = new NamedExpressionCombiner (context);
      var groupAggregateSimplifier = new GroupAggregateSimplifier (stage, context);

      var visitor1 = new ResolvingExpressionVisitor (
          resolver, stage, context, generator, entityIdentityResolver, comparisonSplitter, namedExpressionCombiner, groupAggregateSimplifier, false);
      var result1 = visitor1.VisitExpression (expression);

      var visitor2 = new ResolvingExpressionVisitor (
          resolver, stage, context, generator, entityIdentityResolver, comparisonSplitter, namedExpressionCombiner, groupAggregateSimplifier, true);
      var result2 = visitor2.VisitExpression (result1);
      return result2;
    }

    private readonly IMappingResolver _resolver;
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly IEntityIdentityResolver _entityIdentityResolver;
    private readonly ICompoundExpressionComparisonSplitter _compoundComparisonSplitter;
    private readonly INamedExpressionCombiner _namedExpressionCombiner;
    private readonly IGroupAggregateSimplifier _groupAggregateSimplifier;

    private readonly bool _resolveEntityRefMemberExpressions;
    
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

    protected IEntityIdentityResolver EntityIdentityResolver
    {
      get { return _entityIdentityResolver; }
    }

    protected ICompoundExpressionComparisonSplitter CompoundComparisonSplitter
    {
      get { return _compoundComparisonSplitter; }
    }

    protected INamedExpressionCombiner NamedExpressionCombiner
    {
      get { return _namedExpressionCombiner; }
    }

    public IGroupAggregateSimplifier GroupAggregateSimplifier
    {
      get { return _groupAggregateSimplifier; }
    }

    protected bool ResolveEntityRefMemberExpressions
    {
      get { return _resolveEntityRefMemberExpressions; }
    }

    protected ResolvingExpressionVisitor (
        IMappingResolver resolver,
        IMappingResolutionStage stage,
        IMappingResolutionContext context,
        UniqueIdentifierGenerator generator,
        IEntityIdentityResolver entityIdentityResolver,
        ICompoundExpressionComparisonSplitter compoundComparisonSplitter,
        INamedExpressionCombiner namedExpressionCombiner,
        IGroupAggregateSimplifier groupAggregateSimplifier,
        bool resolveEntityRefMemberExpressions)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("entityIdentityResolver", entityIdentityResolver);
      ArgumentUtility.CheckNotNull ("compoundComparisonSplitter", compoundComparisonSplitter);
      ArgumentUtility.CheckNotNull ("namedExpressionCombiner", namedExpressionCombiner);
      ArgumentUtility.CheckNotNull ("groupAggregateSimplifier", groupAggregateSimplifier);
      
      _resolver = resolver;
      _stage = stage;
      _context = context;
      _generator = generator;
      _entityIdentityResolver = entityIdentityResolver;
      _compoundComparisonSplitter = compoundComparisonSplitter;
      _namedExpressionCombiner = namedExpressionCombiner;
      _groupAggregateSimplifier = groupAggregateSimplifier;

      _resolveEntityRefMemberExpressions = resolveEntityRefMemberExpressions;
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

      var resolvedExpression = _resolver.ResolveConstantExpression (expression);
      if (resolvedExpression != expression)
        return VisitExpression (resolvedExpression);
      else
        return expression;
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // First process any nested expressions
      // E.g, for (kitchen.Cook).FirstName, first process kitchen => newExpression1 (SqlEntity)
      // then newExpression1.Cook => newExpression2 (SqlEntityRef/SqlEntity)
      // then newExpression2.FirstName => result (SqlColumn)

      var sourceExpression = VisitExpression (expression.Expression);
      var resolved = _stage.ResolveMemberAccess (sourceExpression, expression.Member, _resolver, _context);

      Assertion.DebugAssert (resolved != expression);
      return VisitExpression (resolved);
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      var baseVisitedExpression = (BinaryExpression) base.VisitBinaryExpression (expression);

      var binaryExpressionWithEntityComparisonResolved = _entityIdentityResolver.ResolvePotentialEntityComparison (baseVisitedExpression);
      var result = _compoundComparisonSplitter.SplitPotentialCompoundComparison (binaryExpressionWithEntityComparisonResolved);

      if (result != baseVisitedExpression)
        return VisitExpression (result);

      return result;
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
      var resolvedExpression = Equals (resolvedSqlStatement, expression.SqlStatement)
                                   ? expression
                                   : new SqlSubStatementExpression (resolvedSqlStatement);

      return _groupAggregateSimplifier.SimplifyIfPossible (resolvedExpression, expression.SqlStatement.SelectProjection);
    }

    public virtual Expression VisitJoinConditionExpression (JoinConditionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedLeftJoinInfo = expression.JoinedTable.JoinInfo.GetResolvedJoinInfo();
      return VisitExpression (resolvedLeftJoinInfo.JoinCondition);
    }

    public Expression VisitNamedExpression (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var baseVisitedExpression =  (NamedExpression) VisitExtensionExpression (expression);

      var result = _namedExpressionCombiner.ProcessNames (baseVisitedExpression);

      if (result != baseVisitedExpression)
        return VisitExpression (result);
      
      return baseVisitedExpression;
    }

    public Expression VisitSqlExistsExpression (SqlExistsExpression expression)
    {
      var baseVisitedExpression = (SqlExistsExpression) VisitExtensionExpression (expression);

      // Within an EXISTS query, an entity can be replaced by its IdentityExpression, so try to simplify it.
      var newInnerExpression = _entityIdentityResolver.ResolvePotentialEntity (baseVisitedExpression.Expression);

      if (newInnerExpression != baseVisitedExpression.Expression)
        return VisitExpression (new SqlExistsExpression (newInnerExpression));

      return baseVisitedExpression;
    }

    public Expression VisitSqlInExpression (SqlInExpression expression)
    {
      var baseVisitedExpression = (SqlInExpression) VisitExtensionExpression (expression);

      var expressionWithSimplifiedEntities = _entityIdentityResolver.ResolvePotentialEntityComparison (baseVisitedExpression);

      if (expressionWithSimplifiedEntities != baseVisitedExpression)
        return VisitExpression (expressionWithSimplifiedEntities);

      return baseVisitedExpression;
    }

    public Expression VisitSqlIsNullExpression (SqlIsNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var baseVisitedExpression = (SqlIsNullExpression) base.VisitExtensionExpression (expression);

      var expressionWithEntityComparisonResolved = _entityIdentityResolver.ResolvePotentialEntityComparison (baseVisitedExpression);
      var result = _compoundComparisonSplitter.SplitPotentialCompoundComparison (expressionWithEntityComparisonResolved);

      if (baseVisitedExpression != result)
        return VisitExpression (result);

      return baseVisitedExpression;
    }

    public Expression VisitSqlIsNotNullExpression (SqlIsNotNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var baseVisitedExpression = (SqlIsNotNullExpression) base.VisitExtensionExpression (expression);

      var expressionWithEntityComparisonResolved = _entityIdentityResolver.ResolvePotentialEntityComparison (baseVisitedExpression);
      var result = _compoundComparisonSplitter.SplitPotentialCompoundComparison (expressionWithEntityComparisonResolved);

      if (baseVisitedExpression != result)
        return VisitExpression (result);

      return baseVisitedExpression;
    }

    public virtual Expression VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!_resolveEntityRefMemberExpressions)
        return VisitExtensionExpression (expression);

      var unresolvedJoinInfo = new UnresolvedJoinInfo (expression.OriginatingEntity, expression.MemberInfo, JoinCardinality.One);
      // No revisiting required since this visitor does not handle ISqlEntityExpressions.
      return _stage.ResolveEntityRefMemberExpression (expression, unresolvedJoinInfo, _context);
    }
  }
}