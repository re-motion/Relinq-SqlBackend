// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Linq.SqlBackend.MappingResolution;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Used by re-linq's in-memory projections (see <see cref="SqlCommandData.GetInMemoryProjection{T}"/>) in order to access values and entities from
  /// the database result. Implementers of <see cref="IDatabaseResultRow"/> must represent a row in the database result for an executed SQL command.
  /// When the <see cref="SqlCommandData.GetInMemoryProjection{T}"/> is executed against the row, it will read all values and entities needed by the
  /// LINQ query's select projection and then construct the full projection in-memory.
  /// </summary>
  /// <remarks>
  /// The result of scalar or single queries should be represented as if there was one row with one value.
  /// </remarks>
  public interface IDatabaseResultRow
  {
    /// <summary>
    /// Gets the value indicated by <paramref name="columnID"/>. The value is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the value coming from the database result row.</typeparam>
    /// <param name="columnID">The <see cref="ColumnID"/> identifying the value.</param>
    /// <returns>The value of the given column in the current result row.</returns>
    T GetValue<T> (ColumnID columnID);
    /// <summary>
    /// Gets an entity indicated by a number of <paramref name="columnIDs"/>. The entity is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the entity coming from the database result row.</typeparam>
    /// <param name="columnIDs">A list of <see cref="ColumnID"/> instances that identify all values to be used for instantiating the entity. These
    /// values identify the columns returned by <see cref="IMappingResolver.ResolveSimpleTableInfo"/>, and they are given in the same order in which 
    /// that method returned them.</param>
    /// <returns>An entity constructed of the given columns in the current result row.</returns>
    T GetEntity<T> (params ColumnID[] columnIDs);
  }
}