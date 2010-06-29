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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratingExpressionVisitorTest
  {
    private SqlCommandBuilder _commandBuilder;
    private Expression _leftIntegerExpression;
    private Expression _rightIntegerExpression;
    private ISqlGenerationStage _stageMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<ISqlGenerationStage>();
      _commandBuilder = new SqlCommandBuilder();
      _leftIntegerExpression = Expression.Constant (1);
      Expression.Constant ("Left");
      _rightIntegerExpression = Expression.Constant (2);
      Expression.Constant ("Right");
    }

    [Test]
    public void GenerateSql_VisitSqlColumnExpression ()
    {
      SqlColumnExpression sqlColumnExpression = new SqlColumnDefinitionExpression (typeof (int), "s", "ID", false);
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnExpressionWithStar ()
    {
      var sqlColumnExpression = new SqlColumnDefinitionExpression (typeof (Cook), "c", "*", false);
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[c].*"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnDefinitionExpression ()
    {
      var sqlColumnExpression = new SqlColumnDefinitionExpression (typeof (int), "s", "ID", false);
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnReferenceExpression_WithNamedEntity ()
    {
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", "Test", new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true));
      var sqlColumnExpression = new SqlColumnReferenceExpression (typeof (int), "s", "ID", false, entityExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[Test_ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnReferenceExpression_WithNamedEntity_WithStarColumn ()
    {
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook),
          "c",
          "Test",
          new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true),
          new SqlColumnDefinitionExpression (typeof (Cook), "c", "*", false));
      var sqlColumnExpression = new SqlColumnReferenceExpression (typeof (int), "s", "ID", false, entityExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnReferenceExpression_WithUnnamedEntity ()
    {
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true));
      var sqlColumnExpression = new SqlColumnReferenceExpression (typeof (int), "s", "ID", false, entityExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_EntityDefinition ()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "t", "ID", true);
      var sqlColumnListExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          null,
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnListExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_EntityReference_SelectExpressionMode ()
    {
      var referencedEntity = new SqlEntityDefinitionExpression (
          typeof (Cook),
          "c",
          "Cook",
          new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false),
          new[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      var entityExpression = new SqlEntityReferenceExpression (typeof (Cook), "c", null, referencedEntity);

      SqlGeneratingExpressionVisitor.GenerateSql (entityExpression, _commandBuilder, _stageMock, SqlGenerationMode.SelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[c].[Cook_Name] AS [Name],[c].[Cook_City] AS [City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_EntityReference_NonSelectExpressionMode ()
    {
      var referencedEntity = new SqlEntityDefinitionExpression (
          typeof (Cook),
          "c",
          "Cook",
          new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false),
          new[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      var entityExpression = new SqlEntityReferenceExpression (typeof (Cook), "c", null, referencedEntity);

      SqlGeneratingExpressionVisitor.GenerateSql (entityExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[c].[Cook_Name],[c].[Cook_City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_NamedEntity_SelectExpressionMode ()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "t", "ID", true);
      var sqlColumnListExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          "Test",
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnListExpression, _commandBuilder, _stageMock, SqlGenerationMode.SelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID] AS [Test_ID],[t].[Name] AS [Test_Name],[t].[City] AS [Test_City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_NamedEntity_SelectExpressionMode_StarColumn ()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "t", "ID", true);
      var sqlColumnListExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          "Test",
          primaryKeyColumn,
          new[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "*", false)
          });
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnListExpression, _commandBuilder, _stageMock, SqlGenerationMode.SelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].*"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_NamedEntity_NonSelectExpressionMode ()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "t", "ID", true);
      var sqlColumnListExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          "Test",
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnListExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void GenerateSql_BoolExpression_ValueSemantics ()
    {
      var boolExpression = Expression.Equal (Expression.Constant ("hugo"), Expression.Constant ("sepp"));
      SqlGeneratingExpressionVisitor.GenerateSql (
          boolExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 = @2)"));
    }

    [Test]
    public void GenerateSql_BoolExpression_PredicateSemantics ()
    {
      var boolExpression = Expression.Equal (Expression.Constant ("hugo"), Expression.Constant ("sepp"));
      SqlGeneratingExpressionVisitor.GenerateSql (
          boolExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 = @2)"));
    }

    [Test]
    public void GenerateSql_VistNewExpression ()
    {
      var expression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }),
          new[] { Expression.Constant (0) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1"));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo (0));
    }

    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The expression 'CustomExpression' cannot be translated to SQL text by this SQL generator. Expression type 'CustomExpression' is not supported."
        )]
    [Test]
    public void GenerateSql_UnsupportedExpression ()
    {
      var unknownExpression = new CustomExpression (typeof (int));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unknownExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);
    }

    [Test]
    public void VisitConstantExpression ()
    {
      var expression = Expression.Constant (1);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitConstantExpression_NullValue ()
    {
      var expression = Expression.Constant (null);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (0));
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("NULL"));
    }

    [Test]
    public void VisitConstantExpression_Collection ()
    {
      var collectionExpression = Expression.Constant (new[] { "Hugo", "Maier", "Markart" });
      var sqlInExpression = new SqlBinaryOperatorExpression (typeof(bool), "IN", Expression.Constant ("Hubert"), collectionExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (sqlInExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      var expectedParameters = new[]
                               {
                                   new CommandParameter ("@1", "Hubert"),
                                   new CommandParameter ("@2", "Hugo"),
                                   new CommandParameter ("@3", "Maier"),
                                   new CommandParameter ("@4", "Markart")
                               };
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1 IN (@2, @3, @4)"));
      Assert.That (_commandBuilder.GetCommandParameters(), Is.EqualTo (expectedParameters));
    }

    [Test]
    public void VisitConstantExpression_EmptyCollection ()
    {
      var collectionExpression = Expression.Constant (new string[] { });
      var sqlInExpression = new SqlBinaryOperatorExpression (typeof(bool), "IN", Expression.Constant ("Hubert"), collectionExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (sqlInExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1 IN (SELECT NULL WHERE 1 = 0)"));
    }

    [Test]
    public void VisitLiteralExpression ()
    {
      var expression = new SqlLiteralExpression (5);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("5"));
      Assert.That (_commandBuilder.GetCommandParameters(), Is.Empty);
    }

    [Test]
    public void VisitConstantExpression_StringParameter ()
    {
      var expression = Expression.Constant ("Test");
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo ("Test"));
    }

    [Test]
    public void VisitBinaryExpression ()
    {
      Expression binaryExpression = Expression.Add (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (
          binaryExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("(@1 + @2)"));
    }

    [Test]
    public void VisitExistsExpression ()
    {
      var expression = new SqlExistsExpression (Expression.Constant ("test"));

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("EXISTS(@1)"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNot ()
    {
      var unaryNotExpression = Expression.Not (Expression.Equal (Expression.Constant ("hugo"), Expression.Constant ("hugo")));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("NOT (@1 = @2)"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNot_WithBitwiseNot ()
    {
      var unaryNotExpression = Expression.Not (Expression.Constant (1));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("~@1"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNegate ()
    {
      var unaryNotExpression = Expression.Negate (Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("-@1"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryPlus ()
    {
      var unaryNotExpression = Expression.UnaryPlus (Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("+@1"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitUnaryExpression_NotSupported ()
    {
      var unaryExpression = Expression.TypeAs (Expression.Constant ("1"), typeof (string));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);
    }

    [Test]
    public void VistSqlCaseExpression ()
    {
      var caseExpression = new SqlCaseExpression (
          Expression.Equal (Expression.Constant (2), Expression.Constant (2)),
          Expression.Constant (0),
          Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          caseExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("CASE WHEN (@1 = @2) THEN @3 ELSE @4 END"));
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var subStatementExpression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (
              mock =>
              mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("SELECT [t].[Name] FROM [Table] AS [t]"));
      _stageMock.Replay();

      SqlGeneratingExpressionVisitor.GenerateSql (subStatementExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(SELECT [t].[Name] FROM [Table] AS [t])"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitJoinConditionExpression ()
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var primaryColumn = new SqlColumnDefinitionExpression (typeof (Cook), "c", "ID", false);
      var foreignColumn = new SqlColumnDefinitionExpression (typeof (Cook), "a", "FK", false);
      var joinInfo = new ResolvedJoinInfo (resolvedTableInfo, primaryColumn, foreignColumn);
      var sqlTable = new SqlJoinedTable (joinInfo, JoinSemantics.Left);
      var joinConditionExpression = new JoinConditionExpression (sqlTable);

      SqlGeneratingExpressionVisitor.GenerateSql (
          joinConditionExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("([c].[ID] = [a].[FK])"));
    }

    [Test]
    public void VisitSqlBinaryOperatorExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();
      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var sqlInExpression = new SqlBinaryOperatorExpression (typeof(bool), "IN", Expression.Constant (1), sqlSubStatementExpression);

      _stageMock
          .Expect (
              mock =>
              mock.GenerateTextForSqlStatement (
                  Arg.Is (_commandBuilder), Arg<SqlStatement>.Is.Anything))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("test"));
      _stageMock.Replay();

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlInExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1 IN (test)"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlIsNullExpression ()
    {
      var expression = Expression.Constant ("test");
      var sqlIsNullExpression = new SqlIsNullExpression (expression);

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlIsNullExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 IS NULL)"));
    }

    [Test]
    public void VisitSqlLiteralExpression_Int ()
    {
      var expression = new SqlLiteralExpression (1);
      SqlGeneratingExpressionVisitor.GenerateSql (
          expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("1"));
    }

    [Test]
    public void VisitSqlLiteralExpression_String ()
    {
      var expression = new SqlLiteralExpression ("1");
      SqlGeneratingExpressionVisitor.GenerateSql (
          expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("'1'"));
    }

    [Test]
    public void VisitSqlLiteralExpression_Empty ()
    {
      var expression = new SqlLiteralExpression ("");
      SqlGeneratingExpressionVisitor.GenerateSql (
          expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("''"));
    }

    [Test]
    public void VisitSqlIsNotNullExpression ()
    {
      var expression = Expression.Constant ("test");
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (expression);

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlIsNotNullExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 IS NOT NULL)"));
    }

    [Test]
    public void VisitSqlFunctionExpression ()
    {
      var sqlFunctionExpression = new SqlFunctionExpression (typeof (int), "LENFUNC", new SqlLiteralExpression ("test"), new SqlLiteralExpression (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlFunctionExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("LENFUNC('test', 1)"));
    }

    [Test]
    public void VisitSqlConvertExpression ()
    {
      var sqlConvertExpression = new SqlConvertExpression (typeof (string), Expression.Constant ("1"));

      SqlGeneratingExpressionVisitor.GenerateSql (sqlConvertExpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("CONVERT(NVARCHAR, @1)"));
    }

    [Test]
    public void NumberExpression ()
    {
      var ordering1 = new Ordering (Expression.Constant ("order1"), OrderingDirection.Asc);
      var ordering2 = new Ordering (Expression.Constant ("order2"), OrderingDirection.Desc);
      var sqlRowNumberRÉxpression =
          new SqlRowNumberExpression (
              new[]
              {
                  ordering1,
                  ordering2
              });

      _stageMock
          .Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, ordering1))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("order1 ASC"));
      _stageMock
          .Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, ordering2))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("order2 DESC"));
      _stageMock.Replay();

      SqlGeneratingExpressionVisitor.GenerateSql (sqlRowNumberRÉxpression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("ROW_NUMBER() OVER (ORDER BY order1 ASC, order2 DESC)"));
    }

    [Test]
    public void VisitSqlCustomTextGeneratorExpression ()
    {
      var expression = new TestableSqlCustomTextGeneratorExpression (typeof (string));

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("TestableSqlCustomTextGeneratorExpression"));
    }

    [Test]
    public void VisitNamedExpression_SelectExpression_NameIsNotNull ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var expression = new NamedExpression ("test", columnExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.SelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[c].[Name] AS [test]"));
    }

    [Test]
    public void VisitNamedExpression_SelectExpressionMode_NameIsNull ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var expression = new NamedExpression (null, columnExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.SelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[c].[Name] AS [value]"));
    }

    [Test]
    public void VisitNamedExpression_NonSelectExpressionMode ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var expression = new NamedExpression (null, columnExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[c].[Name]"));
    }

    [Test]
    public void VisitAggregationExpression_Max ()
    {
      var columnExpression = new NamedExpression (null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new AggregationExpression (typeof (int), columnExpression, AggregationModifier.Max);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("MAX([c].[Name])"));
    }

    [Test]
    public void VisitAggregationExpression_Min ()
    {
      var columnExpression = new NamedExpression (null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new AggregationExpression (typeof (int), columnExpression, AggregationModifier.Min);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("MIN([c].[Name])"));
    }

    [Test]
    public void VisitAggregationExpression_Sum ()
    {
      var columnExpression = new NamedExpression (null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new AggregationExpression (typeof (int), columnExpression, AggregationModifier.Sum);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SUM([c].[Name])"));
    }

    [Test]
    public void VisitAggregationExpression_Average ()
    {
      var columnExpression = new NamedExpression (null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new AggregationExpression (typeof (double), columnExpression, AggregationModifier.Average);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("AVG([c].[Name])"));
    }

    [Test]
    public void VisitAggregationExpression_Count ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var expression = new AggregationExpression (typeof (int), columnExpression, AggregationModifier.Count);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock, SqlGenerationMode.NonSelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("COUNT(*)"));
    }

    [Test]
    public void VisitSqlGroupingSelectExpression_WithoutAggregationExpressions ()
    {
      var groupingExpression = new SqlGroupingSelectExpression (Expression.Constant ("keyExpression"), Expression.Constant ("elementExpression"));

      SqlGeneratingExpressionVisitor.GenerateSql (groupingExpression, _commandBuilder, _stageMock, SqlGenerationMode.SelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1"));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo ("keyExpression"));
    }

    [Test]
    public void VisitSqlGroupingSelectExpression_WithAggregationExpressions_AndNames ()
    {
      var groupingExpression = SqlGroupingSelectExpression.CreateWithNames (Expression.Constant ("keyExpression"), Expression.Constant ("elementExpression"));
      groupingExpression.AddAggregationExpressionWithName (Expression.Constant ("aggregation1"));
      groupingExpression.AddAggregationExpressionWithName (Expression.Constant ("aggregation2"));

      SqlGeneratingExpressionVisitor.GenerateSql (groupingExpression, _commandBuilder, _stageMock, SqlGenerationMode.SelectExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1 AS [key], @2 AS [a0], @3 AS [a1]"));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo ("keyExpression"));
      Assert.That (_commandBuilder.GetCommandParameters()[1].Value, Is.EqualTo ("aggregation1"));
      Assert.That (_commandBuilder.GetCommandParameters()[2].Value, Is.EqualTo ("aggregation2"));
    }
  }
}