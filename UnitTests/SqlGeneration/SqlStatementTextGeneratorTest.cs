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
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  [TestFixture]
  public class SqlStatementTextGeneratorTest
  {
    private TestableSqlStatementTextGenerator _generator;
    private SqlCommandBuilder _commandBuilder;
    private Mock<ISqlGenerationStage> _stageMock;
    private SqlTable _sqlTable;

    [SetUp]
    public void SetUp ()
    {
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo();
      _stageMock = new Mock<ISqlGenerationStage> (MockBehavior.Strict);
      _generator = new TestableSqlStatementTextGenerator (_stageMock.Object);
      _commandBuilder = new SqlCommandBuilder();
    }

    [Test]
    public void BuildSelectPart_WithSelect ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();

      _stageMock
          .Setup (
              mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection)).Callback (
                  (ISqlCommandBuilder mi, Expression _) => ((SqlCommandBuilder) mi).Append ("[t].[ID],[t].[Name],[t].[City]"))
          .Verifiable();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Verify();
    }

    [Test]
    public void BuildFromPart_WithFrom ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SqlTables = { _sqlTable } });

      _stageMock
          .Setup (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true)).Callback ((ISqlCommandBuilder mi, SqlTable _1, bool _2) => mi.Append ("[Table] AS [t]"))
          .Verifiable();

      _generator.BuildFromPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" FROM [Table] AS [t]"));
      _stageMock.Verify();
    }

    [Test]
    public void BuildSelectPart_WithAggregation ()
    {
      var aggregationExpression = new AggregationExpression (typeof(int), ExpressionHelper.CreateExpression(), AggregationModifier.Count);
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SelectProjection = aggregationExpression });

      _stageMock
          .Setup (
              mock => mock.GenerateTextForSelectExpression (_commandBuilder, aggregationExpression)).Callback (
                  (ISqlCommandBuilder mi, Expression _) => mi.Append ("COUNT(*)"))
         .Verifiable();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT COUNT(*)"));
      _stageMock.Verify();
    }

    [Test]
    public void BuildSelectPart_WithDistinctIsTrue ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { IsDistinctQuery = true });

      _stageMock
          .Setup (mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("[t].[ID],[t].[Name],[t].[City]"))
          .Verifiable();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT DISTINCT [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Verify();
    }

    [Test]
    public void BuildSelectPart_WithTopExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { TopExpression = Expression.Constant (1) });

      _stageMock
          .Setup (mock => mock.GenerateTextForTopExpression (_commandBuilder, sqlStatement.TopExpression))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("@1"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("[t].[ID],[t].[Name],[t].[City]"))
          .Verifiable();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT TOP (@1) [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Verify();
    }

    [Test]
    public void BuildSelectPart_WithDistinctAndTopExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { TopExpression = Expression.Constant (5), IsDistinctQuery = true });

      _stageMock
          .Setup (mock => mock.GenerateTextForTopExpression (_commandBuilder, sqlStatement.TopExpression))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("@1"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("[t].[ID],[t].[Name],[t].[City]"))
          .Verifiable();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT DISTINCT TOP (@1) [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Verify();
    }

    [Test]
    public void BuildSelectPart_OuterSelect_NoSetOperationsAggregation ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();

      _stageMock
          .Setup (
              mock => mock.GenerateTextForOuterSelectExpression (
                  _commandBuilder,
                  sqlStatement.SelectProjection,
                  SetOperationsMode.StatementIsNotSetCombined))
          .Verifiable();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, true);

      _stageMock.Verify();
    }

    [Test]
    public void BuildSelectPart_OuterSelect_WithSetOperationsAggregation ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement(new SqlStatementBuilder 
      {
        SetOperationCombinedStatements = { SqlStatementModelObjectMother.CreateSetOperationCombinedStatement() }
      });

      _stageMock
          .Setup (
              mock => mock.GenerateTextForOuterSelectExpression (
                  _commandBuilder,
                  sqlStatement.SelectProjection,
                  SetOperationsMode.StatementIsSetCombined))
          .Verifiable();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, true);

      _stageMock.Verify();
    }

    [Test]
    public void BuildWhere_WithSingleWhereCondition ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { WhereCondition = Expression.Constant (true) });

      _stageMock
          .Setup (mock => mock.GenerateTextForWhereExpression (_commandBuilder, sqlStatement.WhereCondition))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("(@1 = 1)"))
          .Verifiable();

      _generator.BuildWherePart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" WHERE (@1 = 1)"));
      _stageMock.Verify();
    }

    [Test]
    public void BuildOrderBy_WithSingleOrderByClause ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false);
      var orderByClause = new Ordering (columnExpression, OrderingDirection.Asc);

      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { Orderings = { orderByClause } });

      _stageMock
          .Setup (mock => mock.GenerateTextForOrdering (_commandBuilder, orderByClause))
          .Callback ((ISqlCommandBuilder mi, Ordering _) => mi.Append ("[t].[Name] ASC"))
          .Verifiable();

      _generator.BuildOrderByPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" ORDER BY [t].[Name] ASC"));
      _stageMock.Verify();
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

      _stageMock
          .Setup (mock => mock.GenerateTextForOrdering (_commandBuilder, sqlStatement.Orderings[0]))
          .Callback ((ISqlCommandBuilder mi, Ordering _) => mi.Append ("[t].[ID] ASC"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForOrdering (_commandBuilder, sqlStatement.Orderings[1]))
          .Callback ((ISqlCommandBuilder mi, Ordering _) => mi.Append ("[t].[Name] DESC"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForOrdering (_commandBuilder, sqlStatement.Orderings[2]))
          .Callback ((ISqlCommandBuilder mi, Ordering _) => mi.Append ("[t].[City] DESC"))
          .Verifiable();

      _generator.BuildOrderByPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" ORDER BY [t].[ID] ASC, [t].[Name] DESC, [t].[City] DESC"));
      _stageMock.Verify();
    }

    [Test]
    public void BuildDistinctPart ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { IsDistinctQuery = true });

      _generator.BuildDistinctPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("DISTINCT "));
      _stageMock.Verify();
    }

    [Test]
    public void BuildTopPart ()
    {
      var topExpression = Expression.Constant ("top");
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { TopExpression = topExpression });

      _stageMock
          .Setup (mock => mock.GenerateTextForTopExpression (_commandBuilder, topExpression))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("top"))
          .Verifiable();

      _generator.BuildTopPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("TOP (top) "));
      _stageMock.Verify();
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

      _stageMock
          .Setup (mock => mock.GenerateTextForSqlStatement (_commandBuilder, setOperationCombinedStatement.SqlStatement))
          .Callback ((ISqlCommandBuilder mi, SqlStatement _) => mi.Append ("statement"))
          .Verifiable();

      _generator.BuildSetOperationCombinedStatementsPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" UNION (statement)"));
      _stageMock.Verify();
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

      _stageMock
          .Setup (mock => mock.GenerateTextForSqlStatement (_commandBuilder, setOperationCombinedStatement.SqlStatement))
          .Callback ((ISqlCommandBuilder mi, SqlStatement _) => mi.Append ("statement"))
          .Verifiable();

      _generator.BuildSetOperationCombinedStatementsPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" UNION ALL (statement)"));
      _stageMock.Verify();
    }
    
    [Test]
    public void Build_WithSelectAndFrom ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement(new SqlStatementBuilder { SqlTables = { _sqlTable }});

      _stageMock
          .Setup (mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("[t].[ID],[t].[Name],[t].[City]"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .Callback ((ISqlCommandBuilder mi, SqlTable _1, bool _2) => mi.Append ("[Table] AS [t]"))
          .Verifiable();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand ();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
      _stageMock.Verify();
    }

    [Test]
    public void Build_WithSelectAndNoFrom ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder());

      _stageMock
          .Setup (mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("[t].[ID],[t].[Name],[t].[City]"))
          .Verifiable();

      _generator.Build (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Verify();
    }

    [Test]
    public void Build_WithGroupByExpression ()
    {
      var sqlGroupExpression = SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression ();
      sqlGroupExpression.AddAggregationExpressionWithName (Expression.Constant ("aggregation1"));
      sqlGroupExpression.AddAggregationExpressionWithName (Expression.Constant ("aggregation2"));

      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SqlTables = {_sqlTable }, GroupByExpression = sqlGroupExpression });

      _stageMock
          .Setup (mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("[t].[ID],[t].[Name],[t].[City]"))
         .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .Callback ((ISqlCommandBuilder mi, SqlTable _1, bool _2) => mi.Append ("[Table] AS [t]"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForGroupByExpression (_commandBuilder, sqlStatement.GroupByExpression))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("keyExpression"))
          .Verifiable();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _stageMock.Verify();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] GROUP BY keyExpression"));
    }

    [Test]
    public void Build_WithWhereCondition ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SqlTables = {_sqlTable }, WhereCondition = Expression.Constant(true) });

      _stageMock
          .Setup (mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("[t].[ID],[t].[Name],[t].[City]"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .Callback ((ISqlCommandBuilder mi, SqlTable _1, bool _2) => mi.Append ("[Table] AS [t]"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForWhereExpression (_commandBuilder, sqlStatement.WhereCondition))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("(@1 = 1)"))
          .Verifiable();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand ();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] WHERE (@1 = 1)"));
      _stageMock.Verify();
    }

    [Test]
    public void Build_WithOrderByClause ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false);
      var ordering = new Ordering (columnExpression, OrderingDirection.Asc);

      var sqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (new SqlStatementBuilder { SqlTables = {_sqlTable }, Orderings = { ordering } });

      _stageMock
          .Setup (mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("[t].[ID],[t].[Name],[t].[City]"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .Callback ((ISqlCommandBuilder mi, SqlTable _1, bool _2) => mi.Append ("[Table] AS [t]"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForOrdering (_commandBuilder, sqlStatement.Orderings[0]))
          .Callback ((ISqlCommandBuilder mi, Ordering _) => mi.Append ("[t].[Name] ASC"))
          .Verifiable();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] ORDER BY [t].[Name] ASC"));
      _stageMock.Verify();
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

      _stageMock
          .Setup (mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .Callback ((ISqlCommandBuilder mi, Expression _) => mi.Append ("[t].[ID],[t].[Name],[t].[City]"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .Callback ((ISqlCommandBuilder mi, SqlTable _1, bool _2) => mi.Append ("[Table] AS [t]"))
          .Verifiable();
      _stageMock
          .Setup (mock => mock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement.SetOperationCombinedStatements[0].SqlStatement))
          .Callback ((ISqlCommandBuilder mi, SqlStatement _) => mi.Append ("SELECT FOO FROM BAR"))
          .Verifiable();

      _generator.Build (sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] UNION (SELECT FOO FROM BAR)"));
      _stageMock.Verify();
    }
  }
}