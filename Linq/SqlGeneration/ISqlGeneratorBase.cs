using Remotion.Collections;

namespace Remotion.Data.Linq.SqlGeneration
{
  public interface ISqlGeneratorBase
  {
    Tuple<string, CommandParameter[]> BuildCommandString (QueryModel queryModel);
  }
}