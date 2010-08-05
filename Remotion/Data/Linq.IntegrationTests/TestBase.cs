using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.IntegrationTests.UnitTests.Utilities;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests
{
  public class TestBase 
  {
    public TestMode Mode = TestMode.SaveReferenceResults;
    private INorthwindDataProvider _db;
    private ITestExecutor _testExecutor;

    protected INorthwindDataProvider DB
    {
      get { return _db; }
    }

    protected ITestExecutor TestExecutor 
    {
      get { return _testExecutor; }
    }
   

    [SetUp]
    public virtual void SetUp ()
    {
      if (Mode == TestMode.SaveReferenceResults)
      {
        _db = new LinqToSqlNorthwindDataProvider ();
        var directory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "SavedResults");
        _testExecutor = new SavingTestExecutor (directory);
      }
      else
      {
        _db = new RelinqNorthwindDataProvider ();
        _testExecutor = new CheckingTestExecutor ();
      }
    }
  }
}
