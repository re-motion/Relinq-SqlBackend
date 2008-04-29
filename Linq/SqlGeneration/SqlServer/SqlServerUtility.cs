using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public static class SqlServerUtility
  {
    public static string GetColumnString (Column column)
    {
      if (column.Name != null)
        return WrapSqlIdentifier (column._columnSource.Alias) + "." + WrapSqlIdentifier (column.Name);
      else
        return WrapSqlIdentifier (column._columnSource.Alias);
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