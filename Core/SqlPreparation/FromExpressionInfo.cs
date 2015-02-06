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
    public FromExpressionInfo (SqlAppendedTable appendedTable, Ordering[] extractedOrderings, Expression itemSelector, Expression whereCondition)
    {
      ArgumentUtility.CheckNotNull ("appendedTable", appendedTable);
      ArgumentUtility.CheckNotNull ("extractedOrderings", extractedOrderings);
      ArgumentUtility.CheckNotNull ("itemSelector", itemSelector);
      
      AppendedTable = appendedTable;
      ExtractedOrderings = Array.AsReadOnly(extractedOrderings);
      ItemSelector = itemSelector;
      WhereCondition = whereCondition;
    }

    public readonly SqlAppendedTable AppendedTable;
    public readonly ReadOnlyCollection<Ordering> ExtractedOrderings;
    public readonly Expression ItemSelector;
    public readonly Expression WhereCondition;
  }
}