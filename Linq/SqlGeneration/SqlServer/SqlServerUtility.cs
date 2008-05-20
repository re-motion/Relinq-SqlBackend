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