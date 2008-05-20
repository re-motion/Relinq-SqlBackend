using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class LetData
  {
    public LetColumnSource CorrespondingColumnSource;

    public LetData (List<IEvaluation> evaluations, string name,LetColumnSource columnSource)
    {
      ArgumentUtility.CheckNotNull ("evaluations", evaluations);
      ArgumentUtility.CheckNotNull ("name", name);
      ArgumentUtility.CheckNotNull ("columnSource", columnSource);
      
      Evaluations = evaluations;
      Name = name;
      CorrespondingColumnSource = columnSource;
    }

    public List<IEvaluation> Evaluations { get; private set; }
    public string Name { get; private set; }
  }
}