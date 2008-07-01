using Remotion.Collections;
using Remotion.Data.Linq.Parsing.Details;

namespace Remotion.Data.Linq.SqlGeneration
{
  public interface ISqlGeneratorBase
  {
    CommandData BuildCommand (QueryModel queryModel);
    DetailParser DetailParser { get; }
  }
}