using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests.Utilities
{
  class CheckingTestExecutor : ITestExecutor
  {
    public void Execute (object queryResult, MethodBase executingMethod)
    {
      var referenceResult = GetReferenceResult (executingMethod);
      var actualResult = GetActualResult (queryResult);

      TestResultChecker.Check (referenceResult, actualResult);
    }

    private string GetActualResult (object queryResult)
    {
      var stringWriter = new StringWriter ();
      var serializer = new TestResultSerializer (stringWriter);
      serializer.Serialize (queryResult);
      return stringWriter.ToString ();
    }

    private string GetReferenceResult (MethodBase executingMethod)
    {
      var resourceName = executingMethod.DeclaringType.FullName + "." + executingMethod + ".result";
      using (var resourceStream = executingMethod.DeclaringType.Assembly.GetManifestResourceStream (resourceName))
      using (var streamReader = new StreamReader (resourceStream))
      {
        return streamReader.ReadToEnd ();
      }
    }
  }
}
