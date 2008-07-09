/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using Remotion.Data.Linq.DataObjectModel;

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
