// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class NullTests:TestBase
  {
    /// <summary>
    /// This sample uses the null value to find Employees that do not report to another Employee.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - InvalidCastException - Unable to cast System.Byte[] to System.Data.Linq.Binary")]
    public void LinqToSqlNull01 ()
    {
      var q =
          from e in DB.Employees
          where e.ReportsToEmployee == null
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Nullable<T>.HasValue to find Employees that do not report to another Employee.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - System.NotImplementedException - Implement if needed by integration tests")]
    public void LinqToSqlNull02 ()
    {
      var q =
          from e in DB.Employees
          where !e.ReportsTo.HasValue
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Nullable<T>.Value for Employees that report to another Employee to 
    /// return the EmployeeID number of that employee. Note that the .Value is optional.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - System.NotImplementedException - Implement if needed by integration tests")]
    public void LinqToSqlNull03 ()
    {
      var q =
          from e in DB.Employees
          where e.ReportsTo.HasValue
          select new { e.FirstName, e.LastName, ReportsTo = e.ReportsTo.Value };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}
