using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  // If a fixedCommandBuilder is specified, the SqlServerGenerator can only be used to create one query from one thread. Otherwise, it is
  // stateless and can be used for multiple queries from multiple threads.
  public class InlineSqlServerGenerator : SqlServerGenerator
  {
    private readonly CommandBuilder _fixedCommandBuilder;

    public InlineSqlServerGenerator (IDatabaseInfo databaseInfo, CommandBuilder fixedCommandBuilder, ParseContext parseContext)
      : base (databaseInfo, parseContext)
    {
      ArgumentUtility.CheckNotNull ("fixedCommandBuilder", fixedCommandBuilder);
      _fixedCommandBuilder = fixedCommandBuilder;
    }

    protected override SqlServerGenerationContext CreateContext ()
    {
      return new SqlServerGenerationContext (_fixedCommandBuilder);
    }
  }
}