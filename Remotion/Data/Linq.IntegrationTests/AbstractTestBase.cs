using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests
{
  public abstract class AbstractTestBase 
  {
    public TestMode Mode = TestMode.CheckActualResults;

    private INorthwindDataProvider _db;
    private ITestExecutor _testExecutor;

    protected abstract Func<MethodBase, string> SavedResourceFileNameGenerator { get; }
    protected abstract Func<MethodBase, string> LoadedResourceNameGenerator { get; }

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
        _testExecutor = new SavingTestExecutor (directory, SavedResourceFileNameGenerator);
      }
      else
      {
        _db = new RelinqNorthwindDataProvider ();
        _testExecutor = new CheckingTestExecutor (LoadedResourceNameGenerator);
      }
    }

  }
}
