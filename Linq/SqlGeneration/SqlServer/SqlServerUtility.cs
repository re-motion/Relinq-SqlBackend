using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public static class SqlServerUtility
  {
    public static string GetColumnString (Column column)
    {
      return WrapSqlIdentifier (column.Table.Alias) + "." + WrapSqlIdentifier (column.Name);
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