using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public struct QueryParameter
  {
    public readonly string Name;
    public readonly object Value;

    public QueryParameter (string name, object value)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("value", value);

      Name = name;
      Value = value;
    }
  }
}