// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Composes several <see cref="ISetOperationReconciliationContext"/> instances into one, combining all results.
  /// </summary>
  /// <remarks>
  /// The <see cref="ISetOperationReconciliationContext"/> instances should each handle a distinct set of <see cref="SqlEntityExpression"/>s.
  /// This class assumes this is the case and will use the first <see cref="ISetOperationReconciliationContext"/> available.
  /// </remarks>
  public class CompositeSetOperationReconciliationContext : ISetOperationReconciliationContext
  {
    private readonly IReadOnlyList<ISetOperationReconciliationContext> _reconciliationContexts;

    public CompositeSetOperationReconciliationContext (IEnumerable<ISetOperationReconciliationContext> reconciliationContexts)
    {
      ArgumentUtility.CheckNotNull (nameof(reconciliationContexts), reconciliationContexts);

      _reconciliationContexts = reconciliationContexts.ToList().AsReadOnly();
    }

    public bool IsReconciliationRequired (SqlEntityExpression entityExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);

      return GetResponsibleReconciliationContext (entityExpression) != null;
    }

    public SqlColumnExpression[] GetReconciledColumns (SqlEntityExpression entityExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);

      var reconciliationContext = GetResponsibleReconciliationContext (entityExpression);
      if (reconciliationContext == null)
        throw new InvalidOperationException ($"The specified entity '{entityExpression}' is not supported.");

      return reconciliationContext.GetReconciledColumns (entityExpression);
    }

    [CanBeNull]
    private ISetOperationReconciliationContext GetResponsibleReconciliationContext (SqlEntityExpression entityExpression)
    {
      // We do a linear search here as we expect a low amount of inner contexts
      // and creating a dict would keep instances alive and bring concurrency concerns
      return _reconciliationContexts.FirstOrDefault (reconciliationContext => reconciliationContext.IsReconciliationRequired (entityExpression));
    }
  }
}
