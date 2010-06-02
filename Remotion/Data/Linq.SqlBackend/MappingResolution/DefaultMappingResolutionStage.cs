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
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
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

    public virtual Expression ResolveSelectExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolveExpression (expression, context);
      return ApplyContext (resolvedExpression, SqlExpressionContext.ValueRequired, context);
    }

    public virtual Expression ResolveWhereExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolveExpression (expression, context);
      return ApplyContext (resolvedExpression, SqlExpressionContext.PredicateRequired, context);
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

    public virtual IResolvedTableInfo ResolveTableInfo (ITableInfo tableInfo, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedTableInfo = ResolvingTableInfoVisitor.ResolveTableInfo (tableInfo, _resolver, _uniqueIdentifierGenerator, this, context);
      return (IResolvedTableInfo) ApplyContext (resolvedTableInfo, SqlExpressionContext.ValueRequired, context);
    }

    public virtual ResolvedJoinInfo ResolveJoinInfo (IJoinInfo joinInfo, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedJoinInfo = ResolvingJoinInfoVisitor.ResolveJoinInfo (joinInfo, _resolver, _uniqueIdentifierGenerator, this, context);
      return (ResolvedJoinInfo) ApplyContext (resolvedJoinInfo, SqlExpressionContext.ValueRequired, context);
    }

    public virtual SqlStatement ResolveSqlStatement (SqlStatement sqlStatement, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("context", context);
      
      return SqlStatementResolver.ResolveExpressions (this, sqlStatement, context);
    }

    public virtual SqlEntityExpression ResolveCollectionSourceExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var resolvedExpression = ResolveExpression (expression, context);
      return (SqlEntityExpression) ApplyContext (resolvedExpression, SqlExpressionContext.ValueRequired, context);
    }

    public virtual SqlEntityExpression ResolveEntityRefMemberExpression (SqlEntityRefMemberExpression expression, IJoinInfo joinInfo, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("context", context);

      var originatingSqlTable = context.GetSqlTableForEntityExpression (expression.OriginatingEntity);
      var join = originatingSqlTable.GetOrAddLeftJoin (joinInfo, expression.MemberInfo);
      join.JoinInfo = ResolveJoinInfo (join.JoinInfo, context);
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (join);
      
      return (SqlEntityExpression) ResolveExpression (sqlTableReferenceExpression, context);
    }

    public Expression ResolveTableReferenceExpression (SqlTableReferenceExpression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      return SqlTableReferenceResolver.ResolveTableReference (expression, _resolver, _uniqueIdentifierGenerator, context);
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

    public virtual IJoinInfo ApplyContext (IJoinInfo joinInfo, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      return SqlContextJoinInfoVisitor.ApplyContext (joinInfo, expressionContext, this, mappingResolutionContext);
    }

    protected virtual Expression ResolveExpression (Expression expression, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      return ResolvingExpressionVisitor.ResolveExpression (expression, _resolver, _uniqueIdentifierGenerator, this, context);
    }
  }
}