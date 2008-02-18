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
    private readonly StringBuilder _commandText = new StringBuilder ();
    private readonly List<CommandParameter> _commandParameters = new List<CommandParameter> ();

    protected SqlGeneratorBase (QueryExpression query, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("query", query);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      _query = query;
      _databaseInfo = databaseInfo;
    }

    public Tuple<string, CommandParameter[]> BuildCommandString ()
    {
      SqlGeneratorVisitor visitor = ProcessQuery();

      CreateSelectBuilder(_commandText).BuildSelectPart (visitor.Columns);
      CreateFromBuilder (_commandText).BuildFromPart (visitor.Tables, visitor.Joins);
      CreateWhereBuilder (_commandText, _commandParameters).BuildWherePart (visitor.Criterion);
      CreateOrderByBuilder (_commandText).BuildOrderByPart (visitor.OrderingFields);

      return new Tuple<string, CommandParameter[]> (_commandText.ToString(), _commandParameters.ToArray());
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