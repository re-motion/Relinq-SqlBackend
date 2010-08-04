using System;
using System.IO;
using System.Reflection;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class Executor
  {
    private readonly static string connString = "Data Source=localhost;Initial Catalog=Northwind; Integrated Security=SSPI;";

    protected static Northwind db;

    public static void Main ()
    {
      InitSample();
      CallAllMethods (typeof (GroupWhere));
      Console.Read ();
    }

    private static void InitSample ()
    {
      // Creates a new Northwind object to start fresh with an empty object cache
      // Active ADO.NET connection will be reused by new Northwind object

      TextWriter oldLog;
      if (db == null)
        oldLog = null;
      else
        oldLog = db.Log;

      db = new Northwind (connString) { Log = oldLog };
    }

    private static void CallAllMethods (Type testClass)
    {
      MethodInfo[] methods = testClass.GetMethods ();
      foreach (var methodInfo in methods)
      {
        object instance = Activator.CreateInstance (testClass);

        if (methodInfo.Name.Contains ("LinqToSql"))
        {
          Console.WriteLine ("Call: " + methodInfo.Name);
          methodInfo.Invoke (instance, null);
        }
      }
    }
  }
}
