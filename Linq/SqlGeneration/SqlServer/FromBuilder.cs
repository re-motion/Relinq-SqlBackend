using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;


namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class FromBuilder : IFromBuilder
  {
    private readonly ICommandBuilder _commandBuilder;
    private readonly IDatabaseInfo _databaseInfo;

    public FromBuilder (ICommandBuilder commandBuilder, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      _commandBuilder = commandBuilder;
      _databaseInfo = databaseInfo;
    }

    public void BuildFromPart (List<IColumnSource> fromSources, JoinCollection joins)
    {
      _commandBuilder.Append ("FROM ");

      bool first = true;
      foreach (IColumnSource fromSource in fromSources)
      {
        Table table = fromSource as Table;
        if (table != null)
        {
          if (!first)
            _commandBuilder.Append (", ");
          _commandBuilder.Append (SqlServerUtility.GetTableDeclaration (table));
        }
        else
          AppendCrossApply ((SubQuery) fromSource);

        AppendJoinPart (joins[fromSource]);
        first = false;
      }
    }
    
    private void AppendCrossApply (SubQuery subQuery)
    {
      _commandBuilder.Append (" CROSS APPLY (");
      SqlGeneratorBase subQueryGenerator = CreateSqlGeneratorForSubQuery(subQuery, _databaseInfo, _commandBuilder);
      subQueryGenerator.BuildCommandString (subQuery.QueryModel);
      _commandBuilder.Append (") ");
      _commandBuilder.Append (SqlServerUtility.WrapSqlIdentifier (subQuery.Alias));
    }

    protected virtual SqlGeneratorBase CreateSqlGeneratorForSubQuery (SubQuery subQuery, IDatabaseInfo databaseInfo, ICommandBuilder commandBuilder)
    {
      return new SqlServerGenerator (databaseInfo, commandBuilder, ParseContext.SubQueryInFrom);
    }

    private void AppendJoinPart (IEnumerable<SingleJoin> joins)
    {
      foreach (SingleJoin join in joins)
        AppendJoinExpression (join);
    }

    private void AppendJoinExpression (SingleJoin join)
    {
      _commandBuilder.Append (" LEFT OUTER JOIN ");
      _commandBuilder.Append (SqlServerUtility.GetTableDeclaration ((Table) join.RightSide));
      _commandBuilder.Append (" ON ");
      _commandBuilder.Append (SqlServerUtility.GetColumnString (join.LeftColumn));
      _commandBuilder.Append (" = ");
      _commandBuilder.Append (SqlServerUtility.GetColumnString (join.RightColumn));
    }


    public void BuildLetPart (List<LetData> letData)
    {
      ArgumentUtility.CheckNotNull ("letData", letData);
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor ((CommandBuilder) _commandBuilder, _databaseInfo);
      foreach (var evaluations in letData)
      {
        _commandBuilder.Append (" CROSS APPLY (SELECT ");
        bool first = true;
        foreach (var let in evaluations.Evaluations)
        {
          if (!first)
            _commandBuilder.Append (", ");

          let.Accept (visitor);
          first = false;
        }
        if (!evaluations.CorrespondingColumnSource.IsTable)
          _commandBuilder.Append (" " + SqlServerUtility.WrapSqlIdentifier (evaluations.Name));

        _commandBuilder.Append (") [");
        _commandBuilder.Append (evaluations.Name);
        _commandBuilder.Append ("]");
      }
    }
  }
}