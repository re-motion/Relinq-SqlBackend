using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration
{
  public interface IFromBuilder
  {
    void BuildFromPart (List<IColumnSource> fromSources, JoinCollection joins);
    void BuildLetPart (List<LetData> lets);
  }
}