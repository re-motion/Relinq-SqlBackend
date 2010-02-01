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
using System.Reflection;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq.Backend
{
  /// <summary>
  /// Provides an interface for classes representing the databases to re-linq's SQL backend. Usually, implementations of this interface will be
  /// linked to an O/R mapping infrastructure that knows how types and properties are mapped to tables and columns.
  /// </summary>
  public interface IDatabaseInfo
  {
    /// <summary>
    /// Creates a <see cref="Table"/> object for the query source represented by the given <see cref="FromClauseBase"/>.
    /// </summary>
    /// <param name="fromClause">The clause for which to get a <see cref="Table"/> object.</param>
    /// <param name="alias">The alias to assign to the table.</param>
    /// <returns>
    /// A <see cref="Table"/> object for the <paramref name="fromClause"/>.
    /// </returns>
    /// <exception cref="UnmappedItemException">The given <paramref name="fromClause"/> cannot be mapped to a <see cref="Table"/>.</exception>
    Table GetTableForFromClause (FromClauseBase fromClause, string alias);

    /// <summary>
    /// Determines whether the specified member identifies a mapped relation.
    /// </summary>
    /// <param name="member">The member to check.</param>
    /// <returns>
    /// 	<see langword="true"/> if the specified member identifies a mapped relation; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsRelationMember (MemberInfo member);

    /// <summary>
    /// Creates a <see cref="Table"/> object for the related table represented by the member identified by the given <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="relationMember">The <see cref="MemberInfo"/> identifying the relation.</param>
    /// <param name="alias">The alias to assign to the table.</param>
    /// <returns>
    /// A <see cref="Table"/> object for the given <paramref name="relationMember"/>.
    /// </returns>
    /// <exception cref="UnmappedItemException">The given <paramref name="relationMember"/> cannot be mapped to a <see cref="Table"/>.</exception>
    Table GetTableForRelation (MemberInfo relationMember, string alias);

    /// <summary>
    /// Determines whether the specified <see cref="MemberInfo"/> has an associated column.
    /// </summary>
    /// <param name="member">The member to check.</param>
    /// <returns>
    /// 	<see langword="true"/> if the specified member is associated with a column; otherwise, <see langword="false"/>.
    /// </returns>
    bool HasAssociatedColumn (MemberInfo member);

    /// <summary>
    /// Creates a <see cref="Column"/> instance for the given <see cref="IColumnSource"/> and <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="columnSource">The <see cref="IColumnSource"/> representing the object owning the column, e.g. a <see cref="Table"/> or a 
    /// <see cref="SubQuery"/>.</param>
    /// <param name="member">The <see cref="MemberInfo"/> identifying the column being queried.</param>
    /// <returns>A <see cref="Column"/> for the given <paramref name="member"/>.</returns>
    /// <exception cref="UnmappedItemException">The given <paramref name="member"/> cannot be mapped to a <see cref="Table"/>.</exception>
    Column GetColumnForMember (IColumnSource columnSource, MemberInfo member);

    /// <summary>
    /// Has to be implemented to get affected columns of a join.
    /// </summary>
    /// <param name="relationMember">The <see cref="MemberInfo"/> identifying the relation.</param>
    /// <returns>The names of the primary and foreign key columns to be compared for joining, or <see langword="null" /> if the 
    /// <paramref name="relationMember"/> does not identify relation.</returns>
    /// <exception cref="UnmappedItemException">The given <paramref name="relationMember"/> cannot be mapped to a <see cref="Table"/>.</exception>
    JoinColumnNames GetJoinColumnNames (MemberInfo relationMember);

    /// <summary>
    /// Has to be implemented to get value of a parameter in a where condition.
    /// </summary>
    /// <param name="parameter">The parameter in a where condition.</param>
    /// <returns>The value of the given where parameter.</returns>
    object ProcessWhereParameter (object parameter);

    /// <summary>
    /// Has to be implemented to get primary key member of a given entity.
    /// </summary>
    /// <param name="entityType">The type of the queried entity.</param>
    /// <returns><see cref="MemberInfo"/> of the primary key.</returns>
    MemberInfo GetPrimaryKeyMember (Type entityType);
  }
}
