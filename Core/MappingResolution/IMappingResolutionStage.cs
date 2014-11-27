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

namespace Remotion.Linq.SqlBackend.MappingResolution
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
    void ResolveSqlJoinedTable (SqlJoinedTable sqlJoinedTable, IMappingResolutionContext context);
    Expression ResolveJoinCondition (Expression joinCondition, IMappingResolutionContext mappingResolutionContext);
    SqlStatement ResolveSqlStatement (SqlStatement sqlStatement, IMappingResolutionContext context);
    Expression ResolveCollectionSourceExpression (Expression expression, IMappingResolutionContext context);
    SqlEntityExpression ResolveEntityRefMemberExpression (SqlEntityRefMemberExpression expression, IMappingResolutionContext context);
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