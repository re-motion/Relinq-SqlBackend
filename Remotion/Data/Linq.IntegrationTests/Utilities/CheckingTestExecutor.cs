using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.Utilities
{
  /// <summary>
  /// Checks the queryResult against a previously created resource
  /// </summary>
  public class CheckingTestExecutor : ITestExecutor
  {
    private readonly Func<MethodBase, string> _resourceNameGenerator;

    public CheckingTestExecutor (Func<MethodBase, string> resourceNameGenerator)
    {
      ArgumentUtility.CheckNotNull ("resourceNameGenerator", resourceNameGenerator);
      _resourceNameGenerator = resourceNameGenerator;
    }

    public void Execute (object queryResult, MethodBase executingMethod)
    {
      ArgumentUtility.CheckNotNull ("executingMethod", executingMethod);

      var referenceResult = GetReferenceResult (executingMethod);
      var actualResult = GetActualResult (queryResult);

      var result = new ComparisonResult (referenceResult, actualResult);
      Assert.That (result.IsEqual, Is.True, "The actual results differ from the expected results." + Environment.NewLine + result.GetDiffSet());
    }

    private static string GetActualResult (object queryResult)
    {
      var stringWriter = new StringWriter();
      // Ignore bidirectional associations - we are only interested in the foreign key properties of associations (eg., CategoryID rather than Category)
      var serializer = new TestResultSerializer (stringWriter, info => !info.IsDefined (typeof (System.Data.Linq.Mapping.AssociationAttribute), false));
      serializer.Serialize (queryResult);
      return stringWriter.ToString();
    }

    private string GetReferenceResult (MethodBase executingMethod)
    {
      var resourceName = _resourceNameGenerator (executingMethod);
      using (var resourceStream = executingMethod.DeclaringType.Assembly.GetManifestResourceStream (resourceName))
      {
        using (var streamReader = new StreamReader (resourceStream))
        {
          return streamReader.ReadToEnd();
        }
      }
    }
  }
}