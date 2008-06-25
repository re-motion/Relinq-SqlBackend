using Remotion.Collections;
using Remotion.Data.Linq.Parsing.Details;

namespace Remotion.Data.Linq.SqlGeneration
{
  public interface ISqlGeneratorBase
  {
    Tuple<string, CommandParameter[]> BuildCommandString (QueryModel queryModel);
    DetailParser DetailParser { get; }
  }
}