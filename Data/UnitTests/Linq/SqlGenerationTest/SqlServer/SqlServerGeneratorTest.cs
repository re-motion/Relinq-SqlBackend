using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;

namespace Remotion.Data.UnitTests.Linq.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class SqlServerGeneratorTest
  {
    [Test]
    public void DefaultMethodCallRegistration ()
    {
      MethodCallSqlGeneratorRegistry methodCallSqlGeneratorRegistry = new SqlServerGenerator (StubDatabaseInfo.Instance).MethodCallRegistry;

      IMethodCallSqlGenerator removeGenerator =
          methodCallSqlGeneratorRegistry.GetGenerator (typeof (string).GetMethod ("Remove", new Type[] { typeof (int) }));

      IMethodCallSqlGenerator upperGenerator =
          methodCallSqlGeneratorRegistry.GetGenerator (typeof (string).GetMethod ("ToUpper", new Type[] { }));

      Assert.IsNotNull (removeGenerator);
      Assert.IsNotNull (upperGenerator);
    }
  }
}