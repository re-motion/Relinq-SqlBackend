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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class GroupResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private GroupResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator();
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _handler = new GroupResultOperatorHandler();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement())
                             {
                                 DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                             };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }


    [Test]
    public void HandleResultOperator ()
    {
      var keySelector = new SqlColumnDefinitionExpression(typeof(string), "c", "Name", false);
      var elementSelector = Expression.Constant ("elementSelector");
      var resultOperator = new GroupResultOperator ("itemName", keySelector, elementSelector);

      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (keySelector, _context))
          .Return (keySelector);
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (elementSelector, _context))
          .Return (elementSelector);
      _stageMock.Replay();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      _stageMock.VerifyAllExpectations();
      Assert.That (_sqlStatementBuilder.GroupByExpression, Is.SameAs (keySelector));
      
      var expectedSelectProjection = new SqlGroupingSelectExpression (
          new NamedExpression ("key", keySelector), 
          new NamedExpression ("element", elementSelector));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, _sqlStatementBuilder.SelectProjection);

      Assert.That (
          _sqlStatementBuilder.DataInfo.DataType,
          Is.EqualTo (typeof (IQueryable<>).MakeGenericType (typeof (IGrouping<,>).MakeGenericType (typeof (string), typeof (string)))));
    }

    [Test]
    public void HandleResultOperator_GroupByAfterTopExpression ()
    {
      var topExpression = Expression.Constant ("top");
      _sqlStatementBuilder.TopExpression = topExpression;

      var keySelector = new SqlColumnDefinitionExpression(typeof(string), "c", "Name", false);
      var elementSelector = Expression.Constant ("elementSelector");
      var resultOperator = new GroupResultOperator ("itemName", keySelector, elementSelector);
      
      var originalStatement = _sqlStatementBuilder.GetSqlStatement ();
      var originalDataInfo = _sqlStatementBuilder.DataInfo;

      var fakeFromExpressionInfo = new FromExpressionInfo (
          new SqlTable (new ResolvedSubStatementTableInfo("sc", originalStatement), JoinSemantics.Inner), new Ordering[0], elementSelector, null);
      
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (keySelector, _context))
          .Return (keySelector);
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (elementSelector, _context))
          .Return (elementSelector);
      _stageMock
          .Expect (
              mock => mock.PrepareFromExpression (Arg<Expression>.Is.Anything, Arg.Is (_context), Arg<Func<ITableInfo, SqlTableBase>>.Is.Anything))
          .Return (fakeFromExpressionInfo)
          .WhenCalled (mi =>
          {
            var fromExpression = (Expression) mi.Arguments[0];
            CheckExpressionMovedIntoSubStatement (fromExpression, originalStatement);
          });
      
      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      _stageMock.VerifyAllExpectations();
      Assert.That (_sqlStatementBuilder.GroupByExpression, Is.SameAs (keySelector));
      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SqlTables[0], Is.SameAs (fakeFromExpressionInfo.SqlTable));

      Assert.That (_sqlStatementBuilder.DataInfo, Is.Not.SameAs (originalDataInfo));
      Assert.That (
          _sqlStatementBuilder.DataInfo.DataType,
          Is.EqualTo (typeof (IQueryable<>).MakeGenericType (typeof (IGrouping<,>).MakeGenericType (typeof (string), typeof (string)))));
    }

    [Test]
    public void HandleResultOperator_GroupByAfterDistinct ()
    {
      _sqlStatementBuilder.IsDistinctQuery = true;

      var keySelector = new SqlColumnDefinitionExpression(typeof(string), "c", "Name", false);
      var elementSelector = Expression.Constant ("elementSelector");
      var resultOperator = new GroupResultOperator ("itemName", keySelector, elementSelector);
      var fakeFromExpressionInfo = new FromExpressionInfo (
          new SqlTable (new ResolvedSubStatementTableInfo ("sc", _sqlStatementBuilder.GetSqlStatement ()), JoinSemantics.Inner), new Ordering[0], elementSelector, null);

      var originalStatement = _sqlStatementBuilder.GetSqlStatement ();
      var originalDataInfo = _sqlStatementBuilder.DataInfo;

      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (keySelector, _context))
          .Return (keySelector);
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (elementSelector, _context))
          .Return (elementSelector);
      _stageMock
          .Expect (
              mock => mock.PrepareFromExpression (Arg<Expression>.Is.Anything, Arg.Is (_context), Arg<Func<ITableInfo, SqlTableBase>>.Is.Anything))
          .Return (fakeFromExpressionInfo)
          .WhenCalled (
              mi =>
              {
                var fromExpression = (Expression) mi.Arguments[0];
                CheckExpressionMovedIntoSubStatement (fromExpression, originalStatement);
              });

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      _stageMock.VerifyAllExpectations();
      Assert.That (_sqlStatementBuilder.GroupByExpression, Is.SameAs (keySelector));
      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SqlTables[0], Is.SameAs (fakeFromExpressionInfo.SqlTable));

      Assert.That (_sqlStatementBuilder.DataInfo, Is.Not.SameAs (originalDataInfo));
      Assert.That (
          _sqlStatementBuilder.DataInfo.DataType,
          Is.EqualTo (typeof (IQueryable<>).MakeGenericType (typeof (IGrouping<,>).MakeGenericType (typeof (string), typeof (string)))));
    }

    [Test]
    public void HandleResultOperator_GroupByAfterGroupBy ()
    {
      var groupByExpression = Expression.Constant("group");
      _sqlStatementBuilder.GroupByExpression = groupByExpression;

      var keySelector = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var elementSelector = Expression.Constant ("elementSelector");
      var resultOperator = new GroupResultOperator ("itemName", keySelector, elementSelector);
      var fakeFromExpressionInfo = new FromExpressionInfo (
          new SqlTable (new ResolvedSubStatementTableInfo ("sc", _sqlStatementBuilder.GetSqlStatement ()), JoinSemantics.Inner), new Ordering[0], elementSelector, null);

      var originalStatement = _sqlStatementBuilder.GetSqlStatement ();
      var originalDataInfo = _sqlStatementBuilder.DataInfo;

      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (keySelector, _context))
          .Return (keySelector);
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (elementSelector, _context))
          .Return (elementSelector);
      _stageMock
          .Expect (
              mock => mock.PrepareFromExpression (Arg<Expression>.Is.Anything, Arg.Is (_context), Arg<Func<ITableInfo, SqlTableBase>>.Is.Anything))
          .Return (fakeFromExpressionInfo)
          .WhenCalled (
              mi =>
              {
                var fromExpression = (Expression) mi.Arguments[0];
                CheckExpressionMovedIntoSubStatement (fromExpression, originalStatement);
              });
      _stageMock.Replay ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      _stageMock.VerifyAllExpectations ();
      Assert.That (_sqlStatementBuilder.GroupByExpression, Is.SameAs (keySelector));
      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SqlTables[0], Is.SameAs (fakeFromExpressionInfo.SqlTable));

      Assert.That (_sqlStatementBuilder.DataInfo, Is.Not.SameAs (originalDataInfo));
      Assert.That (
          _sqlStatementBuilder.DataInfo.DataType,
          Is.EqualTo (typeof (IQueryable<>).MakeGenericType (typeof (IGrouping<,>).MakeGenericType (typeof (string), typeof (string)))));
    }

    [Test]
    public void HandleResultOperator_TransformSubqueriesUsedAsGroupByKeys ()
    {
      var keySelector = Expression.Constant ("keySelector");
      var elementSelector = Expression.Constant ("elementSelector");
      var resultOperator = new GroupResultOperator ("itemName", keySelector, elementSelector);

      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Single();
      var preparedSubStatementKeySelector = new SqlSubStatementExpression (sqlStatement);
      
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (keySelector, _context))
          .Return (preparedSubStatementKeySelector);
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (elementSelector, _context))
          .Return (elementSelector);

      _stageMock.Replay ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      _stageMock.VerifyAllExpectations ();

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (2));
      Assert.That (_sqlStatementBuilder.SqlTables[1], Is.TypeOf (typeof(SqlTable)));
      Assert.That (((SqlTable) _sqlStatementBuilder.SqlTables[1]).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));

      var groupKeyTableTableInfo = (ResolvedSubStatementTableInfo) ((SqlTable) _sqlStatementBuilder.SqlTables[1]).TableInfo;
      var expectedStatement = new SqlStatementBuilder (sqlStatement) 
        { DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<int>), sqlStatement.SelectProjection) }
        .GetSqlStatement();
      Assert.That (groupKeyTableTableInfo.SqlStatement, Is.EqualTo (expectedStatement));

      var expectedGroupGyExpression = new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[1]);
      ExpressionTreeComparer.CheckAreEqualTrees (_sqlStatementBuilder.GroupByExpression, expectedGroupGyExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (
          _sqlStatementBuilder.SelectProjection, 
          SqlGroupingSelectExpression.CreateWithNames (expectedGroupGyExpression, elementSelector));
    }

    [Test]
    public void HandleResultOperator_DetectConstantKeysAndReplaceWithSubStatement ()
    {
      var keySelector = Expression.Constant ("keySelector");
      var elementSelector = Expression.Constant ("elementSelector");
      var resultOperator = new GroupResultOperator ("itemName", keySelector, elementSelector);
      
      var preparedConstantKeySelector = Expression.Constant ("test");

      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (keySelector, _context))
          .Return (preparedConstantKeySelector);
      _stageMock
          .Expect (mock => mock.PrepareResultOperatorItemExpression (elementSelector, _context))
          .Return (elementSelector);
      _stageMock.Replay ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      _stageMock.VerifyAllExpectations ();
      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (2));
      Assert.That (_sqlStatementBuilder.SqlTables[1], Is.TypeOf (typeof (SqlTable)));
      Assert.That (((SqlTable) _sqlStatementBuilder.SqlTables[1]).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      
      var groupKeyTableTableInfo = (ResolvedSubStatementTableInfo) ((SqlTable) _sqlStatementBuilder.SqlTables[1]).TableInfo;
      var expectedSelectExpression = new NamedExpression (null, preparedConstantKeySelector);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectExpression, groupKeyTableTableInfo.SqlStatement.SelectProjection);
      
      var expectedStatement = new SqlStatementBuilder 
        {
          DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<string>), groupKeyTableTableInfo.SqlStatement.SelectProjection),
          SelectProjection = groupKeyTableTableInfo.SqlStatement.SelectProjection
        }
        .GetSqlStatement ();
      Assert.That (groupKeyTableTableInfo.SqlStatement, Is.EqualTo (expectedStatement));

      var expectedGroupGyExpression = new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[1]);
      ExpressionTreeComparer.CheckAreEqualTrees (_sqlStatementBuilder.GroupByExpression, expectedGroupGyExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (
          _sqlStatementBuilder.SelectProjection,
          SqlGroupingSelectExpression.CreateWithNames (expectedGroupGyExpression, elementSelector));
    }

    private void CheckExpressionMovedIntoSubStatement (Expression fromExpression, SqlStatement originalStatement)
    {
      Assert.That (fromExpression, Is.TypeOf<SqlSubStatementExpression> ());
      var subStatement = ((SqlSubStatementExpression) fromExpression).SqlStatement;
      // The sub-statement should have the same SelectProjection, but wrapped into a NamedExpression with default name.
      Assert.That (
          subStatement.SelectProjection,
          Is.TypeOf<NamedExpression> ().With.Property ("Name").Null.And.Property ("Expression").SameAs (originalStatement.SelectProjection));
      //  The rest of the sub-statement should be equivalent to the original statement.
      var expectedSubStatement = new SqlStatementBuilder (originalStatement) { SelectProjection = subStatement.SelectProjection }.GetSqlStatement ();
      Assert.That (subStatement, Is.EqualTo (expectedSubStatement));
    }
 }
}