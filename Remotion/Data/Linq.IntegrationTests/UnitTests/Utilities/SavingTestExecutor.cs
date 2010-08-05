using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests.Utilities
{
  class SavingTestExecutor : ITestExecutor 
  {
    private string _directory;

    public SavingTestExecutor (string directory)
    {
      // TODO: Complete member initialization
      this._directory = directory;
    }
    public void Execute (object queryResult, MethodBase executingMethod)
    {
      throw new NotImplementedException ();
    }
  }
}
