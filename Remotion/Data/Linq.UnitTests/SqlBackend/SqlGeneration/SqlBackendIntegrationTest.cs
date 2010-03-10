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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlBackendIntegrationTest
  {
    private IQueryable<Cook> _cooks;
    private IQueryable<Kitchen> _kitchens;

    [SetUp]
    public void SetUp ()
    {
      _cooks = ExpressionHelper.CreateCookQueryable();
      _kitchens = ExpressionHelper.CreateKitchenQueryable();
    }

    [Test]
    public void SimpleSqlQuery_SimpleEntitySelect ()
    {
      var queryable = from s in _cooks select s;
      var result = GenerateSql (queryable.Expression);

      const string expected =
          "SELECT [t0].[ID],[t0].[FirstName],[t0].[Name],[t0].[IsStarredCook],[t0].[IsFullTimeCook],[t0].[SubstitutedID],[t0].[KitchenID] "
          + "FROM [CookTable] AS [t0]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
    }

    [Test]
    public void SimpleSqlQuery_SimplePropertySelect ()
    {
      var queryable = from s in _cooks select s.FirstName;
      var result = GenerateSql (queryable.Expression);

      const string expected = "SELECT [t0].[FirstName] FROM [CookTable] AS [t0]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
    }

    [Test]
    public void SimpleSqlQuery_EntityPropertySelect ()
    {
      var queryable = from k in _kitchens select k.Cook;
      var result = GenerateSql (queryable.Expression);

      const string expected =
          "SELECT [t1].[ID],[t1].[FirstName],[t1].[Name],[t1].[IsStarredCook],[t1].[IsFullTimeCook],[t1].[SubstitutedID],[t1].[KitchenID] "
          + "FROM [KitchenTable] AS [t0] JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[KitchenID]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
    }

    [Test]
    public void SimpleSqlQuery_ChainedPropertySelect_EndingWithSimpleProperty ()
    {
      var queryable = from k in _kitchens select k.Cook.FirstName;
      var result = GenerateSql (queryable.Expression);

      const string expected = "SELECT [t1].[FirstName] FROM [KitchenTable] AS [t0] JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[KitchenID]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
    }

    [Test]
    public void SelectQuery_ChainedPropertySelect_WithSameType ()
    {
      var queryable = from c in _cooks select c.Substitution.FirstName;
      var result = GenerateSql (queryable.Expression);

      const string expected = "SELECT [t1].[FirstName] FROM [CookTable] AS [t0] JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[SubstitutedID]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
    }

    [Test]
    [Ignore ("TODO 2399")]
    public void SimpleSqlQuery_ChainedPropertySelect_EndingWithEntityProperty ()
    {
      var queryable = from k in _kitchens select k.Restaurant.SubKitchen.Cook;
      var result = GenerateSql (queryable.Expression);

      Console.WriteLine (result.CommandText);
      const string expected =
          "SELECT [t3].[ID],[t3].[FirstName],[t3].[Name],[t3].[IsStarredCook],[t3].[IsFullTimeCook],[t3].[SubstitutedID],[t3].[KitchenID] "
          + "FROM [KitchenTable] AS [t0] JOIN [RestaurantTable] AS [t1] ON [t0].[RestaurantID] = [t1].[ID]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
    }

    [Test]
    [Ignore ("TODO 2407")]
    public void SimpleSqlQuery_ChainedPropertySelectAndWhere_SamePathTwice ()
    {
      var queryable = from k in _kitchens where k.Restaurant.SubKitchen.Cook != null select k.Restaurant.SubKitchen.Cook;
      var result = GenerateSql (queryable.Expression);

      Console.WriteLine (result.CommandText);
      const string expected =
          "SELECT [t3].[ID],[t3].[FirstName],[t3].[Name],[t3].[IsStarredCook],[t3].[IsFullTimeCook],[t3].[SubstitutedID],[t3].[KitchenID] "
          + "FROM [KitchenTable] AS [t0] JOIN [RestaurantTable] AS [t1] ON [t0].[RestaurantID] = [t1].[ID] " 
          + "WHERE ([t3].[ID],[t3].[FirstName],[t3].[Name],[t3].[IsStarredCook],[t3].[IsFullTimeCook],[t3].[SubstitutedID],[t3].[KitchenID] IS NOT NULL)";
      Assert.That (result.CommandText, Is.EqualTo (expected));
    }

    [Test]
    [Ignore ("TODO 2399")]
    public void SimpleSqlQuery_ChainedPropertySelectAndWhere_PartialPathTwice ()
    {
      var queryable = from k in _kitchens where k.Restaurant.SubKitchen.Restaurant != null select k.Restaurant.SubKitchen.Cook;
      var result = GenerateSql (queryable.Expression);

      Console.WriteLine (result.CommandText);
      const string expected = "?";
      Assert.That (result.CommandText, Is.EqualTo (expected));
    }

    [Test]
    public void SimpleSqlQuery_ConstantSelect ()
    {
      var queryable = from k in _kitchens select "hugo";
      var result = GenerateSql (queryable.Expression);

      const string expected = "SELECT @1 FROM [KitchenTable] AS [t0]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", "hugo") }));
    }

    [Test]
    public void SimpleSqlQuery_NullSelect ()
    {
      var queryable = _kitchens.Select<Kitchen, object> (k => null);
      var result = GenerateSql (queryable.Expression);

      const string expected = "SELECT NULL FROM [KitchenTable] AS [t0]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
      Assert.That (result.Parameters, Is.Empty);
    }

    [Test]
    public void SimpleSqlQuery_TrueSelect ()
    {
      var queryable = from k in _kitchens select true;
      var result = GenerateSql (queryable.Expression);

      const string expected = "SELECT @1 FROM [KitchenTable] AS [t0]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", 1) }));
    }

    [Test]
    public void SimpleSqlQuery_FalseSelect ()
    {
      var queryable = from k in _kitchens select false;
      var result = GenerateSql (queryable.Expression);

      const string expected = "SELECT @1 FROM [KitchenTable] AS [t0]";
      Assert.That (result.CommandText, Is.EqualTo (expected));
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", 0) }));
    }

    [Test]
    public void SelectQuery_WithWhereCondition ()
    {
      var queryable = from c in _cooks where c.Name == "Huber" select c.FirstName;
      var result = GenerateSql (queryable.Expression);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = @1)"));
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", "Huber") }));
    }

    //result operators
    //(from c in _cooks select c).Count()
    //(from c in _cooks select c).Distinct()
    //(from c in _cooks select c).Take(5)
    //(from c in _cooks select c).Take(c.ID)
    //from k in _kitchen from c in k.Restaurant.Cooks.Take(k.RoomNumber) select c
    //(from c in _cooks select c).Single()
    //(from c in _cooks select c).First()

    //where conditions
    //from c in _cooks where c.Name = "Huber" select c.FirstName
    //from c in _cooks where c.Name = "Huber" && c.FirstName = "Sepp" select c;
    //(from c in _cooks where c.IsFullTimeCook select c)
    //(from c in _cooks where true select c)
    //(from c in _cooks where false select c)

    //binary expression
    //(from c in _cooks where c.Name == null select c)
    //(from c in _cooks where c.ID + c.ID select c)
    // see SqlGeneratingExpressionVisitor.VisitBinaryExpressions for further tests
    //(from c in _cooks where c.IsFullTimeCook == true select c)
    //(from c in _cooks where c.IsFullTimeCook == false select c)

    //unary expressions (unary plus, unary negate, unary not)
    //(from c in _cooks where (-c.ID) == -1 select c)
    //(from c in _cooks where !c.IsStarredCook == true select c)
    //(from c in _cooks where (+c.ID) == -1 select c)

    //method calls (review method)
    //SqlStatementTextGenerator.GenerateSqlGeneratorRegistry

    private SqlCommand GenerateSql (Expression expression)
    {
      var queryModel = ExpressionHelper.ParseQuery (expression);

      var sqlStatement = SqlPreparationQueryModelVisitor.TransformQueryModel (queryModel, new SqlPreparationContext());

      ResolvingSqlStatementVisitor.ResolveExpressions (sqlStatement, new SqlStatementResolverStub(), new UniqueIdentifierGenerator());

      var sqlTextGenerator = new SqlStatementTextGenerator();
      return sqlTextGenerator.Build (sqlStatement);
    }
  }
}