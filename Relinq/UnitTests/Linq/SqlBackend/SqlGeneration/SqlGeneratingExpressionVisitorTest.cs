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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.Clauses;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
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
      _rightIntegerExpression = Expression.Constant (2);
    }

    [Test]
    public void GenerateSql_VisitSqlColumnExpression ()
    {
      SqlColumnExpression sqlColumnExpression = new SqlColumnDefinitionExpression (typeof (int), "s", "ID", false);
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnExpressionWithStar ()
    {
      var sqlColumnExpression = new SqlColumnDefinitionExpression (typeof (Cook), "c", "*", false);
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[c].*"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnDefinitionExpression ()
    {
      var sqlColumnExpression = new SqlColumnDefinitionExpression (typeof (int), "s", "ID", false);
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnReferenceExpression_WithNamedEntity ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), "Test");
      var sqlColumnExpression = new SqlColumnReferenceExpression (typeof (int), "s", "ID", false, entityExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[Test_ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnReferenceExpression_WithNamedEntity_WithStarColumn ()
    {
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook),
          "c",
          "Test",
          e => e.GetColumn (typeof (int), "ID", true),
          new SqlColumnDefinitionExpression (typeof (Cook), "c", "*", false));
      var sqlColumnExpression = new SqlColumnReferenceExpression (typeof (int), "s", "ID", false, entityExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnReferenceExpression_WithUnnamedEntity ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null);
      var sqlColumnExpression = new SqlColumnReferenceExpression (typeof (int), "s", "ID", false, entityExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_EntityDefinition ()
    {
      var sqlColumnListExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          null,
          e => e,
          new SqlColumnExpression[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "ID", true),
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnListExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_EntityReference ()
    {
      var referencedEntity = new SqlEntityDefinitionExpression (
          typeof (Cook),
          "c",
          "Cook",
          e => e,
          new SqlColumnExpression[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      var entityExpression = new SqlEntityReferenceExpression (typeof (Cook), "c", null, referencedEntity);

      SqlGeneratingExpressionVisitor.GenerateSql (entityExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[c].[Cook_Name],[c].[Cook_City]"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression_NamedEntity()
    {
      var sqlColumnListExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          "Test",
          e => e,
          new SqlColumnExpression[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "ID", true),
              new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (string), "t", "City", false)
          });
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnListExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void GenerateSql_BoolExpression_ValueSemantics ()
    {
      var boolExpression = Expression.Equal (Expression.Constant ("hugo"), Expression.Constant ("sepp"));
      SqlGeneratingExpressionVisitor.GenerateSql (
          boolExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 = @2)"));
    }

    [Test]
    public void GenerateSql_BoolExpression_PredicateSemantics ()
    {
      var boolExpression = Expression.Equal (Expression.Constant ("hugo"), Expression.Constant ("sepp"));
      SqlGeneratingExpressionVisitor.GenerateSql (
          boolExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 = @2)"));
    }

    [Test]
    public void GenerateSql_VistNewExpression ()
    {
      var expression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)),
          new[] { Expression.Constant (0) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

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
          unknownExpression, _commandBuilder, _stageMock);
    }

    [Test]
    public void VisitConstantExpression ()
    {
      var expression = Expression.Constant (1);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitConstantExpression_TwiceWithSameExpression ()
    {
      var expression = Expression.Constant (1);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandParameters ().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitConstantExpression_NullValue ()
    {
      var expression = Expression.Constant (null);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (0));
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("NULL"));
    }

    [Test]
    public void VisitConstantExpression_Collection ()
    {
      var collectionExpression = Expression.Constant (new[] { "Hugo", "Maier", "Markart" });
      var sqlInExpression = new SqlInExpression (Expression.Constant ("Hubert"), collectionExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (sqlInExpression, _commandBuilder, _stageMock);

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
      var sqlInExpression = new SqlInExpression (Expression.Constant ("Hubert"), collectionExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (sqlInExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1 IN (SELECT NULL WHERE 1 = 0)"));
    }
    
    [Test]
    public void VisitLiteralExpression ()
    {
      var expression = new SqlLiteralExpression (5);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("5"));
      Assert.That (_commandBuilder.GetCommandParameters(), Is.Empty);
    }

    [Test]
    public void VisitConstantExpression_StringParameter ()
    {
      var expression = Expression.Constant ("Test");
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo ("Test"));
    }

    [Test]
    public void VisitBinaryExpression ()
    {
      Expression binaryExpression = Expression.Add (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (
          binaryExpression, _commandBuilder, _stageMock);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("(@1 + @2)"));
    }

    [Test]
    public void VisitExistsExpression ()
    {
      var expression = new SqlExistsExpression (Expression.Constant ("test"));

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("EXISTS(@1)"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNot ()
    {
      var unaryNotExpression = Expression.Not (Expression.Equal (Expression.Constant ("hugo"), Expression.Constant ("hugo")));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("NOT (@1 = @2)"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNot_NullableBool ()
    {
      var unaryNotExpression = Expression.Not (Expression.Equal (Expression.Constant (4, typeof (int?)), Expression.Constant (5, typeof (int?)), true, null));
      Assert.That (unaryNotExpression.IsLiftedToNull, Is.True);
      Assert.That (unaryNotExpression.Type, Is.SameAs (typeof (bool?)));

      SqlGeneratingExpressionVisitor.GenerateSql (unaryNotExpression, _commandBuilder, _stageMock);
      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo ("NOT (@1 = @2)"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNot_WithBitwiseNot ()
    {
      var unaryNotExpression = Expression.Not (Expression.Constant (1));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("~@1"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNegate ()
    {
      var unaryNotExpression = Expression.Negate (Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("-@1"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryPlus ()
    {
      var unaryNotExpression = Expression.UnaryPlus (Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("+@1"));
    }

    [Test]
    public void VisitUnaryExpression_Convert ()
    {
      var unaryNotExpression = Expression.Convert (Expression.Constant (1), typeof (double));

      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock);
      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo ("@1"));
    }

    [Test]
    public void VisitUnaryExpression_ConvertChecked ()
    {
      var unaryNotExpression = Expression.ConvertChecked (Expression.Constant (1), typeof (double));

      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, _stageMock);
      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo ("@1"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Cannot generate SQL for unary expression '(\"1\" As String)'.")]
    public void VisitUnaryExpression_NotSupported ()
    {
      var unaryExpression = Expression.TypeAs (Expression.Constant ("1"), typeof (string));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryExpression, _commandBuilder, _stageMock);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "The method 'System.Linq.Queryable.Count' is not supported by this code generator, and no custom transformer has been registered. "
        + "Expression: 'TestQueryable<Cook>().Count()'")]
    public void VisitMethodCallExpression_NotSupported ()
    {
      var methodCallExpression = ExpressionHelper.CreateMethodCallExpression();
      SqlGeneratingExpressionVisitor.GenerateSql (methodCallExpression, _commandBuilder, _stageMock);
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

      SqlGeneratingExpressionVisitor.GenerateSql (subStatementExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(SELECT [t].[Name] FROM [Table] AS [t])"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlInExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();
      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var sqlInExpression = new SqlInExpression (Expression.Constant (1), sqlSubStatementExpression);

      _stageMock
          .Expect (
              mock =>
              mock.GenerateTextForSqlStatement (
                  Arg.Is (_commandBuilder), Arg<SqlStatement>.Is.Anything))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("test"));
      _stageMock.Replay();

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlInExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1 IN (test)"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlLikeExpression ()
    {
      var sqlInExpression = new SqlLikeExpression (new SqlLiteralExpression (1), new SqlLiteralExpression (2), new SqlLiteralExpression (@"\"));

      SqlGeneratingExpressionVisitor.GenerateSql (sqlInExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (@"1 LIKE 2 ESCAPE '\'"));
    }

    [Test]
    public void VisitSqlIsNullExpression ()
    {
      var expression = Expression.Constant ("test");
      var sqlIsNullExpression = new SqlIsNullExpression (expression);

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlIsNullExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 IS NULL)"));
    }

    [Test]
    public void VisitSqlLiteralExpression_Int ()
    {
      var expression = new SqlLiteralExpression (1000000000);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("1000000000"));
    }

    [Test]
    public void VisitSqlLiteralExpression_Double ()
    {
      var expression = new SqlLiteralExpression (1.1);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("1.1"));
    }

    [Test]
    [SetCulture ("de-DE")]
    public void VisitSqlLiteralExpression_Double_CultureAgnostic ()
    {
      var expression = new SqlLiteralExpression (1.1);
      Assert.That (expression.Value.ToString(), Is.Not.EqualTo ("1.1"));

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("1.1"));
    }

    [Test]
    public void VisitSqlLengthExpression_String ()
    {
      var innnerExpression = Expression.Constant ("test");
      var expression = new SqlLengthExpression (innnerExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(LEN((@1 + '#')) - 1)"));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo ("test"));
    }

    [Test]
    public void VisitSqlLengthExpression_Char ()
    {
      var innnerExpression = Expression.Constant ('t');
      var expression = new SqlLengthExpression (innnerExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(LEN((@1 + '#')) - 1)"));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo ('t'));
    }

    [Test]
    public void VisitSqlCaseExpression_NoElse ()
    {
      var case1 = new SqlCaseExpression.CaseWhenPair (new SqlCustomTextExpression ("test1", typeof (bool)), new SqlCustomTextExpression ("value1", typeof (int)));
      var case2 = new SqlCaseExpression.CaseWhenPair (new SqlCustomTextExpression ("test2", typeof (bool)), new SqlCustomTextExpression ("value2", typeof (int)));
      var expression = new SqlCaseExpression (typeof (int?), new[] { case1, case2 }, null);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("CASE WHEN test1 THEN value1 WHEN test2 THEN value2 END"));
    }

    [Test]
    public void VisitSqlCaseExpression_WithElse ()
    {
      var case1 = new SqlCaseExpression.CaseWhenPair (new SqlCustomTextExpression ("test1", typeof (bool)), new SqlCustomTextExpression ("value1", typeof (int)));
      var case2 = new SqlCaseExpression.CaseWhenPair (new SqlCustomTextExpression ("test2", typeof (bool)), new SqlCustomTextExpression ("value2", typeof (int)));
      var elseCase = new SqlCustomTextExpression ("elseValue", typeof (int));
      var expression = new SqlCaseExpression (typeof (int), new[] { case1, case2 }, elseCase);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("CASE WHEN test1 THEN value1 WHEN test2 THEN value2 ELSE elseValue END"));
    }

    [Test]
    public void VisitSqlLiteralExpression_String ()
    {
      var expression = new SqlLiteralExpression ("1");
      SqlGeneratingExpressionVisitor.GenerateSql (
          expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("'1'"));
    }

    [Test]
    public void VisitSqlLiteralExpression_Empty ()
    {
      var expression = new SqlLiteralExpression ("");
      SqlGeneratingExpressionVisitor.GenerateSql (
          expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("''"));
    }

    [Test]
    public void VisitSqlIsNotNullExpression ()
    {
      var expression = Expression.Constant ("test");
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (expression);

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlIsNotNullExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 IS NOT NULL)"));
    }

    [Test]
    public void VisitSqlFunctionExpression ()
    {
      var sqlFunctionExpression = new SqlFunctionExpression (typeof (int), "LENFUNC", new SqlLiteralExpression ("test"), new SqlLiteralExpression (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlFunctionExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("LENFUNC('test', 1)"));
    }

    [Test]
    public void VisitSqlConvertExpression ()
    {
      var sqlConvertExpression = new SqlConvertExpression (typeof (string), Expression.Constant ("1"));

      SqlGeneratingExpressionVisitor.GenerateSql (sqlConvertExpression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("CONVERT(NVARCHAR(MAX), @1)"));
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

      SqlGeneratingExpressionVisitor.GenerateSql (sqlRowNumberRÉxpression, _commandBuilder, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("ROW_NUMBER() OVER (ORDER BY order1 ASC, order2 DESC)"));
    }

    [Test]
    public void VisitSqlCustomTextGeneratorExpression ()
    {
      var expression = new TestableSqlCustomTextGeneratorExpression (typeof (string));

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("TestableSqlCustomTextGeneratorExpression"));
    }

    [Test]
    public void VisitNamedExpression ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var expression = new NamedExpression ("xx", columnExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[c].[Name]"));
    }

    [Test]
    public void VisitAggregationExpression_Max ()
    {
      var columnExpression = new NamedExpression (null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new AggregationExpression (typeof (int), columnExpression, AggregationModifier.Max);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("MAX([c].[Name])"));
    }

    [Test]
    public void VisitAggregationExpression_Min ()
    {
      var columnExpression = new NamedExpression (null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new AggregationExpression (typeof (int), columnExpression, AggregationModifier.Min);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("MIN([c].[Name])"));
    }

    [Test]
    public void VisitAggregationExpression_Sum ()
    {
      var columnExpression = new NamedExpression (null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new AggregationExpression (typeof (int), columnExpression, AggregationModifier.Sum);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SUM([c].[Name])"));
    }

    [Test]
    public void VisitAggregationExpression_Average ()
    {
      var columnExpression = new NamedExpression (null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new AggregationExpression (typeof (double), columnExpression, AggregationModifier.Average);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("AVG([c].[Name])"));
    }

    [Test]
    public void VisitAggregationExpression_Count ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var expression = new AggregationExpression (typeof (int), columnExpression, AggregationModifier.Count);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("COUNT(*)"));
    }

    [Test]
    public void VisitSqlConvertedBooleanExpression ()
    {
      var expression = new SqlConvertedBooleanExpression (Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("@1"));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitSqlEntityConstantExpression ()
    {
      var entityConstant = new SqlEntityConstantExpression (typeof (Cook), new Cook(), Expression.Constant (0));

      Assert.That (
          () => SqlGeneratingExpressionVisitor.GenerateSql (entityConstant, _commandBuilder, _stageMock),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (
              "It is not supported to use a constant entity object in any other context than to compare it with another entity. "
              + "Expression: ENTITY(0) (of type: 'Remotion.Linq.UnitTests.Linq.Core.TestDomain.Cook')."));
    }

    [Test]
    public void VisitSqlCollectionExpression ()
     {
       var items = new Expression[] { Expression.Constant (7), new SqlLiteralExpression ("Hello"), new SqlLiteralExpression (12) };
       var sqlCollectionExpression = new SqlCollectionExpression (typeof (List<object>), items);

       SqlGeneratingExpressionVisitor.GenerateSql (sqlCollectionExpression, _commandBuilder, _stageMock);

       Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1, 'Hello', 12)"));
       Assert.That (_commandBuilder.GetCommandParameters(), Is.EqualTo (new[] { new CommandParameter ("@1", 7) }));
    }

    [Test]
    public void VisitSqlCollectionExpression_Empty ()
     {
       var items = new Expression[0];
       var sqlCollectionExpression = new SqlCollectionExpression (typeof (List<object>), items);

       SqlGeneratingExpressionVisitor.GenerateSql (sqlCollectionExpression, _commandBuilder, _stageMock);

       Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(SELECT NULL WHERE 1 = 0)"));
    }
  }
}