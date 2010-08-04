using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class Executor
  {
    private readonly static string connString = "Data Source=localhost;Initial Catalog=Northwind; Integrated Security=SSPI;";

    protected static Northwind db;
    protected static TestResultSerializer serializer;

    public static void Main ()
    {
      InitSample();
      //CallAllMethods (typeof (GroupWhere));
      //CallAllMethods (typeof (GroupSelectDistinct));
      //CallAllMethods (typeof (GroupAggregates));
      //CallAllMethods (typeof (GroupJoin));
      //CallAllMethods (typeof (GroupOrderBy));
      //CallAllMethods (typeof (GroupGroupByHaving));
      //CallAllMethods (typeof (GroupExistsInAnyAllContains));
      //CallAllMethods (typeof (GroupUnionAllIntersect));
      CallAllMethods (typeof (GroupTopBottom));

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
      serializer = new TestResultSerializer (Console.Out,memberInfo => !memberInfo.IsDefined (typeof(System.Data.Linq.Mapping.AssociationAttribute), false));//new StreamWriter ("C:\\csharpTestOut.txt"));
    }

    private static void CallAllMethods (Type testClass)
    {
      MethodInfo[] methods = testClass.GetMethods ();
      foreach (var methodInfo in methods)
      {
        object instance = Activator.CreateInstance (testClass);

        if (methodInfo.Name.Contains ("LinqToSql"))
        {
          Debug.WriteLine ("Call: " + methodInfo.Name);
          methodInfo.Invoke (instance, null);
        }
      }
    }
  }
}
