using System.Collections.Generic;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public interface ISelectBuilder
  {
    void BuildSelectPart (List<IEvaluation> selectEvaluations, bool distinct);
  }
}