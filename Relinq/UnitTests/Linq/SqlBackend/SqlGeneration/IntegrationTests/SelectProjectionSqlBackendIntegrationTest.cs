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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class SelectProjectionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Entity ()
    {
      CheckQuery (
          from s in Cooks select s,
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID],"
          + "[t0].[KnifeID],[t0].[KnifeClassID] "
          + "FROM [CookTable] AS [t0]",
          row => (object) row.GetEntity<Cook> (
              new ColumnID ("ID", 0),
              new ColumnID ("FirstName", 1),
              new ColumnID ("Name", 2),
              new ColumnID ("IsStarredCook", 3),
              new ColumnID ("IsFullTimeCook", 4),
              new ColumnID ("SubstitutedID", 5),
              new ColumnID ("KitchenID", 6),
              new ColumnID ("KnifeID", 7),
              new ColumnID ("KnifeClassID", 8)));
    }

    [Test]
    public void EntityMember ()
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
    public void Null_Boolean ()
    {
      CheckQuery (
          Kitchens.Select<Kitchen, bool?> (k => null),
          "SELECT CONVERT(BIT, NULL) AS [value] FROM [KitchenTable] AS [t0]",
          row => (object) row.GetValue<bool?> (new ColumnID ("value", 0)));
    }

    [Test]
    public void True ()
    {
      CheckQuery (
          from k in Kitchens select true,
          "SELECT CONVERT(BIT, @1) AS [value] FROM [KitchenTable] AS [t0]",
          row => (object) row.GetValue<bool> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void False ()
    {
      CheckQuery (
          from k in Kitchens select false,
          "SELECT CONVERT(BIT, @1) AS [value] FROM [KitchenTable] AS [t0]",
          row => (object) row.GetValue<bool> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 0));
    }

    [Test]
    public void BooleanConditions_Old ()
    {
      CheckQuery (
          from c in Cooks select c.IsStarredCook,
          "SELECT [t0].[IsStarredCook] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<bool> (new ColumnID ("value", 0)));

      CheckQuery (
          from c in Cooks select c.FirstName != null,
          "SELECT CONVERT(BIT, CASE WHEN ([t0].[FirstName] IS NOT NULL) THEN 1 ELSE 0 END) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<bool> (new ColumnID ("value", 0)));
    }

    [Test]
    [Ignore ("TODO 5679")]
    public void BooleanConditions ()
    {
      CheckQuery (
          from c in Cooks select c.IsStarredCook,
          "SELECT [t0].[IsStarredCook] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<bool> (new ColumnID ("value", 0)));

      CheckQuery (
          from c in Cooks select true,
          "SELECT @1 AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<bool> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 1));

      CheckQuery (
          from c in Cooks select c.FirstName != null,
          "SELECT CASE WHEN ([t0].[FirstName] IS NOT NULL) THEN 1 ELSE 0 END AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The member 'Cook.Assistants' describes a collection and can only be used in places where collections are allowed. Expression: '[t0]'")]
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
    public void NestedSubSelectProjection_Member_Tuple ()
    {
      CheckQuery (
          from c in (from sc in Cooks select new Tuple<string, int> (sc.Name, sc.ID)).Distinct () where c.Item2 != 0 select c.Item1,
            "SELECT [q0].[Item1] AS [value] FROM ("
            + "SELECT DISTINCT [t1].[Name] AS [Item1],[t1].[ID] AS [Item2] FROM [CookTable] AS [t1]) AS [q0] "
            + "WHERE ([q0].[Item2] <> @1)",
            row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
            new CommandParameter ("@1", 0)
          );
      CheckQuery (
          from c in (from sc in Cooks select new KeyValuePair<string, int>(sc.Name, sc.ID)).Distinct () where c.Value != 0 select c.Key,
            "SELECT [q0].[Key] AS [value] FROM ("
            + "SELECT DISTINCT [t1].[Name] AS [Key],[t1].[ID] AS [Value] FROM [CookTable] AS [t1]) AS [q0] "
            + "WHERE ([q0].[Value] <> @1)",
            row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
            new CommandParameter ("@1", 0)
          );
    }

    [Test]
    public void NestedSelectProjection_AccessingEntity ()
    {
      CheckQuery (
          from c in (from sc in Cooks select new { A = sc, B = sc.ID }).Distinct () where c.B != 0 select c.A,
            "SELECT [q0].[A_ID] AS [ID],[q0].[A_FirstName] AS [FirstName],[q0].[A_Name] AS [Name],"
            + "[q0].[A_IsStarredCook] AS [IsStarredCook],[q0].[A_IsFullTimeCook] AS [IsFullTimeCook],"
            + "[q0].[A_SubstitutedID] AS [SubstitutedID],[q0].[A_KitchenID] AS [KitchenID],"
            + "[q0].[A_KnifeID] AS [KnifeID],[q0].[A_KnifeClassID] AS [KnifeClassID] "
            + "FROM (SELECT DISTINCT [t1].[ID] AS [A_ID],[t1].[FirstName] AS [A_FirstName],[t1].[Name] AS [A_Name],"
            + "[t1].[IsStarredCook] AS [A_IsStarredCook],[t1].[IsFullTimeCook] AS [A_IsFullTimeCook],"
            + "[t1].[SubstitutedID] AS [A_SubstitutedID],[t1].[KitchenID] AS [A_KitchenID],"
            + "[t1].[KnifeID] AS [A_KnifeID],[t1].[KnifeClassID] AS [A_KnifeClassID],[t1].[ID] AS [B] "
            + "FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[B] <> @1)",
          row => (object) row.GetEntity<Cook> (GetColumnIDsForCook ("")),
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
          + "[t1].[SubstitutedID] AS [A_SubstitutedID],[t1].[KitchenID] AS [A_KitchenID],"
          + "[t1].[KnifeID] AS [A_KnifeID],[t1].[KnifeClassID] AS [A_KnifeClassID],[t1].[ID] AS [B] "
          + "FROM [CookTable] AS [t1]) AS [q0] "
          + "LEFT OUTER JOIN [CookTable] AS [t2] ON ([q0].[A_ID] = [t2].[SubstitutedID])",
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
          "SELECT [q1].[X_FirstName] AS [value] FROM (SELECT DISTINCT [q0].[A_ID] AS [X_ID],"
          + "[q0].[A_FirstName] AS [X_FirstName],[q0].[A_Name] AS [X_Name],"
          + "[q0].[A_IsStarredCook] AS [X_IsStarredCook],[q0].[A_IsFullTimeCook] AS [X_IsFullTimeCook],"
          + "[q0].[A_SubstitutedID] AS [X_SubstitutedID],[q0].[A_KitchenID] AS [X_KitchenID],"
          + "[q0].[A_KnifeID] AS [X_KnifeID],[q0].[A_KnifeClassID] AS [X_KnifeClassID] " 
          + "FROM (SELECT DISTINCT [t2].[ID] AS [A_ID],[t2].[FirstName] AS [A_FirstName],[t2].[Name] AS [A_Name],"
          + "[t2].[IsStarredCook] AS [A_IsStarredCook],[t2].[IsFullTimeCook] AS [A_IsFullTimeCook],"
          + "[t2].[SubstitutedID] AS [A_SubstitutedID],[t2].[KitchenID] AS [A_KitchenID],"
          + "[t2].[KnifeID] AS [A_KnifeID],[t2].[KnifeClassID] AS [A_KnifeClassID],[t2].[ID] AS [B] "
          + "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
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
          "SELECT [q1].[FirstName] AS [value] FROM (SELECT DISTINCT [q0].[A_ID] AS [ID],[q0].[A_FirstName] AS [FirstName],"
          + "[q0].[A_Name] AS [Name],[q0].[A_IsStarredCook] AS [IsStarredCook],"
          + "[q0].[A_IsFullTimeCook] AS [IsFullTimeCook],[q0].[A_SubstitutedID] AS [SubstitutedID],"
          + "[q0].[A_KitchenID] AS [KitchenID],[q0].[A_KnifeID] AS [KnifeID],[q0].[A_KnifeClassID] AS [KnifeClassID] "
          + "FROM (SELECT DISTINCT [t2].[ID] AS [A_ID],[t2].[FirstName] AS [A_FirstName],"
          + "[t2].[Name] AS [A_Name],[t2].[IsStarredCook] AS [A_IsStarredCook],[t2].[IsFullTimeCook] AS [A_IsFullTimeCook],"
          + "[t2].[SubstitutedID] AS [A_SubstitutedID],[t2].[KitchenID] AS [A_KitchenID],"
          + "[t2].[KnifeID] AS [A_KnifeID],[t2].[KnifeClassID] AS [A_KnifeClassID],[t2].[ID] AS [B] "
          + "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
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
          "SELECT [q1].[A_ID] AS [A_ID],[q1].[A_FirstName] AS [A_FirstName],[q1].[A_Name] AS [A_Name],"
          + "[q1].[A_IsStarredCook] AS [A_IsStarredCook],[q1].[A_IsFullTimeCook] AS [A_IsFullTimeCook],"
          + "[q1].[A_SubstitutedID] AS [A_SubstitutedID],[q1].[A_KitchenID] AS [A_KitchenID],"
          + "[q1].[A_KnifeID] AS [A_KnifeID],[q1].[A_KnifeClassID] AS [A_KnifeClassID] " 
          + "FROM (SELECT DISTINCT [q0].[A_ID] AS [A_ID],[q0].[A_FirstName] AS [A_FirstName],[q0].[A_Name] AS [A_Name],"
          + "[q0].[A_IsStarredCook] AS [A_IsStarredCook],[q0].[A_IsFullTimeCook] AS [A_IsFullTimeCook],"
          + "[q0].[A_SubstitutedID] AS [A_SubstitutedID],[q0].[A_KitchenID] AS [A_KitchenID],"
          + "[q0].[A_KnifeID] AS [A_KnifeID],[q0].[A_KnifeClassID] AS [A_KnifeClassID] " 
          + "FROM (SELECT DISTINCT [t2].[ID] AS [A_ID],[t2].[FirstName] AS [A_FirstName],[t2].[Name] AS [A_Name],"
          + "[t2].[IsStarredCook] AS [A_IsStarredCook],[t2].[IsFullTimeCook] AS [A_IsFullTimeCook],"
          + "[t2].[SubstitutedID] AS [A_SubstitutedID],[t2].[KitchenID] AS [A_KitchenID],"
          + "[t2].[KnifeID] AS [A_KnifeID],[t2].[KnifeClassID] AS [A_KnifeClassID] "
          + "FROM [CookTable] AS [t2]) AS [q0]) AS [q1]",
          row => (object) new
                          {
                              A = row.GetEntity<Cook> (
                                  new ColumnID ("A_ID", 0),
                                  new ColumnID ("A_FirstName", 1),
                                  new ColumnID ("A_Name", 2),
                                  new ColumnID ("A_IsStarredCook", 3),
                                  new ColumnID ("A_IsFullTimeCook", 4),
                                  new ColumnID ("A_SubstitutedID", 5),
                                  new ColumnID ("A_KitchenID", 6),
                                  new ColumnID ("A_KnifeID", 7),
                                  new ColumnID ("A_KnifeClassID", 8))
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
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "The member 'TypeForNewExpression.A' cannot be translated to SQL. "
        + "Expression: 'new TypeForNewExpression([t0].[ID] AS m0, [t0].[RoomNumber] AS m1)'")]
    public void NestedSelectProjection_MemberAccess_ToANewExpression_WithoutMembers ()
    {
      CheckQuery (from k in Kitchens select new TypeForNewExpression (k.ID, k.RoomNumber).A, "");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "The member 'TypeForNewExpression.C' cannot be translated to SQL. "
        + "Expression: 'new TypeForNewExpression(A = [t0].[ID] AS A, B = [t0].[RoomNumber] AS B)'")]
    public void NestedSelectProjection_MemberAccess_ToANewExpression_WithMemberNotInitialized ()
    {
      var mainFromClause = new MainFromClause ("k", typeof (Kitchen), Expression.Constant (Kitchens));
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (mainFromClause);
      var constructorInfo = TypeForNewExpression.GetConstructor (typeof (int), typeof (int));
      Trace.Assert (constructorInfo != null);
      var newExpression = Expression.New (
          constructorInfo,
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
          row => (object) new { IsStarredCook = row.GetValue<bool> (new ColumnID ("IsStarredCook", 0)) });

      CheckQuery (
          from c in Cooks select new { c.IsStarredCook, True = true },
          "SELECT [t0].[IsStarredCook] AS [IsStarredCook],CONVERT(BIT, @1) AS [True] FROM [CookTable] AS [t0]",
          row => (object) new 
          {
            IsStarredCook = row.GetValue<bool> (new ColumnID ("IsStarredCook", 0)),
            True = row.GetValue<bool> (new ColumnID ("True", 1)) 
          },
          new CommandParameter ("@1", 1));
      
      CheckQuery (
          from c in Cooks select new { c.IsStarredCook, True = true, HasFirstName = c.FirstName != null },
          "SELECT [t0].[IsStarredCook] AS [IsStarredCook]," 
          + "CONVERT(BIT, @1) AS [True],"
          + "CONVERT(BIT, CASE WHEN ([t0].[FirstName] IS NOT NULL) THEN 1 ELSE 0 END) AS [HasFirstName] FROM [CookTable] AS [t0]",
          row => (object) new
          {
              IsStarredCook = row.GetValue<bool> (new ColumnID ("IsStarredCook", 0)),
              True = row.GetValue<bool> (new ColumnID ("True", 1)),
              HasFirstName = row.GetValue<bool> (new ColumnID ("HasFirstName", 2))
          },
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void NestedSelectProjection_WithCastInsideNamedExpression ()
    {
      CheckQuery (
          from c in Cooks select new { Name = c.Name, ConvertedID = (double) c.ID },
          "SELECT [t0].[Name] AS [Name],[t0].[ID] AS [ConvertedID] FROM [CookTable] AS [t0]",
          row => (object) new { Name = row.GetValue<string> (new ColumnID ("Name", 0)), ConvertedID = (double) row.GetValue<int> (new ColumnID ("ConvertedID", 1)) });

      CheckQuery (
        from c in Cooks select new { Name = c.Name, History = (double) c.ID < 100 ? "Long-time cook" : "New cook" },
          "SELECT [t0].[Name] AS [Name],CASE WHEN ([t0].[ID] < @1) THEN @2 ELSE @3 END AS [History] FROM [CookTable] AS [t0]",
          row => (object) new { Name = row.GetValue<string> (new ColumnID ("Name", 0)), History = row.GetValue<string> (new ColumnID ("History", 1)) },
        new CommandParameter ("@1", 100.0),
        new CommandParameter ("@2", "Long-time cook"),
        new CommandParameter ("@3", "New cook"));
    }

    [Test]
    public void LocallyEvaluatedMethod ()
    {
      CheckQuery (
          from c in Cooks select CustomStaticMethodWithEntity (c),
          "SELECT [t0].[ID] AS [Arg0_ID],[t0].[FirstName] AS [Arg0_FirstName],[t0].[Name] AS [Arg0_Name],[t0].[IsStarredCook] AS [Arg0_IsStarredCook],"
          + "[t0].[IsFullTimeCook] AS [Arg0_IsFullTimeCook],[t0].[SubstitutedID] AS [Arg0_SubstitutedID],[t0].[KitchenID] AS [Arg0_KitchenID],"
          + "[t0].[KnifeID] AS [Arg0_KnifeID],[t0].[KnifeClassID] AS [Arg0_KnifeClassID] "
          + "FROM [CookTable] AS [t0]",
          row => (object) CustomStaticMethodWithEntity (row.GetEntity<Cook> (GetColumnIDsForCook ("Arg0_"))));
      CheckQuery (
          from c in Cooks select CustomStaticMethodWithValue (c.ID),
          "SELECT [t0].[ID] AS [Arg0] FROM [CookTable] AS [t0]",
          row => (object) CustomStaticMethodWithValue (row.GetValue<int> (new ColumnID ("Arg0", 0))));
      CheckQuery (
          from c in Cooks select CustomStaticMethodWithBoolResult (c.ID),
          "SELECT [t0].[ID] AS [Arg0] FROM [CookTable] AS [t0]",
          row => (object) CustomStaticMethodWithBoolResult (row.GetValue<int> (new ColumnID ("Arg0", 0))));
      CheckQuery (
          from c in Cooks select CustomStaticMethodWithBoolParameter (c.IsFullTimeCook),
          "SELECT [t0].[IsFullTimeCook] AS [Arg0] FROM [CookTable] AS [t0]",
          row =>
          (object) CustomStaticMethodWithBoolParameter (row.GetValue<bool> (new ColumnID ("Arg0", 0))));
      CheckQuery (
          from c in Cooks select c.ID.GetTypeCode(),
          "SELECT [t0].[ID] AS [Object] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("Object", 0)).GetTypeCode ());
      CheckQuery (
          from c in Cooks select CustomInstanceMethodWithValue (c.ID),
          "SELECT NULL AS [Object],[t0].[ID] AS [Arg0] FROM [CookTable] AS [t0]",
          row => (object) CustomInstanceMethodWithValue (row.GetValue<int> (new ColumnID ("Arg0", 1))));
      var o = new object();
      CheckQuery (
          from c in Cooks select CustomStaticMethodWithValues (c.ID, o, c.FirstName),
          "SELECT [t0].[ID] AS [Arg0],NULL AS [Arg1],[t0].[FirstName] AS [Arg2] FROM [CookTable] AS [t0]",
          row => (object) CustomStaticMethodWithValues (row.GetValue<int> (new ColumnID ("Arg0", 0)), o, row.GetValue<string> (new ColumnID ("Arg2", 2))));
    }

    [Test]
    public void LocallyEvaluatedMethod_WithoutArguments ()
    {
      CheckQuery (
          from c in Cooks select CustomStaticMethodWithoutArguments (),
          "SELECT @1 AS [value] "
          + "FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "precalculated value!"));

      CheckQuery (
          from c in Cooks select CustomStaticThrowingMethodWithoutArguments(),
          "SELECT NULL FROM [CookTable] AS [t0]",
          row => (object) CustomStaticThrowingMethodWithoutArguments(),
          false);
    }

    [Test]
    public void LocallyEvaluatedMethod_WithConstantArguments ()
    {
      CheckQuery (
          from c in Cooks select CustomStaticMethodWithValues (42, null, "test"),
          "SELECT @1 AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "precalculated value!"));

      CheckQuery (
          from c in Cooks select CustomStaticMethodWithValues (42, null, c.FirstName),
          "SELECT NULL AS [Arg0],NULL AS [Arg1],[t0].[FirstName] AS [Arg2] FROM [CookTable] AS [t0]",
          row => (object) CustomStaticMethodWithValues (42, null, row.GetValue<string> (new ColumnID ("Arg2", 2))));

      CheckQuery (
          from c in Cooks select CustomStaticThrowingMethodWithValue (42),
          "SELECT NULL AS [Arg0] FROM [CookTable] AS [t0]",
          row => (object) CustomStaticThrowingMethodWithValue (42),
          false);
    }

    [Test]
    public void LocallyEvaluatedMethod_InNestedProjection ()
    {
      var columnIDsForCook = GetColumnIDsForCook ("A_Arg0_");
      CheckQuery (
          from c in Cooks select new { A = CustomStaticMethodWithEntity (c), B = CustomStaticMethodWithValue (c.ID) },
          "SELECT [t0].[ID] AS [A_Arg0_ID],[t0].[FirstName] AS [A_Arg0_FirstName],[t0].[Name] AS [A_Arg0_Name],"
          + "[t0].[IsStarredCook] AS [A_Arg0_IsStarredCook],[t0].[IsFullTimeCook] AS [A_Arg0_IsFullTimeCook],"
          + "[t0].[SubstitutedID] AS [A_Arg0_SubstitutedID],[t0].[KitchenID] AS [A_Arg0_KitchenID],"
          + "[t0].[KnifeID] AS [A_Arg0_KnifeID],[t0].[KnifeClassID] AS [A_Arg0_KnifeClassID],"
          + "[t0].[ID] AS [B_Arg0] " +
          "FROM [CookTable] AS [t0]",
          row =>
          (object) new
          {
              A = CustomStaticMethodWithEntity (row.GetEntity<Cook> (columnIDsForCook)),
              B = CustomStaticMethodWithValue (row.GetValue<int> (new ColumnID ("B_Arg0", columnIDsForCook.Length)))
          });

      CheckQuery (
          from c in Cooks select new { A = CustomStaticMethodWithBoolResult (c.ID) },
          "SELECT [t0].[ID] AS [A_Arg0] " +
          "FROM [CookTable] AS [t0]",
          row =>
          (object) new { A = CustomStaticMethodWithBoolResult (row.GetValue<int> (new ColumnID ("A_Arg0", 0))) });
    }

    [Test]
    [Ignore ("TODO 5348")]
    public void LocallyEvaluatedMethod_InsideNonLocalExpression_ShouldThrowException_OrBeExecutedInMemory ()
    {
      Assert.That (
          () => (from c in Cooks select CustomStaticMethodWithValue (c.ID)).Count(),
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("..."));

      Assert.That (
          () => from c in Cooks select CustomStaticMethodWithValue (c.ID) == "test",
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo ("..."));

      Assert.That (
          () => (from c in Cooks select CustomStaticMethodWithValue (c.ID)).Distinct(),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo ("..."));

    }

    private static string CustomStaticMethodWithEntity ([UsedImplicitly] Cook cook)
    {
      throw new NotImplementedException ();
    }

    private static string CustomStaticThrowingMethodWithoutArguments()
    {
      throw new NotImplementedException ("This is a custom exception.");
    }

    private static string CustomStaticMethodWithoutArguments ()
    {
      return "precalculated value!";
    }

    private static string CustomStaticMethodWithValue ([UsedImplicitly]int i)
    {
      throw new NotImplementedException ();
    }

    private static string CustomStaticThrowingMethodWithValue ([UsedImplicitly]int i)
    {
      throw new NotImplementedException ("This is a custom exception.");
    }

    private static string CustomStaticMethodWithValues ([UsedImplicitly]int i, [UsedImplicitly]object o, [UsedImplicitly]string s)
    {
      return "precalculated value!";
    }

    private static bool CustomStaticMethodWithBoolResult ([UsedImplicitly]int i)
    {
      throw new NotImplementedException ();
    }

    private static string CustomStaticMethodWithBoolParameter ([UsedImplicitly]bool b)
    {
      throw new NotImplementedException ();
    }

    private string CustomInstanceMethodWithValue ([UsedImplicitly]int i)
    {
      throw new NotImplementedException ();
    }


    private static ColumnID[] GetColumnIDsForCook (string prefix)
    {
      return new[] {new ColumnID (prefix + "ID", 0), 
                    new ColumnID (prefix + "FirstName", 1),
                    new ColumnID (prefix + "Name", 2),
                    new ColumnID (prefix + "IsStarredCook", 3),
                    new ColumnID (prefix + "IsFullTimeCook", 4),
                    new ColumnID (prefix + "SubstitutedID", 5),
                    new ColumnID (prefix + "KitchenID", 6),
                    new ColumnID (prefix + "KnifeID", 7),
                    new ColumnID (prefix + "KnifeClassID", 8)
                   };
    }
  }
}