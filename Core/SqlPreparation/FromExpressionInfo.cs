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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="FromExpressionInfo"/> is returned by <see cref="SqlPreparationFromExpressionVisitor.AnalyzeFromExpression"/>.
  /// </summary>
  public struct FromExpressionInfo
  {
    public FromExpressionInfo (
        [NotNull] SqlAppendedTable appendedTable,
        [NotNull] Ordering[] extractedOrderings,
        [NotNull] Expression itemSelector,
        [CanBeNull] Expression whereCondition)
    {
      ArgumentUtility.CheckNotNull ("appendedTable", appendedTable);
      ArgumentUtility.CheckNotNull ("extractedOrderings", extractedOrderings);
      ArgumentUtility.CheckNotNull ("itemSelector", itemSelector);
      
      AppendedTable = appendedTable;
      ExtractedOrderings = Array.AsReadOnly(extractedOrderings);
      ItemSelector = itemSelector;
      WhereCondition = whereCondition;
    }

    /// <summary>
    /// Holds the table that was constructed from the analyzed FROM expression. See <see cref="SqlPreparationFromExpressionVisitor"/> for details.
    /// </summary>
    [NotNull]
    public readonly SqlAppendedTable AppendedTable;

    /// <summary>
    /// Because substatements cannot contain ORDER BY clauses in SQL (except when there is a TOP statement or something similar), the code preparing
    /// <see cref="FromExpressionInfo"/> objects needs to extract ORDER BY clauses and puts them into <see cref="ExtractedOrderings"/>.
    /// </summary>
    [NotNull]
    public readonly ReadOnlyCollection<Ordering> ExtractedOrderings;

    /// <summary>
    /// If ORDER BY statements are extracted from a substatement, the projection needs to be extended to include the expressions used as ORDER BY
    /// criteria. Then, if someone wants to access the actual items delivered by the substatement, they need to select the original items.
    /// <see cref="ItemSelector"/> contains an expression that automatically contains this selection if needed.
    /// </summary>
    [NotNull]
    public readonly Expression ItemSelector;

    /// <summary>
    /// A WHERE condition to be appended to the outer statement. Used for joins on collection members.
    /// </summary>
    [CanBeNull]
    public readonly Expression WhereCondition;
  }
}