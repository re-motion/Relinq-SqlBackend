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
using System.Linq.Expressions;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Extends <see cref="IMappingResolver"/> functionality to prevent breaking changes.
  /// Will be merged into <see cref="IMappingResolver"/> with the next major version.
  /// </summary>
  /// <seealso cref="IMappingResolver"/>
  public interface IMappingResolver2 : IMappingResolver
  {
    /// <summary>
    /// Determines if the specified <paramref name="projections"/> for a set operation need to be reconciled.
    /// If a reconciliation is necessary, returns a <paramref name="reconciliationContext"/> that can be used
    /// for reconciliation using <see cref="SetOperationReconciliationVisitor"/>.
    /// </summary>
    /// <param name="projections">The select projections of all the parts of the set operation (primary and all others; usually there are just two).</param>
    /// <param name="reconciliationContext">A context that reconcile the different select projections. Only returned if a reconciliation is necessary.</param>
    /// <returns>
    /// Returns <see langword="true"/> if a reconciliation is necessary and sets <paramref name="reconciliationContext"/> accordingly.
    /// Otherwise, returns <see langword="false"/> and no <paramref name="reconciliationContext"/> is provided.
    /// </returns>
    bool TryResolveSetOperationReconciliationContext (Expression[] projections, out ISetOperationReconciliationContext reconciliationContext);
  }
}