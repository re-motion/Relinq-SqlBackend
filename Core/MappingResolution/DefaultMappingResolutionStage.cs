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
using System.Reflection;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Provides a default implementation of <see cref="IMappingResolutionStage"/>.
  /// </summary>
  public class DefaultMappingResolutionStage : IMappingResolutionStage
  {
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _uniqueIdentifierGenerator;

    public DefaultMappingResolutionStage (IMappingResolver resolver, UniqueIdentifierGenerator uniqueIdentifierGenerator)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("uniqueIdentifierGenerator", uniqueIdentifierGenerator);
      
      _uniqueIdentifierGenerator = uniqueIdentifierGenerator;
      _resolver = resolver;
    }

    public IMappingResolver Resolver
    {
      get { return _resolver; }
    }

    public UniqueIdentifierGenerator UniqueIdentifierGenerator
    {
      get { return _uniqueIdentifierGenerator; }
    }

    public virtual Expression ResolveSelectExpression (Expression expression, SqlStatementBuilder sqlStatementBuilder, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolvingSelectExpressionVisitor.ResolveExpression (expression, _resolver, this, context, _uniqueIdentifierGenerator, sqlStatementBuilder);
      return ApplyContext (resolvedExpression, SqlExpressionContext.ValueRequired, context);
    }

    public virtual Expression ResolveWhereExpression (Expression expression,  IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolveExpression (expression, context);
      return ApplyContext (resolvedExpression, SqlExpressionContext.PredicateRequired, context);
    }

    public Expression ResolveGroupByExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolveExpression (expression, context);
      return ApplyContext (resolvedExpression, SqlExpressionContext.ValueRequired, context);
    }

    public virtual Expression ResolveOrderingExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolveExpression (expression, context);
      return ApplyContext (resolvedExpression, SqlExpressionContext.SingleValueRequired, context);
    }

    public virtual Expression ResolveTopExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolveExpression (expression, context);
      return ApplyContext (resolvedExpression, SqlExpressionContext.SingleValueRequired, context);
    }

    public Expression ResolveAggregationExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolveExpression (expression, context);
      return ApplyContext (resolvedExpression, SqlExpressionContext.ValueRequired, context);
    }

    public virtual IResolvedTableInfo ResolveTableInfo (ITableInfo tableInfo, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedTableInfo = ResolvingTableInfoVisitor.ResolveTableInfo (tableInfo, _resolver, _uniqueIdentifierGenerator, this, context);
      return (IResolvedTableInfo) ApplyContext (resolvedTableInfo, SqlExpressionContext.ValueRequired, context);
    }

    // TODO RMLNQSQL-64: Remove from stage interface? If so, merge tests for ResolveJoinCondition and tests for ResolveEntityRefMemberExpression.
    public Expression ResolveJoinCondition (Expression joinCondition, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("joinCondition", joinCondition);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      var resolvedJoinCondition = ResolveExpression (joinCondition, mappingResolutionContext);
      return ApplyContext (resolvedJoinCondition, SqlExpressionContext.PredicateRequired, mappingResolutionContext);
    }

    public virtual SqlStatement ResolveSqlStatement (SqlStatement sqlStatement, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("context", context);
      
      return SqlStatementResolver.ResolveExpressions (this, sqlStatement, context);
    }

    public virtual Expression ResolveCollectionSourceExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolveExpression (expression, context);
      return ApplyContext (resolvedExpression, SqlExpressionContext.ValueRequired, context);
    }

    public virtual SqlEntityExpression ResolveEntityRefMemberExpression (SqlEntityRefMemberExpression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var originatingSqlTable = context.GetSqlTableForEntityExpression (expression.OriginatingEntity);
      var join = originatingSqlTable.GetOrAddLeftJoinByMember (
          expression.MemberInfo,
          () =>
          {
            var unresolvedJoinTableInfo = new UnresolvedJoinTableInfo (expression.OriginatingEntity, expression.MemberInfo, JoinCardinality.One);
            var joinedTable = new SqlTable (unresolvedJoinTableInfo, JoinSemantics.Inner);
            var joinCondition = new UnresolvedJoinConditionExpression (expression.OriginatingEntity, expression.MemberInfo, joinedTable);
            return new SqlJoin (joinedTable, JoinSemantics.Left, joinCondition);
          });

      join.JoinedTable.TableInfo = ResolveTableInfo (join.JoinedTable.TableInfo, context);
      // TODO RMLNQSQL-64: This assignment breaks immutability of SqlJoin. Is there a better idea? E.g., originatingSqlTable.ReplaceJoin(old, new)?
      join.JoinCondition = ResolveJoinCondition (join.JoinCondition, context);

      var sqlTableReferenceExpression = new SqlTableReferenceExpression (join.JoinedTable);
      return (SqlEntityExpression) ResolveExpression (sqlTableReferenceExpression, context);
    }

    public Expression ResolveTableReferenceExpression (SqlTableReferenceExpression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      return expression.SqlTable.GetResolvedTableInfo ().ResolveReference (expression.SqlTable, _resolver, context, _uniqueIdentifierGenerator);
    }

    public Expression ResolveMemberAccess (Expression resolvedSourceExpression, MemberInfo memberInfo, IMappingResolver mappingResolver, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("resolvedSourceExpression", resolvedSourceExpression);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);
      ArgumentUtility.CheckNotNull ("mappingResolver", mappingResolver);
      ArgumentUtility.CheckNotNull ("context", context);

      return MemberAccessResolver.ResolveMemberAccess (resolvedSourceExpression, memberInfo, mappingResolver, this, context);
    }

    public virtual Expression ApplyContext (Expression expression, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      return SqlContextExpressionVisitor.ApplySqlExpressionContext (expression, expressionContext, this, mappingResolutionContext);
    }

    public virtual SqlStatement ApplySelectionContext (SqlStatement sqlStatement, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      return SqlContextSelectionAdjuster.ApplyContext (sqlStatement, expressionContext, this, mappingResolutionContext);
    }

    public virtual ITableInfo ApplyContext (ITableInfo tableInfo, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      return SqlContextTableInfoVisitor.ApplyContext (tableInfo, expressionContext, this, mappingResolutionContext);
    }

    protected virtual Expression ResolveExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      return ResolvingExpressionVisitor.ResolveExpression (expression, _resolver, this, context, _uniqueIdentifierGenerator);
    }
  }
}