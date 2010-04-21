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
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.ResultOperators;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationQueryModelVisitorTest
  {
    private SqlPreparationContext _context;
    private DefaultSqlPreparationStage _defaultStage;

    private SelectClause _selectClause;
    private OrderByClause _orderByClause;
    private MainFromClause _mainFromClause;
    private QueryModel _queryModel;

    private TestableSqlPreparationQueryModelVisitor _visitor;
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator();
      _context = new SqlPreparationContext();
      _defaultStage = new DefaultSqlPreparationStage (MethodCallTransformerRegistry.CreateDefault(), _context, _generator);

      _mainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      _selectClause = ExpressionHelper.CreateSelectClause (_mainFromClause);
      _orderByClause = ExpressionHelper.CreateOrderByClause();
      _queryModel = new QueryModel (_mainFromClause, _selectClause);

      _stageMock = MockRepository.GenerateStrictMock<ISqlPreparationStage>();
      _visitor = new TestableSqlPreparationQueryModelVisitor (_context, _stageMock);
    }

    [Test]
    public void TransformQueryModel_EmptyQueryModel ()
    {
      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context, _defaultStage, _generator);
      Assert.That (result.SelectProjection, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (result.WhereCondition, Is.Null);
      Assert.That (result.SqlTables.Count, Is.EqualTo (1));
      Assert.That (((SqlTable) result.SqlTables[0]).TableInfo, Is.InstanceOfType (typeof (UnresolvedTableInfo)));
      Assert.That (result.TopExpression, Is.Null);
      Assert.That (result.IsCountQuery, Is.False);
      Assert.That (result.IsDistinctQuery, Is.False);
    }

    [Test]
    public void TransformQueryModel_QueryModel_WithAdditionalClauses ()
    {
      var constantExpression = Expression.Constant (0);
      var additionalFromClause = new AdditionalFromClause ("additional", typeof (int), constantExpression);

      _queryModel.BodyClauses.Add (additionalFromClause);
      _queryModel.BodyClauses.Add (ExpressionHelper.CreateWhereClause());
      _queryModel.ResultOperators.Add (new CountResultOperator());

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context, _defaultStage, _generator);

      Assert.That (result.WhereCondition, Is.Not.Null);
      Assert.That (result.SqlTables.Count, Is.EqualTo (2));
    }

    [Test]
    public void TransformQueryModel_WithCount ()
    {
      var countResultOperator = new CountResultOperator();
      _queryModel.ResultOperators.Add (countResultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context, _defaultStage, _generator);

      Assert.That (result.IsCountQuery, Is.True);
    }

    [Test]
    public void TransformQueryModel_WithDistinct ()
    {
      var distinctResultOperator = new DistinctResultOperator();
      _queryModel.ResultOperators.Add (distinctResultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context, _defaultStage, _generator);

      Assert.That (result.IsDistinctQuery, Is.True);
    }

    [Test]
    public void TransformQueryModel_WithCast ()
    {
      var castResultOperator = new CastResultOperator (typeof (Cook));
      _queryModel.ResultOperators.Add (castResultOperator);
      _visitor.SqlStatementBuilder.DataInfo = _queryModel.GetOutputDataInfo();

      SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context, _defaultStage, _generator);
    }

    [Test]
    public void TransformQueryModel_WithTake ()
    {
      var resultOperator = new TakeResultOperator (Expression.Constant (0));
      _queryModel.ResultOperators.Add (resultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context, _defaultStage, _generator);

      Assert.That (result.TopExpression, Is.Not.Null);
    }

    [Test]
    public void TransformQueryModel_WithFirst ()
    {
      var resultOperator = new FirstResultOperator (false);
      _queryModel.ResultOperators.Add (resultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context, _defaultStage, _generator);

      Assert.That (result.TopExpression, Is.Not.Null);
    }

    [Test]
    public void TransformQueryModel_WithSingle ()
    {
      var resultOperator = new SingleResultOperator (false);
      _queryModel.ResultOperators.Add (resultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context, _defaultStage, _generator);

      Assert.That (result.TopExpression, Is.Not.Null);
    }

    [Test]
    public void VisitMainFromClause_CreatesFromExpression ()
    {
      var preparedExpression = Expression.Constant (0);
      var preparedSqlTable = SqlStatementModelObjectMother.CreateSqlTable();

      _stageMock.Expect (mock => mock.PrepareFromExpression (_mainFromClause.FromExpression)).Return (preparedExpression);
      _stageMock.Expect (mock => mock.PrepareSqlTable (preparedExpression, typeof (Cook))).Return (preparedSqlTable);

      _stageMock.Replay();

      _visitor.VisitMainFromClause (_mainFromClause, _queryModel);
      _stageMock.VerifyAllExpectations();

      Assert.That (_visitor.SqlStatementBuilder.SqlTables, Is.EqualTo (new[] { preparedSqlTable }));
      Assert.That (_context.GetSqlTableForQuerySource (_mainFromClause), Is.SameAs (preparedSqlTable));
    }

    [Test]
    public void VisitAdditionalFromClause_CreatesFromExpression ()
    {
      var fakeSqlTableForMainFromClause = SqlStatementModelObjectMother.CreateSqlTable();
      _visitor.SqlStatementBuilder.SqlTables.Add (fakeSqlTableForMainFromClause);

      var constantExpression = Expression.Constant (0);
      var additionalFromClause = new AdditionalFromClause ("additional", typeof (int), constantExpression);
      _queryModel.BodyClauses.Add (additionalFromClause);

      var preparedExpression = Expression.Constant (0);
      var preparedSqlTable = SqlStatementModelObjectMother.CreateSqlTable();

      _stageMock.Expect (mock => mock.PrepareFromExpression (additionalFromClause.FromExpression)).Return (preparedExpression);
      _stageMock.Expect (mock => mock.PrepareSqlTable (preparedExpression, typeof (int))).Return (preparedSqlTable);

      _stageMock.Replay();

      _visitor.VisitAdditionalFromClause (additionalFromClause, _queryModel, 0);
      _stageMock.VerifyAllExpectations();

      Assert.That (_visitor.SqlStatementBuilder.SqlTables, Is.EqualTo (new[] { fakeSqlTableForMainFromClause, preparedSqlTable }));
      Assert.That (_context.GetSqlTableForQuerySource (additionalFromClause), Is.SameAs (preparedSqlTable));
    }

    [Test]
    public void VisitMainFromClause_AddsWhereCondition_AndCreatesNewSqlTable ()
    {
      var constantExpression = Expression.Constant (0);
      var additionalFromClause = new AdditionalFromClause ("additional", typeof (int), constantExpression);

      var preparedExpression = Expression.Constant (0);
      var preparedSqlTable = SqlStatementModelObjectMother.CreateSqlJoinedTable_WithUnresolvedJoinInfo();

      _stageMock.Expect (mock => mock.PrepareFromExpression (additionalFromClause.FromExpression)).Return (preparedExpression);
      _stageMock.Expect (mock => mock.PrepareSqlTable (preparedExpression, typeof (int))).Return (preparedSqlTable);

      _stageMock.Replay();

      _visitor.VisitAdditionalFromClause (additionalFromClause, _queryModel, 0);

      _stageMock.VerifyAllExpectations();

      Assert.That (_visitor.SqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_visitor.SqlStatementBuilder.WhereCondition, Is.TypeOf (typeof (JoinConditionExpression)));
    }

    [Test]
    public void VisitWhereClause_WithCondition ()
    {
      var predicate = Expression.Constant (true);
      var preparedExpression = Expression.Constant (false);

      var whereClause = new WhereClause (predicate);
      _queryModel.BodyClauses.Add (whereClause);

      _stageMock.Expect (mock => mock.PrepareWhereExpression (whereClause.Predicate)).Return (preparedExpression);
      _stageMock.Replay();

      _visitor.VisitWhereClause (whereClause, _queryModel, 0);

      _stageMock.VerifyAllExpectations();
      Assert.That (_visitor.SqlStatementBuilder.WhereCondition, Is.SameAs (preparedExpression));
    }

    [Test]
    public void VisitWhereClause_MulipleWhereClauses ()
    {
      var predicate1 = Expression.Constant (true);
      var whereClause1 = new WhereClause (predicate1);
      _queryModel.BodyClauses.Add (whereClause1);
      var preparedExpression1 = Expression.Constant (false);

      var predicate2 = Expression.Constant (true);
      var whereClause2 = new WhereClause (predicate2);
      _queryModel.BodyClauses.Add (whereClause2);
      var preparedExpression2 = Expression.Constant (false);

      _stageMock.Expect (mock => mock.PrepareWhereExpression (predicate1)).Return (preparedExpression1);
      _stageMock.Expect (mock => mock.PrepareWhereExpression (predicate2)).Return (preparedExpression2);
      _stageMock.Replay();

      _visitor.VisitWhereClause (whereClause1, _queryModel, 0);
      _visitor.VisitWhereClause (whereClause2, _queryModel, 1);

      _stageMock.VerifyAllExpectations();
      Assert.That (_visitor.SqlStatementBuilder.WhereCondition.NodeType, Is.EqualTo (ExpressionType.AndAlso));
      Assert.That (((BinaryExpression) _visitor.SqlStatementBuilder.WhereCondition).Left, Is.SameAs (preparedExpression1));
      Assert.That (((BinaryExpression) _visitor.SqlStatementBuilder.WhereCondition).Right, Is.SameAs (preparedExpression2));
    }

    [Test]
    public void VisitSelectClause_CreatesSelectProjection ()
    {
      var preparedExpression = Expression.Constant (null, typeof (Cook));

      _stageMock.Expect (mock => mock.PrepareSelectExpression (_selectClause.Selector)).Return (preparedExpression);
      _stageMock.Replay();

      _visitor.VisitSelectClause (_selectClause, _queryModel);

      _stageMock.VerifyAllExpectations();
      Assert.That (_visitor.SqlStatementBuilder.SelectProjection, Is.SameAs (preparedExpression));
    }

    [Test]
    public void VisitJoinClause_CreatesSqlTable ()
    {
      var joinClause = ExpressionHelper.CreateJoinClause ();
      var preparedSqlTable = SqlStatementModelObjectMother.CreateSqlTable ();
      var constantExpression = Expression.Constant (5);
      var fakeWhereCondition = Expression.Constant (1);
      
      _stageMock
        .Expect (mock => mock.PrepareFromExpression (joinClause.InnerSequence))
        .Return (constantExpression);
      _stageMock
          .Expect (
          mock => mock.PrepareSqlTable (constantExpression, joinClause.ItemType))
          .Return (preparedSqlTable);
      _stageMock
        .Expect (mock => mock.PrepareWhereExpression (Arg<Expression>.Is.Anything))
        .Return (fakeWhereCondition);
      _stageMock.Replay ();

      _visitor.VisitJoinClause (joinClause, _queryModel, 5);

      _stageMock.VerifyAllExpectations ();

      Assert.That (_visitor.SqlStatementBuilder.SqlTables, Is.EquivalentTo (new [] { preparedSqlTable }));
    }

    [Test]
    public void VisitJoinClause_CreatesWhereCondition ()
    {
      var joinClause = ExpressionHelper.CreateJoinClause();
      var preparedSqlTable = SqlStatementModelObjectMother.CreateSqlTable();

      var fakeWhereCondition = Expression.Constant (1);

      _stageMock
        .Expect (mock => mock.PrepareFromExpression (joinClause.InnerSequence))
        .Return (Expression.Constant (5));
      _stageMock
          .Expect (
          mock => mock.PrepareSqlTable (Arg<Expression>.Is.Anything, Arg<Type>.Is.Anything))
          .Return (preparedSqlTable);
      _stageMock
        .Expect (mock => mock.PrepareWhereExpression (Arg<Expression>.Is.Anything))
        .Return (fakeWhereCondition);
      _stageMock.Replay();

      _visitor.VisitJoinClause (joinClause, _queryModel, 5);

      _stageMock.VerifyAllExpectations();

      Assert.That (_visitor.SqlStatementBuilder.WhereCondition, Is.SameAs (fakeWhereCondition));
    }

    [Test]
    public void VisitOrderByClause_Single ()
    {
      var preparedOrdering = new Ordering (Expression.Constant ("column"), OrderingDirection.Asc);
      _orderByClause.Orderings.Add (ExpressionHelper.CreateOrdering());

      _stageMock.Expect (mock => mock.PrepareOrderByExpression (_orderByClause.Orderings[0].Expression)).Return (preparedOrdering.Expression);
      _stageMock.Replay();

      _visitor.VisitOrderByClause (_orderByClause, _queryModel, 1);

      _stageMock.VerifyAllExpectations();
      Assert.That (_visitor.SqlStatementBuilder.Orderings, Is.Not.Null);
      Assert.That (_visitor.SqlStatementBuilder.Orderings.Count, Is.EqualTo (1));
      Assert.That (_visitor.SqlStatementBuilder.Orderings[0].Expression, Is.SameAs (preparedOrdering.Expression));
    }

    [Test]
    public void VisitOrderByClause_Multiple ()
    {
      var preparedOrderingExpression1 = Expression.Constant ("column1");
      var preparedOrderingExpression2 = Expression.Constant ("column2");
      var preparedOrderingExpression3 = Expression.Constant ("column3");

      var ordering1 = new Ordering (preparedOrderingExpression1, OrderingDirection.Asc);
      var ordering2 = new Ordering (preparedOrderingExpression2, OrderingDirection.Asc);
      var ordering3 = new Ordering (preparedOrderingExpression3, OrderingDirection.Desc);

      _visitor.SqlStatementBuilder.Orderings.Add (ordering1);

      _orderByClause.Orderings.Add (ordering2);
      _orderByClause.Orderings.Add (ordering3);

      _stageMock.Expect (mock => mock.PrepareOrderByExpression (_orderByClause.Orderings[0].Expression)).Return (preparedOrderingExpression2);
      _stageMock.Expect (mock => mock.PrepareOrderByExpression (_orderByClause.Orderings[1].Expression)).Return (preparedOrderingExpression3);
      _stageMock.Replay();

      _visitor.VisitOrderByClause (_orderByClause, _queryModel, 1);

      _stageMock.VerifyAllExpectations();
      Assert.That (_visitor.SqlStatementBuilder.Orderings, Is.Not.Null);
      Assert.That (_visitor.SqlStatementBuilder.Orderings.Count, Is.EqualTo (3));
      Assert.That (_visitor.SqlStatementBuilder.Orderings[0].Expression, Is.SameAs (preparedOrderingExpression2));
      Assert.That (_visitor.SqlStatementBuilder.Orderings[1].Expression, Is.SameAs (preparedOrderingExpression3));
      Assert.That (_visitor.SqlStatementBuilder.Orderings[2].Expression, Is.SameAs (preparedOrderingExpression1));
    }

    [Test]
    public void VisitResultOperator_First ()
    {
      var resultOperator = new FirstResultOperator (false);
      _queryModel.ResultOperators.Add (resultOperator);
      var preparedExpression = Expression.Constant (null, typeof (Cook));
      _visitor.SqlStatementBuilder.DataInfo = new StreamedSequenceInfo (typeof (Cook[]), preparedExpression);

      _stageMock
          .Expect (
          mock => mock.PrepareTopExpression (
                      Arg<Expression>.Matches (expr => expr is ConstantExpression && ((ConstantExpression) expr).Value.Equals (1))))
          .Return (preparedExpression);
      _stageMock.Replay();

      _visitor.VisitResultOperator (resultOperator, _queryModel, 0);

      _stageMock.VerifyAllExpectations();

      Assert.That (_visitor.SqlStatementBuilder.TopExpression, Is.SameAs (preparedExpression));
    }

    [Test]
    public void VisitResultOperator_Single ()
    {
      var resultOperator = new SingleResultOperator (false);
      _queryModel.ResultOperators.Add (resultOperator);
      var preparedExpression = Expression.Constant (null, typeof (Cook));
      _visitor.SqlStatementBuilder.DataInfo = new StreamedSequenceInfo (typeof (Cook[]), preparedExpression);

      _stageMock
          .Expect (
          mock => mock.PrepareTopExpression (
                      Arg<Expression>.Matches (expr => expr is ConstantExpression && ((ConstantExpression) expr).Value.Equals (2))))
          .Return (preparedExpression);
      _stageMock.Replay();

      _visitor.VisitResultOperator (resultOperator, _queryModel, 0);

      _stageMock.VerifyAllExpectations();

      Assert.That (_visitor.SqlStatementBuilder.TopExpression, Is.SameAs (preparedExpression));
    }

    [Test]
    public void VisitResultOperator_Take ()
    {
      var takeExpression = Expression.Constant (2);
      var resultOperator = new TakeResultOperator (takeExpression);
      _queryModel.ResultOperators.Add (resultOperator);
      var preparedExpression = Expression.Constant (null, typeof (Cook));
      _visitor.SqlStatementBuilder.DataInfo = new StreamedSequenceInfo(typeof (int[]), takeExpression);

      _stageMock.Expect (mock => mock.PrepareTopExpression (takeExpression)).Return (preparedExpression);
      _stageMock.Replay();

      _visitor.VisitResultOperator (resultOperator, _queryModel, 0);

      _stageMock.VerifyAllExpectations();

      Assert.That (_visitor.SqlStatementBuilder.TopExpression, Is.SameAs (preparedExpression));
    }

    [Test]
    public void VisitResultOperator_TakeAndDistinct ()
    {
      var takeExpression = Expression.Constant (2);
      var queryModel = new QueryModel (_mainFromClause, _selectClause);
      var takeResultOperator = new TakeResultOperator (takeExpression);
      queryModel.ResultOperators.Add (takeResultOperator);
      var distinctResultOperator = new DistinctResultOperator();

      _visitor.SqlStatementBuilder.DataInfo = queryModel.GetOutputDataInfo();
      _visitor.SqlStatementBuilder.SelectProjection = Expression.Constant ("select");

      var preparedExpression = Expression.Constant (null, typeof (Cook));
      _stageMock
          .Expect (mock => mock.PrepareTopExpression (takeExpression))
          .Return (preparedExpression);
      _stageMock.Replay();

      _visitor.VisitResultOperator (takeResultOperator, queryModel, 0);
      _visitor.VisitResultOperator (distinctResultOperator, queryModel, 0);

      _stageMock.VerifyAllExpectations();
      Assert.That (_visitor.SqlStatementBuilder.SelectProjection, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (_visitor.SqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (
          ((SqlTable) ((SqlTableReferenceExpression) _visitor.SqlStatementBuilder.SelectProjection).SqlTable).TableInfo,
          Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
    }

    [Test]
    public void VisitResultOperator_Contains ()
    {
      var itemExpression = Expression.Constant (new Cook ());
      var resultOperator = new ContainsResultOperator (itemExpression);
      _queryModel.ResultOperators.Add (resultOperator);

      _visitor.SqlStatementBuilder.DataInfo = new StreamedSequenceInfo (typeof (Cook[]), itemExpression);
      _visitor.SqlStatementBuilder.SelectProjection = Expression.Constant (1);
      _visitor.SqlStatementBuilder.SqlTables.Add (SqlStatementModelObjectMother.CreateSqlTable());

      var preparedExpression = Expression.Constant (new Cook(), typeof (Cook));
      _stageMock.Expect (mock => mock.PrepareItemExpression (itemExpression)).Return (preparedExpression);
      _stageMock.Replay();

      var oldSqlStatementBuilder = _visitor.SqlStatementBuilder;

      _visitor.VisitResultOperator (resultOperator, _queryModel, 0);

      _stageMock.VerifyAllExpectations();

      Assert.That (_visitor.SqlStatementBuilder, Is.Not.SameAs (oldSqlStatementBuilder));
      Assert.That (_visitor.SqlStatementBuilder.SelectProjection, Is.TypeOf (typeof (SqlBinaryOperatorExpression)));
      Assert.That (((SqlBinaryOperatorExpression) _visitor.SqlStatementBuilder.SelectProjection).LeftExpression, Is.SameAs (preparedExpression));
      Assert.That (
          ((SqlBinaryOperatorExpression) _visitor.SqlStatementBuilder.SelectProjection).RightExpression,
          Is.TypeOf (typeof (SqlSubStatementExpression)));

      Assert.That (
          ((SqlSubStatementExpression) ((SqlBinaryOperatorExpression) _visitor.SqlStatementBuilder.SelectProjection).RightExpression).SqlStatement.
              SelectProjection,
          Is.SameAs (oldSqlStatementBuilder.SelectProjection));
      Assert.That (
          ((SqlSubStatementExpression) ((SqlBinaryOperatorExpression) _visitor.SqlStatementBuilder.SelectProjection).RightExpression).SqlStatement.
              SqlTables,
          Is.EqualTo (oldSqlStatementBuilder.SqlTables));
    }

    [Test]
    public void VisitResultOperator_OfType ()
    {
      var resultOperator = new OfTypeResultOperator (typeof (Chef));
      _queryModel.ResultOperators.Add (resultOperator);
      var selectProjection = Expression.Constant (new Chef());
      _visitor.SqlStatementBuilder.DataInfo = new StreamedSequenceInfo (typeof (Chef[]), selectProjection);
      _visitor.SqlStatementBuilder.SelectProjection = selectProjection;

      _visitor.VisitResultOperator (resultOperator, _queryModel, 0);

      Assert.That (_visitor.SqlStatementBuilder.WhereCondition, Is.TypeOf (typeof (TypeBinaryExpression)));
      Assert.That (((TypeBinaryExpression) _visitor.SqlStatementBuilder.WhereCondition).Expression, Is.SameAs (selectProjection));
      Assert.That (((TypeBinaryExpression) _visitor.SqlStatementBuilder.WhereCondition).TypeOperand, Is.EqualTo (typeof (Chef)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "DefaultIfEmpty(1) is not supported.")]
    public void VisitResultOperator_NotSupported ()
    {
      var expression = Expression.Constant (1);
      var resultOperator = new DefaultIfEmptyResultOperator (expression);
      _queryModel.ResultOperators.Add (resultOperator);
      _visitor.SqlStatementBuilder.DataInfo = new StreamedSequenceInfo(typeof(int[]), expression);

      _visitor.VisitResultOperator (resultOperator, _queryModel, 0);
    }

    [Test]
    public void GetStatementAndResetBuilder ()
    {
      var originalSqlStatementBuilder = _visitor.SqlStatementBuilder;

      var constantExpression = Expression.Constant (1);
      _visitor.SqlStatementBuilder.DataInfo = _queryModel.GetOutputDataInfo ();
      _visitor.SqlStatementBuilder.SelectProjection = constantExpression;

      var result = _visitor.GetStatementAndResetBuilder();

      Assert.That (_visitor.SqlStatementBuilder, Is.Not.SameAs (originalSqlStatementBuilder));
      Assert.That (result.SelectProjection, Is.SameAs (constantExpression));
    }
  }
}