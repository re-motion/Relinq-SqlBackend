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
using System.Data;
using System.Data.Common;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Represents a scalar row in the database result for an executed SQL command.
  /// Reads the value from a row.
  /// Implementation for Linq2Sql 
  /// </summary>
  public class ScalarRowWrapper : IDatabaseResultRow
  {
    private readonly DbDataReader _dataReader;

    public ScalarRowWrapper (DbDataReader dataReader)
    {
      _dataReader = dataReader;
    }

    public T GetValue<T> (ColumnID id)
    {
      if (_dataReader.IsDBNull (id.Position))
        return default (T);

      if (id.Position != 0)
        throw new ArgumentException ("Only Columns with the position 0 are valid Scalar Columns!");

      return (T) _dataReader.GetValue (id.Position);
    }


    public T GetEntity<T> (params ColumnID[] columnIDs)
    {
      if (columnIDs == null)
        throw new ArgumentException ("You must provide 1 ColumnID!");

      if (columnIDs.Length != 1)
        throw new ArgumentException ("Only Scalar values are alowed!");

      return GetValue<T> (columnIDs[0]);
    }
  }
}