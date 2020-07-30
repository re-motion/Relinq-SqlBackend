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
using Remotion.Development.UnitTesting;
using Remotion.Linq.Clauses;
using Remotion.Linq.Development.UnitTesting.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Utilities;
using Moq;
using Moq.Language.Flow;
using Moq.Protected;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  [TestFixture]
  public class DefaultSqlGenerationStageTest
  {
    private SqlStatement _sqlStatement;
    private SqlCommandBuilder _commandBuilder;
    private SqlEntityExpression _entityExpression;

    [SetUp]
    public void SetUp ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo();
      _entityExpression = new SqlEntityDefinitionExpression (
          typeof (string),
          "t",
          null,
          e => e.GetColumn (typeof (string), "ID", true),
          new SqlColumnExpression[]
          {
              new SqlColumnDefinitionExpression (typeof (string), "t", "ID", true),
              new SqlColumnDefinitionExpression (typeof (int), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (int), "t", "City", false)
          });

      _sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (
          new SqlStatementBuilder
          {
              SelectProjection = _entityExpression,
              SqlTables = { sqlTable }
          });
      _commandBuilder = new SqlCommandBuilder();
    }

    [Test]
    public void GenerateTextForFromTable ()
    {
      var stage = new DefaultSqlGenerationStage();

      stage.GenerateTextForFromTable (_commandBuilder, _sqlStatement.SqlTables[0], true);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[Table] AS [t]"));
    }
    
    [Test]
    public void GenerateTextForSelectExpression ()
    {
      var stage = new DefaultSqlGenerationStage();

      stage.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void GenerateTextForOuterSelectExpression ()
    {
      var stage = new DefaultSqlGenerationStage();

      stage.GenerateTextForOuterSelectExpression (_commandBuilder, _sqlStatement.SelectProjection, SetOperationsMode.StatementIsSetCombined);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));

      Assert.That (_commandBuilder.GetInMemoryProjectionBody(), Is.Not.Null);
    }

    [Test]
    public void GenerateTextForOuterSelectExpression_PassesOnSetOperationsMode ()
    {
      var stage = new DefaultSqlGenerationStage();

      var projectionWithMethodCall = Expression.Call (MemberInfoFromExpressionUtility.GetMethod (() => SomeMethod (null)), _sqlStatement.SelectProjection);

      Assert.That (
          () => stage.GenerateTextForOuterSelectExpression (_commandBuilder, projectionWithMethodCall, SetOperationsMode.StatementIsSetCombined),
          Throws.TypeOf<NotSupportedException>());
      Assert.That (
          () => stage.GenerateTextForOuterSelectExpression (_commandBuilder, projectionWithMethodCall, SetOperationsMode.StatementIsNotSetCombined),
          Throws.Nothing);
    }

    private static string SomeMethod (string s)
    {
      throw new NotImplementedException();
    }

    [Test]
    public void GenerateTextForTopExpression ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          { DataInfo = new TestStreamedValueInfo (typeof (int)), SelectProjection = _entityExpression, TopExpression = Expression.Constant (5) }.
              GetSqlStatement();

      var stageMock = new Mock<DefaultSqlGenerationStage>();
      stageMock.CallBase = true;
      SetupGenerateTextForNonSelectExpression (stageMock, sqlStatement.TopExpression)
          .Callback ((ISqlCommandBuilder _0, Expression _1) => _commandBuilder.Append ("test"))
          .Verifiable();

      stageMock.Object.GenerateTextForTopExpression (_commandBuilder, sqlStatement.TopExpression);

      stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForWhereExpression ()
    {
      var whereCondition = Expression.AndAlso (Expression.Constant (true), Expression.Constant (true));

      var stageMock = new Mock<DefaultSqlGenerationStage>();
      stageMock.CallBase = true;
      SetupGenerateTextForNonSelectExpression (stageMock, whereCondition)
          .Callback ((ISqlCommandBuilder _0, Expression _1) => _commandBuilder.Append ("test"))
          .Verifiable();

      stageMock.Object.GenerateTextForWhereExpression (_commandBuilder, whereCondition);

      stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForOrderByExpression_ConstantExpression ()
    {
      var expression = Expression.Constant (1);

      var stageMock = new Mock<DefaultSqlGenerationStage>();
      stageMock.CallBase = true;
      SetupGenerateTextForNonSelectExpression (stageMock, expression)
          .Callback ((ISqlCommandBuilder _0, Expression _1) => _commandBuilder.Append ("test"))
          .Verifiable();

      stageMock.Object.GenerateTextForOrderByExpression (_commandBuilder, expression);

      stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForOrdering ()
    {
      var ordering = new Ordering (Expression.Constant (1), OrderingDirection.Asc);

      var stage = new DefaultSqlGenerationStage();

      stage.GenerateTextForOrdering (_commandBuilder, ordering);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("(SELECT @1) ASC"));
    }

    [Test]
    public void GenerateTextForSqlStatement ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (
          _entityExpression, new[] { new SqlTable (new ResolvedSimpleTableInfo (typeof (int), "Table", "t"), JoinSemantics.Inner) });

      var stage = new DefaultSqlGenerationStage();

      stage.GenerateTextForSqlStatement (_commandBuilder, sqlStatement);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }

    [Test]
    public void GenerateTextForOuterSqlStatement ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (
          _entityExpression, new[] { new SqlTable (new ResolvedSimpleTableInfo (typeof (int), "Table", "t"), JoinSemantics.Inner) });

      var stage = new DefaultSqlGenerationStage();

      stage.GenerateTextForOuterSqlStatement (_commandBuilder, sqlStatement);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));

      var inMemoryProjection = _commandBuilder.GetInMemoryProjectionBody();
      Assert.That (inMemoryProjection, Is.AssignableTo (typeof (MethodCallExpression)));

      var methodCallExpression = (MethodCallExpression) inMemoryProjection;
      Assert.That (methodCallExpression.Method, Is.EqualTo ((typeof (IDatabaseResultRow).GetMethod ("GetEntity").MakeGenericMethod (sqlStatement.SelectProjection.Type))));
      Assert.That (methodCallExpression.Arguments.Count, Is.EqualTo (1));
      Assert.That (((ColumnID[]) ((ConstantExpression) methodCallExpression.Arguments[0]).Value)[0].ColumnName, Is.EqualTo ("ID"));
      Assert.That (((ColumnID[]) ((ConstantExpression) methodCallExpression.Arguments[0]).Value)[1].ColumnName, Is.EqualTo ("Name"));
      Assert.That (((ColumnID[]) ((ConstantExpression) methodCallExpression.Arguments[0]).Value)[2].ColumnName, Is.EqualTo ("City"));
    }
    
    [Test]
    public void GenerateTextForJoinKeyExpression ()
    {
      var expression = new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false);

      var stageMock = new Mock<DefaultSqlGenerationStage>();
      stageMock.CallBase = true;
      SetupGenerateTextForNonSelectExpression (stageMock, expression)
          .Callback ((ISqlCommandBuilder sqlCommandBuilder, Expression expressionArg) => _commandBuilder.Append ("test"))
          .Verifiable();

      stageMock.Object.GenerateTextForJoinCondition (_commandBuilder, expression);

      stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForGroupByExpression () //TODO this
    {
      var expression = SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression ();

      var stageMock = new Mock<DefaultSqlGenerationStage>();
      stageMock.CallBase = true;
      SetupGenerateTextForNonSelectExpression (stageMock, expression)
          .Callback ((ISqlCommandBuilder _0, Expression _1) => _commandBuilder.Append ("GROUP BY keyExpression"))
          .Verifiable();

      stageMock.Object.GenerateTextForGroupByExpression (_commandBuilder, expression);

      stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("GROUP BY keyExpression"));
    }

    private ISetup<DefaultSqlGenerationStage> SetupGenerateTextForNonSelectExpression (Mock<DefaultSqlGenerationStage> mock, Expression expression)
    {
      return mock
          .Protected()
          .Setup (
              "GenerateTextForNonSelectExpression",
              _commandBuilder,
              ItExpr.Is<Expression> (_ => _ == expression));
    }
  }
}