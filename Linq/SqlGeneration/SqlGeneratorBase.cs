using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Collections;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public abstract class SqlGeneratorBase
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly QueryExpression _query;

    protected StringBuilder CommandText { get; private set; }
    protected List<CommandParameter> CommandParameters { get; private set; }

    protected SqlGeneratorBase (QueryExpression query, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("query", query);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      _query = query;
      _databaseInfo = databaseInfo;

      CommandText = new StringBuilder();
      CommandParameters = new List<CommandParameter>();
    }

    public virtual Tuple<string, CommandParameter[]> BuildCommandString ()
    {
      SqlGeneratorVisitor visitor = ProcessQuery();

      CreateSelectBuilder(CommandText).BuildSelectPart (visitor.Columns, visitor.Distinct);
      CreateFromBuilder (CommandText).BuildFromPart (visitor.Tables, visitor.Joins);
      CreateWhereBuilder (CommandText, CommandParameters).BuildWherePart (visitor.Criterion);
      CreateOrderByBuilder (CommandText).BuildOrderByPart (visitor.OrderingFields);

      return new Tuple<string, CommandParameter[]> (CommandText.ToString(), CommandParameters.ToArray());
    }

    protected virtual SqlGeneratorVisitor ProcessQuery ()
    {
      JoinedTableContext context = new JoinedTableContext();
      SqlGeneratorVisitor visitor = new SqlGeneratorVisitor (_query, _databaseInfo, context);
      _query.Accept (visitor);
      context.CreateAliases();
      return visitor;
    }

    protected abstract IOrderByBuilder CreateOrderByBuilder (StringBuilder commandText);
    protected abstract IWhereBuilder CreateWhereBuilder (StringBuilder commandText, List<CommandParameter> commandParameters);
    protected abstract IFromBuilder CreateFromBuilder (StringBuilder commandText);
    protected abstract ISelectBuilder CreateSelectBuilder (StringBuilder commandText);
  }
}