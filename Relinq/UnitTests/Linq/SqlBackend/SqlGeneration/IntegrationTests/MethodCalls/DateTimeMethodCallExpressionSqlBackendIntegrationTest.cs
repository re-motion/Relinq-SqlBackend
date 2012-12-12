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
using Remotion.Linq.SqlBackend;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests.MethodCalls
{
  [TestFixture]
  public class DateTimeMethodCallExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    private DateTime _referenceDate;

    public override void SetUp ()
    {
      base.SetUp ();

      _referenceDate = new DateTime (2012, 12, 07);
    }

    [Test]
    public void Add ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.Add (TimeSpan.FromHours (24)) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, (CONVERT (BigInt, @1 * 864000000000) % 864000000000), "
          + "DATEADD (day, (CONVERT (BigInt, @1 * 86400000) / 86400000, [t0].[DateOfIncorporation]))) <= @1)",
          new CommandParameter ("@1", _referenceDate));
    }

    [Test]
    public void AddDays ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddDays (1.5) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, (CONVERT (BigInt, @1 * 86400000) % 86400000), "
          + "DATEADD (day, (CONVERT (BigInt, @1), [t0].[DateOfIncorporation]))) <= @1)",
          new CommandParameter ("@1", 1.5),
          new CommandParameter ("@2", _referenceDate));
    }

    [Test]
    public void AddHours ()
    {
      CheckQuery (
          from c in Companies where c.DateOfIncorporation.AddHours (1.5) <= _referenceDate select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CompanyTable] AS [t0] "
          + "WHERE (DATEADD (millisecond, (CONVERT (BigInt, @1 * 3600000) % 3600000), "
          + "DATEADD (hour, (CONVERT (BigInt, @1), [t0].[DateOfIncorporation]))) <= @1)",
          new CommandParameter ("@1", 1.5),
          new CommandParameter ("@2", _referenceDate));
    }
  }
}