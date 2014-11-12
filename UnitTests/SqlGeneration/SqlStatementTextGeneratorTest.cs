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
using Remotion.Linq.Clauses;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  [TestFixture]
  public class SqlStatementTextGeneratorTest
  {
    private TestableSqlStatementTextGenerator _generator;
    private SqlCommandBuilder _commandBuilder;
    private ISqlGenerationStage _stageMock;
    private SqlTable _sqlTable;

    [SetUp]
    public void SetUp ()
    {
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo();
      _stageMock = MockRepository.GenerateStrictMock<ISqlGenerationStage>();
      _generator = new TestableSqlStatementTextGenerator (_stageMock);
      _commandBuilder = new SqlCommandBuilder();
    }

    [Test]
    public void BuildSelectPart_WithSelect ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildFromPart_WithFrom ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SqlTables = { _sqlTable } });

      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Replay();

      _generator.BuildFromPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" FROM [Table] AS [t]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_WithAggregation ()
    {
      var aggregationExpression = new AggregationExpression (typeof(int), ExpressionHelper.CreateExpression(), AggregationModifier.Count);
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SelectProjection = aggregationExpression });

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, aggregationExpression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("COUNT(*)"));

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT COUNT(*)"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_WithDistinctIsTrue ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { IsDistinctQuery = true });

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT DISTINCT [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_WithTopExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { TopExpression = Expression.Constant (1) });

      _stageMock.Expect (mock => mock.GenerateTextForTopExpression (_commandBuilder, sqlStatement.TopExpression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("@1"));
      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT TOP (@1) [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_WithDistinctAndTopExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { TopExpression = Expression.Constant (5), IsDistinctQuery = true });

      _stageMock.Expect (mock => mock.GenerateTextForTopExpression (_commandBuilder, sqlStatement.TopExpression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("@1"));
      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT DISTINCT TOP (@1) [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_OuterSelect_NoSetOperationsAggregation ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();

      _stageMock.Expect (
          mock => mock.GenerateTextForOuterSelectExpression (
              _commandBuilder,
              sqlStatement.SelectProjection,
              SetOperationsMode.StatementIsNotSetCombined)
          );

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, true);

      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_OuterSelect_WithSetOperationsAggregation ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement(new SqlStatementBuilder 
      {
        SetOperationCombinedStatements = { SqlStatementModelObjectMother.CreateSetOperationCombinedStatement() }
      });

      _stageMock.Expect (
          mock => mock.GenerateTextForOuterSelectExpression (
              _commandBuilder,
              sqlStatement.SelectProjection,
              SetOperationsMode.StatementIsSetCombined)
          );

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, true);

      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildWhere_WithSingleWhereCondition ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { WhereCondition = Expression.Constant (true) });

      _stageMock.Expect (mock => mock.GenerateTextForWhereExpression (_commandBuilder, sqlStatement.WhereCondition))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("(@1 = 1)"));
      _stageMock.Replay();

      _generator.BuildWherePart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" WHERE (@1 = 1)"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildOrderBy_WithSingleOrderByClause ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false);
      var orderByClause = new Ordering (columnExpression, OrderingDirection.Asc);

      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { Orderings = { orderByClause } });

      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, orderByClause))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[Name] ASC"));
      _stageMock.Replay();

      _generator.BuildOrderByPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" ORDER BY [t].[Name] ASC"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildOrderBy_WithMultipleOrderByClauses ()
    {
      var columnExpression1 = new SqlColumnDefinitionExpression (typeof (string), "t", "ID", false);
      var orderByClause1 = new Ordering (columnExpression1, OrderingDirection.Asc);
      var columnExpression2 = new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false);
      var orderByClause2 = new Ordering (columnExpression2, OrderingDirection.Desc);
      var columnExpression3 = new SqlColumnDefinitionExpression (typeof (string), "t", "City", false);
      var orderByClause3 = new Ordering (columnExpression3, OrderingDirection.Desc);

      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { Orderings = { orderByClause1, orderByClause2, orderByClause3 } });

      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, sqlStatement.Orderings[0]))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID] ASC"));
      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, sqlStatement.Orderings[1]))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[Name] DESC"));
      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, sqlStatement.Orderings[2]))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[City] DESC"));
      _stageMock.Replay();

      _generator.BuildOrderByPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" ORDER BY [t].[ID] ASC, [t].[Name] DESC, [t].[City] DESC"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildDistinctPart ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { IsDistinctQuery = true });

      _generator.BuildDistinctPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("DISTINCT "));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildTopPart ()
    {
      var topExpression = Expression.Constant ("top");
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { TopExpression = topExpression });

      _stageMock.Expect (mock => mock.GenerateTextForTopExpression (_commandBuilder, topExpression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("top"));
      _stageMock.Replay ();

      _generator.BuildTopPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("TOP (top) "));
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void BuildSetOperationCombinedStatementsPart_Union ()
    {
      var setOperationCombinedStatement = new SetOperationCombinedStatement(SqlStatementModelObjectMother.CreateSqlStatement(), SetOperation.Union);
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (
          new SqlStatementBuilder
          {
              SetOperationCombinedStatements = { setOperationCombinedStatement }
          });

      _stageMock.Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, setOperationCombinedStatement.SqlStatement))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("statement"));
      _stageMock.Replay ();

      _generator.BuildSetOperationCombinedStatementsPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" UNION (statement)"));
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void BuildSetOperationCombinedStatementsPart_UnionAll ()
    {
      var setOperationCombinedStatement = new SetOperationCombinedStatement(SqlStatementModelObjectMother.CreateSqlStatement(), SetOperation.UnionAll);
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (
          new SqlStatementBuilder
          {
              SetOperationCombinedStatements = { setOperationCombinedStatement }
          });

      _stageMock.Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, setOperationCombinedStatement.SqlStatement))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("statement"));
      _stageMock.Replay ();

      _generator.BuildSetOperationCombinedStatementsPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" UNION ALL (statement)"));
      _stageMock.VerifyAllExpectations ();
    }
    
    [Test]
    public void Build_WithSelectAndFrom ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement(new SqlStatementBuilder { SqlTables = { _sqlTable }});

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Replay();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand ();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void Build_WithSelectAndNoFrom ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder());

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.Build (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void Build_WithGroupByExpression ()
    {
      var sqlGroupExpression = SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression ();
      sqlGroupExpression.AddAggregationExpressionWithName (Expression.Constant ("aggregation1"));
      sqlGroupExpression.AddAggregationExpressionWithName (Expression.Constant ("aggregation2"));

      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SqlTables = {_sqlTable }, GroupByExpression = sqlGroupExpression });

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Expect (
          mock => mock.GenerateTextForGroupByExpression (_commandBuilder, sqlStatement.GroupByExpression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append("keyExpression"));
      _stageMock.Replay ();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] GROUP BY keyExpression"));
    }

    [Test]
    public void Build_WithWhereCondition ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SqlTables = {_sqlTable }, WhereCondition = Expression.Constant(true) });

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Expect (mock => mock.GenerateTextForWhereExpression (_commandBuilder, sqlStatement.WhereCondition))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("(@1 = 1)"));
      _stageMock.Replay();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand ();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] WHERE (@1 = 1)"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void Build_WithOrderByClause ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false);
      var ordering = new Ordering (columnExpression, OrderingDirection.Asc);

      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SqlTables = {_sqlTable }, Orderings = { ordering } });

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, sqlStatement.Orderings[0]))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[Name] ASC"));
      _stageMock.Replay();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] ORDER BY [t].[Name] ASC"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void Build_WithSetOperationCombinedStatement ()
    {
      var setOperationCombinedStatement = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder 
      { 
        SqlTables = {_sqlTable },
        SetOperationCombinedStatements = { setOperationCombinedStatement }
      });

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Expect (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement.SetOperationCombinedStatements[0].SqlStatement))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("SELECT FOO FROM BAR"));
      _stageMock.Replay();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] UNION (SELECT FOO FROM BAR)"));
      _stageMock.VerifyAllExpectations();
    }
  }
}