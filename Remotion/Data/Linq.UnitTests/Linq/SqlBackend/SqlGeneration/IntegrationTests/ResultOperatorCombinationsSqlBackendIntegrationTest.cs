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
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class ResultOperatorCombinationsSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    // TODO Review 2441: Solve this
    [Ignore ("TODO 2441")]
    public void DistinctAndCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct().Count(),
          "SELECT DISTINCT COUNT(*) FROM FROM [CookTable] AS [t0]");
    }

    [Test]
    public void DistinctAndTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct().Take (5),
          "SELECT DISTINCT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 5));
    }

    // TODO Review 2441: Missing: DistinctAndFirst, DistinctAndSingle

    [Test]
    public void TakeAndDistinct ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Distinct(),
          "SELECT DISTINCT [q0].[value] AS [value] FROM (SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void TakeAndCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Count(),
          "SELECT COUNT(*) FROM (SELECT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void TakeAndTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Take (3),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q0]",
          new CommandParameter ("@1", 3),
          new CommandParameter ("@2", 5));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (3).Take (5),
          "SELECT TOP (@1) [q1].[value] AS [value] FROM (SELECT TOP (@2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q1]",
          new CommandParameter ("@1", 5),
          new CommandParameter ("@2", 3));
    }

    [Test]
    public void TakeAndFirst_TakeAndSingle ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).First(),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q0]",
          new CommandParameter ("@1", 1),
          new CommandParameter ("@2", 5));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Single(),
          "SELECT TOP (@1) [q1].[value] AS [value] FROM (SELECT TOP (@2) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]) AS [q1]",
          new CommandParameter ("@1", 1),
          new CommandParameter ("@2", 5));
    }

    [Test]
    public void TakeAndFirst_TakeAndSingle_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).First (fn => fn != null),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[value] IS NOT NULL)",
          new CommandParameter ("@1", 1),
          new CommandParameter ("@2", 5));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Single (fn => fn != null),
          "SELECT TOP (@1) [q0].[value] AS [value] FROM (SELECT TOP (@2) [t1].[FirstName] AS [value] FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[value] IS NOT NULL)",
          new CommandParameter ("@1", 1),
          new CommandParameter ("@2", 5));
    }

    // TODO Review 2441: Missing: TakeAndTakeAndTake
  }
}