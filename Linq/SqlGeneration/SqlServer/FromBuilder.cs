using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class FromBuilder : IFromBuilder
  {
    private readonly StringBuilder _commandText;

    public FromBuilder (StringBuilder commandText)
    {
      ArgumentUtility.CheckNotNull ("commandText", commandText);
      _commandText = commandText;
    }


    public void BuildFromPart (List<Table> tables, IDictionary<Table, List<Join>> joins)
    {
      _commandText.Append ("FROM ");

      IEnumerable<string> tableEntries = CombineTables (tables, joins);
      _commandText.Append (SeparatedStringBuilder.Build (", ", tableEntries));
    }

    private IEnumerable<string> CombineTables (IEnumerable<Table> tables, IDictionary<Table, List<Join>> joins)
    {
      foreach (Table table in tables)
        yield return GetTableDeclaration (table) + BuildJoinPart (joins[table]);
    }

    private string BuildJoinPart (IEnumerable<Join> joins)
    {
      StringBuilder joinStatement = new StringBuilder ();
      foreach (Join join in joins)
        AppendJoinExpression (joinStatement, join);
      return joinStatement.ToString ();
    }

    private void AppendJoinExpression (StringBuilder joinStatement, Join join)
    {
      if (join.RightSide is Join)
      {
        Join rightSide = (Join) join.RightSide;
        AppendJoinExpression (joinStatement, rightSide);
      }

      joinStatement.Append (" INNER JOIN ")
          .Append (GetTableDeclaration (join.LeftSide))
          .Append (" ON ")
          .Append (SqlServerUtility.GetColumnString (join.RightColumn))
          .Append (" = ")
          .Append (SqlServerUtility.GetColumnString (join.LeftColumn));
    }

    private string GetTableDeclaration (Table table)
    {
      return SqlServerUtility.WrapSqlIdentifier (table.Name) + " " + SqlServerUtility.WrapSqlIdentifier (table.Alias);
    }
  }
}