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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class SkipResultOperatorHandlerTest : ResultOperatorHandlerTestBase
  {
    private Mock<ISqlPreparationStage> _stageMock;
    private ISqlPreparationContext _context;
    private SkipResultOperatorHandler _handler;
    private SqlTableReferenceExpression _selectProjection;
    private Ordering _ordering;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ConstructorInfo _tupleCtor;

    public override void SetUp ()
    {
      base.SetUp();

      _stageMock = new Mock<ISqlPreparationStage>();
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();

      _handler = new SkipResultOperatorHandler ();

      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      _selectProjection = new SqlTableReferenceExpression (sqlTable);

      _ordering = new Ordering (Expression.Constant (7), OrderingDirection.Asc);
      _sqlStatementBuilder = new SqlStatementBuilder
      {
        SelectProjection = _selectProjection,
        DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ())),
        SqlTables = { SqlStatementModelObjectMother.CreateSqlAppendedTable(sqlTable) },
      };

      _tupleCtor = _tupleCtor = typeof (KeyValuePair<Cook, int>).GetConstructor (new[] { typeof (Cook), typeof (int) });
    }

    [Test]
    public void HandleResultOperator_CreatesAndPreparesSubStatementWithNewProjection ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));

      _sqlStatementBuilder.Orderings.Add (_ordering);

      var expectedNewProjection = Expression.New (
          _tupleCtor, 
          new Expression[] { _selectProjection, new SqlRowNumberExpression (new[] { _ordering }) }, 
          new MemberInfo[] { GetTupleMethod ("get_Key"), GetTupleMethod ("get_Value") });

      var fakePreparedProjection = GetFakePreparedProjection();

      _stageMock
          .Setup (mock => mock.PrepareSelectExpression (It.IsAny<Expression>(), _context))
          .Callback ((Expression expression, ISqlPreparationContext context) => SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNewProjection, expression))
          .Returns (fakePreparedProjection)
          .Verifiable();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      _stageMock.Verify();

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      var sqlTable = _sqlStatementBuilder.SqlTables[0];
      Assert.That (sqlTable.SqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (sqlTable.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));

      var subStatementTableInfo = ((ResolvedSubStatementTableInfo) sqlTable.SqlTable.TableInfo);
      Assert.That (subStatementTableInfo.SqlStatement.SelectProjection, Is.SameAs (fakePreparedProjection));
      Assert.That (subStatementTableInfo.TableAlias, Is.EqualTo ("q0"));
    }

    [Test]
    public void HandleResultOperator_CreatesAndPreparesSubStatementWithNewProjection_WithoutOrderings ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));

      Assert.That (_sqlStatementBuilder.Orderings, Is.Empty);

      var fakePreparedProjection = GetFakePreparedProjection();

      _stageMock
          .Setup (mock => mock.PrepareSelectExpression (It.IsAny<Expression>(), _context))
          .Callback (
              (Expression expression, ISqlPreparationContext context) =>
              {
                var selectProjection = (NewExpression) expression;
                var rowNumberExpression = (SqlRowNumberExpression) selectProjection.Arguments[1];
                var ordering = rowNumberExpression.Orderings[0];
                SqlExpressionTreeComparer.CheckAreEqualTrees (ordering.Expression, Expression.Constant (1));
              })
          .Returns (fakePreparedProjection)
          .Verifiable();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      _stageMock.Verify();
    }

    [Test]
    public void HandleResultOperator_RemovesOrderingsFromSubStatement_IfNoTopExpression ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));

      _sqlStatementBuilder.Orderings.Add (_ordering);
      Assert.That (_sqlStatementBuilder.TopExpression, Is.Null);
      StubStageMock_PrepareSelectExpression();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      var subStatement = GetSubStatement(_sqlStatementBuilder.SqlTables[0].SqlTable);
      Assert.That (subStatement.Orderings, Is.Empty);
    }

    [Test]
    public void HandleResultOperator_LeavesOrderingsInSubStatement_IfTopExpression ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));

      _sqlStatementBuilder.Orderings.Add (_ordering);
      _sqlStatementBuilder.TopExpression = Expression.Constant (20);
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      var subStatement = GetSubStatement (_sqlStatementBuilder.SqlTables[0].SqlTable);
      Assert.That (subStatement.Orderings, Is.Not.Empty);
      Assert.That (subStatement.Orderings, Is.EqualTo (new[] { _ordering }));
    }

    [Test]
    public void HandleResultOperator_CalculatesDataInfo ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      var subStatement = GetSubStatement (_sqlStatementBuilder.SqlTables[0].SqlTable);
      Assert.That (subStatement.DataInfo, Is.InstanceOf (typeof (StreamedSequenceInfo)));
      Assert.That (subStatement.DataInfo.DataType, Is.EqualTo(typeof (IQueryable<KeyValuePair<Cook, int>>)));
    }

    [Test]
    public void HandleResultOperator_SkipAfterSetOperations_MovesStatementToSubStatement ()
    {
      _sqlStatementBuilder.SetOperationCombinedStatements.Add (SqlStatementModelObjectMother.CreateSetOperationCombinedStatement());

      var resultOperator = new SkipResultOperator (Expression.Constant (0));

      var stage = CreateDefaultSqlPreparationStage();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, stage, _context);

      AssertStatementWasMovedToSubStatement (GetSubStatement (_sqlStatementBuilder.SqlTables[0].SqlTable));
    }

    [Test]
    public void HandleResultOperator_SetsOuterSelect_ToOriginalProjectionSelector ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      var expectedSelectProjection = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0].SqlTable), GetTupleProperty ("Key"));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, _sqlStatementBuilder.SelectProjection);
    }

    [Test]
    public void HandleResultOperator_SetsOuterWhereCondition ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      var expectedRowNumberSelector = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0].SqlTable),
          GetTupleProperty ("Value"));
      var expectedWhereCondition = Expression.GreaterThan (expectedRowNumberSelector, resultOperator.Count);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedWhereCondition, _sqlStatementBuilder.WhereCondition);
    }

    [Test]
    public void HandleResultOperator_SetsOuterOrdering ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      var expectedRowNumberSelector = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0].SqlTable),
          GetTupleProperty ("Value"));

      Assert.That (_sqlStatementBuilder.Orderings.Count, Is.EqualTo (1));
      Assert.That (_sqlStatementBuilder.Orderings[0].OrderingDirection, Is.EqualTo (OrderingDirection.Asc));

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedRowNumberSelector, _sqlStatementBuilder.Orderings[0].Expression);
    }

    [Test]
    public void HandleResultOperator_SetsOuterDataInfo ()
    {
      var originalDataInfo = _sqlStatementBuilder.DataInfo;

      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      Assert.That (_sqlStatementBuilder.DataInfo, Is.SameAs (originalDataInfo));
    }

    [Test]
    public void HandleResultOperator_SetsRowNumberSelector ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      var expectedRowNumberSelector = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0].SqlTable),
          GetTupleProperty ("Value"));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedRowNumberSelector, _sqlStatementBuilder.RowNumberSelector);
    }

    [Test]
    public void HandleResultOperator_SetsCurrentRowNumberOffset ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      Assert.That (_sqlStatementBuilder.CurrentRowNumberOffset, Is.SameAs (resultOperator.Count));
    }

    [Test]
    public void HandleResultOperator_AddsMappingForItemExpression ()
    {
      var originalItemExpression = ((StreamedSequenceInfo) _sqlStatementBuilder.DataInfo).ItemExpression;

      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock.Object, _context);

      Assert.That (_context.GetExpressionMapping (originalItemExpression), Is.EqualTo (_sqlStatementBuilder.SelectProjection));
    }

    private void StubStageMock_PrepareSelectExpression ()
    {
      var fakePreparedProjection = GetFakePreparedProjection ();
      _stageMock
          .Setup (mock => mock.PrepareSelectExpression (It.IsAny<Expression>(), _context))
          .Returns (fakePreparedProjection);
    }

    private NewExpression GetFakePreparedProjection ()
    {
      return Expression.New (
          _tupleCtor,
          new Expression[] { _selectProjection, new SqlRowNumberExpression (new[] { _ordering }) },
          new MemberInfo[] { GetTupleMethod ("get_Key"), GetTupleMethod ("get_Value") });
    }

    private SqlStatement GetSubStatement (SqlTable sqlTableBase)
    {
      return ((ResolvedSubStatementTableInfo) sqlTableBase.TableInfo).SqlStatement;
    }

    private MethodInfo GetTupleMethod(string name)
    {
      Debug.Assert (_tupleCtor.DeclaringType != null);
      return _tupleCtor.DeclaringType.GetMethod(name);
    }

    private PropertyInfo GetTupleProperty(string name)
    {
      Debug.Assert (_tupleCtor.DeclaringType != null);
      return _tupleCtor.DeclaringType.GetProperty(name);
    }
  }
}