using System;
using System.Collections.Generic;
using System.Text;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class SqlServerGenerator : SqlGeneratorBase
  {
    public SqlServerGenerator (QueryExpression query, IDatabaseInfo databaseInfo)
        : base (query, databaseInfo)
    {
    }

    protected override IOrderByBuilder CreateOrderByBuilder (StringBuilder commandText)
    {
      return new OrderByBuilder (commandText);
    }

    protected override IWhereBuilder CreateWhereBuilder (StringBuilder commandText, List<CommandParameter> commandParameters)
    {
      return new WhereBuilder (commandText, commandParameters);
    }

    protected override IFromBuilder CreateFromBuilder (StringBuilder commandText)
    {
      return new FromBuilder (commandText);
    }

    protected override ISelectBuilder CreateSelectBuilder (StringBuilder commandText)
    {
      return new SelectBuilder (commandText);
    }
  }
}