using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.Parsing;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class SqlServerGenerator : SqlGeneratorBase
  {
    private readonly ICommandBuilder _commandBuilder;

    public SqlServerGenerator (QueryModel query, IDatabaseInfo databaseInfo)
        : this (query, databaseInfo, new CommandBuilder (new StringBuilder(), new List<CommandParameter>(),databaseInfo), ParseContext.TopLevelQuery)
    {
    }

    public SqlServerGenerator (QueryModel query, IDatabaseInfo databaseInfo, ICommandBuilder commandBuilder, ParseContext parseContext)
      : base (query, databaseInfo, parseContext)
    {
      _commandBuilder = commandBuilder;
    }

    public override StringBuilder CommandText
    {
      get { return _commandBuilder.CommandText; }
    }

    public override List<CommandParameter> CommandParameters
    {
      get { return _commandBuilder.CommandParameters; }
    }

    protected override IOrderByBuilder CreateOrderByBuilder ()
    {
      return new OrderByBuilder (_commandBuilder);
    }

    protected override IWhereBuilder CreateWhereBuilder ()
    {
      return new WhereBuilder (_commandBuilder, DatabaseInfo);
    }

    protected override IFromBuilder CreateFromBuilder ()
    {
      return new FromBuilder (_commandBuilder, DatabaseInfo);
    }

    protected override ISelectBuilder CreateSelectBuilder ()
    {
      return new SelectBuilder (_commandBuilder);
    }
  }
}