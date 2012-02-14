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

    [Test]
    [Ignore ("TODO 4632: Fix handling of bool? in SqlContextExpressionVisitor")]
    public void NullableBool_CastToBool ()
    {
      CheckQuery (
          from k in Kitchens where (bool) k.PassedLastInspection select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[PassedLastInspection] = @1)",
          new CommandParameter ("@1", 1));

      CheckQuery (
// ReSharper disable RedundantBoolCompare
          from k in Kitchens where k.PassedLastInspection != null && ((bool) k.PassedLastInspection) == true select k.ID,
// ReSharper restore RedundantBoolCompare
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] IS NOT NULL) AND ([t0].[PassedLastInspection] = @1))",
          new CommandParameter ("@1", 1));
    }

    [Test]
    [Ignore ("TODO 4632: Fix handling of bool? in SqlContextExpressionVisitor")]
    public void NullableBool_HasValue_Value ()
    {
      CheckQuery (
          from k in Kitchens where k.PassedLastInspection.Value select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[PassedLastInspection] = @1)",
          new CommandParameter ("@1", 1));

      CheckQuery (
          from k in Kitchens where k.PassedLastInspection.HasValue && k.PassedLastInspection.Value select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] IS NOT NULL) AND ([t0].[PassedLastInspection] = @1))",
          new CommandParameter ("@1", 1));
      CheckQuery (
// ReSharper disable RedundantBoolCompare
          from k in Kitchens where k.PassedLastInspection.HasValue && k.PassedLastInspection.Value == true select k.ID,
// ReSharper restore RedundantBoolCompare
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] IS NOT NULL) AND ([t0].[PassedLastInspection] = @1))",
          new CommandParameter ("@1", 1));
    }

    [Test]
    [Ignore ("TODO 4632: Fix handling of bool? in SqlContextExpressionVisitor")]
    public void NullableBool_Compare ()
    {
      CheckQuery (
          from k in Kitchens where k.PassedLastInspection == true select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[PassedLastInspection] = @1)",
          new CommandParameter ("@1", 1));

      CheckQuery (
          from k in Kitchens where k.PassedLastInspection == false select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[PassedLastInspection] = @1)",
          new CommandParameter ("@1", 0));

      CheckQuery (
          from k in Kitchens where k.PassedLastInspection == null select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[PassedLastInspection] IS NULL)");
    }
  }
}