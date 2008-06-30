using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;


namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class FromBuilder : IFromBuilder
  {
    private readonly CommandBuilder _commandBuilder;
    private readonly IDatabaseInfo _databaseInfo;

    public FromBuilder (CommandBuilder commandBuilder, IDatabaseInfo databaseInfo)
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
      ISqlGeneratorBase subQueryGenerator = CreateSqlGeneratorForSubQuery(subQuery, _databaseInfo, _commandBuilder);
      subQueryGenerator.BuildCommandString (subQuery.QueryModel);
      _commandBuilder.Append (") ");
      _commandBuilder.Append (SqlServerUtility.WrapSqlIdentifier (subQuery.Alias));
    }

    protected virtual ISqlGeneratorBase CreateSqlGeneratorForSubQuery (SubQuery subQuery, IDatabaseInfo databaseInfo, CommandBuilder commandBuilder)
    {
      return new InlineSqlServerGenerator (databaseInfo, commandBuilder, ParseMode.SubQueryInFrom);
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


    public void BuildLetPart (List<LetData> letDataCollection)
    {
      ArgumentUtility.CheckNotNull ("letData", letDataCollection);
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (_commandBuilder, _databaseInfo);
      foreach (var letData in letDataCollection)
      {
        _commandBuilder.Append (" CROSS APPLY (SELECT ");
        
        var let = letData.Evaluation;
        let.Accept (visitor);
        
        if (!letData.CorrespondingColumnSource.IsTable)
          _commandBuilder.Append (" " + SqlServerUtility.WrapSqlIdentifier (letData.Name));

        _commandBuilder.Append (") [");
        _commandBuilder.Append (letData.Name);
        _commandBuilder.Append ("]");
      }
    }
  }
}