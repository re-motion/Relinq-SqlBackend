// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using Remotion.Data.Linq.Backend.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public static class SqlServerUtility
  {
    public static string GetColumnString (Column column)
    {
      if (column.Name != null)
        return WrapSqlIdentifier (column.ColumnSource.Alias) + "." + WrapSqlIdentifier (column.Name);
      if (column.ColumnSource.IsTable)
        return WrapSqlIdentifier (column.ColumnSource.Alias);
      return WrapSqlIdentifier (column.ColumnSource.Alias) + "." + WrapSqlIdentifier (column.ColumnSource.Alias);
    }

    public static string WrapSqlIdentifier (string identifier)
    {
      if (identifier != "*")
        return "[" + identifier + "]";
      else
        return "*";
    }

    public static string GetTableDeclaration (Table table)
    {
      return SqlServerUtility.WrapSqlIdentifier (table.Name) + " " + SqlServerUtility.WrapSqlIdentifier (table.Alias);
    }
  }
}
