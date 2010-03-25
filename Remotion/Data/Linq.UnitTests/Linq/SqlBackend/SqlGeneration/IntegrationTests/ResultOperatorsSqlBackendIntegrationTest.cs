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
using Remotion.Data.Linq.Backend.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class ResultOperatorsSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void SimpleCount ()
    {
      CheckQuery (
          () => (from c in Cooks select c).Count(),
          "SELECT COUNT(*) FROM [CookTable] AS [t0]");
    }

    [Test]
    public void CountWithProperty ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Count(),
          "SELECT COUNT(*) FROM [CookTable] AS [t0]");
    }

    [Test]
    public void CountWithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Count (name => name != null),
          "SELECT COUNT(*) FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)");
    }

    [Test]
    public void SimpleDistinct ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Distinct(),
          "SELECT DISTINCT [t0].[FirstName] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void SimpleTake ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Take (5),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    [Ignore ("TODO 2408")]
    public void TakeWithMemberExpression ()
    {
      CheckQuery (
          () => (from k in Kitchens from c in k.Restaurant.Cooks.Take (k.RoomNumber) select k.Name),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 5));
    }

    [Test]
    public void First ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).First(),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).FirstOrDefault(),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void First_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).First (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).FirstOrDefault (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void Single ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Single(),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).SingleOrDefault(),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void Single_WithPredicate ()
    {
      CheckQuery (
          () => (from c in Cooks select c.FirstName).Single (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
      CheckQuery (
          () => (from c in Cooks select c.FirstName).SingleOrDefault (fn => fn != null),
          "SELECT TOP (@1) [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[FirstName] IS NOT NULL)",
          new CommandParameter ("@1", 1));
    }
  }
}