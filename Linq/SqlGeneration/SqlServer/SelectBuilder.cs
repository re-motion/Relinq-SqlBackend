using System;
using System.Collections.Generic;
using Remotion.Utilities;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class SelectBuilder : ISelectBuilder
  {
    private readonly CommandBuilder _commandBuilder;

    public SelectBuilder (CommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      _commandBuilder = commandBuilder;
    }

    public void BuildSelectPart (List<IEvaluation> selectEvaluations,bool distinct)
    {
      ArgumentUtility.CheckNotNull ("selectEvaluations", selectEvaluations);
      ArgumentUtility.CheckNotNull ("distinct", distinct);
      
      if (distinct)
        _commandBuilder.Append ("SELECT DISTINCT ");
      else
        _commandBuilder.Append ("SELECT ");

      if (selectEvaluations.Count == 0)
        throw new InvalidOperationException ("The query does not select any fields from the data source.");

      _commandBuilder.AppendEvaluations (selectEvaluations);
      _commandBuilder.Append(" ");
    }
  }
}