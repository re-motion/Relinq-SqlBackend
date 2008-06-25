using Remotion.Data.Linq.Parsing;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  // If a fixedCommandBuilder is specified, the SqlServerGenerator can only be used to create one query from one thread. Otherwise, it is
  // stateless and can be used for multiple queries from multiple threads.
  public class SqlServerGenerator : SqlGeneratorBase<SqlServerGenerationContext>
  {
    public SqlServerGenerator (IDatabaseInfo databaseInfo)
      : this (databaseInfo, Parsing.ParseMode.TopLevelQuery)
    {
    }

    protected SqlServerGenerator (IDatabaseInfo databaseInfo, ParseMode parseMode)
      : base (databaseInfo, parseMode)
    {
    }

    protected override SqlServerGenerationContext CreateContext ()
    {
      return new SqlServerGenerationContext (DatabaseInfo);
    }

    protected override IOrderByBuilder CreateOrderByBuilder (SqlServerGenerationContext context)
    {
      return new OrderByBuilder (context.CommandBuilder);
    }

    protected override IWhereBuilder CreateWhereBuilder (SqlServerGenerationContext context)
    {
      return new WhereBuilder (context.CommandBuilder, DatabaseInfo);
    }

    protected override IFromBuilder CreateFromBuilder (SqlServerGenerationContext context)
    {
      return new FromBuilder (context.CommandBuilder, DatabaseInfo);
    }

    protected override ISelectBuilder CreateSelectBuilder (SqlServerGenerationContext context)
    {
      return new SelectBuilder (context.CommandBuilder);
    }
  }
}