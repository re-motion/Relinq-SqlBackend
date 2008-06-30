using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public struct CommandData
  {
    public CommandData (string statement, CommandParameter[] parameters, SqlGenerationData sqlGenerationData)
        : this()
    {
      ArgumentUtility.CheckNotNull ("statement", statement);
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("sqlGenerationData", sqlGenerationData);
      
      Statement = statement;
      Parameters = parameters;
      SqlGenerationData = sqlGenerationData;
    }

    public string Statement { get; private set; }
    public CommandParameter[] Parameters { get; private set; }
    public SqlGenerationData SqlGenerationData { get; private set; }
  }
}