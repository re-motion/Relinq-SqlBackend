using System;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  public class DummyEvaluation : IEvaluation
  {
    public void Accept (IEvaluationVisitor visitor)
    {
      throw new System.NotImplementedException();
    }

  }
}