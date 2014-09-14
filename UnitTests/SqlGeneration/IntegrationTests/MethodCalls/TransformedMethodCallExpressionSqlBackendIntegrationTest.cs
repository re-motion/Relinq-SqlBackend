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
  public class TransformedMethodCallExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void AttributeBasedTransformer_OnMethod ()
    {
      CheckQuery (
          from c in Cooks select c.GetFullName (),
          "SELECT (([t0].[FirstName] + @1) + [t0].[Name]) AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", " "));
    }

    [Test]
    public void AttributeBasedTransformer_OnProperty ()
    {
      CheckQuery (
          from c in Cooks select c.WeightInLbs,
          "SELECT ([t0].[Weight] * @1) AS [value] FROM [CookTable] AS [t0]",
          new CommandParameter ("@1", 2.20462262));
    }

    [Test]
    public void AttributeBasedTransformer_WithSubQuery ()
    {
      CheckQuery (
          from c in Cooks select c.GetAssistantCount(),
          "SELECT (SELECT COUNT(*) AS [value] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID])) AS [value] FROM [CookTable] AS [t0]");
    }

    [Test]
    public void AttributeBasedTransformer_WithSubQuery_OnIndexedProperty ()
    {
      CheckQuery (
          from c in Cooks select c[3].ID,
          "SELECT [q3].[value] AS [value] FROM [CookTable] AS [t1] "
          + "CROSS APPLY (SELECT TOP (1) [q0].[ID] AS [value] "
          + "FROM (SELECT TOP (3) [t2].[ID],[t2].[FirstName],[t2].[Name],[t2].[IsStarredCook],[t2].[IsFullTimeCook],[t2].[SubstitutedID],[t2].[KitchenID],"
          + "[t2].[KnifeID],[t2].[KnifeClassID] "
          + "FROM [CookTable] AS [t2] WHERE ([t1].[ID] = [t2].[AssistedID])) AS [q0]) AS [q3]");
    }

    [Test]
    public void AttributeBasedTransformer_OverridesName ()
    {
      CheckQuery (
          from c in Cooks where c.Equals (c.Substitution) select c.ID,
          "SELECT [t0].[ID] AS [value] FROM [CookTable] AS [t0] LEFT OUTER JOIN [CookTable] AS [t1] ON ([t0].[ID] = [t1].[SubstitutedID]) "
          + "WHERE ((([t0].[FirstName] + @1) + [t0].[Name]) = (([t1].[FirstName] + @2) + [t1].[Name]))",
          new CommandParameter ("@1", " "),
          new CommandParameter ("@2", " "));
    }
  }
}