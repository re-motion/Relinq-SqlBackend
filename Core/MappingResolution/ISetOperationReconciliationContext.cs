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
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ISetOperationReconciliationContext"/> provides methods to reconcile the different SQL queries in a
  /// set operation (e.g. UNION/UNION ALL) to have matching columns.
  /// The main use case is unifying inheritance trees to a common ancestor by padding columns.
  /// </summary>
  public interface ISetOperationReconciliationContext
  {
    /// <summary>
    /// Returns <see langword="true"/> if the specified <paramref name="entityExpression"/> requires reconciliation.
    /// If so use <see cref="GetReconciledColumns"/> to create the reconciled columns view.
    /// </summary>
    bool IsReconciliationRequired (SqlEntityExpression entityExpression);

    /// <summary>
    /// Creates an array of <see cref="SqlColumnExpression"/> for the reconciled column view for the specified <paramref name="entityExpression"/>.
    /// </summary>
    SqlColumnExpression[] GetReconciledColumns (SqlEntityExpression entityExpression);
  }
}