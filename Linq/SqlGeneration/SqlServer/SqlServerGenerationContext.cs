using System.Collections.Generic;
using System.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class SqlServerGenerationContext : ISqlGenerationContext
  {
    public SqlServerGenerationContext (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      CommandBuilder = new CommandBuilder (new StringBuilder(), new List<CommandParameter>(), databaseInfo);
    }

    public SqlServerGenerationContext (CommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      CommandBuilder = commandBuilder;
    }

    public CommandBuilder CommandBuilder { get; private set; }

    public string CommandText
    {
      get { return CommandBuilder.GetCommandText(); }
    }

    public CommandParameter[] CommandParameters
    {
      get { return CommandBuilder.GetCommandParameters (); }
    }
  }
}