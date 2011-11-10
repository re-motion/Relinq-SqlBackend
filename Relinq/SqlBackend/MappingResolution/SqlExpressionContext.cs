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
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Describes whether predicate or value semantics is required for boolean expressions in the current context.
  /// </summary>
  public enum SqlExpressionContext
  {
    /// <summary>
    /// A single value or an entity is required; boolean expressions need to be converted using CASE WHEN.
    /// </summary>
    ValueRequired,
    /// <summary>
    /// A single value is required; boolean expressions need to be converted using CASE WHEN, of <see cref="SqlEntityExpression"/> instances, only 
    /// the primary key is used.
    /// </summary>
    SingleValueRequired,
    /// <summary>
    /// A predicate is required; non-boolean expressions need to be converted, e.g., by comparing them to the literal value 1.
    /// </summary>
    PredicateRequired,
  }
}