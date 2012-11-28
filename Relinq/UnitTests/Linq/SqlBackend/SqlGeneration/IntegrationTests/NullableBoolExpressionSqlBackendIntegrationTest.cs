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
using Remotion.Linq.SqlBackend;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class NullableBoolExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void NullableBool_CastToBool ()
    {
      CheckQuery (
          from k in Kitchens where (bool) k.PassedLastInspection select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[PassedLastInspection] = 1)");

      CheckQuery (
// ReSharper disable RedundantBoolCompare
          from k in Kitchens where k.PassedLastInspection != null && ((bool) k.PassedLastInspection) == true select k.ID,
// ReSharper restore RedundantBoolCompare
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] IS NOT NULL) AND ([t0].[PassedLastInspection] = @1))",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void Bool_CastToNullableBool ()
    {
      CheckQuery (
          from k in Kitchens where ((bool?) (k.Name != null)) == true select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (CASE WHEN ([t0].[Name] IS NOT NULL) THEN 1 ELSE 0 END = @1)",
          new CommandParameter ("@1", 1));
    }

    [Test]
    public void NullableBool_HasValue_Value ()
    {
      CheckQuery (
          from k in Kitchens where k.PassedLastInspection.Value select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[PassedLastInspection] = 1)");

      CheckQuery (
          from k in Kitchens where k.PassedLastInspection.HasValue && k.PassedLastInspection.Value select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] IS NOT NULL) AND ([t0].[PassedLastInspection] = 1))");
      CheckQuery (
// ReSharper disable RedundantBoolCompare
          from k in Kitchens where k.PassedLastInspection.HasValue && k.PassedLastInspection.Value == true select k.ID,
// ReSharper restore RedundantBoolCompare
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] IS NOT NULL) AND ([t0].[PassedLastInspection] = @1))",
          new CommandParameter ("@1", 1));
    }

    [Test]
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

    [Test]
    public void NullableBool_InSelectClause ()
    {
      CheckQuery (
          from k in Kitchens select k.PassedLastInspection,
          "SELECT [t0].[PassedLastInspection] AS [value] FROM [KitchenTable] AS [t0]",
          row =>
          ConvertExpressionMarker (
              BooleanUtility.ConvertNullableIntToNullableBool (row.GetValue<int?> (new ColumnID ("value", 0)))));

      bool? nullableValue = true;
      CheckQuery (
          from k in Kitchens select nullableValue,
          "SELECT @1 AS [value] FROM [KitchenTable] AS [t0]",
          row =>
          ConvertExpressionMarker (
              BooleanUtility.ConvertNullableIntToNullableBool (row.GetValue<int?> (new ColumnID ("value", 0)))),
          new CommandParameter ("@1", 1));

      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var nullablePredicate =
          Expression.Lambda<Func<Kitchen, bool?>> (
              Expression.Equal (Expression.Property (kitchenParameter, "LastInspectionScore"), Expression.Constant (0, typeof (int?)), true, null),
              kitchenParameter);
      CheckQuery (
          Kitchens.Select (nullablePredicate),
          "SELECT CASE WHEN ([t0].[LastInspectionScore] = @1) THEN 1 WHEN NOT ([t0].[LastInspectionScore] = @2) THEN 0 ELSE NULL END AS [value] "
          + "FROM [KitchenTable] AS [t0]",
          row =>
          ConvertExpressionMarker (
              BooleanUtility.ConvertNullableIntToNullableBool (row.GetValue<int?> (new ColumnID ("value", 0)))),
          new CommandParameter ("@1", 0),
          new CommandParameter ("@2", 0));
    }

    [Test]
    public void NullableBool_InOrderByClause ()
    {
      CheckQuery (
          from k in Kitchens orderby k.PassedLastInspection select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] ORDER BY [t0].[PassedLastInspection] ASC");

      bool? nullableValue = true;
      CheckQuery (
          from k in Kitchens orderby nullableValue select k.ID,
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] ORDER BY @1 ASC",
          new CommandParameter ("@1", 1));

      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var nullablePredicate =
          Expression.Lambda<Func<Kitchen, bool?>> (
              Expression.Equal (Expression.Property (kitchenParameter, "LastInspectionScore"), Expression.Constant (0, typeof (int?)), true, null),
              kitchenParameter);
      CheckQuery (
          Kitchens.OrderBy (nullablePredicate).Select (k => k.ID),
          "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] "
          + "ORDER BY CASE WHEN ([t0].[LastInspectionScore] = @1) THEN 1 WHEN NOT ([t0].[LastInspectionScore] = @2) THEN 0 ELSE NULL END ASC",
          new CommandParameter ("@1", 0),
          new CommandParameter ("@2", 0));
    }

    [Test]
    public void NullableBool_InWhereClause ()
    {
      CheckQuery (
         from k in Kitchens where (bool) k.PassedLastInspection select k.ID,
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[PassedLastInspection] = 1)");

      bool? nullableValue = true;
      CheckQuery (
         from k in Kitchens where (bool) nullableValue select k.ID,
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (@1 = 1)",
         new CommandParameter ("@1", 1));

      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var convertedNullablePredicate =
          Expression.Lambda<Func<Kitchen, bool>> (
            Expression.Convert (
              Expression.Equal (Expression.Property (kitchenParameter, "LastInspectionScore"), Expression.Constant (0, typeof (int?)), true, null),
              typeof (bool)),
              kitchenParameter);
      CheckQuery (
         Kitchens.Where (convertedNullablePredicate).Select (k => k.ID),
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[LastInspectionScore] = @1)",
         new CommandParameter ("@1", 0));
    }

    [Test]
    public void NullableBool_AndAlso_Lifted ()
    {
      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var conjunction = Expression.AndAlso (
          Expression.Property (kitchenParameter, "PassedLastInspection"), Expression.Property (kitchenParameter, "PassedLastInspection"));
      Assert.That (conjunction.IsLiftedToNull, Is.True);
      var selector = Expression.Lambda<Func<Kitchen, bool?>> (conjunction, kitchenParameter);
      CheckQuery (
         Kitchens.Select (selector),
         "SELECT CASE WHEN (([t0].[PassedLastInspection] = 1) AND ([t0].[PassedLastInspection] = 1)) "
          + "THEN 1 WHEN NOT (([t0].[PassedLastInspection] = 1) AND ([t0].[PassedLastInspection] = 1)) "
          + "THEN 0 ELSE NULL END AS [value] FROM [KitchenTable] AS [t0]");

      var predicate = Expression.Lambda<Func<Kitchen, bool>> (Expression.Convert (conjunction, typeof (bool)), kitchenParameter);
      CheckQuery (
         Kitchens.Where (predicate).Select (k => k.ID),
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] = 1) AND ([t0].[PassedLastInspection] = 1))");
    }

    [Test]
    public void NullableBool_And_Lifted ()
    {
      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var conjunction = Expression.And (
          Expression.Property (kitchenParameter, "PassedLastInspection"), Expression.Property (kitchenParameter, "PassedLastInspection"));
      Assert.That (conjunction.IsLiftedToNull, Is.True);
      var selector = Expression.Lambda<Func<Kitchen, bool?>> (conjunction, kitchenParameter);
      CheckQuery (
         Kitchens.Select (selector),
         "SELECT CASE WHEN (([t0].[PassedLastInspection] = 1) AND ([t0].[PassedLastInspection] = 1)) "
          + "THEN 1 WHEN NOT (([t0].[PassedLastInspection] = 1) AND ([t0].[PassedLastInspection] = 1)) "
          + "THEN 0 ELSE NULL END AS [value] FROM [KitchenTable] AS [t0]");

      var predicate = Expression.Lambda<Func<Kitchen, bool>> (Expression.Convert (conjunction, typeof (bool)), kitchenParameter);
      CheckQuery (
         Kitchens.Where (predicate).Select (k => k.ID),
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] = 1) AND ([t0].[PassedLastInspection] = 1))");
    }

    [Test]
    public void NullableBool_OrElse_Lifted ()
    {
      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var conjunction = Expression.OrElse (
          Expression.Property (kitchenParameter, "PassedLastInspection"), Expression.Property (kitchenParameter, "PassedLastInspection"));
      Assert.That (conjunction.IsLiftedToNull, Is.True);
      var selector = Expression.Lambda<Func<Kitchen, bool?>> (conjunction, kitchenParameter);
      CheckQuery (
         Kitchens.Select (selector),
         "SELECT CASE WHEN (([t0].[PassedLastInspection] = 1) OR ([t0].[PassedLastInspection] = 1)) "
          + "THEN 1 WHEN NOT (([t0].[PassedLastInspection] = 1) OR ([t0].[PassedLastInspection] = 1)) "
          + "THEN 0 ELSE NULL END AS [value] FROM [KitchenTable] AS [t0]");

      var predicate = Expression.Lambda<Func<Kitchen, bool>> (Expression.Convert (conjunction, typeof (bool)), kitchenParameter);
      CheckQuery (
         Kitchens.Where (predicate).Select (k => k.ID),
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] = 1) OR ([t0].[PassedLastInspection] = 1))");
    }

    [Test]
    public void NullableBool_Or_Lifted ()
    {
      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var conjunction = Expression.Or (
          Expression.Property (kitchenParameter, "PassedLastInspection"), Expression.Property (kitchenParameter, "PassedLastInspection"));
      Assert.That (conjunction.IsLiftedToNull, Is.True);
      var selector = Expression.Lambda<Func<Kitchen, bool?>> (conjunction, kitchenParameter);
      CheckQuery (
         Kitchens.Select (selector),
         "SELECT CASE WHEN (([t0].[PassedLastInspection] = 1) OR ([t0].[PassedLastInspection] = 1)) "
          + "THEN 1 WHEN NOT (([t0].[PassedLastInspection] = 1) OR ([t0].[PassedLastInspection] = 1)) "
          + "THEN 0 ELSE NULL END AS [value] FROM [KitchenTable] AS [t0]");

      var predicate = Expression.Lambda<Func<Kitchen, bool>> (Expression.Convert (conjunction, typeof (bool)), kitchenParameter);
      CheckQuery (
         Kitchens.Where (predicate).Select (k => k.ID),
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE (([t0].[PassedLastInspection] = 1) OR ([t0].[PassedLastInspection] = 1))");
    }

    [Test]
    public void NullableBool_Not_Lifted ()
    {
      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var conjunction = Expression.Not (Expression.Property (kitchenParameter, "PassedLastInspection"));
      Assert.That (conjunction.IsLiftedToNull, Is.True);
      var selector = Expression.Lambda<Func<Kitchen, bool?>> (conjunction, kitchenParameter);
      CheckQuery (
         Kitchens.Select (selector),
         "SELECT CASE WHEN NOT ([t0].[PassedLastInspection] = 1) "
          + "THEN 1 WHEN NOT NOT ([t0].[PassedLastInspection] = 1) "
          + "THEN 0 ELSE NULL END AS [value] FROM [KitchenTable] AS [t0]");

      var predicate = Expression.Lambda<Func<Kitchen, bool>> (Expression.Convert (conjunction, typeof (bool)), kitchenParameter);
      CheckQuery (
         Kitchens.Where (predicate).Select (k => k.ID),
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE NOT ([t0].[PassedLastInspection] = 1)");
    }

    [Test]
    public void NullableBool_Coalesce_ToFalse_InSelectClause ()
    {
      CheckQuery (
         from k in Kitchens select k.PassedLastInspection ?? false,
         "SELECT (COALESCE ([t0].[PassedLastInspection], @1)) AS [value] FROM [KitchenTable] AS [t0]",
         row => ConvertExpressionMarker (Convert.ToBoolean (row.GetValue<int>(new ColumnID ("value", 0)))),
         new  CommandParameter ("@1", 0));

      // Note: Can't coalesce a constant value, this would be replaced by the partial evaluator.

      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var coalescedNullablePredicate =
          Expression.Lambda<Func<Kitchen, bool>> (
          Expression.Coalesce (
              Expression.Equal (Expression.Property (kitchenParameter, "LastInspectionScore"), Expression.Constant (0, typeof (int?)), true, null),
              Expression.Constant (false)),
              kitchenParameter);
      CheckQuery (
         Kitchens.Select (coalescedNullablePredicate),
         "SELECT (COALESCE (CASE WHEN ([t0].[LastInspectionScore] = @1) THEN 1 WHEN NOT ([t0].[LastInspectionScore] = @2) THEN 0 ELSE NULL END, @3)) "
          + "AS [value] FROM [KitchenTable] AS [t0]",
         row => ConvertExpressionMarker (Convert.ToBoolean (row.GetValue<int> (new ColumnID ("value", 0)))),
         new CommandParameter ("@1", 0),
         new CommandParameter ("@2", 0),
         new CommandParameter ("@3", 0));
    }

    [Test]
    public void NullableBool_Coalesce_ToTrue_InSelectClause ()
    {
      CheckQuery (
         from k in Kitchens select k.PassedLastInspection ?? true,
         "SELECT (COALESCE ([t0].[PassedLastInspection], @1)) AS [value] FROM [KitchenTable] AS [t0]",
         row => ConvertExpressionMarker (Convert.ToBoolean (row.GetValue<int> (new ColumnID ("value", 0)))),
         new CommandParameter ("@1", 1));

      // Note: Can't coalesce a constant value, this would be replaced by the partial evaluator.

      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var coalescedNullablePredicate =
          Expression.Lambda<Func<Kitchen, bool>> (
          Expression.Coalesce (
              Expression.Equal (Expression.Property (kitchenParameter, "LastInspectionScore"), Expression.Constant (0, typeof (int?)), true, null),
              Expression.Constant (true)),
              kitchenParameter);
      CheckQuery (
         Kitchens.Select (coalescedNullablePredicate),
         "SELECT (COALESCE (CASE WHEN ([t0].[LastInspectionScore] = @1) THEN 1 WHEN NOT ([t0].[LastInspectionScore] = @2) THEN 0 ELSE NULL END, @3))"
          + " AS [value] FROM [KitchenTable] AS [t0]",
         row => ConvertExpressionMarker (Convert.ToBoolean (row.GetValue<int> (new ColumnID ("value", 0)))),
         new CommandParameter ("@1", 0),
         new CommandParameter ("@2", 0),
         new CommandParameter ("@3", 1));
    }

    [Test]
    public void NullableBool_Coalesce_ToFalse_InWhereClause ()
    {
      // COALESCE to false is simply ignored, as SQL behaves "falsey" with NULL values.

      CheckQuery (
         from k in Kitchens where k.PassedLastInspection ?? false select k.ID,
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ([t0].[PassedLastInspection] = 1)");

      // Note: Can't coalesce a constant value, this would be replaced by the partial evaluator.

      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var coalescedNullablePredicate =
          Expression.Lambda<Func<Kitchen, bool>> (
          Expression.Coalesce (
              Expression.Equal (Expression.Property (kitchenParameter, "LastInspectionScore"), Expression.Constant (0, typeof (int?)), true, null),
              Expression.Constant (false)),
              kitchenParameter);
      CheckQuery (
         Kitchens.Where (coalescedNullablePredicate).Select (k => k.ID),
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] "
          + "WHERE ([t0].[LastInspectionScore] = @1)",
         new CommandParameter ("@1", 0));
    }
    
    [Test]
    public void NullableBool_Coalesce_ToTrue_InWhereClause ()
    {
      // COALESCE to true cannot be ignored.

      CheckQuery (
         from k in Kitchens where k.PassedLastInspection ?? true select k.ID,
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] WHERE ((COALESCE ([t0].[PassedLastInspection], @1)) = 1)",
         new CommandParameter ("@1", 1));
      
      // Note: Can't coalesce a constant value, this would be replaced by the partial evaluator.

      var kitchenParameter = Expression.Parameter (typeof (Kitchen), "k");
      var coalescedNullablePredicate =
          Expression.Lambda<Func<Kitchen, bool>> (
          Expression.Coalesce (
              Expression.Equal (Expression.Property (kitchenParameter, "LastInspectionScore"), Expression.Constant (0, typeof (int?)), true, null),
              Expression.Constant (true)),
              kitchenParameter);
      CheckQuery (
         Kitchens.Where (coalescedNullablePredicate).Select (k => k.ID),
         "SELECT [t0].[ID] AS [value] FROM [KitchenTable] AS [t0] "
         + "WHERE ((COALESCE (CASE WHEN ([t0].[LastInspectionScore] = @1) THEN 1 WHEN NOT ([t0].[LastInspectionScore] = @2) THEN 0 ELSE NULL END, @3)) "
          + "= 1)",
         new CommandParameter ("@1", 0),
         new CommandParameter ("@2", 0),
         new CommandParameter ("@3", 1));
    }
  }
}