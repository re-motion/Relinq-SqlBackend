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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class CompoundMemberSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void SelectingCompoundMember ()
    {
      CheckQuery (
          from c in Cooks select c.KnifeID,
          "SELECT [t0].[KnifeID] AS [Value],[t0].[KnifeClassID] AS [ClassID] FROM [CookTable] AS [t0]",
          AddNewMetaIDMemberDecoration (
              row => (object) new MetaID (row.GetValue<int> (new ColumnID ("Value", 0)), row.GetValue<string> (new ColumnID ("ClassID", 1))))
          );
    }

    [Test]
    public void SelectingCompoundMember_FromSubquery ()
    {
      CheckQuery (
          from m in Cooks.Select (x => x.KnifeID).Distinct() select m,
          "SELECT [q0].[Value] AS [Value],[q0].[ClassID] AS [ClassID] "
          + "FROM (SELECT DISTINCT [t1].[KnifeID] AS [Value],[t1].[KnifeClassID] AS [ClassID] FROM [CookTable] AS [t1]) AS [q0]",
          AddNewMetaIDMemberDecoration (
              row => (object) new MetaID (row.GetValue<int> (new ColumnID ("Value", 0)), row.GetValue<string> (new ColumnID ("ClassID", 1)))));
    }

    [Test]
    public void ChainedMemberAccess_OnCompoundMember ()
    {
      CheckQuery (
          from c in Cooks select c.KnifeID.ClassID,
          "SELECT [t0].[KnifeClassID] AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0))
          );
    }

    [Test]
    public void ChainedMemberAccess_OnCompoundMember_FromSubquery ()
    {
      CheckQuery (
          from m in Cooks.Select (x => x.KnifeID).Distinct () select m.ClassID,
          "SELECT [q0].[ClassID] AS [value] "
          + "FROM (SELECT DISTINCT [t1].[KnifeID] AS [Value],[t1].[KnifeClassID] AS [ClassID] FROM [CookTable] AS [t1]) AS [q0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)));
    }

    [Test]
    public void SelectingMultipleCompoundMembers ()
    {
      CheckQuery (
          from c in Cooks
          from c2 in Cooks
          select new { ID1 = c.KnifeID, ID2 = c2.KnifeID },
          "SELECT [t0].[KnifeID] AS [ID1_Value],[t0].[KnifeClassID] AS [ID1_ClassID],[t1].[KnifeID] AS [ID2_Value],[t1].[KnifeClassID] AS [ID2_ClassID] "
          + "FROM [CookTable] AS [t0] CROSS JOIN [CookTable] AS [t1]",
          AddNewMetaIDMemberDecoration (
              row => (object)
                     new
                     {
                         ID1 = new MetaID (row.GetValue<int> (new ColumnID ("ID1_Value", 0)), row.GetValue<string> (new ColumnID ("ID1_ClassID", 1))),
                         ID2 = new MetaID (row.GetValue<int> (new ColumnID ("ID2_Value", 2)), row.GetValue<string> (new ColumnID ("ID2_ClassID", 3)))
                     }));
    }

    [Test]
    public void ComparingCompoundMember ()
    {
      CheckQuery (
          from c in Cooks 
          from c2 in Cooks
          where c.KnifeID.Equals (c2.KnifeID)
          select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] CROSS JOIN [CookTable] AS [t1] "
          + "WHERE (([t0].[KnifeID] = [t1].[KnifeID]) AND ([t0].[KnifeClassID] = [t1].[KnifeClassID]))");
    }

    [Test]
    public void ComparingCompoundMember_Value ()
    {
      CheckQuery (
          from c in Cooks
          from c2 in Cooks
          where c.KnifeID.Value.Equals (c2.KnifeID.Value)
          select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] CROSS JOIN [CookTable] AS [t1] "
          + "WHERE ([t0].[KnifeID] = [t1].[KnifeID])");
    }

    [Test]
    public void ComparingCompoundMember_WithConstant ()
    {
      var someID = new MetaID (0, "C0");
      CheckQuery (
          from c in Cooks
          where c.KnifeID.Equals (someID)
          select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] "
          + "WHERE (([t0].[KnifeID] = @1) AND ([t0].[KnifeClassID] = @2))",
          new CommandParameter ("@1", 0),
          new CommandParameter ("@2", "C0"));
    }

    [Test]
    public void Contains_WithCompoundMember_IsNotSupported ()
    {
      var someIDs = new[] { new MetaID (0, "C0"), new MetaID (1, "C1") };
      Assert.That (
          () => CheckQuery (
          from c in Cooks
          where someIDs.Contains (c.KnifeID)
          select c.ID,
          "Not supported"),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The SQL 'IN' operator (originally probably a call to a 'Contains' method) requires a single value, so the following expression cannot be "
                  + "translated to SQL: 'new MetaID(Value = [t0].[KnifeID] AS Value, ClassID = [t0].[KnifeClassID] AS ClassID) "
                  + "IN (value(Remotion.Linq.SqlBackend.UnitTests.TestDomain.MetaID),value(Remotion.Linq.SqlBackend.UnitTests.TestDomain.MetaID))'."));
    }

    [Test]
    public void Contains_WithCompoundMember_Value ()
    {
      var someIDs = new[] { new MetaID (0, "C0"), new MetaID (1, "C1") };
      var someIDValues = someIDs.Select (id => id.Value).ToArray();
      CheckQuery (
          from c in Cooks
          where someIDValues.Contains (c.KnifeID.Value)
          select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] "
          + "WHERE [t0].[KnifeID] IN (@1, @2)",
          new CommandParameter ("@1", 0),
          new CommandParameter ("@2", 1));
    }

    [Test]
    public void SelectingEntityWithCompoundID ()
    {
      CheckQuery (
          from k in Knives select k,
          "SELECT [t0].[ID],[t0].[ClassID],[t0].[Sharpness] FROM [KnifeTable] AS [t0]",
          row => (object) row.GetEntity<Knife> (new ColumnID ("ID", 0), new ColumnID ("ClassID", 1), new ColumnID ("Sharpness", 2)));
    }

    [Test]
    public void SelectingID_OfEntityWithCompoundID ()
    {
      CheckQuery (
          from k in Knives select k.ID,
          "SELECT [t0].[ID] AS [Value],[t0].[ClassID] AS [ClassID] FROM [KnifeTable] AS [t0]",
          AddNewMetaIDMemberDecoration (
            row => (object) new MetaID (row.GetValue<int> (new ColumnID ("Value", 0)), row.GetValue<string> (new ColumnID ("ClassID", 1)))));
    }

    [Test]
    public void ComparingEntitiesWithCompoundID ()
    {
      CheckQuery (
          from k1 in Knives
          from k2 in Knives
          where k1 == k2
          select k1.ID,
          "SELECT [t0].[ID] AS [Value],[t0].[ClassID] AS [ClassID] FROM [KnifeTable] AS [t0] CROSS JOIN [KnifeTable] AS [t1] "
          + "WHERE (([t0].[ID] = [t1].[ID]) AND ([t0].[ClassID] = [t1].[ClassID]))");

      CheckQuery (
          from k1 in Knives
          from k2 in Knives
          where k1 != k2
          select k1.ID,
          "SELECT [t0].[ID] AS [Value],[t0].[ClassID] AS [ClassID] FROM [KnifeTable] AS [t0] CROSS JOIN [KnifeTable] AS [t1] "
          + "WHERE (([t0].[ID] <> [t1].[ID]) OR ([t0].[ClassID] <> [t1].[ClassID]))");
    }

    [Test]
    public void ComparingEntitiesWithCompoundID_ToConstant ()
    {
      var someEntity = new Knife { ID = new MetaID (0, "C0") };
      CheckQuery (
          from k in Knives
          where k == someEntity
          select k.ID,
          "SELECT [t0].[ID] AS [Value],[t0].[ClassID] AS [ClassID] FROM [KnifeTable] AS [t0] "
          + "WHERE (([t0].[ID] = @1) AND ([t0].[ClassID] = @2))",
          new CommandParameter ("@1", 0),
          new CommandParameter ("@2", "C0"));
      CheckQuery (
          from k in Knives
          where k != someEntity
          select k.ID,
          "SELECT [t0].[ID] AS [Value],[t0].[ClassID] AS [ClassID] FROM [KnifeTable] AS [t0] "
          + "WHERE (([t0].[ID] <> @1) OR ([t0].[ClassID] <> @2))",
          new CommandParameter ("@1", 0),
          new CommandParameter ("@2", "C0"));
    }

    [Test]
    public void ComparingEntitiesWithCompoundID_ToNull ()
    {
      CheckQuery (
          from k in Knives
          where k == null
          select k.ID,
          "SELECT [t0].[ID] AS [Value],[t0].[ClassID] AS [ClassID] FROM [KnifeTable] AS [t0] "
          + "WHERE (([t0].[ID] IS NULL) AND ([t0].[ClassID] IS NULL))");

      CheckQuery (
          from k in Knives
          where k != null
          select k.ID,
          "SELECT [t0].[ID] AS [Value],[t0].[ClassID] AS [ClassID] FROM [KnifeTable] AS [t0] "
          + "WHERE (([t0].[ID] IS NOT NULL) OR ([t0].[ClassID] IS NOT NULL))");
    }

    [Test]
    public void Join_OverCompoundColumn_WithCompoundID ()
    {
      CheckQuery (
          from c in Cooks
          join k in Knives on c.KnifeID equals k.ID
          select new { Cook = c.ID, Knife = k.ID },
          "SELECT [t0].[ID] AS [Cook],[t1].[ID] AS [Knife_Value],[t1].[ClassID] AS [Knife_ClassID] "
          + "FROM [CookTable] AS [t0] CROSS JOIN [KnifeTable] AS [t1] "
          + "WHERE (([t0].[KnifeID] = [t1].[ID]) AND ([t0].[KnifeClassID] = [t1].[ClassID]))");
    }

    [Test]
    public void ImplicitJoin_WithCompoundID ()
    {
      CheckQuery (
          from c in Cooks
          select c.Knife.Sharpness,
          "SELECT [t1].[Sharpness] AS [value] "
          + "FROM [CookTable] AS [t0] "
          + "LEFT OUTER JOIN [KnifeTable] AS [t1] ON (([t0].[KnifeID] = [t1].[ID]) AND ([t0].[KnifeClassID] = [t1].[ClassID]))");
    }

    [Test]
    public void ImplicitJoin_WithOptimizedAwayLeftOuterJoin_ForCompoundID ()
    {
      CheckQuery (
          from c in Cooks
          where c.Knife == null
          select c.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CookTable] AS [t0] WHERE (([t0].[KnifeID] IS NULL) AND ([t0].[KnifeClassID] IS NULL))");
    }

    [Test]
    public void ImplicitJoin_WithOptimizedAwayLeftOuterJoin_ForCompoundID_IDMember ()
    {
      CheckQuery (
          from c in Cooks
          where c.Knife.ID == null
          select c.Knife.ID,
          "SELECT [t0].[KnifeID] AS [Value],[t0].[KnifeClassID] AS [ClassID] "
          + "FROM [CookTable] AS [t0] WHERE (([t0].[KnifeID] IS NULL) AND ([t0].[KnifeClassID] IS NULL))");
    }

    // Adds Value = ... and ClassID = ... member decorations to all NewExpressions for MetaID within the given expression.
    private Expression<Func<IDatabaseResultRow, object>> AddNewMetaIDMemberDecoration (Expression<Func<IDatabaseResultRow, object>> expression)
    {
      return AdHocExpressionVisitor.TransformAndRetainType (
          expression,
          expr =>
          {
            var newExpression = expr as NewExpression;
            if (newExpression != null && newExpression.Constructor.DeclaringType == typeof (MetaID))
            {
              return Expression.New (
                  newExpression.Constructor,
                  newExpression.Arguments,
                  new[] { typeof (MetaID).GetProperty ("Value"), typeof (MetaID).GetProperty ("ClassID") });
            }
            return expr;
          });
    }
  }
}