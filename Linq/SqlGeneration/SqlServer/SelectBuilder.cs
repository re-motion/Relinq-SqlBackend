using System;
using System.Collections.Generic;
using Rubicon.Utilities;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class SelectBuilder : ISelectBuilder
  {
    private readonly ICommandBuilder _commandBuilder;

    public SelectBuilder (ICommandBuilder commandBuilder)
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

      _commandBuilder.AppendColumns (columns);
      _commandBuilder.Append(" ");
    }
  }
}