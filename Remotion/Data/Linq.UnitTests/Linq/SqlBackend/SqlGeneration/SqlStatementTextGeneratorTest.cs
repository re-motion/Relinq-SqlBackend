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
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlStatementTextGeneratorTest
  {
    private SqlStatement _sqlStatement;
    private TestableSqlStatementTextGenerator _generator;
    private SqlCommandBuilder _commandBuilder;
    private ISqlGenerationStage _stageMock;
    private SqlEntityExpression _columnListExpression;
    private SqlTable _sqlTable;

    [SetUp]
    public void SetUp ()
    {
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo();
      var primaryKeyColumn = new SqlColumnExpression (typeof (int), "t", "ID", true);
      _columnListExpression = new SqlEntityExpression (
          _sqlTable,
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnExpression (typeof (int), "t", "Name", false),
              new SqlColumnExpression (typeof (int), "t", "City", false)
          });

      _stageMock = MockRepository.GenerateStrictMock<ISqlGenerationStage>();
      _generator = new TestableSqlStatementTextGenerator (_stageMock);
      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _columnListExpression,
          new[] { _sqlTable },
          new Ordering[] { },
          null,
          null,
          false,
          AggregationModifier.None);
      _commandBuilder = new SqlCommandBuilder();
    }

    [Test]
    public void BuildSelectPart_WithSelect ()
    {
      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (_sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_NoEntityExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (new SqlColumnExpression (typeof (string), "t", "FirstName", false));
      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[FirstName]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[FirstName] AS [value]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildFromPart_WithFrom ()
    {
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, _sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Replay();

      _generator.BuildFromPart (_sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" FROM [Table] AS [t]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_WithCount ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          {
              DataInfo = new TestStreamedValueInfo (typeof (int)),
              SelectProjection = _columnListExpression,
              AggregationModifier = AggregationModifier.Count
          }.GetSqlStatement();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT COUNT(*) AS [value]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_WithAverage ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          {
            DataInfo = new TestStreamedValueInfo (typeof (int)),
            SelectProjection = _columnListExpression,
            AggregationModifier = AggregationModifier.Average
          }.GetSqlStatement ();

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID]"));

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("SELECT AVERAGE([t].[ID]) AS [value]"));
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void BuildSelectPart_WithSum ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          {
            DataInfo = new TestStreamedValueInfo (typeof (int)),
            SelectProjection = _columnListExpression,
            AggregationModifier = AggregationModifier.Sum
          }.GetSqlStatement ();

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID]"));

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("SELECT SUM([t].[ID]) AS [value]"));
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void BuildSelectPart_WithMin ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          {
            DataInfo = new TestStreamedValueInfo (typeof (int)),
            SelectProjection = _columnListExpression,
            AggregationModifier = AggregationModifier.Min
          }.GetSqlStatement ();

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID]"));

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("SELECT MIN([t].[ID]) AS [value]"));
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void BuildSelectPart_WithMax ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          {
            DataInfo = new TestStreamedValueInfo (typeof (int)),
            SelectProjection = _columnListExpression,
            AggregationModifier = AggregationModifier.Max
          }.GetSqlStatement ();

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID]"));

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("SELECT MAX([t].[ID]) AS [value]"));
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void BuildSelectPart_WithDistinctIsTrue ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          { DataInfo = new TestStreamedValueInfo (typeof (int)), SelectProjection = _columnListExpression, IsDistinctQuery = true }.GetSqlStatement();

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT DISTINCT [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_WithTopExpression ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          { DataInfo = new TestStreamedValueInfo (typeof (int)), SelectProjection = _columnListExpression, TopExpression = Expression.Constant (1) }.
              GetSqlStatement();

      _stageMock.Expect (mock => mock.GenerateTextForTopExpression (_commandBuilder, sqlStatement.TopExpression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("@1"));
      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT TOP (@1) [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_WithDistinctAndTopExpression ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          {
              DataInfo = new TestStreamedValueInfo (typeof (int)),
              SelectProjection = _columnListExpression,
              TopExpression = Expression.Constant (5),
              IsDistinctQuery = true
          }.
              GetSqlStatement();

      _stageMock.Expect (mock => mock.GenerateTextForTopExpression (_commandBuilder, sqlStatement.TopExpression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("@1"));
      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT DISTINCT TOP (@1) [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_HasValueSemantics ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Equal (Expression.Constant (0), Expression.Constant (1)));

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("CASE WHEN (@1 = @2) THEN 1 ELSE 0 END"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT CASE WHEN (@1 = @2) THEN 1 ELSE 0 END AS [value]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildWhere_WithSingleWhereCondition_PredicateSemantics ()
    {
      var sqlStatement =
          new SqlStatementBuilder
          {
              DataInfo = new TestStreamedValueInfo (typeof (int)),
              SelectProjection = _columnListExpression,
              WhereCondition = Expression.Constant (true)
          }
              .GetSqlStatement();

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
      var columnExpression = new SqlColumnExpression (typeof (string), "t", "Name", false);
      var orderByClause = new Ordering (columnExpression, OrderingDirection.Asc);

      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _columnListExpression,
          new[] { _sqlTable },
          new[] { orderByClause },
          null,
          null,
          false,
          AggregationModifier.None);

      _stageMock.Expect (mock => mock.GenerateTextForOrderByExpression (_commandBuilder, _sqlStatement.Orderings[0].Expression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[Name]"));
      _stageMock.Replay();

      _generator.BuildOrderByPart (_sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" ORDER BY [t].[Name] ASC"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildOrderBy_WithMultipleOrderByClauses ()
    {
      var columnExpression1 = new SqlColumnExpression (typeof (string), "t", "ID", false);
      var orderByClause1 = new Ordering (columnExpression1, OrderingDirection.Asc);
      var columnExpression2 = new SqlColumnExpression (typeof (string), "t", "Name", false);
      var orderByClause2 = new Ordering (columnExpression2, OrderingDirection.Desc);
      var columnExpression3 = new SqlColumnExpression (typeof (string), "t", "City", false);
      var orderByClause3 = new Ordering (columnExpression3, OrderingDirection.Desc);

      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _columnListExpression,
          new[] { _sqlTable },
          new[] { orderByClause1, orderByClause2, orderByClause3 },
          null,
          null,
          false,
          AggregationModifier.None);

      _stageMock.Expect (mock => mock.GenerateTextForOrderByExpression (_commandBuilder, _sqlStatement.Orderings[0].Expression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID]"));
      _stageMock.Expect (mock => mock.GenerateTextForOrderByExpression (_commandBuilder, _sqlStatement.Orderings[1].Expression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[Name]"));
      _stageMock.Expect (mock => mock.GenerateTextForOrderByExpression (_commandBuilder, _sqlStatement.Orderings[2].Expression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[City]"));
      _stageMock.Replay();

      _generator.BuildOrderByPart (_sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" ORDER BY [t].[ID] ASC, [t].[Name] DESC, [t].[City] DESC"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void Build_WithSelectAndFrom ()
    {
      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, _sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Replay();

      _generator.Build (_sqlStatement, _commandBuilder);
      var result = _commandBuilder.GetCommand();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void Build_WithSelectAndNoFrom ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant ("test"));

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.Build (sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] AS [value]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void Build_WithWhereCondition ()
    {
      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _columnListExpression,
          new[] { _sqlTable },
          new Ordering[] { },
          Expression.Constant (true),
          null,
          false,
          AggregationModifier.None);

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Expect (mock => mock.GenerateTextForWhereExpression (_commandBuilder, sqlStatement.WhereCondition))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("(@1 = 1)"));
      _stageMock.Replay();

      _generator.Build (sqlStatement, _commandBuilder);
      var result = _commandBuilder.GetCommand();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] WHERE (@1 = 1)"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void Build_WithOrderByClause ()
    {
      var columnExpression = new SqlColumnExpression (typeof (string), "t", "Name", false);
      var orderByClause = new Ordering (columnExpression, OrderingDirection.Asc);

      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _columnListExpression,
          new[] { _sqlTable },
          new[] { orderByClause },
          null,
          null,
          false,
          AggregationModifier.None);

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, _sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Expect (mock => mock.GenerateTextForOrderByExpression (_commandBuilder, _sqlStatement.Orderings[0].Expression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[Name]"));
      _stageMock.Replay();

      _generator.Build (_sqlStatement, _commandBuilder);
      var result = _commandBuilder.GetCommand();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] ORDER BY [t].[Name] ASC"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    [Ignore ("TODO 2364")]
    public void GenerateSqlGeneratorRegistry ()
    {
      Assert.Fail();
    }
  }
}