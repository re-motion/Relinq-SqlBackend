using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public struct CommandData
  {
    public CommandData (string statement, CommandParameter[] parameters, ConstructorInfo constructor)
        : this()
    {
      ArgumentUtility.CheckNotNull ("statement", statement);
      ArgumentUtility.CheckNotNull ("parameters", parameters);

      Statement = statement;
      Parameters = parameters;
      Constructor = constructor;
    }

    public string Statement { get; private set; }
    public CommandParameter[] Parameters { get; private set; }
    public ConstructorInfo Constructor { get; private set; }
  }
}