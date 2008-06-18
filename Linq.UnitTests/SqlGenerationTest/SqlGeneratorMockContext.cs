using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest
{
  public class SqlGeneratorMockContext : ISqlGenerationContext
  {
    public readonly StringBuilder CommandText = new StringBuilder ();
    public readonly List<CommandParameter> CommandParameters = new List<CommandParameter> ();

    string ISqlGenerationContext.CommandText
    {
      get { return CommandText.ToString(); }
    }

    CommandParameter[] ISqlGenerationContext.CommandParameters
    {
      get { return CommandParameters.ToArray(); }
    }
  }
}