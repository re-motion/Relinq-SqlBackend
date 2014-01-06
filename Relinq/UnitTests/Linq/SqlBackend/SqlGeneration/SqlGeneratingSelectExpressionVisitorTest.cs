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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratingSelectExpressionVisitorTest
  {
    private SqlCommandBuilder _commandBuilder;
    private ISqlGenerationStage _stageMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<ISqlGenerationStage> ();
      _commandBuilder = new SqlCommandBuilder ();
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "Queries selecting collections are not supported because SQL is not well-suited to returning collections.", 
        MatchType = MessageMatch.Contains)]
    public void GenerateSql_Collection ()
    {
      var expression = Expression.Constant (new Cook[] { });
      SqlGeneratingSelectExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);
    }

    [Test]
    public void GenerateSql_Collection_Grouping ()
    {
      var expression = SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression ();
      SqlGeneratingSelectExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("@1"));
    }

    [Test]
    public void GenerateTextForSelectExpression_CollectionInSelectProjection_StringsNotDetectedAsCollections ()
    {
      var expression = Expression.Constant ("test");
      SqlGeneratingSelectExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("@1"));
    }

    [Test]
    public void VisitNamedExpression_NameIsNull ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var expression = new NamedExpression (null, columnExpression);

      SqlGeneratingSelectExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[c].[Name] AS [value]"));
    }

    [Test]
    public void VisitNamedExpression_NameIsNotNull ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var expression = new NamedExpression ("test", columnExpression);

      SqlGeneratingSelectExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[c].[Name] AS [test]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_UnnamedEntity ()
    {
      var sqlColumnListExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          null,
          e => e,
          new[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "ID", true),
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      SqlGeneratingSelectExpressionVisitor.GenerateSql (
          sqlColumnListExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_NamedEntity ()
    {
      var sqlColumnListExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          "Test",
          e => e,
          new[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "ID", true),
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      SqlGeneratingSelectExpressionVisitor.GenerateSql (
          sqlColumnListExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[t].[ID] AS [Test_ID],[t].[Name] AS [Test_Name],[t].[City] AS [Test_City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_NamedEntity_StarColumn ()
    {
      var sqlColumnListExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          "Test",
          e => e.GetColumn (typeof (int), "ID", true),
          new[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "*", false)
          });
      SqlGeneratingSelectExpressionVisitor.GenerateSql (sqlColumnListExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[t].*"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_UnnamedEntity_ReferencingNamed ()
    {
      var referencedEntity = new SqlEntityDefinitionExpression (
          typeof (Cook),
          "c",
          "Cook",
          e => e,
          new[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      var entityExpression = new SqlEntityReferenceExpression (typeof (Cook), "c", null, referencedEntity);

      SqlGeneratingSelectExpressionVisitor.GenerateSql (entityExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[c].[Cook_Name] AS [Name],[c].[Cook_City] AS [City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_UnnamedEntity_ReferencingUnnamed ()
    {
      var referencedEntity = new SqlEntityDefinitionExpression (
          typeof (Cook),
          "c",
          null,
          e => e,
          new[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      var entityExpression = new SqlEntityReferenceExpression (typeof (Cook), "c", null, referencedEntity);

      SqlGeneratingSelectExpressionVisitor.GenerateSql (entityExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[c].[Name],[c].[City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_NamedEntity_ReferencingNamed ()
    {
      var referencedEntity = new SqlEntityDefinitionExpression (
          typeof (Cook),
          "c",
          "Cook",
          e => e,
          new[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      var entityExpression = new SqlEntityReferenceExpression (typeof (Cook), "c", "ref", referencedEntity);

      SqlGeneratingSelectExpressionVisitor.GenerateSql (entityExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[c].[Cook_Name] AS [ref_Name],[c].[Cook_City] AS [ref_City]"));
    }

    [Test]
    public void VisitSqlGroupingSelectExpression_WithoutAggregationExpressions ()
    {
      var groupingExpression = new SqlGroupingSelectExpression (Expression.Constant ("keyExpression"), Expression.Constant ("elementExpression"));

      SqlGeneratingSelectExpressionVisitor.GenerateSql (groupingExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("@1"));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo ("keyExpression"));
    }

    [Test]
    public void VisitSqlGroupingSelectExpression_WithAggregationExpressions_AndNames ()
    {
      var groupingExpression = SqlGroupingSelectExpression.CreateWithNames (Expression.Constant ("keyExpression"), Expression.Constant ("elementExpression"));
      groupingExpression.AddAggregationExpressionWithName (Expression.Constant ("aggregation1"));
      groupingExpression.AddAggregationExpressionWithName (Expression.Constant ("aggregation2"));

      SqlGeneratingSelectExpressionVisitor.GenerateSql (groupingExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("@1 AS [key], @2 AS [a0], @3 AS [a1]"));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo ("keyExpression"));
      Assert.That (_commandBuilder.GetCommandParameters ()[1].Value, Is.EqualTo ("aggregation1"));
      Assert.That (_commandBuilder.GetCommandParameters ()[2].Value, Is.EqualTo ("aggregation2"));
    }
  }
}