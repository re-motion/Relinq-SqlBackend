using System.Collections.Generic;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public abstract class SqlGeneratorBase<TContext> : ISqlGeneratorBase where TContext : ISqlGenerationContext
  {
    protected SqlGeneratorBase (IDatabaseInfo databaseInfo, ParseMode parseMode)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      DatabaseInfo = databaseInfo;
      ParseMode = parseMode;
    }

    public IDatabaseInfo DatabaseInfo { get; private set; }
    public ParseMode ParseMode { get; private set; }

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
      JoinedTableContext joinedTableContext = new JoinedTableContext();
      ParseContext parseContext = new ParseContext (queryModel, queryModel.GetExpressionTree(), new List<FieldDescriptor>(), joinedTableContext);
      DetailParser detailParser = new DetailParser (queryModel, DatabaseInfo, joinedTableContext, ParseMode);
      SqlGeneratorVisitor visitor = new SqlGeneratorVisitor (DatabaseInfo, ParseMode, detailParser, parseContext);
      queryModel.Accept (visitor);
      joinedTableContext.CreateAliases();
      return visitor.SqlGenerationData;
    }

    protected abstract IOrderByBuilder CreateOrderByBuilder (TContext context);
    protected abstract IWhereBuilder CreateWhereBuilder (TContext context);
    protected abstract IFromBuilder CreateFromBuilder (TContext context);
    protected abstract ISelectBuilder CreateSelectBuilder (TContext context);
  }
}