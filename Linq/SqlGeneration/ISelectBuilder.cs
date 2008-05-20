using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration
{
  public interface ISelectBuilder
  {
    void BuildSelectPart (List<IEvaluation> selectEvaluations, bool distinct);
  }
}