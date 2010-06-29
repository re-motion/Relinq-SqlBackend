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
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
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
          "SELECT @1 AS [value] FROM [KitchenTable] AS [t0]",
          new CommandParameter ("@1", "hugo"));
    }

    [Test]
    public void Null ()
    {
      CheckQuery (
          Kitchens.Select<Kitchen, object> (k => null),
          "SELECT NULL AS [value] FROM [KitchenTable] AS [t0]");
    }

    [Test]
    public void True ()
    {
      CheckQuery (
          from k in Kitchens select true,
          "SELECT @1 AS [value] FROM [KitchenTable] AS [t0]",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void False ()
    {
      CheckQuery (
          from k in Kitchens select false,
          "SELECT @1 AS [value] FROM [KitchenTable] AS [t0]",
          new CommandParameter ("@1", 0));
    }

    [Test]
    public void BooleanConditions ()
    {
      CheckQuery (
          from c in Cooks select c.IsStarredCook,
          "SELECT [t0].[IsStarredCook] AS [value] FROM [CookTable] AS [t0]");

      CheckQuery (
          from c in Cooks select true,
          "SELECT @1 AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));

      CheckQuery (
          from c in Cooks select c.FirstName != null,
          "SELECT CASE WHEN ([t0].[FirstName] IS NOT NULL) THEN 1 ELSE 0 END AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "The member 'Cook.Assistants' describes a collection and can only be used in places where collections are allowed.")]
    public void Collection_ThrowsNotSupportedException ()
    {
      CheckQuery (
          from c in Cooks select c.Assistants,
          "");
    }

    [Test]
    public void NestedSelectProjection ()
    {
      CheckQuery (
          from c in (from sc in Cooks select new { A = sc.Name, B = sc.ID }).Distinct() where c.B != 0 select c.A,
            "SELECT [q0].[get_A] AS [get_A] FROM ("
            + "SELECT DISTINCT [t1].[Name] AS [get_A],[t1].[ID] AS [get_B] FROM [CookTable] AS [t1]) AS [q0] "
            + "WHERE ([q0].[get_B] <> @1)",
            new CommandParameter("@1", 0)
          );
    }

    [Test]
    public void NestedSelectProjection_AccessingEntity ()
    {
      CheckQuery (
          from c in (from sc in Cooks select new { A = sc, B = sc.ID }).Distinct () where c.B != 0 select c.A,
            "SELECT [q0].[get_A_ID] AS [get_A_ID],[q0].[get_A_FirstName] AS [get_A_FirstName],[q0].[get_A_Name] AS [get_A_Name],"+
            "[q0].[get_A_IsStarredCook] AS [get_A_IsStarredCook],[q0].[get_A_IsFullTimeCook] AS [get_A_IsFullTimeCook],"+
            "[q0].[get_A_SubstitutedID] AS [get_A_SubstitutedID],[q0].[get_A_KitchenID] AS [get_A_KitchenID] "+
            "FROM (SELECT DISTINCT [t1].[ID] AS [get_A_ID],[t1].[FirstName] AS [get_A_FirstName],[t1].[Name] AS [get_A_Name],"+
            "[t1].[IsStarredCook] AS [get_A_IsStarredCook],[t1].[IsFullTimeCook] AS [get_A_IsFullTimeCook],"+
            "[t1].[SubstitutedID] AS [get_A_SubstitutedID],[t1].[KitchenID] AS [get_A_KitchenID],[t1].[ID] AS [get_B] "+
            "FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[get_B] <> @1)",
            new CommandParameter ("@1", 0)
          );
    }

    [Test]
    public void NestedNestedSelectProjection ()
    {
      CheckQuery (
          from x in Kitchens
          from c in
            ( // SubStatementTableInfo (SqlStatement (
              from sc in Cooks
              select // SelectProjection = NamedExpression (null, 
                new
                { // NewExpression (
                  A = 10, // NamedExpression ("get_A", ...),  => SqlValueReference ("get_A")
                  B = sc.Name, // NamedExpression ("get_B", ...),  => SqlValueReference ("get_B")
                  C = new
                  { // NewExpression ( => SqlCompoundReference
                    D = sc.Name
                  }
                }) // NamedExpression ("get_C_get_D", ...))))) => SqlValueReference ("get_C_get_D")
          where c.C.D != null // MemberExpression (MemberExpression (SqlTableReferenceExpression))
          select c.B,
          "SELECT [q0].[get_B] AS [get_B] FROM [KitchenTable] AS [t1] CROSS APPLY "+
          "(SELECT @1 AS [get_A],[t2].[Name] AS [get_B],[t2].[Name] AS [get_C_get_D] FROM [CookTable] AS [t2]) AS [q0] "+
          "WHERE ([q0].[get_C_get_D] IS NOT NULL)",
          new CommandParameter("@1", 10));
    }

    [Test]
    public void NestedSelectProjection_WithJoinOnCompoundReferenceMember ()
    {
      CheckQuery (
          from x in (from c in Cooks select new { A = c, B = c.ID }).Distinct () select x.A.Substitution.FirstName,
          "SELECT [t2].[FirstName] AS [value] "
          + "FROM (SELECT DISTINCT [t1].[ID] AS [get_A_ID],[t1].[FirstName] AS [get_A_FirstName],[t1].[Name] AS [get_A_Name],"
          + "[t1].[IsStarredCook] AS [get_A_IsStarredCook],[t1].[IsFullTimeCook] AS [get_A_IsFullTimeCook],"
          + "[t1].[SubstitutedID] AS [get_A_SubstitutedID],[t1].[KitchenID] AS [get_A_KitchenID],[t1].[ID] AS [get_B] "
          + "FROM [CookTable] AS [t1]) AS [q0] "
          + "LEFT OUTER JOIN [CookTable] AS [t2] ON [q0].[get_A_ID] = [t2].[SubstitutedID]");
    }

    [Test]
    public void NestedSelectProjection_TwoSubStatements_ReferencedEntity_NamedAgain ()
    {
      CheckQuery (
          from x in (
            from c in (
              from y in Cooks select new { A = y, B = y.ID }).Distinct () 
            select new { X = c.A }).Distinct () 
          select x.X.FirstName,
          "SELECT [q1].[get_X_get_A_FirstName] AS [value] FROM (SELECT DISTINCT [q0].[get_A_ID] AS [get_X_get_A_ID],"+
          "[q0].[get_A_FirstName] AS [get_X_get_A_FirstName],[q0].[get_A_Name] AS [get_X_get_A_Name],"+
          "[q0].[get_A_IsStarredCook] AS [get_X_get_A_IsStarredCook],[q0].[get_A_IsFullTimeCook] AS [get_X_get_A_IsFullTimeCook],"+
          "[q0].[get_A_SubstitutedID] AS [get_X_get_A_SubstitutedID],[q0].[get_A_KitchenID] AS [get_X_get_A_KitchenID] "+
          "FROM (SELECT DISTINCT [t2].[ID] AS [get_A_ID],[t2].[FirstName] AS [get_A_FirstName],[t2].[Name] AS [get_A_Name],"+
          "[t2].[IsStarredCook] AS [get_A_IsStarredCook],[t2].[IsFullTimeCook] AS [get_A_IsFullTimeCook],"+
          "[t2].[SubstitutedID] AS [get_A_SubstitutedID],[t2].[KitchenID] AS [get_A_KitchenID],[t2].[ID] AS [get_B] "+
          "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]");
    }

    [Test]
    public void NestedSelectProjection_TwoSubStatements_ReferencedEntity_NotNamedAgain ()
    {
      CheckQuery (
          from x in (
            from c in (
              from y in Cooks select new { A = y, B = y.ID }).Distinct () 
            select c.A).Distinct () 
          select x.FirstName,
          "SELECT [q1].[get_A_FirstName] AS [value] FROM (SELECT DISTINCT [q0].[get_A_ID] AS [get_A_ID],[q0].[get_A_FirstName] AS [get_A_FirstName],"+
          "[q0].[get_A_Name] AS [get_A_Name],[q0].[get_A_IsStarredCook] AS [get_A_IsStarredCook],"+
          "[q0].[get_A_IsFullTimeCook] AS [get_A_IsFullTimeCook],[q0].[get_A_SubstitutedID] AS [get_A_SubstitutedID],"+
          "[q0].[get_A_KitchenID] AS [get_A_KitchenID] FROM (SELECT DISTINCT [t2].[ID] AS [get_A_ID],[t2].[FirstName] AS [get_A_FirstName],"+
          "[t2].[Name] AS [get_A_Name],[t2].[IsStarredCook] AS [get_A_IsStarredCook],[t2].[IsFullTimeCook] AS [get_A_IsFullTimeCook],"+
          "[t2].[SubstitutedID] AS [get_A_SubstitutedID],[t2].[KitchenID] AS [get_A_KitchenID],[t2].[ID] AS [get_B] "+
          "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]");
    }

    [Test]
    public void NestedSelectProjection_CompoundWithoutMemberAccess ()
    {
      CheckQuery (
          from x in (from y in (from c in Cooks select new { A = c.FirstName, B = c.ID }).Distinct() select y).Distinct() select x.A,
          "SELECT [q1].[get_A] AS [get_A] FROM (SELECT DISTINCT [q0].[get_A] AS [get_A],[q0].[get_B] AS [get_B] "+
          "FROM (SELECT DISTINCT [t2].[FirstName] AS [get_A],[t2].[ID] AS [get_B] FROM [CookTable] AS [t2]) AS [q0]) AS [q1]");

      CheckQuery (
          from x in (from y in (from c in Cooks select new { A = c.FirstName, B = c.ID }).Distinct () select y).Distinct () select x,
          "SELECT [q1].[get_A] AS [get_A],[q1].[get_B] AS [get_B] FROM (SELECT DISTINCT [q0].[get_A] AS [get_A],[q0].[get_B] AS [get_B] " +
          "FROM (SELECT DISTINCT [t2].[FirstName] AS [get_A],[t2].[ID] AS [get_B] FROM [CookTable] AS [t2]) AS [q0]) AS [q1]");

      CheckQuery (
          from x in (from y in (from c in Cooks select new { A = c }).Distinct () select y).Distinct () select x,
          "SELECT [q1].[get_A_ID] AS [get_A_ID],[q1].[get_A_FirstName] AS [get_A_FirstName],[q1].[get_A_Name] AS [get_A_Name],"+
          "[q1].[get_A_IsStarredCook] AS [get_A_IsStarredCook],[q1].[get_A_IsFullTimeCook] AS [get_A_IsFullTimeCook],"+
          "[q1].[get_A_SubstitutedID] AS [get_A_SubstitutedID],[q1].[get_A_KitchenID] AS [get_A_KitchenID] "+
          "FROM (SELECT DISTINCT [q0].[get_A_ID] AS [get_A_ID],[q0].[get_A_FirstName] AS [get_A_FirstName],[q0].[get_A_Name] AS [get_A_Name],"+
          "[q0].[get_A_IsStarredCook] AS [get_A_IsStarredCook],[q0].[get_A_IsFullTimeCook] AS [get_A_IsFullTimeCook],"+
          "[q0].[get_A_SubstitutedID] AS [get_A_SubstitutedID],[q0].[get_A_KitchenID] AS [get_A_KitchenID] "+
          "FROM (SELECT DISTINCT [t2].[ID] AS [get_A_ID],[t2].[FirstName] AS [get_A_FirstName],[t2].[Name] AS [get_A_Name],"+
          "[t2].[IsStarredCook] AS [get_A_IsStarredCook],[t2].[IsFullTimeCook] AS [get_A_IsFullTimeCook],"+
          "[t2].[SubstitutedID] AS [get_A_SubstitutedID],[t2].[KitchenID] AS [get_A_KitchenID] FROM [CookTable] AS [t2]) AS [q0]) AS [q1]");

      CheckQuery (
          from x in (from y in (from c in Cooks select new { A = c.ID }).Distinct () select new { B = y }).Distinct () select x,
          "SELECT [q1].[get_B_get_A] AS [get_B_get_A] FROM (SELECT DISTINCT [q0].[get_A] AS [get_B_get_A] FROM (SELECT DISTINCT [t2].[ID] AS [get_A] " +
          "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]");
    }

    [Test]
    public void NestedSelectProjection_NoMembersNames ()
    {
      CheckQuery (from k in Kitchens select new TypeForNewExpression(k.ID, k.RoomNumber),
        "SELECT [t0].[ID] AS [m0],[t0].[RoomNumber] AS [m1] FROM [KitchenTable] AS [t0]");
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'TypeForNewExpression.A' cannot be translated to SQL. "+
      "Expression: 'new TypeForNewExpression([t0].[ID] AS m0, [t0].[RoomNumber] AS m1)'")]
    public void NestedSelectProjection_MemberAccess_ToANewExpression_WithoutMembers ()
    {
      CheckQuery (from k in Kitchens select new TypeForNewExpression (k.ID, k.RoomNumber).A, "");
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'TypeForNewExpression.C' cannot be translated to SQL. " +
      "Expression: 'new TypeForNewExpression([t0].[ID] AS A, [t0].[RoomNumber] AS B)'")]
    [Ignore ("TODO Review 2885")]
    public void NestedSelectProjection_MemberAccess_ToANewExpression_WithMemberNotInitialized ()
    {
      //TODO: The C# compiler will not allow you to write such code, but you can construct it using Expression.New (...)
      // CheckQuery (from k in Kitchens select new TypeForNewExpression (A = k.ID, B = k.RoomNumber).C, "");
    }

    [Test]
    [Ignore ("TODO 2985/TODO 2986")]
    public void NestedSelectProjection_WithBooleanConditions ()
    {
      CheckQuery (
          from c in Cooks select new { c.IsStarredCook },
          "SELECT [t0].[IsStarredCook] AS [get_IsStarredCook] FROM [CookTable] AS [t0]");

      CheckQuery (
          from c in Cooks select new { c.IsStarredCook, True = true },
          "SELECT [t0].[IsStarredCook] AS [get_IsStarredCook],@1 AS [get_True] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
      
      CheckQuery (
          from c in Cooks select new { c.IsStarredCook, True = true, HasFirstName = c.FirstName != null },
          "SELECT [t0].[IsStarredCook] AS [get_IsStarredCook]," 
          + "@1 AS [get_True], CASE WHEN ([t0].[FirstName] IS NOT NULL) THEN 1 ELSE 0 END AS [get_HasFirstName] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 1));
    }

  }
}