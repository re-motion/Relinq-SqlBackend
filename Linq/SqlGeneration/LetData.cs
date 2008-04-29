using System.Collections.Generic;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class LetData
  {
    public LetData (List<IEvaluation> evaluations, string name)
    {
      ArgumentUtility.CheckNotNull ("evaluations", evaluations);
      ArgumentUtility.CheckNotNull ("name", name);

      Evaluations = evaluations;
      Name = name;
    }

    public List<IEvaluation> Evaluations { get; private set; }
    public string Name { get; private set; }
  }
}