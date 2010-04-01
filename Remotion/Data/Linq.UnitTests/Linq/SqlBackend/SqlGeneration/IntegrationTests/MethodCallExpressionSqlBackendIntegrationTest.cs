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
using Remotion.Data.Linq.SqlBackend;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class MethodCallExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Contains ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.Contains ("abc") select c.ID,
          "SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1",
          new CommandParameter ("@1", "'%abc%'")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.Contains ("a%b_c[a] [^]") select c.ID,
          "SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1",
          new CommandParameter ("@1", "'%a[%]b[_]c[[]a] [[]^]%'")
          );
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Only expressions that can be evaluated locally can be used as the argument for Contains.")]
    public void Contains_Unevaluatable ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.Contains (c.Name) select c.ID,
          ""
          );
    }

    [Test]
    public void StartsWith ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.StartsWith ("abc") select c.ID,
          "SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1",
          new CommandParameter ("@1", "'abc%'")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.StartsWith ("a%b_c[a] [^]") select c.ID,
          "SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1",
          new CommandParameter ("@1", "'a[%]b[_]c[[]a] [[]^]%'")
          );
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Only expressions that can be evaluated locally can be used as the argument for StartsWith.")]
    public void StartsWith_Unevaluatable ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.StartsWith (c.Name) select c.ID,
          ""
          );
    }

    [Test]
    public void EndsWith ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.EndsWith ("abc") select c.ID,
          "SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1",
          new CommandParameter ("@1", "'%abc'")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.EndsWith ("a%b_c[a] [^]") select c.ID,
          "SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1",
          new CommandParameter ("@1", "'%a[%]b[_]c[[]a] [[]^]'")
          );
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Only expressions that can be evaluated locally can be used as the argument for EndsWith.")]
    public void EndsWith_Unevaluatable ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.EndsWith (c.Name) select c.ID,
          ""
          );
    }

    [Test]
    [Ignore ("TODO 2490")]
    public void Convert ()
    {
      // Test convert for different type combinations
    }

    [Test]
    public void ToLower_ToUpper ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName.ToLower(),
          "SELECT LOWER([t0].[FirstName]) FROM [CookTable] AS [t0]"
          );
      CheckQuery (
          from c in Cooks select c.FirstName.ToUpper(),
          "SELECT UPPER([t0].[FirstName]) FROM [CookTable] AS [t0]"
          );
    }

    [Test]
    public void Remove ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName.Remove (3),
          "SELECT STUFF([t0].[FirstName], (@1 + 1), LEN([t0].[FirstName]), '') FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 3)
          );
      CheckQuery (
          from c in Cooks select c.FirstName.Remove (3, 5),
          "SELECT STUFF([t0].[FirstName], (@1 + 1), @2, '') FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 3),
          new CommandParameter ("@2", 5)
          );
    }

    [Test]
    public void Substring ()
    {
      CheckQuery (
          from c in Cooks select c.FirstName.Substring (3),
          "SELECT SUBSTRING([t0].[FirstName], (@1 + 1), LEN([t0].[FirstName])) FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 3)
          );
      CheckQuery (
          from c in Cooks select c.FirstName.Substring(3, 5),
          "SELECT SUBSTRING([t0].[FirstName], (@1 + 1), @2) FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 3),
          new CommandParameter ("@2", 5)
          );
    }

    //TODO extension methods

    [Test]
    [Ignore ("TODO 2490")]
    public void Like ()
    {
      CheckQuery (
          from c in Cooks where c.FirstName.Like ("%ab%c") select c.ID,
          "SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE @1",
          new CommandParameter ("@1", "%a%bc")
          );
      CheckQuery (
          from c in Cooks where c.FirstName.Like (c.Name) select c.ID,
          "SELECT [t0].[ID] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] LIKE [t0].[Name]",
          new CommandParameter ("@1", "%a%bc")
          );
    }

    [Test]
    [Ignore ("TODO 2490")]
    public void ContainsFulltext ()
    {
    }

    [Test]
    [Ignore ("TODO 2490")]
    public void ContainsFreetext ()
    {
    }

    //TODO IndexOf integration tests
  }
}