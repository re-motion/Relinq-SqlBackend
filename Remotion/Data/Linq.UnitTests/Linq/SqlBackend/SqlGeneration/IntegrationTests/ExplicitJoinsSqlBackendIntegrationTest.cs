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
  public class ExplicitJoinsSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void ExplicitJoin ()
    {
      CheckQuery (
          from k in Kitchens join c in Cooks on k.Name equals c.FirstName select k.Name,
          "SELECT [t0].[Name] AS [value] FROM [KitchenTable] AS [t0] CROSS JOIN [CookTable] AS [t1] WHERE ([t0].[Name] = [t1].[FirstName])"
          );
    }

    [Test]
    public void ExplicitJoin_DependentExpressions ()
    {
      CheckQuery (
          from k in Kitchens join c in Cooks on k.Cook.ID equals c.ID select k.Name,
          "SELECT [t0].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t2] ON [t0].[ID] = [t2].[KitchenID] "
          +"CROSS JOIN [CookTable] AS [t1] WHERE ([t2].[ID] = [t1].[ID])"
          );
    }

    [Test]
    public void ExplicitJoinWithInto_Once ()
    {
      CheckQuery (
          from k in Kitchens
          join c in Cooks on k.Cook equals c into gkc
          from kc in gkc
          select kc.Name,
          "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t2] ON [t0].[ID] = [t2].[KitchenID] "+
          "CROSS JOIN [CookTable] AS [t1] WHERE ([t2].[ID] = [t1].[ID])"
          );
    }

    [Test]
    public void ExplicitJoinWithInto_Twice ()
    {
      CheckQuery (
          from k in Kitchens
          join c in Cooks on k.Cook equals c into gkc
          join r in Restaurants on k.Restaurant equals r into gkr
          from kc in gkc
          from kr in gkr 
          select kr.ID,
          "SELECT [t2].[ID] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t3] ON [t0].[ID] = [t3].[KitchenID] "+
          "LEFT OUTER JOIN [RestaurantTable] AS [t4] ON [t0].[RestaurantID] = [t4].[ID] CROSS JOIN [CookTable] AS [t1] "+
          "CROSS JOIN [RestaurantTable] AS [t2] WHERE (([t3].[ID] = [t1].[ID]) AND ([t4].[ID] = [t2].[ID]))"
          );
    }

    [Test]
    public void ExplicitJoinWithInto_UseIntoVariableTwiceInSameStatement ()
    {
      CheckQuery (
          from k in Kitchens
          join c in Cooks on k.Cook equals c into gkc
          from kc in gkc
          where kc.Name == "Steiner"
          select kc.FirstName,
          "SELECT [t1].[FirstName] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t2] ON [t0].[ID] = [t2].[KitchenID] "+
          "CROSS JOIN [CookTable] AS [t1] WHERE (([t2].[ID] = [t1].[ID]) AND ([t1].[Name] = @1))",
          new CommandParameter("@1", "Steiner")
          );
    }

    [Test]
    public void ExplicitJoinWithInto_UseIntoVariableTwiceInSameStatementAndInSubStatement ()
    {
      CheckQuery(
      from k in Kitchens
          join c in Cooks on k.Cook equals c into gkc
          from kc in gkc
           where k.Name == 
            (from sk in Kitchens 
             from skc in gkc select skc.Name).First ()
      select kc.Name, 
      "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t4] ON [t0].[ID] = [t4].[KitchenID] "+
      "CROSS JOIN [CookTable] AS [t1] WHERE (([t4].[ID] = [t1].[ID]) AND ([t0].[Name] = (SELECT TOP (@1) [t3].[Name] AS [value] "+
      "FROM [KitchenTable] AS [t2] CROSS JOIN [CookTable] AS [t3] WHERE ([t4].[ID] = [t3].[ID]))))",
      new CommandParameter("@1", 1));
    }

    [Test]
    public void ExplicitJoinWithInto_UseIntoVariableTwiceInSameStatementAndTwoSubStatement ()
    {
      CheckQuery (
      from k in Kitchens
      join c in Cooks on k.Cook equals c into gkc
      from kc in gkc
      where k.Name ==
       (from sk in Kitchens
        from skc in gkc
        where sk.Name ==
       (from ssk in Kitchens
        from sskc in gkc
        select skc.Name).First ()
        select skc.Name).First ()
      select kc.Name,
      "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t6] ON [t0].[ID] = [t6].[KitchenID] "+
      "CROSS JOIN [CookTable] AS [t1] WHERE (([t6].[ID] = [t1].[ID]) AND ([t0].[Name] = (SELECT TOP (@1) [t3].[Name] AS [value] "+
      "FROM [KitchenTable] AS [t2] CROSS JOIN [CookTable] AS [t3] WHERE (([t6].[ID] = [t3].[ID]) AND ([t2].[Name] = "+
      "(SELECT TOP (@2) [t3].[Name] AS [value] FROM [KitchenTable] AS [t4] CROSS JOIN [CookTable] AS [t5] WHERE ([t6].[ID] = [t5].[ID])))))))",
      new CommandParameter("@1", 1),
      new CommandParameter("@2", 1));
    }


    [Test]
    public void ExplicitJoinWithInto_InSubstatement_Once ()
    {
      CheckQuery (
          from c in Cooks where c.Name == 
            (from k in Kitchens 
             join a in Cooks on k.Cook equals a into gak 
             from ak in gak select ak.FirstName).First () 
            select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = (SELECT TOP (@1) [t2].[FirstName] AS [value] "+
          "FROM [KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t3] ON [t1].[ID] = [t3].[KitchenID] CROSS JOIN [CookTable] AS [t2] "+
          "WHERE ([t3].[ID] = [t2].[ID])))",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void ExplicitJoinWithInto_InSubstatement_Twice ()
    {
      CheckQuery (
          from c in Cooks where c.Name == 
            (from k in Kitchens 
             join a in Cooks on k.Cook equals a into gak
             join r in Restaurants on k.Restaurant equals r into gkr
             from ak in gak
             from kr in gkr 
             select ak.FirstName).First () select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = (SELECT TOP (@1) [t2].[FirstName] AS [value] "+
          "FROM [KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t4] ON [t1].[ID] = [t4].[KitchenID] "+
          "LEFT OUTER JOIN [RestaurantTable] AS [t5] ON [t1].[RestaurantID] = [t5].[ID] CROSS JOIN [CookTable] AS [t2] "+
          "CROSS JOIN [RestaurantTable] AS [t3] WHERE (([t4].[ID] = [t2].[ID]) AND ([t5].[ID] = [t3].[ID]))))",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void ExplicitJoinWithInto_InTwoSubstatements ()
    {
      CheckQuery (
          from c in Cooks
          where c.Name ==
            (from k in Kitchens
             join a in Cooks on k.Cook equals a into gak
             from ak in gak
             select ak.FirstName).First ()
            && c.FirstName ==
              (from k in Kitchens
              join a in Cooks on k.Cook equals a into gak
              from ak in gak
             select ak.Name).First ()
          select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE (([t0].[Name] = (SELECT TOP (@1) [t2].[FirstName] AS [value] "+
          "FROM [KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t3] ON [t1].[ID] = [t3].[KitchenID] CROSS JOIN [CookTable] AS [t2] "+
          "WHERE ([t3].[ID] = [t2].[ID]))) AND ([t0].[FirstName] = (SELECT TOP (@2) [t5].[Name] AS [value] FROM [KitchenTable] AS [t4] "+
          "LEFT OUTER JOIN [CookTable] AS [t6] ON [t4].[ID] = [t6].[KitchenID] CROSS JOIN [CookTable] AS [t5] WHERE ([t6].[ID] = [t5].[ID]))))",
          new CommandParameter ("@1", 1),
          new CommandParameter ("@2", 1));
    }

    [Test]
    public void ExplicitJoinWithInto_InSameStatementAndInSubstatement ()
    {
        CheckQuery (
          from k in Kitchens
          join c in Cooks on k.Cook equals c into gkc
          from kc in gkc
          where kc.Name ==
          (from i in Kitchens
             join a in Cooks on i.Cook equals a into gia
             from ia in gia
             select ia.FirstName).First ()
          select kc.Name,
          "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t5] ON [t0].[ID] = [t5].[KitchenID] CROSS JOIN "+
          "[CookTable] AS [t1] WHERE (([t5].[ID] = [t1].[ID]) AND ([t1].[Name] = (SELECT TOP (@1) [t3].[FirstName] AS [value] FROM "+
          "[KitchenTable] AS [t2] LEFT OUTER JOIN [CookTable] AS [t4] ON [t2].[ID] = [t4].[KitchenID] CROSS JOIN [CookTable] AS [t3] "+
          "WHERE ([t4].[ID] = [t3].[ID]))))",
          new CommandParameter("@1", 1)
          );
     }

    [Test]
    public void ExplicitJoinWithInto_DefaultIfEmptyOnGroupJoinVariable ()
    {
      CheckQuery (
          from k in Kitchens
          join c in Cooks on k.Cook equals c into gkc
          from kc in gkc.DefaultIfEmpty()
          select kc.Name,
          "SELECT [q1].[Name] AS [value] FROM [KitchenTable] AS [t2] LEFT OUTER JOIN [CookTable] AS [t4] ON [t2].[ID] = [t4].[KitchenID] "+
          "CROSS APPLY (SELECT [q0].[ID],[q0].[FirstName],[q0].[Name],[q0].[IsStarredCook],[q0].[IsFullTimeCook],[q0].[SubstitutedID],[q0].[KitchenID] "+
          "FROM (SELECT NULL AS [Empty]) AS [Empty] LEFT OUTER JOIN "+
          "(SELECT [t3].[ID],[t3].[FirstName],[t3].[Name],[t3].[IsStarredCook],[t3].[IsFullTimeCook],[t3].[SubstitutedID],[t3].[KitchenID] "+
          "FROM [CookTable] AS [t3] WHERE ([t4].[ID] = [t3].[ID])) AS [q0] ON 1 = 1) AS [q1]");
    }

  }
}