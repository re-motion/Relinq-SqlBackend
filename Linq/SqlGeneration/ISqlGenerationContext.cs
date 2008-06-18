namespace Remotion.Data.Linq.SqlGeneration
{
  public interface ISqlGenerationContext
  {
    string CommandText { get; }
    CommandParameter[] CommandParameters { get; }
  }
}