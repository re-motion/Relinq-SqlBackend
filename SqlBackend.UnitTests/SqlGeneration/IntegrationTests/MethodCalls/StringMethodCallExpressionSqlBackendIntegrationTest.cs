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

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests.MethodCalls
{
  [TestFixture]
  public class StringMethodCallExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
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
          "SELECT CONVERT(NVARCHAR(MAX), [t0].[ID]) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select string.Concat (c.ID, c.ID),
          "SELECT (CONVERT(NVARCHAR(MAX), [t0].[ID]) + CONVERT(NVARCHAR(MAX), [t0].[ID])) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select string.Concat (c.ID, c.ID, c.FirstName),
          "SELECT ((CONVERT(NVARCHAR(MAX), [t0].[ID]) + CONVERT(NVARCHAR(MAX), [t0].[ID])) + [t0].[FirstName]) AS [value] FROM [CookTable] AS [t0]"
          );

      // The overload with (object, object, object, object, __arglist) is not supported, but this call uses the params object[] overload anyway.
      CheckQuery (
          from c in Cooks select string.Concat (c.ID, c.ID, c.FirstName, c.Name),
          "SELECT (((CONVERT(NVARCHAR(MAX), [t0].[ID]) + CONVERT(NVARCHAR(MAX), [t0].[ID])) + [t0].[FirstName]) + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]"
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
          "SELECT (CONVERT(NVARCHAR(MAX), [t0].[ID]) + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]"
          );

      // IEnumerable<string> overload is not tested because it's not possible to call it using a constant or NewArray expression.
      // - string.Concat (new[] { c.FirstName, c.Name }) would call the string[] overload.
      // - string.Concat (constantEnumerable) would partially evaluate the whole expression.
      // - string.Concat ((IEnumerable<string>) new[] { c.FirstName, c.Name }) is not currently supported.

      // IEnumerable<T> overload
      CheckQuery (
          from c in Cooks select string.Concat (new[] { c.ID, c.ID }),
          "SELECT (CONVERT(NVARCHAR(MAX), [t0].[ID]) + CONVERT(NVARCHAR(MAX), [t0].[ID])) AS [value] FROM [CookTable] AS [t0]"
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
      // ReSharper disable StringIndexOfIsCultureSpecific.1
      // ReSharper disable StringIndexOfIsCultureSpecific.2
      // ReSharper disable StringIndexOfIsCultureSpecific.3
      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ("test"),
          "SELECT CASE WHEN ((LEN((@1 + '#')) - 1) = 0) THEN 0 ELSE (CHARINDEX(@1, [t0].[FirstName]) - 1) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", "test")
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ('t'),
          "SELECT CASE WHEN ((LEN((@1 + '#')) - 1) = 0) THEN 0 ELSE (CHARINDEX(@1, [t0].[FirstName]) - 1) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 't')
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ("test", 2),
          "SELECT CASE WHEN (((LEN((@1 + '#')) - 1) = 0) AND ((@2 + 1) <= (LEN(([t0].[FirstName] + '#')) - 1))) THEN @2 ELSE (CHARINDEX(@1, [t0].[FirstName], (@2 + 1)) - 1) END AS [value] "
          + "FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", "test"),
          new CommandParameter ("@2", 2)
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ('t', 2),
          "SELECT CASE WHEN (((LEN((@1 + '#')) - 1) = 0) AND ((@2 + 1) <= (LEN(([t0].[FirstName] + '#')) - 1))) THEN @2 ELSE (CHARINDEX(@1, [t0].[FirstName], (@2 + 1)) - 1) END AS [value] "
          + "FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 't'),
          new CommandParameter ("@2", 2)
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ("test", 2, 5),
          "SELECT CASE WHEN (((LEN((@1 + '#')) - 1) = 0) AND ((@2 + 1) <= (LEN(([t0].[FirstName] + '#')) - 1))) THEN @2 "
          + "ELSE (CHARINDEX(@1, SUBSTRING([t0].[FirstName], 1, (@2 + @3)), (@2 + 1)) - 1) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", "test"),
          new CommandParameter ("@2", 2),
          new CommandParameter ("@3", 5)
          );

      CheckQuery (
          from c in Cooks select c.FirstName.IndexOf ('t', 2, 5),
          "SELECT CASE WHEN (((LEN((@1 + '#')) - 1) = 0) AND ((@2 + 1) <= (LEN(([t0].[FirstName] + '#')) - 1))) THEN @2 "
          + "ELSE (CHARINDEX(@1, SUBSTRING([t0].[FirstName], 1, (@2 + @3)), (@2 + 1)) - 1) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 't'),
          new CommandParameter ("@2", 2),
          new CommandParameter ("@3", 5)
          );
      // ReSharper restore StringIndexOfIsCultureSpecific.3
      // ReSharper restore StringIndexOfIsCultureSpecific.2
      // ReSharper restore StringIndexOfIsCultureSpecific.1
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
          "SELECT CASE WHEN ((LEN(([t0].[FirstName] + '#')) - 1) = (@1 + 1)) "
          + "THEN ([t0].[FirstName] + @2) "
          + "ELSE STUFF([t0].[FirstName], (@1 + 1), 0, @2) END AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter("@1", 3),
          new CommandParameter ("@2", "Test"));
    }
  }
}