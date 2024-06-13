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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests.ResultOperators
{
  [TestFixture]
  public class ContainsResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Contains_WithQuery ()
    {
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Contains (s) select s.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CookTable] AS [t0] "
          + "WHERE [t0].[ID] "
          + "IN (SELECT [t1].[ID] FROM [CookTable] AS [t1])");
    }

    [Test]
    public void Contains_WithConstant ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Contains (cook) select s.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] FROM [CookTable] AS [t1])",
          new CommandParameter("@1", 23));
    }

    [Test]
    public void Contains_WithDerivedType ()
    {
      var chef = new Chef { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where s.Assistants.Contains (chef) select s.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID]))",
          new CommandParameter ("@1", 23));
    }

    [Test]
    public void Contains_WithConstantAndDependentQuery ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where s.Assistants.Contains (cook) select s.ID,
          "SELECT [t0].[ID] AS [value] "
          +"FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID]))",
          new CommandParameter("@1", 23));
    }

    [Test]
    public void Contains_OnTopLevel ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          ( () => (from s in Cooks select s).Contains(cook)),
          "SELECT CONVERT(BIT, CASE WHEN @1 IN (SELECT [t0].[ID] FROM [CookTable] AS [t0]) THEN 1 ELSE 0 END) AS [value]",
          row => (object) row.GetValue<bool> (new ColumnID ("value", 0)),
          new CommandParameter("@1", 23) );
    }

    [Test]
    public void Contains_WithCollection ()
    {
      var values = new[] { "hugo", "hans", "heinz" };
      var cookNamesStub = new Mock<IEnumerable<string>>(); 
      // ReSharper disable GenericEnumeratorNotDisposed
      cookNamesStub.As<ICollection>().Setup (_ => _.GetEnumerator()).Returns (values.GetEnumerator());
      cookNamesStub.Setup (_ => _.GetEnumerator()).Returns (((IEnumerable<string>)values).GetEnumerator());
      // ReSharper restore GenericEnumeratorNotDisposed

      var cookNames = cookNamesStub.Object;
      CheckQuery (
          from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (@1, @2, @3)",
          new CommandParameter ("@1", "hugo"),
          new CommandParameter ("@2", "hans"),
          new CommandParameter ("@3", "heinz"));
    }

    [Test]
    public void Contains_WithCollectionOfT ()
    {
      var values = new[] { "hugo", "hans", "heinz" };
      var cookNamesStub = new Mock<ICollection<string>>(); 
      // ReSharper disable GenericEnumeratorNotDisposed
      cookNamesStub.As<IEnumerable>().Setup (_ => _.GetEnumerator()).Returns (values.GetEnumerator());
      cookNamesStub.Setup (_ => _.GetEnumerator()).Returns (((IEnumerable<string>)values).GetEnumerator());
      // ReSharper restore GenericEnumeratorNotDisposed

      var cookNames = cookNamesStub.Object;
      CheckQuery (
          from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (@1, @2, @3)",
          new CommandParameter ("@1", "hugo"),
          new CommandParameter ("@2", "hans"),
          new CommandParameter ("@3", "heinz"));
    }

    [Test]
    public void Contains_WithReadOnlyCollectionOfT ()
    {
      var values = new[] { "hugo", "hans", "heinz" };
      var cookNamesStub = new Mock<IReadOnlyCollection<string>>(); 
      // ReSharper disable GenericEnumeratorNotDisposed
      cookNamesStub.As<IEnumerable>().Setup (_ => _.GetEnumerator()).Returns (values.GetEnumerator());
      cookNamesStub.Setup (_ => _.GetEnumerator()).Returns (((IEnumerable<string>)values).GetEnumerator());
      // ReSharper restore GenericEnumeratorNotDisposed

      var cookNames = cookNamesStub.Object;
      CheckQuery (
          from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (@1, @2, @3)",
          new CommandParameter ("@1", "hugo"),
          new CommandParameter ("@2", "hans"),
          new CommandParameter ("@3", "heinz"));
    }

    [Test]
    public void Contains_WithEmptyCollection ()
    {
      var cookNames = new string[] { };
      CheckQuery (
          from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (SELECT NULL WHERE 1 = 0)");
    }

    [Test]
    public void Contains_WithNonCollectionEnumerable ()
    {
      var cookNames = new[] { "hugo", "hans", "heinz" }.Where (s => s.StartsWith ("h"));
      var query = from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName;
      var queryModel = ExpressionHelper.ParseQuery (query);
      Assert.That (
          () => GenerateSql (queryModel),
          Throws.InstanceOf<NotSupportedException>().With.Message.EqualTo (
              "Only collection can be used as a parameter. "
              + "Use an object that implements the ICollection, ICollection<T>, or IReadOnlyCollection<T> interface "
              + "instead of a sequence based on IEnumerable."));
    }
  }
}