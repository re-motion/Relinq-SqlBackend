using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class SqlGenerator
  {
    private readonly QueryExpression _query;

    public SqlGenerator (QueryExpression query)
    {
      ArgumentUtility.CheckNotNull ("query", query);
      _query = query;
    }

    public string GetCommandString (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      SqlGeneratorVisitor visitor = new SqlGeneratorVisitor (databaseInfo);
      _query.Accept (visitor);
      StringBuilder sb = new StringBuilder ();

      sb.Append ("SELECT ");

      if (visitor.Columns.Count == 0)
        throw new InvalidOperationException ("The query does not select any fields from the data source.");
      else
      {
        IEnumerable<string> columnEntries = JoinColumnItems (visitor.Columns);
        sb.Append (SeparatedStringBuilder.Build (", ", columnEntries)).Append (" ");
      }

      sb.Append ("FROM ");

      IEnumerable<string> tableEntries = JoinTableItems (visitor.Tables);
      sb.Append (SeparatedStringBuilder.Build (", ", tableEntries));

      //WHERE



      return sb.ToString();
    }

    public IDbCommand GetCommand (IDatabaseInfo databaseInfo, IDbConnection connection)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("connection", connection);

      IDbCommand command = connection.CreateCommand();
      command.CommandText = GetCommandString (databaseInfo);
      command.CommandType = CommandType.Text;
      return command;
    }

    private IEnumerable<string> JoinTableItems (IEnumerable<Table> tables)
    {
      foreach (Table table in tables)
        yield return WrapSqlIdentifier (table.Name) + " " + WrapSqlIdentifier (table.Alias);
    }

    private IEnumerable<string> JoinColumnItems (IEnumerable<Column> columns)
    {
      foreach (Column column in columns)
        yield return WrapSqlIdentifier (column.Table.Alias) + "." + WrapSqlIdentifier (column.Name);
    }
    
    private string WrapSqlIdentifier (string identifier)
    {
      if (identifier != "*")
        return "[" + identifier + "]";
      else
        return "*";
    }
  }
}