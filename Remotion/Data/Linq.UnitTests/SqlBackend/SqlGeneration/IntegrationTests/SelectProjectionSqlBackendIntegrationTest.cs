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
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class SelectProjectionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Entity ()
    {
      CheckQuery (
          from s in Cooks select s,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          + "FROM [CookTable] AS [t0]");
    }

    [Test]
    public void Constant ()
    {
      CheckQuery (
          from k in Kitchens select "hugo",
          "SELECT @1 FROM [KitchenTable] AS [t0]",
          new CommandParameter ("@1", "hugo"));
    }

    [Test]
    public void Null ()
    {
      CheckQuery (
          Kitchens.Select<Kitchen, object> (k => null),
          "SELECT NULL FROM [KitchenTable] AS [t0]");
    }

    [Test]
    public void True ()
    {
      CheckQuery (
          from k in Kitchens select true,
          "SELECT @1 FROM [KitchenTable] AS [t0]",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void False ()
    {
      CheckQuery (
          from k in Kitchens select false,
          "SELECT @1 FROM [KitchenTable] AS [t0]",
          new CommandParameter ("@1", 0));
    }

    [Test]
    [Ignore ("TODO 2362")]
    public void BooleanConditions ()
    {
      CheckQuery (
          from k in Kitchens select k.Name == "SpecialKitchen",
          "SELECT CASE WHEN [t0].[Name] = @1 THEN 1 ELSE 0 END FROM [KitchenTable] AS [t0]",
          new CommandParameter ("@1", 0));
    }
  }
}