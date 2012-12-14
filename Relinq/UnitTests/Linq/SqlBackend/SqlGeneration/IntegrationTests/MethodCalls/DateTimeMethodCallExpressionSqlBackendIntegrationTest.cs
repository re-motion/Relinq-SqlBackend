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
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests.MethodCalls
{
  [TestFixture]
  [Ignore ("TODO 5242")]
  public class DateTimeMethodCallExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    private DateTime _referenceDate;

    public override void SetUp ()
    {
      base.SetUp ();

      _referenceDate = new DateTime (2012, 12, 07);
    }

    [Test]
    public void AddYears ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddYears (17) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (year, @1, [t0].[DateOfIncorporation]) <= @2)",
          new CommandParameter ("@1", 17),
          new CommandParameter ("@2", _referenceDate));
    }

    [Test]
    public void AddMonths ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddMonths (17) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (month, @1, [t0].[DateOfIncorporation]) <= @2)",
          new CommandParameter ("@1", 17),
          new CommandParameter ("@2", _referenceDate));
    }

    [Test]
    public void AddDays ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddDays (12.13) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, CONVERT (BIGINT,(@1 * 86400000)), [t0].[DateOfIncorporation]) <= @2)",
          new CommandParameter ("@1", 12.13),
          new CommandParameter ("@2", _referenceDate));
    }

    [Test]
    public void AddHours ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddHours (12.13) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, CONVERT (BIGINT,(@1 * 3600000)), [t0].[DateOfIncorporation]) <= @2)",
          new CommandParameter ("@1", 12.13),
          new CommandParameter ("@2", _referenceDate));
    }

    [Test]
    public void AddMinutes ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddMinutes (12.13) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, CONVERT (BIGINT,(@1 * 60000)), [t0].[DateOfIncorporation]) <= @2)",
          new CommandParameter ("@1", 12.13),
          new CommandParameter ("@2", _referenceDate));
    }

    [Test]
    public void AddSeconds ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddSeconds (12.13) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, CONVERT (BIGINT,(@1 * 1000)), [t0].[DateOfIncorporation]) <= @2)",
          new CommandParameter ("@1", 12.13),
          new CommandParameter ("@2", _referenceDate));
    }

    [Test]
    public void AddMilliseconds ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddMilliseconds (12.13) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, CONVERT (BIGINT,@1)) <= @2)",
          new CommandParameter ("@1", 12.13),
          new CommandParameter ("@2", _referenceDate));
    }

    [Test]
    public void AddTicks ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddTicks (12) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, CONVERT (BIGINT,(@1 / 10000))) <= @2)",
          new CommandParameter ("@1", 12),
          new CommandParameter ("@2", _referenceDate));
    }

    [Test]
    public void Add_Timestamp ()
    {
      var timespan = TimeSpan.FromDays (24.1703);
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.Add (timespan) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, (CONVERT (BigInt, @1), [t0].[DateOfIncorporation])) <= @2)",
          new CommandParameter ("@1", 12),
          new CommandParameter ("@1", timespan.TotalMilliseconds));
    }
  }
}