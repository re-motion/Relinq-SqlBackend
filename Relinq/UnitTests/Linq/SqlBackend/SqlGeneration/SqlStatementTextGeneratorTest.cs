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
using Remotion.Linq.UnitTests.Linq.Core.Clauses.StreamedData;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.Clauses;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlStatementTextGeneratorTest
  {
    private SqlStatement _sqlStatement;
    private TestableSqlStatementTextGenerator _generator;
    private SqlCommandBuilder _commandBuilder;
    private ISqlGenerationStage _stageMock;
    private SqlEntityExpression _entityExpression;
    private SqlTable _sqlTable;

    [SetUp]
    public void SetUp ()
    {
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo();
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "t", "ID", true);
      _entityExpression = new SqlEntityDefinitionExpression (
          typeof(string),
          "t", null,
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnDefinitionExpression (typeof (int), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (int), "t", "City", false)
          });

      _stageMock = MockRepository.GenerateStrictMock<ISqlGenerationStage>();
      _generator = new TestableSqlStatementTextGenerator (_stageMock);
      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _entityExpression,
          new[] { _sqlTable }, null, null, new Ordering[] { }, null, false, null, null);
      _commandBuilder = new SqlCommandBuilder();
    }

    [Test]
    public void BuildSelectPart_WithSelect ()
    {
      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (_sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildSelectPart_NoEntityExpression ()
    {
      var sqlStatement = new SqlStatementBuilder { SelectProjection = new SqlColumnDefinitionExpression (typeof (string), "t", "FirstName", false), DataInfo = new TestStreamedValueInfo (typeof (object)) }.GetSqlStatement ();
      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[FirstName]"));
      _stageMock.Replay();

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[FirstName]"));
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
    public void BuildSelectPart_WithAggregation ()
    {
      var aggregationExpression = new AggregationExpression (typeof(int), _entityExpression,AggregationModifier.Count);
      var sqlStatement = new SqlStatementBuilder { SelectProjection = aggregationExpression, DataInfo = new TestStreamedValueInfo (typeof (object)) }.GetSqlStatement ();

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
      var sqlStatement =
          new SqlStatementBuilder
          { DataInfo = new TestStreamedValueInfo (typeof (int)), SelectProjection = _entityExpression, IsDistinctQuery = true }.GetSqlStatement();

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
      var sqlStatement =
          new SqlStatementBuilder
          { DataInfo = new TestStreamedValueInfo (typeof (int)), SelectProjection = _entityExpression, TopExpression = Expression.Constant (1) }.
              GetSqlStatement();

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
      var sqlStatement =
          new SqlStatementBuilder
          {
              DataInfo = new TestStreamedValueInfo (typeof (int)),
              SelectProjection = _entityExpression,
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

      _generator.BuildSelectPart (sqlStatement, _commandBuilder, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT DISTINCT TOP (@1) [t].[ID],[t].[Name],[t].[City]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildWhere_WithSingleWhereCondition ()
    {
      var sqlStatement = 
          new SqlStatementBuilder
          {
              DataInfo = new TestStreamedValueInfo (typeof (int)),
              SelectProjection = _entityExpression,
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
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "t", "Name", false);
      var orderByClause = new Ordering (columnExpression, OrderingDirection.Asc);

      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _entityExpression,
          new[] { _sqlTable }, null, null, new[] { orderByClause }, null, false, null, null);

      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, orderByClause))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[Name] ASC"));
      _stageMock.Replay();

      _generator.BuildOrderByPart (_sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" ORDER BY [t].[Name] ASC"));
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

      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _entityExpression,
          new[] { _sqlTable }, null, null, new[] { orderByClause1, orderByClause2, orderByClause3 }, null, false, null, null);

      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, _sqlStatement.Orderings[0]))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID] ASC"));
      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, _sqlStatement.Orderings[1]))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[Name] DESC"));
      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, _sqlStatement.Orderings[2]))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[City] DESC"));
      _stageMock.Replay();

      _generator.BuildOrderByPart (_sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" ORDER BY [t].[ID] ASC, [t].[Name] DESC, [t].[City] DESC"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void BuildDistinctPart ()
    {
      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _entityExpression,
          new[] { _sqlTable }, null, null, new Ordering[] { }, null, true, null, null);

      _generator.BuildDistinctPart (_sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("DISTINCT "));
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void BuildTopPart ()
    {
      var topExpression = Expression.Constant("top");
      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _entityExpression,
          new[] { _sqlTable }, null, null, new Ordering[] { }, topExpression, false, null, null);

      _stageMock.Expect (mock => mock.GenerateTextForTopExpression (_commandBuilder, topExpression))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("top"));
      _stageMock.Replay ();

      _generator.BuildTopPart (_sqlStatement, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("TOP (top) "));
      _stageMock.VerifyAllExpectations ();
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

      _generator.Build (_sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand ();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void Build_WithSelectAndNoFrom ()
    {
      var sqlStatement = new SqlStatementBuilder { SelectProjection = Expression.Constant (null), DataInfo = new TestStreamedValueInfo (typeof (object)) }.GetSqlStatement ();

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

      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _entityExpression,
          new[] { _sqlTable }, null, sqlGroupExpression, new Ordering[] { }, null, false, null, null);

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
      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _entityExpression,
          new[] { _sqlTable }, Expression.Constant (true), null, new Ordering[] { }, null, false, null, null);

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
      var orderByClause = new Ordering (columnExpression, OrderingDirection.Asc);

      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          _entityExpression,
          new[] { _sqlTable }, null, null, new[] { orderByClause }, null, false, null, null);

      _stageMock.Expect (
          mock => mock.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[ID],[t].[Name],[t].[City]"));
      _stageMock.Expect (mock => mock.GenerateTextForFromTable (_commandBuilder, _sqlStatement.SqlTables[0], true))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[Table] AS [t]"));
      _stageMock.Expect (mock => mock.GenerateTextForOrdering (_commandBuilder, _sqlStatement.Orderings[0]))
          .WhenCalled (mi => ((SqlCommandBuilder) mi.Arguments[0]).Append ("[t].[Name] ASC"));
      _stageMock.Replay();

      _generator.Build (_sqlStatement, _commandBuilder, false);

      _commandBuilder.SetInMemoryProjectionBody (Expression.Constant (0));
      var result = _commandBuilder.GetCommand();

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] ORDER BY [t].[Name] ASC"));
      _stageMock.VerifyAllExpectations();
    }
  }
}