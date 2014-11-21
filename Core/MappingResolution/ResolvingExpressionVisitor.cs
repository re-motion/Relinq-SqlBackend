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
      ExpressionVisitor, 
      IUnresolvedSqlExpressionVisitor, 
      ISqlSubStatementVisitor,
      IJoinConditionExpressionVisitor,
      IUnresolvedJoinConditionExpressionVisitor, 
      IUnresolvedCollectionJoinConditionExpressionVisitor,
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
      var result1 = visitor1.Visit (expression);

      var visitor2 = new ResolvingExpressionVisitor (
          resolver, stage, context, generator, entityIdentityResolver, comparisonSplitter, namedExpressionCombiner, groupAggregateSimplifier, true);
      var result2 = visitor2.Visit (result1);
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

    public Expression VisitSqlTableReference (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedExpression = _stage.ResolveTableReferenceExpression (expression, _context);
      return Visit (resolvedExpression);
    }

    protected override Expression VisitConstant (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedExpression = _resolver.ResolveConstantExpression (expression);
      if (resolvedExpression != expression)
        return Visit (resolvedExpression);
      else
        return expression;
    }

    protected override Expression VisitMember (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // First process any nested expressions
      // E.g, for (kitchen.Cook).FirstName, first process kitchen => newExpression1 (SqlEntity)
      // then newExpression1.Cook => newExpression2 (SqlEntityRef/SqlEntity)
      // then newExpression2.FirstName => result (SqlColumn)

      var sourceExpression = Visit (expression.Expression);
      var resolved = _stage.ResolveMemberAccess (sourceExpression, expression.Member, _resolver, _context);

      Assertion.DebugAssert (resolved != expression);
      return Visit (resolved);
    }

    protected override Expression VisitBinary (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var baseVisitedExpression = (BinaryExpression) base.VisitBinary (expression);

      var binaryExpressionWithEntityComparisonResolved = _entityIdentityResolver.ResolvePotentialEntityComparison (baseVisitedExpression);
      var result = _compoundComparisonSplitter.SplitPotentialCompoundComparison (binaryExpressionWithEntityComparisonResolved);

      if (result != baseVisitedExpression)
        return Visit (result);

      return result;
    }

    protected override Expression VisitTypeBinary (TypeBinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = Visit (expression.Expression);
      var resolvedTypeExpression = _resolver.ResolveTypeCheck (newExpression, expression.TypeOperand);
      return Visit (resolvedTypeExpression);
    }

    public virtual Expression VisitSqlSubStatement (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedSqlStatement = _stage.ResolveSqlStatement (expression.SqlStatement, _context);
      var resolvedExpression = Equals (resolvedSqlStatement, expression.SqlStatement)
                                   ? expression
                                   : new SqlSubStatementExpression (resolvedSqlStatement);

      return _groupAggregateSimplifier.SimplifyIfPossible (resolvedExpression, expression.SqlStatement.SelectProjection);
    }

    public virtual Expression VisitJoinCondition (JoinConditionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resolvedLeftJoinInfo = expression.JoinedTable.JoinInfo.GetResolvedJoinInfo();
      return Visit (resolvedLeftJoinInfo.JoinCondition);
    }
      
    public Expression VisitUnresolvedJoinConditionExpression (UnresolvedJoinConditionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var joinedTableInfo = expression.JoinedTable.GetResolvedTableInfo();
      var resolvedJoinCondition = _resolver.ResolveJoinCondition (expression.OriginatingEntity, expression.MemberInfo, joinedTableInfo);
      return Visit (resolvedJoinCondition);
    }

    public Expression VisitUnresolvedCollectionJoinConditionExpression (UnresolvedCollectionJoinConditionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var originatingEntity = _context.GetOriginatingEntityForUnresolvedCollectionJoinTableInfo (expression.UnresolvedCollectionJoinTableInfo);
      var actualJoinConditionExpression = new UnresolvedJoinConditionExpression (
          originatingEntity,
          expression.UnresolvedCollectionJoinTableInfo.MemberInfo,
          expression.JoinedTable);
      return Visit (actualJoinConditionExpression);
    }

    public Expression VisitNamed (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var baseVisitedExpression =  (NamedExpression) VisitExtension (expression);

      var result = _namedExpressionCombiner.ProcessNames (baseVisitedExpression);

      if (result != baseVisitedExpression)
        return Visit (result);
      
      return baseVisitedExpression;
    }

    public Expression VisitSqlExists (SqlExistsExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var baseVisitedExpression = (SqlExistsExpression) VisitExtension (expression);

      // Within an EXISTS query, an entity can be replaced by its IdentityExpression, so try to simplify it.
      var newInnerExpression = _entityIdentityResolver.ResolvePotentialEntity (baseVisitedExpression.Expression);

      if (newInnerExpression != baseVisitedExpression.Expression)
        return Visit (new SqlExistsExpression (newInnerExpression));

      return baseVisitedExpression;
    }

    public Expression VisitSqlIn (SqlInExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var baseVisitedExpression = (SqlInExpression) VisitExtension (expression);

      var expressionWithSimplifiedEntities = _entityIdentityResolver.ResolvePotentialEntityComparison (baseVisitedExpression);

      if (expressionWithSimplifiedEntities != baseVisitedExpression)
        return Visit (expressionWithSimplifiedEntities);

      return baseVisitedExpression;
    }

    public Expression VisitSqlIsNull (SqlIsNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var baseVisitedExpression = (SqlIsNullExpression) VisitExtension (expression);

      var expressionWithEntityComparisonResolved = _entityIdentityResolver.ResolvePotentialEntityComparison (baseVisitedExpression);
      var result = _compoundComparisonSplitter.SplitPotentialCompoundComparison (expressionWithEntityComparisonResolved);

      if (baseVisitedExpression != result)
        return Visit (result);

      return baseVisitedExpression;
    }

    public Expression VisitSqlIsNotNull (SqlIsNotNullExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var baseVisitedExpression = (SqlIsNotNullExpression) VisitExtension (expression);

      var expressionWithEntityComparisonResolved = _entityIdentityResolver.ResolvePotentialEntityComparison (baseVisitedExpression);
      var result = _compoundComparisonSplitter.SplitPotentialCompoundComparison (expressionWithEntityComparisonResolved);

      if (baseVisitedExpression != result)
        return Visit (result);

      return baseVisitedExpression;
    }

    public virtual Expression VisitSqlEntityRefMember (SqlEntityRefMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!_resolveEntityRefMemberExpressions)
        return VisitExtension (expression);

      var unresolvedJoinInfo = new UnresolvedJoinInfo (expression.OriginatingEntity, expression.MemberInfo, JoinCardinality.One);
      // No revisiting required since this visitor does not handle ISqlEntityExpressions.
      return _stage.ResolveEntityRefMemberExpression (expression, unresolvedJoinInfo, _context);
    }
  }
}