using System;
using System.Reflection;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  public class TestBase : AbstractTestBase
  {
    protected override Func<MethodBase, string> SavedResourceFileNameGenerator
    {
      // C# will automatically add the folder structure to the resource file name when embedding a resource
      // The desired resource name is: Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101.Resources.TestClass.TestMethod.result
      // This is achieved by putting a file called "TestClass.TestMethod.result" into the LinqSamples101\Resources folder
      get { return method => method.DeclaringType.Name + "." + method.Name + ".result"; }
    }

    protected override Func<MethodBase, string> LoadedResourceNameGenerator
    {
      // When loading the resource, we must specify the full name as described above
      get { return method => method.DeclaringType.Namespace + ".Resources." + method.DeclaringType.Name + "." + method.Name + ".result"; }
    }
  }
}