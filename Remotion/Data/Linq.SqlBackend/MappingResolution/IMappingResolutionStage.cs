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
using System.Reflection;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Provides entry points for all transformations that occur during the mapping resolution phase.
  /// </summary>
  public interface IMappingResolutionStage
  {
    Expression ResolveSelectExpression (Expression expression, SqlStatementBuilder sqlStatementBuilder, IMappingResolutionContext context);
    Expression ResolveWhereExpression (Expression expression, IMappingResolutionContext context);
    Expression ResolveGroupByExpression (Expression expression, IMappingResolutionContext context);
    Expression ResolveOrderingExpression (Expression expression, IMappingResolutionContext context);
    Expression ResolveTopExpression (Expression expression, IMappingResolutionContext context);
    Expression ResolveAggregationExpression (Expression expression, IMappingResolutionContext context);
    IResolvedTableInfo ResolveTableInfo (ITableInfo tableInfo, IMappingResolutionContext context);
    ResolvedJoinInfo ResolveJoinInfo (IJoinInfo joinInfo, IMappingResolutionContext context);
    SqlStatement ResolveSqlStatement (SqlStatement sqlStatement, IMappingResolutionContext context);
    Expression ResolveCollectionSourceExpression (Expression expression, IMappingResolutionContext context);
    SqlEntityExpression ResolveEntityRefMemberExpression (SqlEntityRefMemberExpression expression, IJoinInfo joinInfo, IMappingResolutionContext context);
    Expression ResolveTableReferenceExpression (SqlTableReferenceExpression expression, IMappingResolutionContext context);
    Expression ResolveMemberAccess (
        Expression resolvedSourceExpression,
        MemberInfo memberInfo,
        IMappingResolver mappingResolver,
        IMappingResolutionContext context);

    Expression ApplyContext (Expression expression, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext);
    ITableInfo ApplyContext (ITableInfo tableInfo, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext);
    IJoinInfo ApplyContext (IJoinInfo tableInfo, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext);

    SqlStatement ApplySelectionContext (SqlStatement sqlStatement, SqlExpressionContext expressionContext, IMappingResolutionContext mappingResolutionContext);
  }
}