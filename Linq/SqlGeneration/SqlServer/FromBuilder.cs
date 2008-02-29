using System.Collections.Generic;
using System.Linq;
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

    public void BuildFromPart (List<Table> tables, JoinCollection joins)
    {
      _commandText.Append ("FROM ");

      IEnumerable<string> tableEntries = CombineTables (tables, joins);
      _commandText.Append (SeparatedStringBuilder.Build (", ", tableEntries));
    }

    private IEnumerable<string> CombineTables (IEnumerable<Table> tables, JoinCollection joins)
    {
      foreach (Table table in tables)
        yield return GetTableDeclaration (table) + BuildJoinPart (joins[table]);
    }

    private string BuildJoinPart (IEnumerable<SingleJoin> joins)
    {
      StringBuilder joinStatement = new StringBuilder ();
      foreach (SingleJoin join in joins)
        AppendJoinExpression (joinStatement, join);
      return joinStatement.ToString ();
    }

    private void AppendJoinExpression (StringBuilder joinStatement, SingleJoin join)
    {
      joinStatement.Append (" LEFT OUTER JOIN ")
          .Append (GetTableDeclaration (join.RightSide))
          .Append (" ON ")
          .Append (SqlServerUtility.GetColumnString (join.LeftColumn))
          .Append (" = ")
          .Append (SqlServerUtility.GetColumnString (join.RightColumn));
    }

    private string GetTableDeclaration (Table table)
    {
      return SqlServerUtility.WrapSqlIdentifier (table.Name) + " " + SqlServerUtility.WrapSqlIdentifier (table.Alias);
    }
  }
}