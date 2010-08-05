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
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class BinaryExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Equals_NotEquals ()
    {
      CheckQuery (
          from c in Cooks where c.Name == "Huber" select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = @1)",
          new CommandParameter ("@1", "Huber"));
      CheckQuery (
          from c in Cooks where c.Name != "Huber" select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] <> @1)",
          new CommandParameter ("@1", "Huber"));
    }

    [Test]
    public void Equals_NotEquals_WithNull ()
    {
      CheckQuery (
          from c in Cooks where c.Name == null select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] IS NULL)"
          );
      CheckQuery (
          from c in Cooks where c.Name != null select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] IS NOT NULL)"
          );
    }

    [Test]
    public void Equals_NotEquals_WithTrue ()
    {
      // ReSharper disable RedundantBoolCompare
      CheckQuery (
          from c in Cooks where c.IsFullTimeCook == true select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[IsFullTimeCook] = @1)",
          new CommandParameter ("@1", 1)
          );
      // ReSharper restore RedundantBoolCompare
      CheckQuery (
          from c in Cooks where c.IsFullTimeCook != true select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[IsFullTimeCook] <> @1)",
          new CommandParameter ("@1", 1)
          );
    }

    [Test]
    public void Equals_NotEquals_False ()
    {
      CheckQuery (
          from c in Cooks where c.IsFullTimeCook == false select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[IsFullTimeCook] = @1)",
          new CommandParameter ("@1", 0)
          );
      // ReSharper disable RedundantBoolCompare
      CheckQuery (
          from c in Cooks where c.IsFullTimeCook != false select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[IsFullTimeCook] <> @1)",
          new CommandParameter ("@1", 0)
          );
      // ReSharper restore RedundantBoolCompare
    }

    [Test]
    public void Equals_WithBinaryRightSide ()
    {
      CheckQuery (
          from c in Cooks where c.IsStarredCook == (c.FirstName == "Sepp") select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[IsStarredCook] = CASE WHEN ([t0].[FirstName] = @1) THEN 1 ELSE 0 END)",
          new CommandParameter ("@1", "Sepp"));
    }

    [Test]
    public void LessThan_GreaterThan_OrEquals ()
    {
      CheckQuery (
          from c in Cooks where c.ID > 0 select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] > @1)",
          new CommandParameter ("@1", 0)
          );
      CheckQuery (
          from c in Cooks where c.ID >= 0 select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] >= @1)",
          new CommandParameter ("@1", 0)
          );
      CheckQuery (
          from c in Cooks where c.ID < 0 select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] < @1)",
          new CommandParameter ("@1", 0)
          );
      CheckQuery (
          from c in Cooks where c.ID <= 0 select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] <= @1)",
          new CommandParameter ("@1", 0)
          );
    }

    [Test]
    public void AndAlso_OrElse ()
    {
      CheckQuery (
          from c in Cooks where c.Name == "Huber" && c.FirstName == "Sepp" select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE (([t0].[Name] = @1) AND ([t0].[FirstName] = @2))",
          new CommandParameter ("@1", "Huber"),
          new CommandParameter ("@2", "Sepp"));
      CheckQuery (
          from c in Cooks where c.Name == "Huber" || c.FirstName == "Sepp" select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE (([t0].[Name] = @1) OR ([t0].[FirstName] = @2))",
          new CommandParameter ("@1", "Huber"),
          new CommandParameter ("@2", "Sepp"));

      CheckQuery (
          from c in Cooks where (c.Name == "Huber" && c.FirstName == "Sepp") || (c.Name == "Scott" && c.FirstName == "John") select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ((([t0].[Name] = @1) AND ([t0].[FirstName] = @2)) OR (([t0].[Name] = @3) AND ([t0].[FirstName] = @4)))",
          new CommandParameter ("@1", "Huber"),
          new CommandParameter ("@2", "Sepp"),
          new CommandParameter ("@3", "Scott"),
          new CommandParameter ("@4", "John"));
    }

    [Test]
    public void AndAlso_OrElse_WithTrueFalse ()
    {
      CheckQuery (
        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        // ReSharper disable RedundantLogicalConditionalExpressionOperand
          from c in Cooks where ((c.Name == "Huber") && true) || (false && (c.Name == "Huber")) select c.FirstName,
      // ReSharper restore RedundantLogicalConditionalExpressionOperand
        // ReSharper restore ConditionIsAlwaysTrueOrFalse
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ((([t0].[Name] = @1) AND (@2 = 1)) OR ((@3 = 1) AND ([t0].[Name] = @4)))",
          new CommandParameter ("@1", "Huber"),
          new CommandParameter ("@2", 1),
          new CommandParameter ("@3", 0),
          new CommandParameter ("@4", "Huber"));
    }

    [Test]
    public void Coalesce ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName ?? "hugo",
          "SELECT (COALESCE ([t0].[FirstName], @1)) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "hugo")
          );
    }

    [Test]
    public void MemberAccessOnCoalesceExpression ()
    {
      CheckQuery (Cooks.Select (c => (c.FirstName ?? c.Name).ToUpper ()),
        "SELECT UPPER((COALESCE ([t0].[FirstName], [t0].[Name]))) AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void MemberAccessOnConditionalExpression ()
    {
      CheckQuery (Cooks.Select (c => (c.IsStarredCook ? c.Name : c.SpecificInformation).Length),
        "SELECT CASE WHEN ([t0].[IsStarredCook] = 1) THEN LEN([t0].[Name]) ELSE LEN([t0].[SpecificInformation]) END AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void StringConcatenation ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName + "Test",
          "SELECT ([t0].[FirstName] + @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", "Test")
          );
      CheckQuery (
          from c in Cooks select c.FirstName + 10,
          "SELECT ([t0].[FirstName] + @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select c.FirstName + " " + c.Name,
          "SELECT (([t0].[FirstName] + @1) + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<string> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", " ")
          );
    }

    [Test]
    public void ArithmeticOperators ()
    {
      CheckQuery (
          from c in Cooks select c.ID + 10,
          "SELECT ([t0].[ID] + @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select c.ID - 10,
          "SELECT ([t0].[ID] - @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select c.ID * 10,
          "SELECT ([t0].[ID] * @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select c.ID / 10,
          "SELECT ([t0].[ID] / @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select c.ID % 10,
          "SELECT ([t0].[ID] % @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
    }

    [Test]
    public void ArithmeticOperators_Checked ()
    {
      CheckQuery (
          from c in Cooks select checked (c.ID + 10),
          "SELECT ([t0].[ID] + @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select checked (c.ID - 10),
          "SELECT ([t0].[ID] - @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select checked (c.ID * 10),
          "SELECT ([t0].[ID] * @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
    }

    [Test]
    public void ArithmeticOperators_Unchecked ()
    {
      CheckQuery (
          from c in Cooks select unchecked (c.ID + 10),
          "SELECT ([t0].[ID] + @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select unchecked (c.ID - 10),
          "SELECT ([t0].[ID] - @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select unchecked (c.ID * 10),
          "SELECT ([t0].[ID] * @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
    }

    [Test]
    public void Power ()
    {
      var parameter = Expression.Parameter (typeof (Cook), "c");
      var weight = Expression.MakeMemberAccess (parameter, typeof (Cook).GetProperty ("Weight"));
      var selector = Expression.Lambda<Func<Cook, double>> (Expression.Power (weight, Expression.Constant (3.0)), parameter);
      var query = Cooks.Select (selector); // from c in Cooks select c.Weight**3

      CheckQuery (
          query,
          "SELECT (POWER ([t0].[Weight], @1)) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<double> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 3.0)
          );
    }

    [Test]
    public void BitwiseOperators ()
    {
      CheckQuery (
          from c in Cooks select c.ID & 10,
          "SELECT ([t0].[ID] & @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select c.ID | 10,
          "SELECT ([t0].[ID] | @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
      CheckQuery (
          from c in Cooks select c.ID ^ 10,
          "SELECT ([t0].[ID] ^ @1) AS [value] FROM [CookTable] AS [t0]",
          row => (object) row.GetValue<int> (new ColumnID ("value", 0)),
          new CommandParameter ("@1", 10)
          );
    }

    [Test]
    public void Equals_EntityComparisonWithNull ()
    {
      CheckQuery (
          from k in Kitchens where k.Cook == null select k.Name,
          "SELECT [t0].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[KitchenID] "
          + "WHERE ([t1].[ID] IS NULL)");
    }


    [Test]
    public void EntityConstantExpression_WithIDMember ()
    {
      var cook = new Cook { ID = 5, Name = "Maier", FirstName = "Hugo" };
      CheckQuery (
          from c in Cooks where c.ID == cook.ID select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
          new CommandParameter ("@1", 5)
          );
    }

    [Test]
    public void EntityConstantExpression_WithReference ()
    {
      var cook = new Cook { ID = 5, Name = "Maier", FirstName = "Hugo" };
      CheckQuery (
          from c in Cooks where c == cook select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
          new CommandParameter ("@1", cook.ID)
          );
    }

    [Test]
    public void EntityConstantExpression_WithConstantID ()
    {
      const int id = 5;
      CheckQuery (
          from c in Cooks where c.ID == id select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
          new CommandParameter ("@1", 5)
          );
    }

    [Test]
    public void EntityComparison_WithNull ()
    {
      CheckQuery (
          from c in Cooks where c == null select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] IS NULL)"
          );
    }

    [Test]
    public void Equals_EntityComparison ()
    {
      CheckQuery (
          from k in Kitchens where k.Cook == k.Restaurant.SubKitchen.Cook select k.Name,
          "SELECT [t0].[Name] AS [value] FROM [KitchenTable] AS [t0] "+
          "LEFT OUTER JOIN [RestaurantTable] AS [t1] ON [t0].[RestaurantID] = [t1].[ID] "+
          "LEFT OUTER JOIN [KitchenTable] AS [t2] ON [t1].[ID] = [t2].[RestaurantID] "+
          "LEFT OUTER JOIN [CookTable] AS [t4] ON [t2].[ID] = [t4].[KitchenID] "+
          "LEFT OUTER JOIN [CookTable] AS [t3] ON [t0].[ID] = [t3].[KitchenID] "+
          "WHERE ([t3].[ID] = [t4].[ID])");
    }

    [Test]
    public void Equals_EntityComparison_WithSubQuery ()
    {
      CheckQuery (
          from c in Cooks where c == (from k in Kitchens select k.Cook).First () select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = (SELECT TOP (1) [t2].[ID] FROM [KitchenTable] AS [t1] "
          +"LEFT OUTER JOIN [CookTable] AS [t2] ON [t1].[ID] = [t2].[KitchenID]))");
    }

    [Test]
    public void CompoundValuesComparison_Equal_OnTopLevel ()
    {
      CheckQuery (
          from c in Cooks where new { X = c.Name, Y = c.IsFullTimeCook } == new { X = c.FirstName, Y = c.IsStarredCook } select c.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE (([t0].[Name] = [t0].[FirstName]) AND ([t0].[IsFullTimeCook] = "
          +"[t0].[IsStarredCook]))");
    }

    [Test]
    public void CompoundValuesComparison_NotEqual_OnTopLevel ()
    {
      CheckQuery (
          from c in Cooks where new { X = c.Name, Y = c.IsFullTimeCook } != new { X = c.FirstName, Y = c.IsStarredCook } select c.Name,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE (([t0].[Name] <> [t0].[FirstName]) AND ([t0].[IsFullTimeCook] <> "
          + "[t0].[IsStarredCook]))");
    }

    [Test]
    public void CompoundValuesComparison_ValuesComingFromSubquery ()
    {
      CheckQuery (
          from x in Cooks.Select (c => new { FirstName = c.FirstName, LastName = c.Name }).Distinct()
          where x == new { FirstName = "Hugo", LastName = "Boss" }
          select x.FirstName,
          "SELECT [q0].[FirstName] AS [value] FROM (SELECT DISTINCT [t1].[FirstName] AS [FirstName],[t1].[Name] AS [LastName] "+
          "FROM [CookTable] AS [t1]) AS [q0] WHERE (([q0].[FirstName] = @1) AND ([q0].[LastName] = @2))",
          new CommandParameter("@1", "Hugo"),
          new CommandParameter("@2", "Boss")
          );
     }

  }
}