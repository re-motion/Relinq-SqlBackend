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
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests
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
          "SELECT [t0].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t2] ON ([t0].[ID] = [t2].[KitchenID]) "
          +"CROSS JOIN [CookTable] AS [t1] WHERE ([t2].[ID] = [t1].[ID])"
          );
    }

    [Test]
    public void ExplicitJoin_WithMultipleIndependentJoinsClauses ()
    {
      CheckQuery (
          from c in Cooks
          join kitchen in Kitchens on c.Kitchen equals kitchen
          join knife in Knives on c.Knife equals knife
          select new { CookID = c.ID, KitchenID = kitchen.ID, KnifeID = knife.ID },
          "SELECT [t0].[ID] AS [CookID],[t1].[ID] AS [KitchenID],[t2].[ID] AS [KnifeID_Value],[t2].[ClassID] AS [KnifeID_ClassID] "
          + "FROM [CookTable] AS [t0] "
          + "CROSS JOIN [KitchenTable] AS [t1] "
          + "CROSS JOIN [KnifeTable] AS [t2] "
          + "WHERE (([t0].[KitchenID] = [t1].[ID]) AND (([t0].[KnifeID] = [t2].[ID]) AND ([t0].[KnifeClassID] = [t2].[ClassID])))");
    }

    [Test]
    public void ExplicitJoin_WithImplicitJoinAddedByJoinCondition ()
    {
      CheckQuery (
          from c in Cooks
          join restaurant in Restaurants on c.Kitchen.Restaurant equals restaurant
          select new { CookID = c.ID, RestaurantID = restaurant.ID },
          "SELECT [t0].[ID] AS [CookID],[t1].[ID] AS [RestaurantID] "
          + "FROM [CookTable] AS [t0] LEFT OUTER JOIN [KitchenTable] AS [t2] ON ([t0].[KitchenID] = [t2].[ID]) "
          + "CROSS JOIN [RestaurantTable] AS [t1] "
          + "WHERE ([t2].[RestaurantID] = [t1].[ID])");
    }

    [Test]
    public void ExplicitJoin_WithOuterWhereClause ()
    {
      CheckQuery (
          from c in Cooks
          join kitchen in Kitchens on c.Kitchen equals kitchen
          where kitchen.Name != null
          select new { CookID = c.ID, KitchenID = kitchen.ID },
          "SELECT [t0].[ID] AS [CookID],[t1].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t0] "
          + "CROSS JOIN [KitchenTable] AS [t1] "
          + "WHERE (([t0].[KitchenID] = [t1].[ID]) AND ([t1].[Name] IS NOT NULL))");
    }

    [Test]
    public void ExplicitJoin_WithDependentJoins ()
    {
      CheckQuery (
          from c in Cooks
          join kitchen in Kitchens on c.Kitchen equals kitchen
          join restaurant in Restaurants on kitchen.Restaurant equals restaurant
          select new { CookID = c.ID, RestaurantID = restaurant.ID, KitchenID = kitchen.ID },
          "SELECT [t0].[ID] AS [CookID],[t2].[ID] AS [RestaurantID],[t1].[ID] AS [KitchenID] "
          + "FROM [CookTable] AS [t0] "
          + "CROSS JOIN [KitchenTable] AS [t1] "
          + "CROSS JOIN [RestaurantTable] AS [t2] "
          + "WHERE (([t0].[KitchenID] = [t1].[ID]) AND ([t1].[RestaurantID] = [t2].[ID]))");
    }

    [Test]
    public void ExplicitJoin_WithMultipleJoinClauses_WithImplicitJoinAddedByJoinCondition_WithOuterWhereClause ()
    {
      CheckQuery (
          from c in Cooks
          join restaurant in Restaurants on c.Kitchen.Restaurant equals restaurant
          join kitchen in Kitchens on c.Kitchen equals kitchen
          join company in Companies on restaurant.CompanyIfAny equals company
          where kitchen.Name != null
          select new { CookID = c.ID, RestaurantID = restaurant.ID, KitchenID = kitchen.ID, CompanyID = company.ID },
          "SELECT [t0].[ID] AS [CookID],[t1].[ID] AS [RestaurantID],[t2].[ID] AS [KitchenID],[t3].[ID] AS [CompanyID] "
          + "FROM [CookTable] AS [t0] LEFT OUTER JOIN [KitchenTable] AS [t4] ON ([t0].[KitchenID] = [t4].[ID]) "
          + "CROSS JOIN [RestaurantTable] AS [t1] "
          + "CROSS JOIN [KitchenTable] AS [t2] "
          + "CROSS JOIN [CompanyTable] AS [t3] "
          + "WHERE (((([t4].[RestaurantID] = [t1].[ID]) AND ([t0].[KitchenID] = [t2].[ID])) AND ([t1].[CompanyID] = [t3].[ID])) AND ([t2].[Name] IS NOT NULL))");
    }

    [Test]
    [Ignore ("TODO 5120")]
    public void ExplicitJoin_IncludingOptimizableImplicitJoins ()
    {
      CheckQuery (
          from c in Cooks join k in Kitchens on c.Kitchen equals k select k.Name,
          "SELECT [t1].[Name] AS [value] FROM [CookTable] AS [t0] CROSS JOIN [KitchenTable] AS [t1] WHERE ([t0].[KitchenID] = [t1].[ID])"
          );

      CheckQuery (
          from k in Kitchens join c in Cooks on k.Cook equals c select c.Name,
          "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t0] CROSS JOIN [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[KitchenID])"
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
          "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t2] ON ([t0].[ID] = [t2].[KitchenID]) "+
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
          "SELECT [t2].[ID] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t3] ON ([t0].[ID] = [t3].[KitchenID]) "+
          "CROSS JOIN [CookTable] AS [t1] "+
          "CROSS JOIN [RestaurantTable] AS [t2] WHERE (([t3].[ID] = [t1].[ID]) AND ([t0].[RestaurantID] = [t2].[ID]))"
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
          "SELECT [t1].[FirstName] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t2] ON ([t0].[ID] = [t2].[KitchenID]) "+
          "CROSS JOIN [CookTable] AS [t1] WHERE (([t2].[ID] = [t1].[ID]) AND ([t1].[Name] = @1))",
          new CommandParameter("@1", "Steiner")
          );
    }

    [Test]
    public void ExplicitJoinWithInto_UseIntoVariableTwiceInSameStatementAndInSubStatement ()
    {
      CheckQuery (
          from k in Kitchens
          join c in Cooks on k.Cook equals c into gkc
          from kc in gkc
          where k.Name ==
                (from sk in Kitchens
                 from skc in gkc
                 select skc.Name).First()
          select kc.Name,
          "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t2] ON ([t0].[ID] = [t2].[KitchenID]) "
          + "CROSS JOIN [CookTable] AS [t1] WHERE (([t2].[ID] = [t1].[ID]) AND ([t0].[Name] = (SELECT TOP (1) [t4].[Name] AS [value] "
          + "FROM [KitchenTable] AS [t3] CROSS JOIN [CookTable] AS [t4] WHERE ([t2].[ID] = [t4].[ID]))))");
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
      "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t2] ON ([t0].[ID] = [t2].[KitchenID]) "
      + "CROSS JOIN [CookTable] AS [t1] WHERE (([t2].[ID] = [t1].[ID]) AND ([t0].[Name] = (SELECT TOP (1) [t4].[Name] AS [value] "
      + "FROM [KitchenTable] AS [t3] CROSS JOIN [CookTable] AS [t4] WHERE (([t2].[ID] = [t4].[ID]) AND ([t3].[Name] = "
      + "(SELECT TOP (1) [t4].[Name] AS [value] FROM [KitchenTable] AS [t5] CROSS JOIN [CookTable] AS [t6] WHERE ([t2].[ID] = [t6].[ID])))))))");
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
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = (SELECT TOP (1) [t2].[FirstName] AS [value] "+
          "FROM [KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t3] ON ([t1].[ID] = [t3].[KitchenID]) CROSS JOIN [CookTable] AS [t2] "+
          "WHERE ([t3].[ID] = [t2].[ID])))");
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
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = (SELECT TOP (1) [t2].[FirstName] AS [value] "+
          "FROM [KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t4] ON ([t1].[ID] = [t4].[KitchenID]) "+
          "CROSS JOIN [CookTable] AS [t2] "+
          "CROSS JOIN [RestaurantTable] AS [t3] WHERE (([t4].[ID] = [t2].[ID]) AND ([t1].[RestaurantID] = [t3].[ID]))))");
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
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE (([t0].[Name] = (SELECT TOP (1) [t2].[FirstName] AS [value] "+
          "FROM [KitchenTable] AS [t1] LEFT OUTER JOIN [CookTable] AS [t3] ON ([t1].[ID] = [t3].[KitchenID]) CROSS JOIN [CookTable] AS [t2] "+
          "WHERE ([t3].[ID] = [t2].[ID]))) AND ([t0].[FirstName] = (SELECT TOP (1) [t5].[Name] AS [value] FROM [KitchenTable] AS [t4] "+
          "LEFT OUTER JOIN [CookTable] AS [t6] ON ([t4].[ID] = [t6].[KitchenID]) CROSS JOIN [CookTable] AS [t5] WHERE ([t6].[ID] = [t5].[ID]))))");
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
          "SELECT [t1].[Name] AS [value] FROM [KitchenTable] AS [t0] "
          + "LEFT OUTER JOIN [CookTable] AS [t2] ON ([t0].[ID] = [t2].[KitchenID]) "
          + "CROSS JOIN [CookTable] AS [t1] "
          + "WHERE (([t2].[ID] = [t1].[ID]) AND ([t1].[Name] = ("
          + "SELECT TOP (1) [t4].[FirstName] AS [value] FROM [KitchenTable] AS [t3] "
          + "LEFT OUTER JOIN [CookTable] AS [t5] ON ([t3].[ID] = [t5].[KitchenID]) "
          + "CROSS JOIN [CookTable] AS [t4] "
          + "WHERE ([t5].[ID] = [t4].[ID]))))");
     }

    [Test]
    public void ExplicitJoinWithInto_DefaultIfEmptyOnGroupJoinVariable ()
    {
      // This test duplicates a scenario from DefaultIfEmptyResultOperatorSqlBackendIntegrationTest 
      // in order to provide a more complete set of explicit join options.

      CheckQuery (
          from k in Kitchens
          join c in Cooks on k.Cook equals c into gkc
          from kc in gkc.DefaultIfEmpty()
          select kc.Name,
          "SELECT [t2].[Name] AS [value] "
          + "FROM [KitchenTable] AS [t1] "
          + "LEFT OUTER JOIN [CookTable] AS [t3] ON ([t1].[ID] = [t3].[KitchenID]) "
          + "LEFT OUTER JOIN [CookTable] AS [t2] ON ([t3].[ID] = [t2].[ID])");
    }

    [Test]
    public void ExplicitJoinWithInto_WithOrderBy ()
    {
      CheckQuery (
          from k in Kitchens
          join c in Cooks.OrderBy(c => c.FirstName).Select (c => c.ID) on k.Cook.ID equals c into gkc
          from kc in gkc
          select kc,
          "SELECT [q0].[Key] AS [value] "
          + "FROM [KitchenTable] AS [t1] "
          + "LEFT OUTER JOIN [CookTable] AS [t3] ON ([t1].[ID] = [t3].[KitchenID]) "
          + "CROSS APPLY (SELECT [t2].[ID] AS [Key],[t2].[FirstName] AS [Value] FROM [CookTable] AS [t2]) AS [q0] "
          + "WHERE ([t3].[ID] = [q0].[Key]) ORDER BY [q0].[Value] ASC");
    }

    [Test]
    public void ExplicitJoinWithInto_PropagatedFromSubStatement ()
    {
      // To enable this test, see RM-3037
      Assert.That (
          () => CheckQuery (
              from cooks in (from k in Kitchens join c in Cooks on k.Name equals c.FirstName into cooks select cooks).Take (2)
              from c in cooks
              select c.Name,
              "SELECT [t2].[Name] AS [value] "
              + "FROM (SELECT [k].[Name] AS [key] FROM [KitchenTable] AS [t0]) AS [q1] "
              + "CROSS JOIN [CookTable] AS [t2] WHERE ([t0].[Name] = [q1].[FirstName])"
              ),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo ("The results of a GroupJoin ('cooks') can only be used as a query source, for example, in a from expression."));
    }

  }
}