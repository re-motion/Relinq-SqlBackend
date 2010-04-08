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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestUtilities;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class DefaultSqlGenerationStageTest
  {
    private SqlStatement _sqlStatement;
    private DefaultSqlGenerationStage _stageMock;
    private SqlCommandBuilder _commandBuilder;
    private SqlEntityExpression _columnListExpression;

    [SetUp]
    public void SetUp ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo();
      var primaryKeyColumn = new SqlColumnExpression (typeof (int), "t", "ID");
      _columnListExpression = new SqlEntityExpression (
          typeof (Cook),
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnExpression (typeof (int), "t", "Name"),
              new SqlColumnExpression (typeof (int), "t", "City")
          });

      _sqlStatement = new SqlStatement (_columnListExpression, new[] { sqlTable }, new Ordering[] { }, null, null,false, false);
      _commandBuilder = new SqlCommandBuilder();

      _stageMock = MockRepository.GeneratePartialMock<DefaultSqlGenerationStage>();
    }

    [Test]
    public void GenerateTextForFromTable ()
    {
      _stageMock.GenerateTextForFromTable (_commandBuilder, _sqlStatement.SqlTables[0], true);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    public void GenerateTextForSelectExpression ()
    {
      _stageMock
          .Expect (
          mock =>
          PrivateInvoke.InvokeNonPublicMethod (
              mock, "GenerateTextForExpression", _commandBuilder, _sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .WhenCalled (c => _commandBuilder.Append ("[t].[ID],[t].[Name],[t].[City]"));

      _stageMock.Replay();

      _stageMock.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void GenerateTextForTopExpression ()
    {
      // TODO Review 2546: Create a new builder and set the relevant properties: new SqlStatementBuilder { SelectExpression = _columListExpression, TopExpression = Expression.Constant(5) }.GetStatement()
      _sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithNewTopExpression (_sqlStatement, Expression.Constant (5));

      _stageMock
          .Expect (
          mock =>
          PrivateInvoke.InvokeNonPublicMethod (
              mock, "GenerateTextForExpression", _commandBuilder, _sqlStatement.TopExpression, SqlExpressionContext.SingleValueRequired))
          .WhenCalled (c => _commandBuilder.Append ("test"));
      _stageMock.Replay();

      _stageMock.GenerateTextForTopExpression (_commandBuilder, _sqlStatement.TopExpression);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForWhereExpression ()
    {
      // TODO Review 2546: Instead of the following line, add a CreateSqlStatement method that allows one to specify a select projection, a where condition, and a params array of sql tables. Use the _columnListExpression as the select projection.
      var whereCondition = Expression.AndAlso (Expression.Constant (true), Expression.Constant (true));
      _sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithNewWhereCondition (_sqlStatement, whereCondition);

      _stageMock
          .Expect (
          mock =>
          PrivateInvoke.InvokeNonPublicMethod (
              mock, "GenerateTextForExpression", _commandBuilder, _sqlStatement.WhereCondition, SqlExpressionContext.PredicateRequired))
          .WhenCalled (c => _commandBuilder.Append ("test"));
      _stageMock.Replay();

      _stageMock.GenerateTextForWhereExpression (_commandBuilder, _sqlStatement.WhereCondition);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForOrderByExpression_ConstantExpression ()
    {
      var expression = Expression.Constant (1);

      _stageMock
          .Expect (
          mock =>
          PrivateInvoke.InvokeNonPublicMethod (
              mock, "GenerateTextForExpression", _commandBuilder, expression, SqlExpressionContext.SingleValueRequired))
          .WhenCalled (c => _commandBuilder.Append ("test"));
      _stageMock.Replay();

      _stageMock.GenerateTextForOrderByExpression (_commandBuilder, expression);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForSqlStatement ()
    {
      // TODO Review 2546: Instead of using CreateSqlStatementWithNew..., add a CreateSqlStatement method that allows one to specify a select projection and a params array of tables.
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithNewSelectProjection (sqlStatement, _columnListExpression);
      ((SqlTable) sqlStatement.SqlTables[0]).TableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");

      _stageMock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement, SqlExpressionContext.ValueRequired);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }

    [Test]
    public void GenerateTextForJoinKeyExpression ()
    {
      var expression = new SqlColumnExpression (typeof (int), "c", "ID");

      _stageMock
          .Expect (
          mock =>
          PrivateInvoke.InvokeNonPublicMethod (
              mock, "GenerateTextForExpression", _commandBuilder, expression, SqlExpressionContext.SingleValueRequired))
          .WhenCalled (c => _commandBuilder.Append ("test"));
      _stageMock.Replay();

      _stageMock.GenerateTextForJoinKeyExpression (_commandBuilder, expression);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }
  }
}