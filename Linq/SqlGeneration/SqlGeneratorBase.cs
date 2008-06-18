using Remotion.Collections;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public abstract class SqlGeneratorBase<TContext> : ISqlGeneratorBase where TContext : ISqlGenerationContext
  {
    protected SqlGeneratorBase (IDatabaseInfo databaseInfo, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      DatabaseInfo = databaseInfo;
      ParseContext = parseContext;
    }

    public IDatabaseInfo DatabaseInfo { get; private set; }
    public ParseContext ParseContext { get; private set; }

    protected abstract TContext CreateContext ();

    public virtual Tuple<string, CommandParameter[]> BuildCommandString (QueryModel queryModel)
    {
      SqlGenerationData sqlGenerationData = ProcessQuery (queryModel);

      TContext context = CreateContext ();
      CreateSelectBuilder (context).BuildSelectPart (sqlGenerationData.SelectEvaluations, sqlGenerationData.Distinct);
      CreateFromBuilder (context).BuildFromPart (sqlGenerationData.FromSources, sqlGenerationData.Joins);
      CreateFromBuilder (context).BuildLetPart (sqlGenerationData.LetEvaluations);
      CreateWhereBuilder (context).BuildWherePart (sqlGenerationData.Criterion);
      CreateOrderByBuilder (context).BuildOrderByPart (sqlGenerationData.OrderingFields);

      return new Tuple<string, CommandParameter[]> (context.CommandText, context.CommandParameters);
    }

    protected virtual SqlGenerationData ProcessQuery (QueryModel queryModel)
    {
      JoinedTableContext context = new JoinedTableContext();
      DetailParser detailParser = new DetailParser (queryModel, DatabaseInfo, context, ParseContext);
      SqlGeneratorVisitor visitor = new SqlGeneratorVisitor (queryModel, DatabaseInfo, context, ParseContext, detailParser);
      queryModel.Accept (visitor);
      context.CreateAliases();
      return visitor.SqlGenerationData;
    }

    protected abstract IOrderByBuilder CreateOrderByBuilder (TContext context);
    protected abstract IWhereBuilder CreateWhereBuilder (TContext context);
    protected abstract IFromBuilder CreateFromBuilder (TContext context);
    protected abstract ISelectBuilder CreateSelectBuilder (TContext context);
  }
}