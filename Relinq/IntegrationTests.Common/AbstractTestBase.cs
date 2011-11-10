// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind;
using Remotion.Linq.IntegrationTests.Common.Utilities;

namespace Remotion.Linq.IntegrationTests.Common
{
  public abstract class AbstractTestBase 
  {
    private INorthwindDataProvider _db;
    private ITestExecutor _testExecutor;

    protected abstract Func<MethodBase, string> SavedResultFileNameGenerator { get; }
    protected abstract Func<MethodBase, string> LoadedResultFileNameGenerator { get; }

    protected INorthwindDataProvider DB
    {
      get { return _db; }
    }

    protected ITestExecutor TestExecutor 
    {
      get { return _testExecutor; }
    }

    public virtual TestMode Mode
    {
      get { return TestMode.CheckActualResults; }
    }

    [SetUp]
    public virtual void SetUp ()
    {
      if (Mode == TestMode.SaveReferenceResults)
      {
        _db = new LinqToSqlNorthwindDataProvider ();
        var directory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "SavedResults");
        _testExecutor = new SavingTestExecutor (directory, SavedResultFileNameGenerator);
      }
      else
      {
        _db = new RelinqNorthwindDataProvider ();
        _testExecutor = new CheckingTestExecutor (LoadedResultFileNameGenerator);
      }
    }

  }
}
