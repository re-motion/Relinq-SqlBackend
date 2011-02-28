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

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class NullableExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void HasValue ()
    {
      CheckQuery (
          from k in Kitchens select k.LastCleaningDay.HasValue,
          "SELECT CASE WHEN ([t0].[LastCleaningDay] IS NOT NULL) THEN 1 ELSE 0 END AS [value] FROM [KitchenTable] AS [t0]");
      CheckQuery (
          from k in Kitchens select !k.LastCleaningDay.HasValue,
          "SELECT CASE WHEN NOT ([t0].[LastCleaningDay] IS NOT NULL) THEN 1 ELSE 0 END AS [value] FROM [KitchenTable] AS [t0]");
    }

    [Test]
    public void Value ()
    {
      CheckQuery (
          from k in Kitchens select k.LastCleaningDay.Value,
          "SELECT [t0].[LastCleaningDay] AS [value] FROM [KitchenTable] AS [t0]");
    }
  }
}