// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Linq.SqlBackend;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class MethodCallExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void String_Length_Property ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName != null && c.FirstName.Length > 0 select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE (([t0].[FirstName] IS NOT NULL) AND ((LEN(([t0].[FirstName] + '#')) - 1) > @1))",
          new CommandParameter ("@1", 0)
          );
    }

    [Test]
    public void String_IsNullOrEmpty ()
    {
      CheckQuery (
          from c in Cooks where string.IsNullOrEmpty (c.FirstName) select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE (([t0].[FirstName] IS NULL) OR ((LEN(([t0].[FirstName] + '#')) - 1) = 0))");
    }

    [Test]
    public void String_Contains ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.Contains ("abc") select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1 ESCAPE '\'",
          new CommandParameter ("@1", "%abc%")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.Contains ("a%b_c[a] [^]") select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1 ESCAPE '\'",
          new CommandParameter ("@1", @"%a\%b\_c\[a] \[^]%")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.Contains (null) select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE (@1 = 1)",
          new CommandParameter ("@1", 0));
    }

    [Test]
    public void String_Contains_WithNonConstantValue ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.Contains (c.Name) select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE (('%' + REPLACE(REPLACE(REPLACE(REPLACE([t0].[Name], '\', '\\'), '%', '\%'), '_', '\_'), '[', '\[')) + '%') ESCAPE '\'"
        );
    }

    [Test]
    public void String_Concat ()
    {
      // object overloads
      CheckQuery (
          from c in Cooks select string.Concat (c.ID),
          "SELECT CONVERT(NVARCHAR, [t0].[ID]) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select string.Concat (c.ID, c.ID),
          "SELECT (CONVERT(NVARCHAR, [t0].[ID]) + CONVERT(NVARCHAR, [t0].[ID])) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select string.Concat (c.ID, c.ID, c.FirstName),
          "SELECT ((CONVERT(NVARCHAR, [t0].[ID]) + CONVERT(NVARCHAR, [t0].[ID])) + [t0].[FirstName]) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select string.Concat (c.ID, c.ID, c.FirstName, c.Name),
          "SELECT (((CONVERT(NVARCHAR, [t0].[ID]) + CONVERT(NVARCHAR, [t0].[ID])) + [t0].[FirstName]) + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]"
          );

      // string overloads
      CheckQuery (
          from c in Cooks select string.Concat (c.FirstName, c.Name),
          "SELECT ([t0].[FirstName] + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select string.Concat (c.FirstName, c.Name, c.FirstName),
          "SELECT (([t0].[FirstName] + [t0].[Name]) + [t0].[FirstName]) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select string.Concat (c.FirstName, c.Name, c.FirstName, c.Name),
          "SELECT ((([t0].[FirstName] + [t0].[Name]) + [t0].[FirstName]) + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]"
          );

      // string[] overload
      CheckQuery (
          from c in Cooks select string.Concat (new[] { c.FirstName, c.Name }),
          "SELECT ([t0].[FirstName] + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]"
          );

      // object[] overload
      CheckQuery (
          from c in Cooks select string.Concat (new object[] { c.ID, c.Name }),
          "SELECT (CONVERT(NVARCHAR, [t0].[ID]) + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]"
          );
    }

    [Test]
    public void StartsWith ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.StartsWith ("abc") select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1 ESCAPE '\'",
          new CommandParameter ("@1", "abc%")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.StartsWith ("a%b_c[a] [^]") select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1 ESCAPE '\'",
          new CommandParameter ("@1", @"a\%b\_c\[a] \[^]%")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.StartsWith (null) select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE (@1 = 1)",
          new CommandParameter ("@1", 0)
          );
    }

    [Test]
    public void StartsWith_WithNonConstantValue()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.StartsWith (c.Name) select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE (REPLACE(REPLACE(REPLACE(REPLACE([t0].[Name], '\', '\\'), '%', '\%'), '_', '\_'), '[', '\[') + '%') ESCAPE '\'"
        );
    }

    [Test]
    public void EndsWith ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.EndsWith ("abc") select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1 ESCAPE '\'",
          new CommandParameter ("@1", "%abc")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.EndsWith ("a%b_c[a] [^]") select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1 ESCAPE '\'",
          new CommandParameter ("@1", @"%a\%b\_c\[a] \[^]")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.EndsWith (null) select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE (@1 = 1)",
          new CommandParameter ("@1", 0)
          );
    }

    [Test]
    public void EndsWith_WithNonConstantValue ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.EndsWith(c.Name) select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE ('%' + REPLACE(REPLACE(REPLACE(REPLACE([t0].[Name], '\', '\\'), '%', '\%'), '_', '\_'), '[', '\[')) ESCAPE '\'"
        );
    }

    [Test]
    public void Convert ()
    {
      CheckQuery (
          from c in Cooks select c.ID.ToString(),
          "SELECT CONVERT(NVARCHAR, [t0].[ID]) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select System.Convert.ToInt32 (c.FirstName),
          "SELECT CONVERT(INT, [t0].[FirstName]) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select System.Convert.ToString (c.ID),
          "SELECT CONVERT(NVARCHAR, [t0].[ID]) AS [value] FROM [CookTable] AS [t0]"
          );
    }

    [Test]
    public void ToLower_ToUpper ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName.ToLower(),
          "SELECT LOWER([t0].[FirstName]) AS [value] FROM [CookTable] AS [t0]"
          );
      CheckQuery (
          from c in Cooks select c.FirstName.ToUpper(),
          "SELECT UPPER([t0].[FirstName]) AS [value] FROM [CookTable] AS [t0]"
          );
    }

    [Test]
    public void Remove ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName.Remove (3),
          "SELECT STUFF([t0].[FirstName], (@1 + 1), (LEN(([t0].[FirstName] + '#')) - 1), '') AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 3)
          );
      CheckQuery (
          from c in Cooks select c.FirstName.Remove (3, 5),
          "SELECT STUFF([t0].[FirstName], (@1 + 1), @2, '') AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 3),
          new CommandParameter ("@2", 5)
          );
    }

    [Test]
    public void Substring ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName.Substring (3),
          "SELECT SUBSTRING([t0].[FirstName], (@1 + 1), (LEN(([t0].[FirstName] + '#')) - 1)) AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 3)
          );
      CheckQuery (
          from c in Cooks select c.FirstName.Substring (3, 5),
          "SELECT SUBSTRING([t0].[FirstName], (@1 + 1), @2) AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 3),
          new CommandParameter ("@2", 5)
          );
    }

    [Test]
    public void IndexOf ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ("test"),
          "SELECT CASE WHEN ((LEN((@1 + '#')) - 1) = 0) THEN 0 ELSE (CHARINDEX(@2, [t0].[FirstName]) - 1) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", "test"),
          new CommandParameter ("@2", "test")
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ('t'),
          "SELECT CASE WHEN ((LEN((@1 + '#')) - 1) = 0) THEN 0 ELSE (CHARINDEX(@2, [t0].[FirstName]) - 1) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 't'),
          new CommandParameter ("@2", 't')
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ("test", 2),
          "SELECT CASE WHEN (((LEN((@1 + '#')) - 1) = 0) AND ((@2 + 1) <= (LEN(([t0].[FirstName] + '#')) - 1))) THEN @3 ELSE (CHARINDEX(@4, [t0].[FirstName], (@5 + 1)) - 1) END AS [value] "
          + "FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", "test"),
          new CommandParameter ("@2", 2),
          new CommandParameter ("@3", 2),
          new CommandParameter ("@4", "test"),
          new CommandParameter ("@5", 2)
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ('t', 2),
          "SELECT CASE WHEN (((LEN((@1 + '#')) - 1) = 0) AND ((@2 + 1) <= (LEN(([t0].[FirstName] + '#')) - 1))) THEN @3 ELSE (CHARINDEX(@4, [t0].[FirstName], (@5 + 1)) - 1) END AS [value] "
          + "FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 't'),
          new CommandParameter ("@2", 2),
          new CommandParameter ("@3", 2),
          new CommandParameter ("@4", 't'),
          new CommandParameter ("@5", 2)
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ("test", 2, 5),
          "SELECT CASE WHEN (((LEN((@1 + '#')) - 1) = 0) AND ((@2 + 1) <= (LEN(([t0].[FirstName] + '#')) - 1))) THEN @3 "
          + "ELSE (CHARINDEX(@4, SUBSTRING([t0].[FirstName], 1, (@5 + @6)), (@7 + 1)) - 1) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", "test"),
          new CommandParameter ("@2", 2),
          new CommandParameter ("@3", 2),
          new CommandParameter ("@4", "test"),
          new CommandParameter ("@5", 2),
          new CommandParameter ("@6", 5),
          new CommandParameter ("@7", 2)
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ('t', 2, 5),
          "SELECT CASE WHEN (((LEN((@1 + '#')) - 1) = 0) AND ((@2 + 1) <= (LEN(([t0].[FirstName] + '#')) - 1))) THEN @3 "
          + "ELSE (CHARINDEX(@4, SUBSTRING([t0].[FirstName], 1, (@5 + @6)), (@7 + 1)) - 1) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 't'),
          new CommandParameter ("@2", 2),
          new CommandParameter ("@3", 2),
          new CommandParameter ("@4", 't'),
          new CommandParameter ("@5", 2),
          new CommandParameter ("@6", 5),
          new CommandParameter ("@7", 2)
          );
    }

    [Test]
    public void Like ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.SqlLike ("%ab%c") select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1 ESCAPE '\'",
          new CommandParameter ("@1", "%ab%c")
          );

      CheckQuery (
          from c in Cooks where c.FirstName.SqlLike (c.Name) select c.ID,
          @"SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE [t0].[Name] ESCAPE '\'");
    }

    [Test]
    public void ContainsFulltext ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.SqlContainsFulltext ("%ab%c") select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE CONTAINS([t0].[FirstName], @1)",
          new CommandParameter ("@1", "%ab%c")
          );

      CheckQuery (
          from c in Cooks where c.FirstName.SqlContainsFulltext ("%ab%c", "de") select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE CONTAINS([t0].[FirstName], @1, LANGUAGE @2)",
          new CommandParameter ("@1", "%ab%c"),
          new CommandParameter ("@2", "de")
          );

      CheckQuery (
          from c in Cooks where c.FirstName.SqlContainsFulltext (c.Name) select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE CONTAINS([t0].[FirstName], [t0].[Name])");
    }

    [Test]
    public void ContainsFreetext ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.SqlContainsFreetext ("%ab%c") select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE FREETEXT([t0].[FirstName], @1)",
          new CommandParameter ("@1", "%ab%c")
          );

      CheckQuery (
          from c in Cooks where c.FirstName.SqlContainsFreetext ("%ab%c", "de") select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE FREETEXT([t0].[FirstName], @1, LANGUAGE @2)",
          new CommandParameter ("@1", "%ab%c"),
          new CommandParameter ("@2", "de")
          );

      CheckQuery (
          from c in Cooks where c.FirstName.SqlContainsFreetext (c.Name) select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] WHERE FREETEXT([t0].[FirstName], [t0].[Name])");
    }

    [Test]
    public void Trim ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName.Trim(),
          "SELECT LTRIM(RTRIM([t0].[FirstName])) AS [value] FROM [CookTable] AS [t0]"
          );
    }

    [Test]
    public void Insert ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName.Insert (3, "Test"),
          "SELECT CASE WHEN ((LEN(([t0].[FirstName] + '#')) - 1) = 4) "
          + "THEN ([t0].[FirstName] + @1) "
          + "ELSE STUFF([t0].[FirstName], 4, 0, @2) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter("@1", "Test"),
          new CommandParameter("@2", "Test"));
    }

    [Test]
    public void Equals ()
    {
      CheckQuery (from c in Cooks where c.Name.Equals ("abc") select c.Name, 
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = @1)",
        new CommandParameter("@1", "abc"));

      CheckQuery (from k in Kitchens where k.Equals (k.Restaurant.SubKitchen) select k.Name,
        "SELECT [t0].[Name] AS [value] FROM [KitchenTable] AS [t0] LEFT OUTER JOIN [RestaurantTable] AS [t1] ON [t0].[RestaurantID] = [t1].[ID] "
        + "LEFT OUTER JOIN [KitchenTable] AS [t2] ON [t1].[ID] = [t2].[RestaurantID] "
        + "WHERE ([t0].[ID] = [t2].[ID])");

      CheckQuery (from c in Cooks where Equals(c.Name, "abc") select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = @1)",
        new CommandParameter ("@1", "abc"));

      CheckQuery (from c in Cooks where Equals (c, c.Substitution) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[SubstitutedID] "+
        "WHERE ([t0].[ID] = [t1].[ID])");

      CheckQuery (from c in Cooks where Equals (c, null) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] IS NULL)");

      CheckQuery (from c in Cooks where c.ID.Equals (10) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", 10));
    }

    [Test]
    [Ignore ("TODO 3316")]
    public void Equals_WithNonMatchingTypes ()
    {
      CheckQuery (from c in Cooks where c.ID.Equals ((int?) 10) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", (int?) 10));

      CheckQuery (from c in Cooks where c.ID.Equals ("10") select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", "10"));

      CheckQuery (from c in Cooks where Equals (c.ID, "10") select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", "10"));

      CheckQuery (from c in Cooks where Equals (c.ID, (int?)10) select c.Name,
        "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = @1)",
        new CommandParameter ("@1", (int?) 10));
    }

    [Test]
    public void AttributeBasedTransformer_OnMethod ()
    {
      CheckQuery (
          from c in Cooks select c.GetFullName(),
          "SELECT (([t0].[FirstName] + ' ') + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void AttributeBasedTransformer_OnProperty ()
    {
      CheckQuery (
          from c in Cooks select c.WeightInLbs,
          "SELECT ([t0].[Weight] * 2.20462262) AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void AttributeBasedTransformer_WithSubQuery ()
    {
      CheckQuery (
          from c in Cooks select c.GetAssistantCount(),
          "SELECT (SELECT COUNT(*) AS [value] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID])) AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void AttributeBasedTransformer_OverridesName ()
    {
      CheckQuery (
          from c in Cooks where c.Equals (c.Substitution) select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[SubstitutedID] "
          + "WHERE ((([t0].[FirstName] + ' ') + [t0].[Name]) = (([t1].[FirstName] + ' ') + [t1].[Name]))");
    }
  }
}