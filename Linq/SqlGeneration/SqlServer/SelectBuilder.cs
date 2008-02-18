using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Text;
using Rubicon.Utilities;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class SelectBuilder : ISelectBuilder
  {
    private readonly StringBuilder _commandText;
    
    public SelectBuilder (StringBuilder commandText)
    {
      ArgumentUtility.CheckNotNull ("commandText", commandText);
      _commandText = commandText;
    }

    public void BuildSelectPart (List<Column> columns)
    {
      ArgumentUtility.CheckNotNull ("columns", columns);
      _commandText.Append ("SELECT ");

      if (columns.Count == 0)
        throw new InvalidOperationException ("The query does not select any fields from the data source.");

      IEnumerable<string> columnEntries = CombineColumnItems (columns);
      _commandText.Append (SeparatedStringBuilder.Build (", ", columnEntries)).Append (" ");
    }

    private IEnumerable<string> CombineColumnItems (IEnumerable<Column> columns)
    {
      foreach (Column column in columns)
        yield return SqlServerUtility.GetColumnString (column);
    }

  }
}