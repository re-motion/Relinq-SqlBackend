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
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.ResultOperators;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class ResultOperatorHandlerTest
  {
    private TestableResultOperatorHandler _handler;
    private TestChoiceResultOperator _resultOperator;
    private SqlStatementBuilder _statementBuilder;
    private UniqueIdentifierGenerator _generator;
    private ISqlPreparationStage _stageMock;
    private SqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _handler = new TestableResultOperatorHandler();
      _resultOperator = new TestChoiceResultOperator (false);
      _statementBuilder = new SqlStatementBuilder();
      _statementBuilder.SelectProjection = Expression.Constant ("select");
      _statementBuilder.DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()));
      _generator = new UniqueIdentifierGenerator();
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _context = new SqlPreparationContext();
    }

    [Test]
    public void MoveCurrentStatementToSqlTable ()
    {
      var originalStatement = _statementBuilder.GetSqlStatement ();

      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo(new Ordering[0]);
      Func<ITableInfo, SqlTableBase> tableGenerator = info => new SqlTable (info, JoinSemantics.Inner);

      _stageMock
          .Expect (mock => mock.PrepareFromExpression (
              Arg<Expression>.Matches (expr => IsSubStatementExpression(expr, originalStatement)),
              Arg.Is (_context),
              Arg.Is (tableGenerator)))
          .Return (fakeFromExpressionInfo);
      _stageMock.Replay ();

      _handler.MoveCurrentStatementToSqlTable (_statementBuilder, _generator, _context, tableGenerator, _stageMock);

      _stageMock.VerifyAllExpectations();

      Assert.That (_statementBuilder.DataInfo, Is.SameAs (originalStatement.DataInfo));
      Assert.That (_statementBuilder.SqlTables[0], Is.SameAs (fakeFromExpressionInfo.SqlTable));
      Assert.That (_statementBuilder.SelectProjection, Is.TypeOf(typeof(NamedExpression)));
      Assert.That (((NamedExpression) _statementBuilder.SelectProjection).Expression, Is.SameAs(fakeFromExpressionInfo.ItemSelector));

      var mappedItemExpression = _context.TryGetExpressionMappingFromHierarchy (((StreamedSequenceInfo) originalStatement.DataInfo).ItemExpression);
      Assert.That (mappedItemExpression, Is.Not.Null);
      Assert.That (mappedItemExpression, Is.SameAs (fakeFromExpressionInfo.ItemSelector));
    }

    [Test]
    public void MoveCurrentStatementToSqlTable_WithOrderings ()
    {
      var ordering = new Ordering (Expression.Constant ("order1"), OrderingDirection.Desc);

      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo (new[] { ordering });

      _stageMock
          .Expect (
              mock => mock.PrepareFromExpression (Arg<Expression>.Is.Anything, Arg<ISqlPreparationContext>.Is.Anything, Arg<Func<ITableInfo, SqlTableBase>>.Is.Anything))
          .Return (fakeFromExpressionInfo);
      _stageMock.Replay ();

      _handler.MoveCurrentStatementToSqlTable (_statementBuilder, _generator, _context, info => new SqlTable (info, JoinSemantics.Inner), _stageMock);

      Assert.That (_statementBuilder.Orderings[0], Is.SameAs (ordering));
    }

    [Test]
    public void EnsureNoTopExpression_WithTopExpression ()
    {
      _statementBuilder.TopExpression = Expression.Constant ("top");
      var originalStatement = _statementBuilder.GetSqlStatement();
      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo(new Ordering[0]);

      _stageMock
          .Expect (
              mock => mock.PrepareFromExpression (Arg<Expression>.Is.Anything, Arg<ISqlPreparationContext>.Is.Anything, Arg<Func<ITableInfo, SqlTableBase>>.Is.Anything))
          .Return (fakeFromExpressionInfo);
      _stageMock.Replay ();

      _handler.EnsureNoTopExpression (_statementBuilder, _generator, _stageMock, _context);

      _stageMock.VerifyAllExpectations();
      Assert.That (originalStatement, Is.Not.EqualTo (_statementBuilder.GetSqlStatement()));
      Assert.That (
          originalStatement, Is.EqualTo (((ResolvedSubStatementTableInfo) ((SqlTable) _statementBuilder.SqlTables[0]).TableInfo).SqlStatement));
    }

    [Test]
    public void EnsureNoTopExpressionAndSetDataInfo_WithoutTopExpression ()
    {
      var sqlStatement = _statementBuilder.GetSqlStatement();

      _handler.EnsureNoTopExpression (_statementBuilder, _generator, _stageMock, _context);

      Assert.That (sqlStatement, Is.EqualTo (_statementBuilder.GetSqlStatement()));
    }

    [Test]
    public void EnsureNoTopExpression ()
    {
      _statementBuilder.GroupByExpression = Expression.Constant ("top");
      var originalStatement = _statementBuilder.GetSqlStatement ();
      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo (new Ordering[0]);

      _stageMock
          .Expect (
              mock => mock.PrepareFromExpression (Arg<Expression>.Is.Anything, Arg<ISqlPreparationContext>.Is.Anything, Arg<Func<ITableInfo, SqlTableBase>>.Is.Anything))
          .Return (fakeFromExpressionInfo);
      _stageMock.Replay ();

      _handler.EnsureNoGroupExpression (_statementBuilder, _generator, _stageMock, _context);

      _stageMock.VerifyAllExpectations ();
      Assert.That (originalStatement, Is.Not.EqualTo (_statementBuilder.GetSqlStatement ()));
    }

    [Test]
    public void SetDataInfo_WithoutGroupExpression ()
    {
      var sqlStatement = _statementBuilder.GetSqlStatement ();

      _handler.EnsureNoGroupExpression (_statementBuilder, _generator, _stageMock, _context);

      Assert.That (sqlStatement, Is.EqualTo (_statementBuilder.GetSqlStatement ()));
    }

    [Test]
    public void EnsureNoDistinctQuery_DistinctQuery ()
    {
      _statementBuilder.IsDistinctQuery = true;
      var sqlStatement = _statementBuilder.GetSqlStatement ();

      var fakeFromExpressionInfo = CreateFakeFromExpressionInfo(new Ordering[0]);

      _stageMock
          .Expect (
              mock => mock.PrepareFromExpression (Arg<Expression>.Is.Anything, Arg<ISqlPreparationContext>.Is.Anything, Arg<Func<ITableInfo, SqlTableBase>>.Is.Anything))
          .Return (fakeFromExpressionInfo);
      _stageMock.Replay ();

      _handler.EnsureNoDistinctQuery(_statementBuilder, _generator, _stageMock, _context);

      _stageMock.VerifyAllExpectations();
      Assert.That (sqlStatement, Is.Not.EqualTo (_statementBuilder.GetSqlStatement ()));
    }

    [Test]
    public void EnsureDistinctQuery_NoDistinctQuery ()
    {
      _statementBuilder.IsDistinctQuery = false;
      var sqlStatement = _statementBuilder.GetSqlStatement ();

      _handler.EnsureNoDistinctQuery(_statementBuilder, _generator, _stageMock, _context);

      Assert.That (sqlStatement, Is.EqualTo (_statementBuilder.GetSqlStatement ()));
    }

    [Test]
    public void UpdateDataInfo ()
    {
      var streamDataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()));

      _handler.UpdateDataInfo (_resultOperator, _statementBuilder, streamDataInfo);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
    }

    private bool IsSubStatementExpression (Expression expr, SqlStatement originalStatement)
    {
      return expr is SqlSubStatementExpression
             && ((SqlSubStatementExpression) expr).SqlStatement.Equals (originalStatement);
    }

    private FromExpressionInfo CreateFakeFromExpressionInfo (Ordering[] extractedOrderings)
    {
      return new FromExpressionInfo (
          new SqlTable (new ResolvedSubStatementTableInfo("sc", _statementBuilder.GetSqlStatement()), JoinSemantics.Inner),
          extractedOrderings,
          Expression.Constant (0),
          null,
          false);
    }

  }
}