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
    [Ignore ("TODO 2370")]
    public void DistinctAndCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct().Count(),
          "SELECT COUNT(*) FROM (SELECT DISTINCT [t0].[FirstName] FROM [CookTable] AS [t0]) AS [t1]");
    }

    [Test]
    public void DistinctAndTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct().Take (5),
          "SELECT DISTINCT TOP (@1) [t0].[FirstName] AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    [Ignore ("TODO 2370")]
    public void TakeAndDistinct ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Distinct(),
          "SELECT DISTINCT [t1].[FirstName] AS [value] FROM (SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]) AS [t1]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    [Ignore ("TODO 2370")]
    public void TakeAndCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Count(),
          "SELECT COUNT(*) FROM (SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]) AS [t1]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    [Ignore ("TODO 2370")]
    public void TakeAndTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Take (3),
          "SELECT TOP @2 [t1].[FirstName] FROM (SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]) AS [t1]",
          new CommandParameter ("@1", 5),
          new CommandParameter ("@2", 3));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (3).Take (5),
          "SELECT TOP @2 [t1].[FirstName] FROM (SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]) AS [t1]",
          new CommandParameter ("@1", 3),
          new CommandParameter ("@2", 5));
    }

    [Test]
    [Ignore ("TODO 2370")]
    public void TakeAndFirst_TakeAndSingle ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).First(),
          "SELECT TOP @2 [t1].[FirstName] FROM (SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]) AS [t1]",
          new CommandParameter ("@1", 5),
          new CommandParameter ("@2", 1));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Single(),
          "SELECT TOP @2 [t1].[FirstName] FROM (SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]) AS [t1]",
          new CommandParameter ("@1", 5),
          new CommandParameter ("@2", 1));
    }

    [Test]
    [Ignore ("TODO 2370")]
    public void TakeAndFirst_TakeAndSingle_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).First (fn => fn != null),
          "SELECT TOP @2 [t1].[FirstName] FROM (SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]) AS [t1] WHERE ([t1].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 5),
          new CommandParameter ("@2", 1));

      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5).Single (fn => fn != null),
          "SELECT TOP @2 [t1].[FirstName] FROM (SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]) AS [t1] WHERE ([t1].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 5),
          new CommandParameter ("@2", 1));
    }
  }
}