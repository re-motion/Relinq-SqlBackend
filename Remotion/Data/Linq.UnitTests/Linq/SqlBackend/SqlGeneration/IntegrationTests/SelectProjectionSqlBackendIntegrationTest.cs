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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
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
          + "FROM [CookTable] AS [t0]",
          row => (object) row.GetEntity<Cook> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6)));
    }

    [Test]
    public void EntityWithFullQualifiedName ()
    {
      CheckQuery (
          from c in Chefs select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [dbo].[ChefTable] AS [t0]",
          row => (object)row.GetValue<string>(new ColumnID("value", 0)));
    }

    [Test]
    public void Constant ()
    {
      CheckQuery (
          from k in Kitchens select "hugo",
          "SELECT @1 AS [value] FROM [KitchenTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "hugo"));
    }

    [Test]
    public void Null ()
    {
      CheckQuery (
          Kitchens.Select<Kitchen, object> (k => null),
          "SELECT NULL AS [value] FROM [KitchenTable] AS [t0]",
          row => row.GetValue<object> (new ColumnID ("value", 0)));
    }

    [Test]
    public void True ()
    {
      CheckQuery (
          from k in Kitchens select true,
          "SELECT @1 AS [value] FROM [KitchenTable] AS [t0]",
          row => (object) Convert.ToBoolean(row.GetValue<int> (new ColumnID ("value", 0))),
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void False ()
    {
      CheckQuery (
          from k in Kitchens select false,
          "SELECT @1 AS [value] FROM [KitchenTable] AS [t0]",
          row => (object) Convert.ToBoolean(row.GetValue<int> (new ColumnID ("value", 0))),
          new CommandParameter ("@1", 0));
    }

    [Test]
    public void BooleanConditions ()
    {
      CheckQuery (
          from c in Cooks select c.IsStarredCook,
          "SELECT [t0].[IsStarredCook] AS [value] FROM [CookTable] AS [t0]",
          row => (object)Convert.ToBoolean(row.GetValue<int> (new ColumnID ("value", 0))));

      CheckQuery (
          from c in Cooks select true,
          "SELECT @1 AS [value] FROM [CookTable] AS [t0]",
          row => (object) Convert.ToBoolean(row.GetValue<int> (new ColumnID ("value", 0))),
          new CommandParameter ("@1", 1));

      CheckQuery (
          from c in Cooks select c.FirstName != null,
          "SELECT CASE WHEN ([t0].[FirstName] IS NOT NULL) THEN 1 ELSE 0 END AS [value] FROM [CookTable] AS [t0]",
          row => (object)Convert.ToBoolean(row.GetValue<int> (new ColumnID ("value", 0))));
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
          from c in Cooks select new { A = c.Name, B = c.ID },
            "SELECT [t0].[Name] AS [A],[t0].[ID] AS [B] FROM [CookTable] AS [t0]",
            row => (object) new { A = row.GetValue<string> (new ColumnID ("A", 0)), B = row.GetValue<int> (new ColumnID ("B", 1)) });
      
    }

    [Test]
    public void NestedSubSelectProjection_Member ()
    {
      CheckQuery (
          from c in (from sc in Cooks select new { A = sc.Name, B = sc.ID }).Distinct() where c.B != 0 select c.A,
            "SELECT [q0].[A] AS [value] FROM ("
            + "SELECT DISTINCT [t1].[Name] AS [A],[t1].[ID] AS [B] FROM [CookTable] AS [t1]) AS [q0] "
            + "WHERE ([q0].[B] <> @1)",
            row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
            new CommandParameter("@1", 0)
          );
    }

    [Test]
    public void NestedSelectProjection_AccessingEntity ()
    {
      CheckQuery (
          from c in (from sc in Cooks select new { A = sc, B = sc.ID }).Distinct () where c.B != 0 select c.A,
            "SELECT [q0].[A_ID] AS [ID],[q0].[A_FirstName] AS [FirstName],[q0].[A_Name] AS [Name],"+
            "[q0].[A_IsStarredCook] AS [IsStarredCook],[q0].[A_IsFullTimeCook] AS [IsFullTimeCook],"+
            "[q0].[A_SubstitutedID] AS [SubstitutedID],[q0].[A_KitchenID] AS [KitchenID] "+
            "FROM (SELECT DISTINCT [t1].[ID] AS [A_ID],[t1].[FirstName] AS [A_FirstName],[t1].[Name] AS [A_Name],"+
            "[t1].[IsStarredCook] AS [A_IsStarredCook],[t1].[IsFullTimeCook] AS [A_IsFullTimeCook],"+
            "[t1].[SubstitutedID] AS [A_SubstitutedID],[t1].[KitchenID] AS [A_KitchenID],[t1].[ID] AS [B] "+
            "FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[B] <> @1)",
            row => (object) row.GetEntity<Cook> (new ColumnID ("ID", 0), 
                                                 new ColumnID ("FirstName", 1),
                                                 new ColumnID ("Name", 2),
                                                 new ColumnID ("IsStarredCook", 3),
                                                 new ColumnID ("IsFullTimeCook", 4),
                                                 new ColumnID ("SubstitutedID", 5),
                                                 new ColumnID ("KitchenID", 6)),
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
          "SELECT [q0].[B] AS [value] FROM [KitchenTable] AS [t1] CROSS APPLY "+
          "(SELECT @1 AS [A],[t2].[Name] AS [B],[t2].[Name] AS [C_D] FROM [CookTable] AS [t2]) AS [q0] "+
          "WHERE ([q0].[C_D] IS NOT NULL)",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter("@1", 10));
    }

    [Test]
    public void NestedSelectProjection_WithJoinOnCompoundReferenceMember ()
    {
      CheckQuery (
          from x in (from c in Cooks select new { A = c, B = c.ID }).Distinct () select x.A.Substitution.FirstName,
          "SELECT [t2].[FirstName] AS [value] "
          + "FROM (SELECT DISTINCT [t1].[ID] AS [A_ID],[t1].[FirstName] AS [A_FirstName],[t1].[Name] AS [A_Name],"
          + "[t1].[IsStarredCook] AS [A_IsStarredCook],[t1].[IsFullTimeCook] AS [A_IsFullTimeCook],"
          + "[t1].[SubstitutedID] AS [A_SubstitutedID],[t1].[KitchenID] AS [A_KitchenID],[t1].[ID] AS [B] "
          + "FROM [CookTable] AS [t1]) AS [q0] "
          + "LEFT OUTER JOIN [CookTable] AS [t2] ON [q0].[A_ID] = [t2].[SubstitutedID]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
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
          "SELECT [q1].[X_FirstName] AS [value] FROM (SELECT DISTINCT [q0].[A_ID] AS [X_ID],"+
          "[q0].[A_FirstName] AS [X_FirstName],[q0].[A_Name] AS [X_Name],"+
          "[q0].[A_IsStarredCook] AS [X_IsStarredCook],[q0].[A_IsFullTimeCook] AS [X_IsFullTimeCook],"+
          "[q0].[A_SubstitutedID] AS [X_SubstitutedID],[q0].[A_KitchenID] AS [X_KitchenID] "+
          "FROM (SELECT DISTINCT [t2].[ID] AS [A_ID],[t2].[FirstName] AS [A_FirstName],[t2].[Name] AS [A_Name],"+
          "[t2].[IsStarredCook] AS [A_IsStarredCook],[t2].[IsFullTimeCook] AS [A_IsFullTimeCook],"+
          "[t2].[SubstitutedID] AS [A_SubstitutedID],[t2].[KitchenID] AS [A_KitchenID],[t2].[ID] AS [B] "+
          "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
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
          "SELECT [q1].[FirstName] AS [value] FROM (SELECT DISTINCT [q0].[A_ID] AS [ID],[q0].[A_FirstName] AS [FirstName],"+
          "[q0].[A_Name] AS [Name],[q0].[A_IsStarredCook] AS [IsStarredCook],"+
          "[q0].[A_IsFullTimeCook] AS [IsFullTimeCook],[q0].[A_SubstitutedID] AS [SubstitutedID],"+
          "[q0].[A_KitchenID] AS [KitchenID] FROM (SELECT DISTINCT [t2].[ID] AS [A_ID],[t2].[FirstName] AS [A_FirstName],"+
          "[t2].[Name] AS [A_Name],[t2].[IsStarredCook] AS [A_IsStarredCook],[t2].[IsFullTimeCook] AS [A_IsFullTimeCook],"+
          "[t2].[SubstitutedID] AS [A_SubstitutedID],[t2].[KitchenID] AS [A_KitchenID],[t2].[ID] AS [B] "+
          "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void NestedSelectProjection_CompoundWithoutMemberAccess ()
    {
      CheckQuery (
          from x in (from y in (from c in Cooks select new { A = c.FirstName, B = c.ID }).Distinct() select y).Distinct() select x.A,
          "SELECT [q1].[A] AS [value] FROM (SELECT DISTINCT [q0].[A] AS [A],[q0].[B] AS [B] "+
          "FROM (SELECT DISTINCT [t2].[FirstName] AS [A],[t2].[ID] AS [B] FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));

      CheckQuery (
          from x in (from y in (from c in Cooks select new { A = c.FirstName, B = c.ID }).Distinct () select y).Distinct () select x,
          "SELECT [q1].[A] AS [A],[q1].[B] AS [B] FROM (SELECT DISTINCT [q0].[A] AS [A],[q0].[B] AS [B] " +
          "FROM (SELECT DISTINCT [t2].[FirstName] AS [A],[t2].[ID] AS [B] FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
          row => (object) new { A = row.GetValue<string> (new ColumnID ("A", 0)), B = row.GetValue<int> (new ColumnID ("B", 1)) });

      CheckQuery (
          from x in (from y in (from c in Cooks select new { A = c }).Distinct () select y).Distinct () select x,
          "SELECT [q1].[A_ID] AS [A_ID],[q1].[A_FirstName] AS [A_FirstName],[q1].[A_Name] AS [A_Name],"+
          "[q1].[A_IsStarredCook] AS [A_IsStarredCook],[q1].[A_IsFullTimeCook] AS [A_IsFullTimeCook],"+
          "[q1].[A_SubstitutedID] AS [A_SubstitutedID],[q1].[A_KitchenID] AS [A_KitchenID] "+
          "FROM (SELECT DISTINCT [q0].[A_ID] AS [A_ID],[q0].[A_FirstName] AS [A_FirstName],[q0].[A_Name] AS [A_Name],"+
          "[q0].[A_IsStarredCook] AS [A_IsStarredCook],[q0].[A_IsFullTimeCook] AS [A_IsFullTimeCook],"+
          "[q0].[A_SubstitutedID] AS [A_SubstitutedID],[q0].[A_KitchenID] AS [A_KitchenID] "+
          "FROM (SELECT DISTINCT [t2].[ID] AS [A_ID],[t2].[FirstName] AS [A_FirstName],[t2].[Name] AS [A_Name],"+
          "[t2].[IsStarredCook] AS [A_IsStarredCook],[t2].[IsFullTimeCook] AS [A_IsFullTimeCook],"+
          "[t2].[SubstitutedID] AS [A_SubstitutedID],[t2].[KitchenID] AS [A_KitchenID] FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
          row => (object) new { A = row.GetEntity<Cook> (new ColumnID ("A_ID", 0),
                                                         new ColumnID ("A_FirstName", 1),
                                                         new ColumnID ("A_Name", 2),
                                                         new ColumnID ("A_IsStarredCook", 3),
                                                         new ColumnID ("A_IsFullTimeCook", 4),
                                                         new ColumnID ("A_SubstitutedID", 5),
                                                         new ColumnID ("A_KitchenID", 6))
          });

      CheckQuery (
          from x in (from y in (from c in Cooks select new { A = c.ID }).Distinct () select new { B = y }).Distinct () select x,
          "SELECT [q1].[B_A] AS [B_A] FROM (SELECT DISTINCT [q0].[A] AS [B_A] FROM (SELECT DISTINCT [t2].[ID] AS [A] " +
          "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
          row => (object) new { B = new { A = row.GetValue<int> (new ColumnID ("B_A", 0)) } });
    }

    [Test]
    public void NestedSelectProjection_NoMembersNames ()
    {
      CheckQuery (from k in Kitchens select new TypeForNewExpression(k.ID, k.RoomNumber),
        "SELECT [t0].[ID] AS [m0],[t0].[RoomNumber] AS [m1] FROM [KitchenTable] AS [t0]",
        row => (object) new TypeForNewExpression (row.GetValue<int> (new ColumnID ("m0", 0)), row.GetValue<int> (new ColumnID ("m1", 1))));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'TypeForNewExpression.A' cannot be translated to SQL. "+
      "Expression: 'new TypeForNewExpression([t0].[ID] AS m0, [t0].[RoomNumber] AS m1)'")]
    public void NestedSelectProjection_MemberAccess_ToANewExpression_WithoutMembers ()
    {
      CheckQuery (from k in Kitchens select new TypeForNewExpression (k.ID, k.RoomNumber).A, "");
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'TypeForNewExpression.C' cannot be translated to SQL. "
      +"Expression: 'new TypeForNewExpression(A = [t0].[ID] AS A, B = [t0].[RoomNumber] AS B)'")]
    public void NestedSelectProjection_MemberAccess_ToANewExpression_WithMemberNotInitialized ()
    {
      var mainFromClause = new MainFromClause ("k", typeof (Kitchen), Expression.Constant (new Core.IntegrationTests.TestQueryable<Kitchen> ("Kitchen")));
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (mainFromClause);
      var newExpression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int), typeof (int) }),
          new[] { MemberExpression.MakeMemberAccess (querySourceReferenceExpression, typeof (Kitchen).GetProperty ("ID")),
                  MemberExpression.MakeMemberAccess (querySourceReferenceExpression, typeof (Kitchen).GetProperty ("RoomNumber")) },
          new MemberInfo[] { typeof (TypeForNewExpression).GetProperty ("A"), typeof (TypeForNewExpression).GetProperty ("B") });
      var selectClause = new SelectClause (MemberExpression.MakeMemberAccess (newExpression, typeof (TypeForNewExpression).GetField ("C")));
      var queryModel = new QueryModel (mainFromClause, selectClause);

      CheckQuery (queryModel, "");
    }

    [Test]
    public void NestedSelectProjection_WithBooleanConditions ()
    {
      CheckQuery (
          from c in Cooks select new { c.IsStarredCook },
          "SELECT [t0].[IsStarredCook] AS [IsStarredCook] FROM [CookTable] AS [t0]",
          row => (object) new { IsStarredCook = Convert.ToBoolean (row.GetValue<int> (new ColumnID ("IsStarredCook", 0))) });

      CheckQuery (
          from c in Cooks select new { c.IsStarredCook, True = true },
          "SELECT [t0].[IsStarredCook] AS [IsStarredCook],@1 AS [True] FROM [CookTable] AS [t0]",
          row => (object) new 
          { 
              IsStarredCook = Convert.ToBoolean (row.GetValue<int> (new ColumnID ("IsStarredCook", 0))), 
              True = Convert.ToBoolean (row.GetValue<int> (new ColumnID("True", 1))) 
          },
          new CommandParameter ("@1", 1));
      
      CheckQuery (
          from c in Cooks select new { c.IsStarredCook, True = true, HasFirstName = c.FirstName != null },
          "SELECT [t0].[IsStarredCook] AS [IsStarredCook]," 
          + "@1 AS [True],CASE WHEN ([t0].[FirstName] IS NOT NULL) THEN 1 ELSE 0 END AS [HasFirstName] FROM [CookTable] AS [t0]",
          row => (object) new
          { 
              IsStarredCook = Convert.ToBoolean (row.GetValue<int> (new ColumnID ("IsStarredCook", 0))),
              True = Convert.ToBoolean (row.GetValue<int> (new ColumnID("True", 1))),
              HasFirstName = Convert.ToBoolean (row.GetValue<int> (new ColumnID ("HasFirstName", 2)))
          },
          new CommandParameter ("@1", 1));
    }

  }
}