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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
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
      var sqlColumnExpression = new SqlColumnExpression (typeof (int), "s", "ID");
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnExpressionWithStart ()
    {
      var sqlColumnExpression = new SqlColumnExpression (typeof (Cook), "c", "*");
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[c].*"));
    }

    [Test]
    public void GenerateSql_VisitSqlEntityExpression ()
    {
      var primaryKeyColumn = new SqlColumnExpression (typeof (string), "t", "ID");
      var sqlColumnListExpression = new SqlEntityExpression (
          SqlStatementModelObjectMother.CreateSqlTable(),
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnExpression (typeof (string), "t", "Name"),
              new SqlColumnExpression (typeof (string), "t", "City")
          });
      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlColumnListExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void GenerateSql_BoolExpression_ValueSemantics ()
    {
      var boolExpression = Expression.Equal (Expression.Constant ("hugo"), Expression.Constant ("sepp"));
      SqlGeneratingExpressionVisitor.GenerateSql (
          boolExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("CASE WHEN (@1 = @2) THEN 1 ELSE 0 END"));
    }

    [Test]
    public void GenerateSql_BoolExpression_PredicateSemantics ()
    {
      var boolExpression = Expression.Equal (Expression.Constant ("hugo"), Expression.Constant ("sepp"));
      SqlGeneratingExpressionVisitor.GenerateSql (
          boolExpression, _commandBuilder, SqlExpressionContext.PredicateRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 = @2)"));
    }

    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The expression 'CustomExpression' cannot be translated to SQL text by this SQL generator. Expression type 'CustomExpression' is not supported."
        )]
    [Test]
    public void GenerateSql_UnsupportedExpression ()
    {
      var unknownExpression = new CustomExpression (typeof (int));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unknownExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);
    }

    [Test]
    public void VisitConstantExpression_TrueParameter ()
    {
      var expression = Expression.Constant (true);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitConstantExpression_FalseParameter ()
    {
      var expression = Expression.Constant (false);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo (0));
    }

    [Test]
    public void VisitConstantExpression_NullValue ()
    {
      var expression = Expression.Constant (null);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (0));
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("NULL"));
    }

    [Test]
    public void VisitConstantExpression_Collection ()
    {
      var collectionExpression = Expression.Constant (new [] {"Hugo", "Maier", "Markart"});
      var sqlInExpression = new SqlBinaryOperatorExpression ("IN", Expression.Constant ("Hubert"), collectionExpression);
      
      SqlGeneratingExpressionVisitor.GenerateSql (sqlInExpression, _commandBuilder, SqlExpressionContext.PredicateRequired, _stageMock);

      var expectedParameters = new[] { 
          new CommandParameter("@1", "Hubert"),
          new CommandParameter("@2", "Hugo"),
          new CommandParameter("@3", "Maier"),
          new CommandParameter("@4", "Markart")};
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("@1 IN (@2, @3, @4)"));
      Assert.That (_commandBuilder.GetCommandParameters (), Is.EqualTo (expectedParameters));
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void VisitConstantExpression_EmptyCollection ()
    {
      var collectionExpression = Expression.Constant (new string[] { });
      var sqlInExpression = new SqlBinaryOperatorExpression ("IN", Expression.Constant ("Hubert"), collectionExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (sqlInExpression, _commandBuilder, SqlExpressionContext.PredicateRequired, _stageMock);
    }

    [Test]
    public void VisitLiteralExpression ()
    {
      var expression = new SqlLiteralExpression (5);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("5"));
      Assert.That (_commandBuilder.GetCommandParameters(), Is.Empty);
    }

    [Test]
    public void VisitConstantExpression_StringParameter ()
    {
      var expression = Expression.Constant ("Test");
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo ("Test"));
    }

    [Test]
    public void VisitBinaryExpression ()
    {
      Expression binaryExpression = Expression.Add (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (
          binaryExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("(@1 + @2)"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNot ()
    {
      var unaryNotExpression = Expression.Not (Expression.Equal (Expression.Constant ("hugo"), Expression.Constant ("hugo")));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, SqlExpressionContext.PredicateRequired, _stageMock);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("NOT (@1 = @2)"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNot_WithBitwiseNot ()
    {
      var unaryNotExpression = Expression.Not (Expression.Constant (1));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("~@1"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNegate ()
    {
      var unaryNotExpression = Expression.Negate (Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("-@1"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryPlus ()
    {
      var unaryNotExpression = Expression.UnaryPlus (Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryNotExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("+@1"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitUnaryExpression_NotSupported ()
    {
      var unaryExpression = Expression.TypeAs (Expression.Constant ("1"), typeof (string));
      SqlGeneratingExpressionVisitor.GenerateSql (
          unaryExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);
    }

    [Test]
    public void VistSqlCaseExpression ()
    {
      var caseExpression = new SqlCaseExpression (
          Expression.Equal (Expression.Constant (2), Expression.Constant (2)),
          Expression.Constant (0),
          Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          caseExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("CASE WHEN (@1 = @2) THEN @3 ELSE @4 END"));
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var subStatementExpression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement, SqlExpressionContext.SingleValueRequired))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("SELECT [t].[Name] FROM [Table] AS [t]"));

      SqlGeneratingExpressionVisitor.GenerateSql (
          subStatementExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(SELECT [t].[Name] FROM [Table] AS [t])"));
    }

    [Test]
    public void VisitJoinConditionExpression ()
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var primaryColumn = new SqlColumnExpression (typeof (Cook), "c", "ID");
      var foreignColumn = new SqlColumnExpression (typeof (Cook), "a", "FK");
      var joinInfo = new ResolvedJoinInfo (resolvedTableInfo, primaryColumn, foreignColumn);
      var sqlTable = new SqlJoinedTable (joinInfo);
      var joinConditionExpression = new JoinConditionExpression (sqlTable);

      SqlGeneratingExpressionVisitor.GenerateSql (
          joinConditionExpression, _commandBuilder, SqlExpressionContext.PredicateRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("([c].[ID] = [a].[FK])"));
    }

    [Test]
    public void VisitSqlBinaryOperatorExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();
      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var sqlInExpression = new SqlBinaryOperatorExpression ("IN", Expression.Constant (1), sqlSubStatementExpression);

      _stageMock
          .Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement, SqlExpressionContext.SingleValueRequired))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("test"));

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlInExpression, _commandBuilder, SqlExpressionContext.PredicateRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1 IN (test)"));
    }

    [Test]
    public void VisitSqlIsNullExpression ()
    {
      var expression = Expression.Constant ("test");
      var sqlIsNullExpression = new SqlIsNullExpression (expression);

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlIsNullExpression, _commandBuilder, SqlExpressionContext.PredicateRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 IS NULL)"));
    }

    [Test]
    public void VisitSqlLiteralExpression_Int ()
    {
      var expression = new SqlLiteralExpression (1);
      SqlGeneratingExpressionVisitor.GenerateSql (
          expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("1"));

    }

    [Test]
    public void VisitSqlLiteralExpression_String ()
    {
      var expression = new SqlLiteralExpression ("1");
      SqlGeneratingExpressionVisitor.GenerateSql (
          expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("'1'"));
    }

    [Test]
    public void VisitSqlLiteralExpression_Empty ()
    {
      var expression = new SqlLiteralExpression ("");
      SqlGeneratingExpressionVisitor.GenerateSql (
          expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("''"));
    }

    [Test]
    public void VisitSqlIsNotNullExpression ()
    {
      var expression = Expression.Constant ("test");
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (expression);

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlIsNotNullExpression, _commandBuilder, SqlExpressionContext.PredicateRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(@1 IS NOT NULL)"));
    }

    [Test]
    public void VisitSqlFunctionExpression ()
    {
      var sqlFunctionExpression = new SqlFunctionExpression (typeof (int), "LENFUNC", new SqlLiteralExpression ("test"), new SqlLiteralExpression (1));

      SqlGeneratingExpressionVisitor.GenerateSql (
          sqlFunctionExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("LENFUNC('test', 1)"));
    }

    [Test]
    public void VisitSqlConvertExpression ()
    {
      var sqlConvertExpression = new SqlConvertExpression (typeof (string), Expression.Constant ("1"));

      SqlGeneratingExpressionVisitor.GenerateSql (sqlConvertExpression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("CONVERT(NVARCHAR, @1)"));
    }

    [Test]
    public void VisitSqlValueTableReferenceExpression ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (string));
      var expression = new SqlValueTableReferenceExpression (sqlTable);

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[t].[value]"));
    }

    [Test]
    public void VisitSqlCustomTextGeneratorExpression ()
    {
      var expression = new TestableSqlCustomTextGeneratorExpression (typeof (string));

      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("TestableSqlCustomTextGeneratorExpression"));
    }
  }
}