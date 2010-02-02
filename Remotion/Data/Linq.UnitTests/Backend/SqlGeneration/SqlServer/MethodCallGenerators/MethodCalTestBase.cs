using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer.MethodCallGenerators
{
  public class MethodCalTestBase
  {
    [SetUp]
    public virtual void SetUp ()
    {
      CommandText = new StringBuilder ();
      CommandText.Append ("xyz ");

      DefaultParameter = new CommandParameter ("abc", 5);
      CommandParameters = new List<CommandParameter> { DefaultParameter };
      
      CommandBuilder = new CommandBuilder (
          new SqlServerGenerator (StubDatabaseInfo.Instance),
          CommandText,
          CommandParameters,
          StubDatabaseInfo.Instance,
          new MethodCallSqlGeneratorRegistry ());
    }

    public StringBuilder CommandText { get; private set; }
    public CommandParameter DefaultParameter { get; private set; }
    public List<CommandParameter> CommandParameters { get; private set; }
    public CommandBuilder CommandBuilder { get; private set; }
  }
}