using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  public class DummyEvaluation : IEvaluation
  {
    public void Accept (IEvaluationVisitor visitor)
    {
      throw new System.NotImplementedException();
    }
  }
}