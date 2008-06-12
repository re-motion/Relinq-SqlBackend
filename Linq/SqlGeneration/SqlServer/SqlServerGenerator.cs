using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.Parsing;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class SqlServerGenerator : SqlGeneratorBase
  {
    private readonly ICommandBuilder _commandBuilder;

    public SqlServerGenerator (IDatabaseInfo databaseInfo)
        : this (databaseInfo, new CommandBuilder (new StringBuilder(), new List<CommandParameter>(),databaseInfo), ParseContext.TopLevelQuery)
    {
    }

    public SqlServerGenerator (IDatabaseInfo databaseInfo, ICommandBuilder commandBuilder, ParseContext parseContext)
      : base (databaseInfo, parseContext)
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