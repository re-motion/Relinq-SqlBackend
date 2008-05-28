using System.Collections.Generic;
using System.Text;
using Remotion.Collections;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public abstract class SqlGeneratorBase
  {
    public QueryModel QueryModel { get; private set; }
    public IDatabaseInfo DatabaseInfo { get; private set; }
    public ParseContext ParseContext { get; private set; }

    public abstract StringBuilder CommandText { get; }
    public abstract List<CommandParameter> CommandParameters { get; }

    public SqlGeneratorBase (QueryModel query, IDatabaseInfo databaseInfo, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("query", query);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      QueryModel = query;
      DatabaseInfo = databaseInfo;
      ParseContext = parseContext;
    }

    public virtual Tuple<string, CommandParameter[]> BuildCommandString ()
    {
      SqlGenerationData sqlGenerationData = ProcessQuery ();
      CreateSelectBuilder ().BuildSelectPart (sqlGenerationData.SelectEvaluations, sqlGenerationData.Distinct);
      CreateFromBuilder ().BuildFromPart (sqlGenerationData.FromSources, sqlGenerationData.Joins);
      CreateFromBuilder ().BuildLetPart (sqlGenerationData.LetEvaluations);
      CreateWhereBuilder ().BuildWherePart (sqlGenerationData.Criterion);
      CreateOrderByBuilder ().BuildOrderByPart (sqlGenerationData.OrderingFields);

      return new Tuple<string, CommandParameter[]> (CommandText.ToString(), CommandParameters.ToArray());
    }


    protected virtual SqlGenerationData ProcessQuery ()
    {
      JoinedTableContext context = new JoinedTableContext();
      SqlGeneratorVisitor visitor = new SqlGeneratorVisitor (QueryModel, DatabaseInfo, context, ParseContext);
      QueryModel.Accept (visitor);
      context.CreateAliases();
      return visitor.SqlGenerationData;
    }

    protected abstract IOrderByBuilder CreateOrderByBuilder ();
    protected abstract IWhereBuilder CreateWhereBuilder ();
    protected abstract IFromBuilder CreateFromBuilder ();
    protected abstract ISelectBuilder CreateSelectBuilder ();
  }
}