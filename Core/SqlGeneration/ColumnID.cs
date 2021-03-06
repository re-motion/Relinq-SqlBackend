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
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Represents a column in the SQL result returned by a LINQ query. The in-memory projections generated by re-linq use this as a parameter
  /// to the methods of <see cref="IDatabaseResultRow"/> when they need to retrieve a value from a result row. Each column is identified both by
  /// its <see cref="ColumnName"/> and its <see cref="Position"/>. The implementer of <see cref="IDatabaseResultRow"/> is free to choose which 
  /// identifier to use when retrieving a value from a result row.
  /// </summary>
  public struct ColumnID
  {
    public ColumnID (string columnName, int position)
    {
      ArgumentUtility.CheckNotNull ("columnName", columnName);

      ColumnName = columnName;
      Position = position;
    }

    public readonly string ColumnName;
    public readonly int Position;

    public override string ToString ()
    {
      return string.Format ("col: {0} ({1})", ColumnName, Position);
    }
  }
}