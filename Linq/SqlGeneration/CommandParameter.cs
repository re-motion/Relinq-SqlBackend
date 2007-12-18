using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public struct CommandParameter
  {
    public readonly string Name;
    public readonly object Value;

    public CommandParameter (string name, object value)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      Name = name;
      Value = value;
    }
  }
}