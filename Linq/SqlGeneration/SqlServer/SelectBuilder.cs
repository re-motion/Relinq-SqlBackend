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
    private readonly CommandBuilder _commandBuilder;

    public SelectBuilder (CommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      _commandBuilder = commandBuilder;
    }

    public void BuildSelectPart (List<Column> columns,bool distinct)
    {
      ArgumentUtility.CheckNotNull ("columns", columns);
      ArgumentUtility.CheckNotNull ("distinct", distinct);

      if (distinct)
        _commandBuilder.Append ("SELECT DISTINCT ");
      else
        _commandBuilder.Append ("SELECT ");

      if (columns.Count == 0)
        throw new InvalidOperationException ("The query does not select any fields from the data source.");

      IEnumerable<string> columnEntries = CombineColumnItems (columns);
      _commandBuilder.Append (SeparatedStringBuilder.Build (", ", columnEntries));
      _commandBuilder.Append(" ");
    }

    private IEnumerable<string> CombineColumnItems (IEnumerable<Column> columns)
    {
      foreach (Column column in columns)
        yield return SqlServerUtility.GetColumnString (column);
    }
    

  }
}