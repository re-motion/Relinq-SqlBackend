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

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="IMappingResolver"/> provides methods to resolve expressions with database-specific information delivered by an O/R mapper. This
  /// interface is implemented by LINQ providers making use of the re-linq SQL backend.
  /// </summary>
  public interface IMappingResolver
  {
    /// <summary>
    /// Takes an <see cref="UnresolvedTableInfo"/> and an <see cref="UniqueIdentifierGenerator"/> 
    /// to generate an <see cref="IResolvedTableInfo"/> that represents the table in the database.
    /// </summary>
    /// <param name="tableInfo">The <see cref="UnresolvedTableInfo"/> which is to be resolved.</param>
    /// <param name="generator">A <see cref="UniqueIdentifierGenerator"/> that can be used to generate unique identifiers such as table aliases.</param>
    /// <returns>An <see cref="IResolvedTableInfo"/> instance representing the  <paramref name="tableInfo"/> in the database.</returns>
    /// <exception cref="UnmappedItemException">The given <see cref="UnresolvedTableInfo"/> cannot be resolved to a mapped database item.</exception>
    IResolvedTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator);

    /// <summary>
    /// Takes an <see cref="UnresolvedJoinInfo"/> and an <see cref="UniqueIdentifierGenerator"/> to generate a 
    /// <see cref="ResolvedJoinInfo"/> that represents the join in the database.
    /// </summary>
    /// <param name="joinInfo">The <see cref="UnresolvedJoinInfo"/> which is to be resolved.</param>
    /// <param name="generator">A <see cref="UniqueIdentifierGenerator"/> that can be used to generate unique identifiers such as table aliases.</param>
    /// <returns>An instance of <see cref="ResolvedJoinInfo"/> representing the <paramref name="joinInfo"/> in the database.</returns>
    /// <exception cref="UnmappedItemException">The given <see cref="UnresolvedJoinInfo"/> cannot be resolved to a mapped database item.</exception>
    ResolvedJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo, UniqueIdentifierGenerator generator);

    /// <summary>
    /// Analyses the <see cref="SqlTableReferenceExpression"/> and returns a resolved version of the expression. The resolved version will usually
    /// be a <see cref="SqlEntityExpression"/> representing the entity described by the <paramref name="tableReferenceExpression"/> in the database. 
    /// If the item type of the table is not a queryable entity, the resolver should return a <see cref="SqlValueTableReferenceExpression"/>.
    /// </summary>
    /// <param name="tableReferenceExpression">A <see cref="SqlTableReferenceExpression"/> which has to be resolved. 
    /// The expression represents a reference to an entity retrieved from a <see cref="SqlTableBase"/>.</param>
    /// <param name="generator">A <see cref="UniqueIdentifierGenerator"/> that can be used to generate unique identifiers such as column aliases.</param>
    /// <returns>A resolved version of <paramref name="tableReferenceExpression"/>, usually a <see cref="SqlEntityExpression"/> containing all the 
    /// columns of the referenced <see cref="SqlTableBase"/>. If the <see cref="SqlTableReferenceExpression"/> is not a queryable entity, 
    /// the resolver has to return a <see cref="SqlValueTableReferenceExpression"/>. 
    /// This method can return a partial result that itself again needs to be resolved.</returns>
    Expression ResolveTableReferenceExpression (SqlTableReferenceExpression tableReferenceExpression, UniqueIdentifierGenerator generator);

    /// <summary>
    /// Analyses the <see cref="SqlMemberExpression"/> and returns a resolved version of the expression. The resolved version will usually
    /// be a <see cref="SqlColumnExpression"/> representing the member described by the <paramref name="memberExpression"/> in the database, or a
    /// <see cref="SqlEntityExpression"/> if the member references another entity.
    /// </summary>
    /// <param name="memberExpression">The <see cref="SqlMemberExpression"/> which is to be resolved. 
    /// The expression represents a reference to the member of an entity.</param>
    /// <param name="generator">A <see cref="UniqueIdentifierGenerator"/> that can be used to generate unique identifiers such as column aliases.</param>
    /// <returns>A resolved version of <paramref name="memberExpression"/>, usually a <see cref="SqlColumnExpression"/> if the member is resolved to a 
    /// simple column, or a <see cref="SqlEntityExpression"/> if the member references another entity.
    /// This method can return a partial result that itself again needs to be resolved.</returns>
    /// <exception cref="UnmappedItemException">The given <see cref="SqlMemberExpression"/> cannot be resolved to a mapped database item.</exception>
    Expression ResolveMemberExpression (SqlMemberExpression memberExpression, UniqueIdentifierGenerator generator);

    /// <summary>
    /// Analyses the given <see cref="ConstantExpression"/> and resolves it to a database-compatible expression if necessary. For example, if the 
    /// constant value is another entity, this method should return a <see cref="SqlEntityConstantExpression"/>.
    /// </summary>
    /// <param name="constantExpression">The <see cref="ConstantExpression"/> to be analyzed.</param>
    /// <returns>A resolved version of <paramref name="constantExpression"/>, usually a <see cref="SqlEntityConstantExpression"/>, or the
    /// <paramref name="constantExpression"/> itself.</returns>
    Expression ResolveConstantExpression (ConstantExpression constantExpression);
  }
}