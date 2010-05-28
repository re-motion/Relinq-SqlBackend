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
  public class OrderByClausesSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void OneOrderByClause ()
    {
      CheckQuery (
          from s in Cooks orderby s.Name select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] ORDER BY [t0].[Name] ASC");
    }

    [Test]
    public void SeveralOrderings ()
    {
      CheckQuery (
          from s in Cooks orderby s.Name , s.FirstName descending , s.Weight select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] ORDER BY [t0].[Name] ASC, [t0].[FirstName] DESC, [t0].[Weight] ASC");
    }

    [Test]
    public void SeveralOrderByClauses ()
    {
      CheckQuery (
          from s in Cooks
          orderby s.Name , s.FirstName descending , s.Weight
          orderby s.ID , s.IsFullTimeCook descending
          select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] "
          + "ORDER BY [t0].[ID] ASC, [t0].[IsFullTimeCook] DESC, [t0].[Name] ASC, [t0].[FirstName] DESC, [t0].[Weight] ASC");
    }

    [Test]
    public void SeveralOrderByClauses_SeparatedByOtherClauses ()
    {
      CheckQuery (
          from s in Cooks
          orderby s.Name
          where s.FirstName != null
          orderby s.ID
          select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] "
          + "WHERE ([t0].[FirstName] IS NOT NULL) "
          + "ORDER BY [t0].[ID] ASC, [t0].[Name] ASC");
    }

    [Test]
    public void WithConstantExpression ()
    {
      CheckQuery (
          from s in Cooks orderby 1 select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] ORDER BY (SELECT @1) ASC",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void WithConstantExpression_InComplexExpression ()
    {
      CheckQuery (
          from s in Cooks orderby s.Name + " " + s.FirstName select s.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] ORDER BY (([t0].[Name] + @1) + [t0].[FirstName]) ASC",
          new CommandParameter ("@1", " "));
    }

    [Test]
    public void AutomaticOrderByHandlingInSubStatements_InSelectClause_WithoutTopExpression ()
    {
      CheckQuery (
          from k in Kitchens 
          from c in (from sc in Cooks orderby sc.Name select sc) select c,
          "SELECT [q0].[ID],[q0].[FirstName],[q0].[Name],[q0].[IsStarredCook],[q0].[IsFullTimeCook],[q0].[SubstitutedID],[q0].[KitchenID] "+
          "FROM [KitchenTable] AS [t1] CROSS APPLY (SELECT [t2].[ID],[t2].[FirstName],[t2].[Name],[t2].[IsStarredCook],[t2].[IsFullTimeCook],"+
          "[t2].[SubstitutedID],[t2].[KitchenID] FROM [CookTable] AS [t2]) AS [q0]");
    }

    [Test]
    public void AutomaticOrderByHandlingInSubStatements_InSelectClause_WithTopExpression ()
    {
      CheckQuery (
          from k in Kitchens
          from c in
            (from sc in Cooks orderby sc.Name select sc).Take(10)
          select c,
          "SELECT [q0].[get_Key_ID],[q0].[get_Key_FirstName],[q0].[get_Key_Name],[q0].[get_Key_IsStarredCook],[q0].[get_Key_IsFullTimeCook],"+
          "[q0].[get_Key_SubstitutedID],[q0].[get_Key_KitchenID] FROM [KitchenTable] AS [t1] CROSS APPLY (SELECT TOP (@1) [t2].[ID] AS [get_Key_ID],"+
          "[t2].[FirstName] AS [get_Key_FirstName],[t2].[Name] AS [get_Key_Name],[t2].[IsStarredCook] AS [get_Key_IsStarredCook],"+
          "[t2].[IsFullTimeCook] AS [get_Key_IsFullTimeCook],[t2].[SubstitutedID] AS [get_Key_SubstitutedID],[t2].[KitchenID] AS [get_Key_KitchenID],"+
          "[t2].[Name] AS [get_Value_get_Key],NULL AS [get_Value_get_Value] FROM [CookTable] AS [t2] ORDER BY [t2].[Name] ASC) AS [q0] "+
          "ORDER BY [q0].[get_Value_get_Key] ASC",
          new CommandParameter("@1", 10));
    }

    [Test]
    public void AutomaticOrderByHandlingInSubStatements_InWhereClause_WithTopExpression ()
    {
      CheckQuery (
          from c in Cooks
          where c.Name== (from sc in Cooks orderby sc.Name select sc.Name).Single()
          select c,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] " +
          "FROM [CookTable] AS [t0] WHERE ([t0].[Name] = (SELECT TOP (@1) [t1].[Name] AS [value] FROM [CookTable] AS [t1] " +
          "ORDER BY [t1].[Name] ASC))",
          new CommandParameter("@1", 2));
    }

    [Test]
    public void AutomaticOrderByHandlingInSubStatements_InWhereClause_WithoutTopExpression ()
    {
      CheckQuery (
          (from c in Cooks
          where (from sc in Cooks orderby sc.Name select sc).Contains(c)
          select c),
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "+
          "FROM [CookTable] AS [t0] WHERE [t0].[ID] IN (SELECT [t1].[ID] FROM [CookTable] AS [t1])");
    }
  }
}