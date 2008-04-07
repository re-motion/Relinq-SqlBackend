using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Collections;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
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
      SqlGeneratorVisitor visitor = ProcessQuery();

      CreateSelectBuilder().BuildSelectPart (visitor.Columns, visitor.Distinct);
      CreateFromBuilder ().BuildFromPart (visitor.FromSources, visitor.Joins);
      //CreateFromBuilder ().BuildLetPart (visitor.LetExpressions);
      CreateWhereBuilder ().BuildWherePart (visitor.Criterion);
      CreateOrderByBuilder ().BuildOrderByPart (visitor.OrderingFields);

      return new Tuple<string, CommandParameter[]> (CommandText.ToString(), CommandParameters.ToArray());
    }

    protected virtual SqlGeneratorVisitor ProcessQuery ()
    {
      JoinedTableContext context = new JoinedTableContext();
      SqlGeneratorVisitor visitor = new SqlGeneratorVisitor (QueryModel, DatabaseInfo, context, ParseContext);
      QueryModel.Accept (visitor);
      context.CreateAliases();
      return visitor;
    }

    protected abstract IOrderByBuilder CreateOrderByBuilder ();
    protected abstract IWhereBuilder CreateWhereBuilder ();
    protected abstract IFromBuilder CreateFromBuilder ();
    protected abstract ISelectBuilder CreateSelectBuilder ();
  }
}