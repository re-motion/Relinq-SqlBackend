using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class LetData
  {
    public LetColumnSource CorrespondingColumnSource;

    public LetData (IEvaluation evaluation, string name,LetColumnSource columnSource)
    {
      ArgumentUtility.CheckNotNull ("evaluation", evaluation);
      ArgumentUtility.CheckNotNull ("name", name);
      ArgumentUtility.CheckNotNull ("columnSource", columnSource);
      
      Evaluation = evaluation;
      Name = name;
      CorrespondingColumnSource = columnSource;
    }

    public IEvaluation Evaluation { get; private set; }
    public string Name { get; private set; }
  }
}