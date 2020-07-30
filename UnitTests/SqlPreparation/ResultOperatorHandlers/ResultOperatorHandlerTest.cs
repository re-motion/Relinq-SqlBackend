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
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Remotion.Linq.SqlBackend.UnitTests.Utilities;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class ResultOperatorHandlerTest
  {
    private TestableResultOperatorHandler _handler;
    private TestChoiceResultOperator _resultOperator;
    private SqlStatementBuilder _statementBuilder;
    private UniqueIdentifierGenerator _generator;
    private Mock<ISqlPreparationStage> _stageMock;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _handler = new TestableResultOperatorHandler();
      _resultOperator = new TestChoiceResultOperator (false);
      _statementBuilder = new SqlStatementBuilder();
      _statementBuilder.SelectProjection = Expression.Constant ("select");
      _statementBuilder.DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()));
      _generator = new UniqueIdentifierGenerator();
      _stageMock = new Mock<ISqlPreparationStage>();
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void MoveCurrentStatementToSqlTable ()
    {
      var originalStatement = _statementBuilder.GetSqlStatement ();

      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo(new Ordering[0]);
      Func<ITableInfo, SqlTable> tableGenerator = info => new SqlTable (info, JoinSemantics.Inner);

      var someOrderingExtractionPolicy = Some.Item (
          OrderingExtractionPolicy.DoNotExtractOrderings,
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.IsAny<SqlSubStatementExpression>(),
                  _context,
                  tableGenerator,
                  someOrderingExtractionPolicy))
          .Returns (fakeFromExpressionInfo)
          .Callback (
              (Expression expression, ISqlPreparationContext sqlPreparationContext, Func<ITableInfo, SqlTable> tableGeneratorArg, OrderingExtractionPolicy orderingExtractionPolicy) =>
              {
                var sqlStatement = ((SqlSubStatementExpression) expression).SqlStatement;
                SqlExpressionTreeComparer.CheckAreEqualTrees (new NamedExpression (null, originalStatement.SelectProjection), sqlStatement.SelectProjection);

                Assert.That (sqlStatement.DataInfo, Is.SameAs (originalStatement.DataInfo));
                Assert.That (sqlStatement.WhereCondition, Is.SameAs (originalStatement.WhereCondition));
              })
          .Verifiable();

      _handler.MoveCurrentStatementToSqlTable (_statementBuilder, _context, tableGenerator, _stageMock.Object, someOrderingExtractionPolicy);

      _stageMock.Verify();

      Assert.That (_statementBuilder.DataInfo, Is.SameAs (originalStatement.DataInfo));
      Assert.That (_statementBuilder.SqlTables[0], Is.SameAs (fakeFromExpressionInfo.SqlTable));
      Assert.That (_statementBuilder.SelectProjection, Is.SameAs (fakeFromExpressionInfo.ItemSelector));

      var mappedItemExpression = _context.GetExpressionMapping (((StreamedSequenceInfo) originalStatement.DataInfo).ItemExpression);
      Assert.That (mappedItemExpression, Is.Not.Null);
      Assert.That (mappedItemExpression, Is.SameAs (fakeFromExpressionInfo.ItemSelector));
    }

    [Test]
    public void MoveCurrentStatementToSqlTable_WithOrderings ()
    {
      var ordering = new Ordering (Expression.Constant ("order1"), OrderingDirection.Desc);

      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo (new[] { ordering });

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.IsAny<Expression>(),
                  It.IsAny<ISqlPreparationContext>(),
                  It.IsAny<Func<ITableInfo, SqlTable>>(),
                  It.Is<OrderingExtractionPolicy> (param => param == OrderingExtractionPolicy.ExtractOrderingsIntoProjection)))
          .Returns (fakeFromExpressionInfo)
          .Verifiable();

      _handler.MoveCurrentStatementToSqlTable (
          _statementBuilder,
          _context,
          info => new SqlTable (info, JoinSemantics.Inner),
          _stageMock.Object,
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      Assert.That (_statementBuilder.Orderings[0], Is.SameAs (ordering));
    }

    [Test]
    public void EnsureNoTopExpression_WithTopExpression ()
    {
      _statementBuilder.TopExpression = Expression.Constant ("top");
      var originalStatement = _statementBuilder.GetSqlStatement();
      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo(new Ordering[0]);

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.IsAny<Expression>(),
                  It.IsAny<ISqlPreparationContext>(),
                  It.IsAny<Func<ITableInfo, SqlTable>>(),
                  It.Is<OrderingExtractionPolicy> (param => param == OrderingExtractionPolicy.ExtractOrderingsIntoProjection)))
          .Returns (fakeFromExpressionInfo)
          .Verifiable();

      _handler.EnsureNoTopExpression (_statementBuilder, _generator, _stageMock.Object, _context);

      _stageMock.Verify();
      Assert.That (_statementBuilder.GetSqlStatement(), Is.Not.EqualTo (originalStatement));
      Assert.That (_statementBuilder.TopExpression, Is.Null);
      
      var sqlTable = _statementBuilder.SqlTables[0];
      Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement, Is.EqualTo (originalStatement));
      Assert.That (sqlTable.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
    }

    [Test]
    public void EnsureNoTopExpressionAndSetDataInfo_WithoutTopExpression ()
    {
      var sqlStatement = _statementBuilder.GetSqlStatement();

      _handler.EnsureNoTopExpression (_statementBuilder, _generator, _stageMock.Object, _context);

      Assert.That (sqlStatement, Is.EqualTo (_statementBuilder.GetSqlStatement()));
    }

    [Test]
    public void EnsureNoGroupExpression_WithoutGroupExpression ()
    {
      var sqlStatement = _statementBuilder.GetSqlStatement ();

      _handler.EnsureNoGroupExpression (_statementBuilder, _generator, _stageMock.Object, _context);

      Assert.That (sqlStatement, Is.EqualTo (_statementBuilder.GetSqlStatement ()));
    }

    [Test]
    public void EnsureNoGroupExpression_WithGroupExpression ()
    {
      _statementBuilder.GroupByExpression = Expression.Constant ("top");
      var originalStatement = _statementBuilder.GetSqlStatement ();
      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo (new Ordering[0]);

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.IsAny<Expression>(),
                  It.IsAny<ISqlPreparationContext>(),
                  It.IsAny<Func<ITableInfo, SqlTable>>(),
                  It.Is<OrderingExtractionPolicy> (param => param == OrderingExtractionPolicy.ExtractOrderingsIntoProjection)))
          .Returns (fakeFromExpressionInfo)
          .Verifiable();

      _handler.EnsureNoGroupExpression (_statementBuilder, _generator, _stageMock.Object, _context);

      _stageMock.Verify();
      Assert.That (_statementBuilder.GetSqlStatement(), Is.Not.EqualTo (originalStatement));
      Assert.That (_statementBuilder.GroupByExpression, Is.Null);

      var sqlTable = _statementBuilder.SqlTables[0];
      Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement, Is.EqualTo (originalStatement));
      Assert.That (sqlTable.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
    }

    [Test]
    public void EnsureNoDistinctQuery_WithDistinctQuery ()
    {
      _statementBuilder.IsDistinctQuery = true;
      var originalStatement = _statementBuilder.GetSqlStatement ();
      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo (new Ordering[0]);

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.IsAny<Expression>(),
                  It.IsAny<ISqlPreparationContext>(),
                  It.IsAny<Func<ITableInfo, SqlTable>>(),
                  It.Is<OrderingExtractionPolicy> (param => param == OrderingExtractionPolicy.ExtractOrderingsIntoProjection)))
          .Returns (fakeFromExpressionInfo)
          .Verifiable();

      _handler.EnsureNoDistinctQuery (_statementBuilder, _generator, _stageMock.Object, _context);

      _stageMock.Verify();
      Assert.That (_statementBuilder.GetSqlStatement(), Is.Not.EqualTo (originalStatement));
      Assert.That (_statementBuilder.IsDistinctQuery, Is.False);

      var sqlTable = _statementBuilder.SqlTables[0];
      Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement, Is.EqualTo (originalStatement));
      Assert.That (sqlTable.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
    }

    [Test]
    public void EnsureNoDistinctQuery_NoDistinctQuery ()
    {
      _statementBuilder.IsDistinctQuery = false;
      var sqlStatement = _statementBuilder.GetSqlStatement ();

      _handler.EnsureNoDistinctQuery(_statementBuilder, _generator, _stageMock.Object, _context);

      Assert.That (sqlStatement, Is.EqualTo (_statementBuilder.GetSqlStatement ()));
    }

    [Test]
    public void EnsureNoSetOperations_WithSetOperations ()
    {
      _statementBuilder.SetOperationCombinedStatements.Add(SqlStatementModelObjectMother.CreateSetOperationCombinedStatement());
      
      var originalStatement = _statementBuilder.GetSqlStatement ();
      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo (new Ordering[0]);

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.IsAny<Expression>(),
                  It.IsAny<ISqlPreparationContext>(),
                  It.IsAny<Func<ITableInfo, SqlTable>>(),
                  It.Is<OrderingExtractionPolicy> (param => param == OrderingExtractionPolicy.ExtractOrderingsIntoProjection)))
          .Returns (fakeFromExpressionInfo)
          .Verifiable();

      _handler.EnsureNoSetOperations (_statementBuilder, _generator, _stageMock.Object, _context);

      _stageMock.Verify();
      Assert.That (_statementBuilder.GetSqlStatement(), Is.Not.EqualTo (originalStatement));
      Assert.That (_statementBuilder.SetOperationCombinedStatements, Is.Empty);

      var sqlTable = _statementBuilder.SqlTables[0];
      Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement, Is.EqualTo (originalStatement));
      Assert.That (sqlTable.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
    }

    [Test]
    public void EnsureNoSetOperations_NoSetOperations ()
    {
      var sqlStatement = _statementBuilder.GetSqlStatement ();

      _handler.EnsureNoSetOperations(_statementBuilder, _generator, _stageMock.Object, _context);

      Assert.That (sqlStatement, Is.EqualTo (_statementBuilder.GetSqlStatement ()));
    }

    [Test]
    public void UpdateDataInfo ()
    {
      var streamDataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()));

      _handler.UpdateDataInfo (_resultOperator, _statementBuilder, streamDataInfo);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
    }

    private FromExpressionInfo CreateFakeFromExpressionInfo (Ordering[] extractedOrderings)
    {
      return new FromExpressionInfo (
          new SqlTable (new ResolvedSubStatementTableInfo("sc", _statementBuilder.GetSqlStatement()), JoinSemantics.Inner),
          extractedOrderings,
          Expression.Constant (0),
          null);
    }

  }
}