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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="IMappingResolutionContext"/> provides methods to handle a concrete mapping resolution context.
  /// </summary>
  // TODO: Consider removing this interface and keeping only the implementation
  public interface IMappingResolutionContext
  {
    void AddSqlEntityMapping (SqlEntityExpression entityExpression, SqlTable sqlTable);
    void AddGroupReferenceMapping (SqlGroupingSelectExpression expression, SqlTable table);
    SqlTable GetSqlTableForEntityExpression (SqlEntityExpression entityExpression);
    SqlTable GetReferencedGroupSource (SqlGroupingSelectExpression groupingSelectExpression);
    SqlEntityExpression UpdateEntityAndAddMapping (SqlEntityExpression entityExpression, Type itemType, string tableAlias, string newName);
    SqlGroupingSelectExpression UpdateGroupingSelectAndAddMapping (
        SqlGroupingSelectExpression expression, Expression newKey, Expression newElement, IEnumerable<Expression> aggregations);
    void AddSqlTable (SqlAppendedTable appendedTable, SqlStatementBuilder sqlStatementBuilder);
    Expression RemoveNamesAndUpdateMapping (Expression expression);

    void AddOriginatingEntityMappingForUnresolvedCollectionJoinTableInfo (
        UnresolvedCollectionJoinTableInfo unresolvedCollectionJoinTableInfo,
        SqlEntityExpression resolvedOriginatingEntity);
    SqlEntityExpression GetOriginatingEntityForUnresolvedCollectionJoinTableInfo (UnresolvedCollectionJoinTableInfo unresolvedCollectionJoinTableInfo);
  }
}