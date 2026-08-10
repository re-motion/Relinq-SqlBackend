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
using JetBrains.Annotations;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Extends <see cref="IMappingResolutionStage"/> functionality to prevent breaking changes.
  /// Will be merged into <see cref="IMappingResolutionStage"/> with the next major version.
  /// </summary>
  /// <seealso cref="IMappingResolutionStage"/>
  public interface IMappingResolutionStage2 : IMappingResolutionStage
  {
    [CanBeNull]
    ISetOperationReconciliationContext ResolveSetOperationReconciliationContext (Expression[] projectionExpressions);

    SqlEntityExpression ApplySetOperationReconciliationContext (SqlEntityExpression entityExpression, ISetOperationReconciliationContext reconciliationContext);
  }
}