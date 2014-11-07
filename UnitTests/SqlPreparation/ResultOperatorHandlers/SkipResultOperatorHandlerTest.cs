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
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class SkipResultOperatorHandlerTest : ResultOperatorHandlerTestBase
  {
    private ISqlPreparationStage _stageMock;
    private ISqlPreparationContext _context;
    private SkipResultOperatorHandler _handler;
    private SqlTable _sqlTable;
    private SqlTableReferenceExpression _selectProjection;
    private Ordering _ordering;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ConstructorInfo _tupleCtor;

    public override void SetUp ()
    {
      base.SetUp();

      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage> ();
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();

      _handler = new SkipResultOperatorHandler ();
      
      _sqlTable = new SqlTable (new UnresolvedTableInfo (typeof (Cook)), JoinSemantics.Inner);
      _selectProjection = new SqlTableReferenceExpression (_sqlTable);

      _ordering = new Ordering (Expression.Constant (7), OrderingDirection.Asc);
      _sqlStatementBuilder = new SqlStatementBuilder
      {
        SelectProjection = _selectProjection,
        DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ())),
        SqlTables = { _sqlTable },
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
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg.Is (_context)))
          .WhenCalled (mi => SqlExpressionTreeComparer.CheckAreEqualTrees (expectedNewProjection, (Expression) mi.Arguments[0]))
          .Return (fakePreparedProjection);
      _stageMock.Replay();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      _stageMock.VerifyAllExpectations();

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      var sqlTable = _sqlStatementBuilder.SqlTables[0];
      Assert.That (sqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (sqlTable.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));

      var subStatementTableInfo = ((ResolvedSubStatementTableInfo) sqlTable.TableInfo);
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
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg.Is (_context)))
          .WhenCalled (mi =>
          {
            var selectProjection = (NewExpression) mi.Arguments[0];
            var rowNumberExpression = (SqlRowNumberExpression) selectProjection.Arguments[1];
            var ordering = rowNumberExpression.Orderings[0];
            SqlExpressionTreeComparer.CheckAreEqualTrees (ordering.Expression, Expression.Constant (1));
          })
          .Return (fakePreparedProjection);
      _stageMock.Replay ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void HandleResultOperator_RemovesOrderingsFromSubStatement_IfNoTopExpression ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));

      _sqlStatementBuilder.Orderings.Add (_ordering);
      Assert.That (_sqlStatementBuilder.TopExpression, Is.Null);
      StubStageMock_PrepareSelectExpression();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      var subStatement = GetSubStatement(_sqlStatementBuilder.SqlTables[0]);
      Assert.That (subStatement.Orderings, Is.Empty);
    }

    [Test]
    public void HandleResultOperator_LeavesOrderingsInSubStatement_IfTopExpression ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));

      _sqlStatementBuilder.Orderings.Add (_ordering);
      _sqlStatementBuilder.TopExpression = Expression.Constant (20);
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      var subStatement = GetSubStatement (_sqlStatementBuilder.SqlTables[0]);
      Assert.That (subStatement.Orderings, Is.Not.Empty);
      Assert.That (subStatement.Orderings, Is.EqualTo (new[] { _ordering }));
    }

    [Test]
    public void HandleResultOperator_CalculatesDataInfo ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      var subStatement = GetSubStatement (_sqlStatementBuilder.SqlTables[0]);
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

      AssertStatementWasMovedToSubStatement (GetSubStatement (_sqlStatementBuilder.SqlTables[0]));
    }

    [Test]
    public void HandleResultOperator_SetsOuterSelect_ToOriginalProjectionSelector ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (0));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      var expectedSelectProjection = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0]), GetTupleProperty ("Key"));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, _sqlStatementBuilder.SelectProjection);
    }

    [Test]
    public void HandleResultOperator_SetsOuterWhereCondition ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      var expectedRowNumberSelector = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0]), 
          GetTupleProperty ("Value"));
      var expectedWhereCondition = Expression.GreaterThan (expectedRowNumberSelector, resultOperator.Count);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedWhereCondition, _sqlStatementBuilder.WhereCondition);
    }

    [Test]
    public void HandleResultOperator_SetsOuterOrdering ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      var expectedRowNumberSelector = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0]),
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

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      Assert.That (_sqlStatementBuilder.DataInfo, Is.SameAs (originalDataInfo));
    }

    [Test]
    public void HandleResultOperator_SetsRowNumberSelector ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      var expectedRowNumberSelector = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0]),
          GetTupleProperty ("Value"));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedRowNumberSelector, _sqlStatementBuilder.RowNumberSelector);
    }

    [Test]
    public void HandleResultOperator_SetsCurrentRowNumberOffset ()
    {
      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      Assert.That (_sqlStatementBuilder.CurrentRowNumberOffset, Is.SameAs (resultOperator.Count));
    }

    [Test]
    public void HandleResultOperator_AddsMappingForItemExpression ()
    {
      var originalItemExpression = ((StreamedSequenceInfo) _sqlStatementBuilder.DataInfo).ItemExpression;

      var resultOperator = new SkipResultOperator (Expression.Constant (10));
      StubStageMock_PrepareSelectExpression ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stageMock, _context);

      Assert.That (_context.GetExpressionMapping (originalItemExpression), Is.EqualTo (_sqlStatementBuilder.SelectProjection));
    }

    private void StubStageMock_PrepareSelectExpression ()
    {
      var fakePreparedProjection = GetFakePreparedProjection ();
      _stageMock
          .Stub (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg.Is (_context)))
          .Return (fakePreparedProjection);
      _stageMock.Replay ();
    }

    private NewExpression GetFakePreparedProjection ()
    {
      return Expression.New (
          _tupleCtor,
          new Expression[] { _selectProjection, new SqlRowNumberExpression (new[] { _ordering }) },
          new MemberInfo[] { GetTupleMethod ("get_Key"), GetTupleMethod ("get_Value") });
    }

    private SqlStatement GetSubStatement (SqlTableBase sqlTableBase)
    {
      return ((ResolvedSubStatementTableInfo) ((SqlTable) sqlTableBase).TableInfo).SqlStatement;
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