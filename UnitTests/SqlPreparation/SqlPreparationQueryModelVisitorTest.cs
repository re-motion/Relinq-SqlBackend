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
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationQueryModelVisitorTest
  {
    private ISqlPreparationContext _context;
    private DefaultSqlPreparationStage _defaultStage;

    private SelectClause _selectClause;
    private OrderByClause _orderByClause;
    private MainFromClause _mainFromClause;
    private QueryModel _queryModel;

    private TestableSqlPreparationQueryModelVisitor _visitor;
    private Mock<ISqlPreparationStage> _stageMock;
    private UniqueIdentifierGenerator _generator;
    private Mock<TestableSqlPreparationQueryModelVisitor> _visitorPartialMock;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator();
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext();
      _defaultStage = new DefaultSqlPreparationStage (
          CompoundMethodCallTransformerProvider.CreateDefault(), ResultOperatorHandlerRegistry.CreateDefault(), _generator);

      _mainFromClause = ExpressionHelper.CreateMainFromClause<Cook>();
      _selectClause = ExpressionHelper.CreateSelectClause (_mainFromClause);
      _orderByClause = ExpressionHelper.CreateOrderByClause();
      _queryModel = new QueryModel (_mainFromClause, _selectClause);

      _stageMock = new Mock<ISqlPreparationStage> (MockBehavior.Strict);
      _visitor = new TestableSqlPreparationQueryModelVisitor (_context, _stageMock.Object);
      _visitorPartialMock = new Mock<TestableSqlPreparationQueryModelVisitor> (_context, _stageMock.Object) { CallBase = true };
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_visitor.Context, Is.TypeOf (typeof (SqlPreparationContext)));
      Assert.That (_visitor.Context, Is.Not.SameAs (_context));
    }

    [Test]
    public void TransformQueryModel_EmptyQueryModel ()
    {
      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (
          _queryModel, _context, _defaultStage, _generator, ResultOperatorHandlerRegistry.CreateDefault());
      Assert.That (result.SelectProjection, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (result.WhereCondition, Is.Null);
      Assert.That (result.SqlTables.Count, Is.EqualTo (1));
      Assert.That (result.SqlTables[0].SqlTable.TableInfo, Is.InstanceOf (typeof (UnresolvedTableInfo)));
      Assert.That (result.TopExpression, Is.Null);
      Assert.That (result.IsDistinctQuery, Is.False);
    }

    [Test]
    public void TransformQueryModel_QueryModel_WithAdditionalClauses ()
    {
      var constantExpression = Expression.Constant (new Cook[0]);
      var additionalFromClause = new AdditionalFromClause ("additional", typeof (int), constantExpression);

      _queryModel.BodyClauses.Add (additionalFromClause);
      _queryModel.BodyClauses.Add (ExpressionHelper.CreateWhereClause());
      _queryModel.ResultOperators.Add (new CountResultOperator());

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (
          _queryModel, _context, _defaultStage, _generator, ResultOperatorHandlerRegistry.CreateDefault());

      Assert.That (result.WhereCondition, Is.Not.Null);
      Assert.That (result.SqlTables.Count, Is.EqualTo (2));
    }

    [Test]
    public void VisitQueryModel_VisitsClauses_AndNamesSelectClause ()
    {
      var fakeSelectProjection = Expression.Constant (0);

      _visitorPartialMock.Setup (mock => mock.VisitMainFromClause (_queryModel.MainFromClause, _queryModel)).Verifiable();
      _visitorPartialMock
          .Setup (mock => mock.VisitSelectClause (_queryModel.SelectClause, _queryModel))
          .Callback ((SelectClause selectClause, QueryModel queryModel) => _visitorPartialMock.Object.SqlStatementBuilder.SelectProjection = fakeSelectProjection)
          .Verifiable();

      _visitorPartialMock.Object.VisitQueryModel (_queryModel);

      _visitorPartialMock.Verify();

      Assert.That (_visitorPartialMock.Object.SqlStatementBuilder.SelectProjection, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) _visitorPartialMock.Object.SqlStatementBuilder.SelectProjection).Name, Is.Null);
      Assert.That (((NamedExpression) _visitorPartialMock.Object.SqlStatementBuilder.SelectProjection).Expression, Is.SameAs (fakeSelectProjection));
    }

    [Test]
    public void VisitQueryModel_AdjustsDataInfo_IfRequired ()
    {
      _visitorPartialMock.Setup (mock => mock.VisitMainFromClause (_queryModel.MainFromClause, _queryModel));
      _visitorPartialMock
          .Setup (mock => mock.VisitSelectClause (_queryModel.SelectClause, _queryModel))
          .Callback (
              (SelectClause selectClause, QueryModel queryModel) =>
              {
                _visitorPartialMock.Object.SqlStatementBuilder.SelectProjection = Expression.Constant (0);
                _visitorPartialMock.Object.SqlStatementBuilder.DataInfo =
                    new StreamedSequenceInfo (typeof (IEnumerable<Cook>), Expression.Constant (null, typeof (Cook)));
              });

      _queryModel.ResultTypeOverride = typeof (List<>);

      _visitorPartialMock.Object.VisitQueryModel (_queryModel);

      _visitorPartialMock.Verify();

      Assert.That (_visitorPartialMock.Object.SqlStatementBuilder.DataInfo, Is.Not.Null);
      Assert.That (_visitorPartialMock.Object.SqlStatementBuilder.DataInfo.DataType, Is.SameAs (typeof (List<Cook>)));
    }

    [Test]
    public void VisitQueryModel_ConstantExpression_Collection ()
    {
      var constantExpression = Expression.Constant (new[] { "t1", "t2" });
      _queryModel.MainFromClause.FromExpression = constantExpression;

      _visitor.VisitQueryModel (_queryModel);

      Assert.That (_visitor.SqlStatementBuilder.SelectProjection, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) _visitor.SqlStatementBuilder.SelectProjection).Expression, Is.TypeOf (typeof (ConstantExpression)));
      
      var constantExpressionInSelectProjection = ((ConstantExpression) ((NamedExpression) _visitor.SqlStatementBuilder.SelectProjection).Expression);
      Assert.That (constantExpressionInSelectProjection.Value, Is.EqualTo (constantExpression.Value));
      Assert.That (_visitor.SqlStatementBuilder.SqlTables.Count, Is.EqualTo (0));

      var expectedDataInfo = _queryModel.SelectClause.GetOutputDataInfo();
      Assert.That (((StreamedSequenceInfo) _visitor.SqlStatementBuilder.DataInfo).ItemExpression, Is.SameAs (expectedDataInfo.ItemExpression));
    }

    [Test]
    public void VisitQueryModel_ConstantExpression_NullCollection ()
    {
      var constantExpression = Expression.Constant (null, typeof (int[]));
      _queryModel.MainFromClause.FromExpression = constantExpression;

      Assert.That (
          () => _visitor.VisitQueryModel (_queryModel),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo ("Data sources cannot be null."));
    }

    [Test]
    public void VisitQueryModel_ConstantExpression_Collection_TypeIsEnumerable_ButValueIsCollection ()
    {
      var constantExpression = Expression.Constant (new[] { "t1", "t2" }, typeof (IEnumerable<string>));
      _queryModel.MainFromClause.FromExpression = constantExpression;

      _visitor.VisitQueryModel (_queryModel);

      Assert.That (_visitor.SqlStatementBuilder.SelectProjection, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) _visitor.SqlStatementBuilder.SelectProjection).Expression, Is.TypeOf (typeof (ConstantExpression)));

      var constantExpressionInSelectProjection = ((ConstantExpression) ((NamedExpression) _visitor.SqlStatementBuilder.SelectProjection).Expression);
      Assert.That (constantExpressionInSelectProjection.Value, Is.EqualTo (constantExpression.Value));
      Assert.That (_visitor.SqlStatementBuilder.SqlTables.Count, Is.EqualTo (0));

      var expectedDataInfo = _queryModel.SelectClause.GetOutputDataInfo ();
      Assert.That (((StreamedSequenceInfo) _visitor.SqlStatementBuilder.DataInfo).ItemExpression, Is.SameAs (expectedDataInfo.ItemExpression));
    }

    [Test]
    public void VisitQueryModel_ConstantExpression_Collection_AdjustsDataInfo_IfRequired ()
    {
      var constantExpression = Expression.Constant (new[] { "t1", "t2" });
      _queryModel.MainFromClause.FromExpression = constantExpression;
      _queryModel.MainFromClause.ItemType = typeof (string);
      _queryModel.SelectClause.Selector = new QuerySourceReferenceExpression (_queryModel.MainFromClause);

      _queryModel.ResultTypeOverride = typeof (string[]);

      _visitor.VisitQueryModel (_queryModel);

      Assert.That (_visitor.SqlStatementBuilder.DataInfo.DataType, Is.SameAs (typeof (string[])));
    }

    [Test]
    public void VisitQueryModel_ConstantExpressionCollection_VisitResultOperatorsIsCalled ()
    {
      var handlerMock = new Mock<IResultOperatorHandler> (MockBehavior.Strict);
      var registry = new ResultOperatorHandlerRegistry();
      registry.Register (typeof (TestChoiceResultOperator), handlerMock.Object);
      var queryModelVisitor = new TestableSqlPreparationQueryModelVisitor (_context, _stageMock.Object, _generator, registry);

      var constantExpression = Expression.Constant (new[] { "t1", "t2" });
      _queryModel.MainFromClause.FromExpression = constantExpression;
      var resultOperator = new TestChoiceResultOperator (false);
      _queryModel.ResultOperators.Add (resultOperator);
      var sqlStatementBuilder = queryModelVisitor.SqlStatementBuilder;

      handlerMock
          .Setup (
              mock => mock.HandleResultOperator (
                  resultOperator,
                  sqlStatementBuilder,
                  _generator,
                  _stageMock.Object,
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Verifiable();

      queryModelVisitor.VisitQueryModel (_queryModel);

      handlerMock.Verify();
    }

    [Test]
    public void VisitQueryModel_ConstantExpression_NoCollection ()
    {
      var constantExpression = Expression.Constant ("test");
      _queryModel.MainFromClause.FromExpression = constantExpression;

      var fakeSelectProjection = Expression.Constant (0);

      _visitorPartialMock.Setup (mock => mock.VisitMainFromClause (_queryModel.MainFromClause, _queryModel)).Verifiable();
      _visitorPartialMock
          .Setup (mock => mock.VisitSelectClause (_queryModel.SelectClause, _queryModel))
          .Callback ((SelectClause selectClause, QueryModel queryModel) => _visitorPartialMock.Object.SqlStatementBuilder.SelectProjection = fakeSelectProjection)
          .Verifiable();

      _visitorPartialMock.Object.VisitQueryModel (_queryModel);

      _visitorPartialMock.Verify();

      Assert.That (((NamedExpression) _visitorPartialMock.Object.SqlStatementBuilder.SelectProjection).Expression, Is.SameAs (fakeSelectProjection));
    }

    [Test]
    public void VisitMainFromClause_CreatesFromExpression ()
    {
      var preparedFromExpressionInfo = CreateFromExpressionInfo ();

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.Is<Expression> (e => e == _mainFromClause.FromExpression),
                  It.Is<ISqlPreparationContext> (c => c != _context),
                  OrderingExtractionPolicy.ExtractOrderingsIntoProjection))
          .Returns (preparedFromExpressionInfo)
          .Verifiable();

      _visitor.VisitMainFromClause (_mainFromClause, _queryModel);
      _stageMock.Verify();

      Assert.That (_visitor.SqlStatementBuilder.SqlTables, Is.EqualTo (new[] { preparedFromExpressionInfo.AppendedTable }));
      Assert.That (
          ((SqlTableReferenceExpression) _visitor.Context.GetExpressionMapping (new QuerySourceReferenceExpression (_mainFromClause))).SqlTable,
          Is.SameAs (preparedFromExpressionInfo.AppendedTable.SqlTable));
    }

    [Test]
    public void VisitAdditionalFromClause_AddsSqlTable_AndContextMapping ()
    {
      var fakeSqlTableForMainFromClause = SqlStatementModelObjectMother.CreateSqlAppendedTable();
      _visitor.SqlStatementBuilder.SqlTables.Add (fakeSqlTableForMainFromClause);

      var constantExpression = Expression.Constant (0);
      var additionalFromClause = new AdditionalFromClause ("additional", typeof (int), constantExpression);
      _queryModel.BodyClauses.Add (additionalFromClause);

      var preparedFromExpressionInfo = CreateFromExpressionInfo();

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.Is<Expression> (e => e == constantExpression),
                  It.Is<ISqlPreparationContext> (c => c != _context),
                  OrderingExtractionPolicy.ExtractOrderingsIntoProjection))
          .Returns (preparedFromExpressionInfo)
          .Verifiable();

      _visitor.VisitAdditionalFromClause (additionalFromClause, _queryModel, 0);

      _stageMock.Verify();

      Assert.That (
          _visitor.SqlStatementBuilder.SqlTables,
          Is.EqualTo (new[] { fakeSqlTableForMainFromClause, preparedFromExpressionInfo.AppendedTable }));
      var contextMapping = _visitor.Context.GetExpressionMapping (new QuerySourceReferenceExpression (additionalFromClause));
      Assert.That (contextMapping, Is.Not.Null);
      Assert.That (((SqlTableReferenceExpression) contextMapping).SqlTable, Is.SameAs (preparedFromExpressionInfo.AppendedTable.SqlTable));
    }

    [Test]
    public void VisitAdditionalFromClause_WithWhereCondition_AddsWhereCondition ()
    {
      var fromClause = ExpressionHelper.CreateAdditionalFromClause();
      
      var whereCondition = ExpressionHelper.CreateExpression(typeof(bool));
      var preparedFromExpressionInfo = CreateFromExpressionInfo (whereCondition);

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.Is<Expression> (e => e == fromClause.FromExpression),
                  It.Is<ISqlPreparationContext> (c => c != _context),
                  OrderingExtractionPolicy.ExtractOrderingsIntoProjection))
          .Returns (preparedFromExpressionInfo)
          .Verifiable();

      _visitor.VisitAdditionalFromClause (fromClause, _queryModel, 0);

      _stageMock.Verify();

      Assert.That (_visitor.SqlStatementBuilder.WhereCondition, Is.SameAs (whereCondition));
    }

    [Test]
    public void VisitWhereClause_WithCondition ()
    {
      var predicate = Expression.Constant (true);
      var preparedExpression = Expression.Constant (false);

      var whereClause = new WhereClause (predicate);
      _queryModel.BodyClauses.Add (whereClause);

      _stageMock
          .Setup (
              mock => mock.PrepareWhereExpression (
                  It.Is<Expression> (e => e == whereClause.Predicate),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (preparedExpression)
          .Verifiable();

      _visitor.VisitWhereClause (whereClause, _queryModel, 0);

      _stageMock.Verify();
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

      _stageMock
          .Setup (
              mock => mock.PrepareWhereExpression (
                  It.Is<Expression> (e => e == predicate1),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (preparedExpression1)
          .Verifiable();
      _stageMock
          .Setup (
              mock => mock.PrepareWhereExpression (
                  It.Is<Expression> (e => e == predicate2),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (preparedExpression2)
          .Verifiable();

      _visitor.VisitWhereClause (whereClause1, _queryModel, 0);
      _visitor.VisitWhereClause (whereClause2, _queryModel, 1);

      _stageMock.Verify();
      Assert.That (_visitor.SqlStatementBuilder.WhereCondition.NodeType, Is.EqualTo (ExpressionType.AndAlso));
      Assert.That (((BinaryExpression) _visitor.SqlStatementBuilder.WhereCondition).Left, Is.SameAs (preparedExpression1));
      Assert.That (((BinaryExpression) _visitor.SqlStatementBuilder.WhereCondition).Right, Is.SameAs (preparedExpression2));
    }

    [Test]
    public void VisitSelectClause_CreatesSelectProjection ()
    {
      var preparedExpression = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook)));

      _stageMock
          .Setup (
              mock => mock.PrepareSelectExpression (
                  It.Is<Expression> (e => e == _selectClause.Selector),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (preparedExpression)
          .Verifiable();

      _visitor.VisitSelectClause (_selectClause, _queryModel);

      _stageMock.Verify();
      Assert.That (_visitor.SqlStatementBuilder.SelectProjection, Is.SameAs (preparedExpression));
      Assert.That (_visitor.SqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) _visitor.SqlStatementBuilder.DataInfo).DataType, Is.EqualTo (_selectClause.GetOutputDataInfo().DataType));
    }

    [Test]
    public void VisitJoinClause_CreatesSqlTable ()
    {
      var joinClause = ExpressionHelper.CreateJoinClause<Cook>();
      var preparedFromExpressionInfo = CreateFromExpressionInfo();
      var fakeWhereCondition = Expression.Constant (1);

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.Is<Expression> (e => e == joinClause.InnerSequence),
                  It.Is<ISqlPreparationContext> (c => c != _context),
                  OrderingExtractionPolicy.ExtractOrderingsIntoProjection))
          .Returns (preparedFromExpressionInfo)
          .Verifiable();
      _stageMock
          .Setup (
              mock => mock.PrepareWhereExpression (
                  It.IsAny<Expression>(),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (fakeWhereCondition)
          .Verifiable();

      _visitor.VisitJoinClause (joinClause, _queryModel, 5);

      _stageMock.Verify();

      Assert.That (_visitor.SqlStatementBuilder.SqlTables, Is.EquivalentTo (new[] { preparedFromExpressionInfo.AppendedTable }));
    }

    [Test]
    public void VisitJoinClause_CreatesWhereCondition ()
    {
      var joinClause = ExpressionHelper.CreateJoinClause<Cook>();
      var preparedFromExpressionInfo = CreateFromExpressionInfo();

      var fakeWhereCondition = Expression.Constant (1);

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.Is<Expression> (e => e == joinClause.InnerSequence),
                  It.IsAny<ISqlPreparationContext>(),
                  OrderingExtractionPolicy.ExtractOrderingsIntoProjection))
          .Returns (preparedFromExpressionInfo)
          .Verifiable();
      _stageMock
          .Setup (
              mock => mock.PrepareWhereExpression (
                  It.IsAny<Expression>(),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (fakeWhereCondition)
          .Verifiable();

      _visitor.VisitJoinClause (joinClause, _queryModel, 5);

      _stageMock.Verify();

      Assert.That (_visitor.SqlStatementBuilder.WhereCondition, Is.SameAs (fakeWhereCondition));
    }

    [Test]
    public void VisitOrderByClause_Single ()
    {
      var preparedOrdering = new Ordering (Expression.Constant ("column"), OrderingDirection.Asc);
      _orderByClause.Orderings.Add (ExpressionHelper.CreateOrdering());

      _stageMock
          .Setup (
              mock => mock.PrepareOrderByExpression (
                  It.Is<Expression> (e => e == _orderByClause.Orderings[0].Expression),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (preparedOrdering.Expression)
          .Verifiable();

      _visitor.VisitOrderByClause (_orderByClause, _queryModel, 1);

      _stageMock.Verify();
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

      _stageMock
          .Setup (
              mock => mock.PrepareOrderByExpression (
                  It.Is<Expression> (e => e == _orderByClause.Orderings[0].Expression),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (preparedOrderingExpression2)
          .Verifiable();
      _stageMock
          .Setup (
              mock => mock.PrepareOrderByExpression (
                  It.Is<Expression> (e => e == _orderByClause.Orderings[1].Expression),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (preparedOrderingExpression3)
          .Verifiable();

      _visitor.VisitOrderByClause (_orderByClause, _queryModel, 1);

      _stageMock.Verify();
      Assert.That (_visitor.SqlStatementBuilder.Orderings, Is.Not.Null);
      Assert.That (_visitor.SqlStatementBuilder.Orderings.Count, Is.EqualTo (3));
      Assert.That (_visitor.SqlStatementBuilder.Orderings[0].Expression, Is.SameAs (preparedOrderingExpression2));
      Assert.That (_visitor.SqlStatementBuilder.Orderings[1].Expression, Is.SameAs (preparedOrderingExpression3));
      Assert.That (_visitor.SqlStatementBuilder.Orderings[2].Expression, Is.SameAs (preparedOrderingExpression1));
    }

    [Test]
    public void VisitResultOperator_HandlesResultOperator ()
    {
      var resultOperator = new TestChoiceResultOperator (false);

      var handlerMock = new Mock<IResultOperatorHandler>();
      
      var registry = new ResultOperatorHandlerRegistry();
      registry.Register (typeof (TestChoiceResultOperator), handlerMock.Object);
      var queryModelVisitor = new TestableSqlPreparationQueryModelVisitor (_context, _stageMock.Object, _generator, registry);
      var sqlStatementBuilder = queryModelVisitor.SqlStatementBuilder;

      handlerMock
          .Setup (
              mock => mock.HandleResultOperator (
                  resultOperator,
                  sqlStatementBuilder,
                  _generator,
                  _stageMock.Object,
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Verifiable();

      queryModelVisitor.VisitResultOperator (resultOperator, _queryModel, 0);

      handlerMock.Verify();
    }

    [Test]
    public void VisitResultOperator_NoHandlerFound ()
    {
      var resultOperator = new TestChoiceResultOperator (false);
      var registry = new ResultOperatorHandlerRegistry ();
      var queryModelVisitor = new TestableSqlPreparationQueryModelVisitor (_context, _stageMock.Object, _generator, registry);

      Assert.That (
          () => queryModelVisitor.VisitResultOperator (resultOperator, _queryModel, 0),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo ("The result operator 'TestChoiceResultOperator' is not supported and no custom handler has been registered."));
    }

    [Test]
    public void AddQuerySource ()
    {
      var preparedFromExpressionInfo = CreateFromExpressionInfo ();

      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  _mainFromClause.FromExpression,
                  It.Is<ISqlPreparationContext> (c => c != _context),
                  OrderingExtractionPolicy.ExtractOrderingsIntoProjection))
          .Returns (preparedFromExpressionInfo)
          .Verifiable();

      var result = _visitor.AddQuerySource (_mainFromClause, _mainFromClause.FromExpression);

      _stageMock.Verify();
      Assert.That (result, Is.SameAs (preparedFromExpressionInfo.AppendedTable.SqlTable));
      Assert.That (_visitor.Context.GetExpressionMapping (new QuerySourceReferenceExpression (_mainFromClause)), Is.Not.Null);
    }

    [Test]
    public void AddPreparedFromExpression_AddSqlTable ()
    {
      var preparedFromExpressionInfo = CreateFromExpressionInfo();

      _visitor.SqlStatementBuilder.SqlTables.Clear();

      _visitor.AddPreparedFromExpression (preparedFromExpressionInfo);

      Assert.That (_visitor.SqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_visitor.SqlStatementBuilder.SqlTables[0], Is.SameAs (preparedFromExpressionInfo.AppendedTable));
    }

    [Test]
    public void AddPreparedFromExpression_OrderingIsAdded ()
    {
      var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Asc);
      var preparedFromExpressionInfo = CreateFromExpressionInfo(orderings: new [] { ordering });
      
      _visitor.SqlStatementBuilder.Orderings.Clear ();
      
      _visitor.AddPreparedFromExpression (preparedFromExpressionInfo);

      Assert.That (_visitor.SqlStatementBuilder.Orderings.Count, Is.EqualTo (1));
      Assert.That (_visitor.SqlStatementBuilder.Orderings[0], Is.SameAs (ordering));
    }

    [Test]
    public void AddPreparedFromExpression_NoOrderings ()
    {
      var preparedFromExpressionInfo = CreateFromExpressionInfo ();

      _visitor.SqlStatementBuilder.Orderings.Clear ();

      _visitor.AddPreparedFromExpression (preparedFromExpressionInfo);

      Assert.That (_visitor.SqlStatementBuilder.Orderings.Count, Is.EqualTo (0));
    }

    [Test]
    public void AddPreparedFromExpression_WhereConditionIsSet ()
    {
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable ();
      var whereCondition = Expression.Constant (true);
      var preparedFromExpressionInfo = new FromExpressionInfo (
          appendedTable, new Ordering[0], new SqlTableReferenceExpression (appendedTable.SqlTable), whereCondition);

      _visitor.SqlStatementBuilder.WhereCondition = null;

      _visitor.AddPreparedFromExpression (preparedFromExpressionInfo);

      Assert.That (_visitor.SqlStatementBuilder.WhereCondition, Is.SameAs (whereCondition));
    }

    [Test]
    public void AddPreparedFromExpression_WhereConditionIsAdded ()
    {
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable ();
      var whereCondition = Expression.Constant(true);
      var preparedFromExpressionInfo = new FromExpressionInfo (
          appendedTable, new Ordering[0] , new SqlTableReferenceExpression (appendedTable.SqlTable), whereCondition);

      var originalWhereCondition = Expression.Constant (false);
      _visitor.SqlStatementBuilder.WhereCondition = originalWhereCondition;

      _visitor.AddPreparedFromExpression (preparedFromExpressionInfo);

      var expectedCombinedWhereCondition = Expression.AndAlso (originalWhereCondition, whereCondition);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedCombinedWhereCondition, _visitor.SqlStatementBuilder.WhereCondition);
    }

    [Test]
    public void AddPreparedFromExpression_NoWhereCondition ()
    {
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable ();
      var preparedFromExpressionInfo = new FromExpressionInfo (
          appendedTable, new Ordering[0], new SqlTableReferenceExpression (appendedTable.SqlTable), null);

      var originalWhereCondition = Expression.Constant (false);
      _visitor.SqlStatementBuilder.WhereCondition = originalWhereCondition;

      _visitor.AddPreparedFromExpression (preparedFromExpressionInfo);

      Assert.That (_visitor.SqlStatementBuilder.WhereCondition, Is.SameAs (originalWhereCondition));
    }

    [Test]
    public void AddJoinClause ()
    {
      var joinClause = ExpressionHelper.CreateJoinClause<Cook>();
      
      var preparedFromExpressionInfo = CreateFromExpressionInfo();
      _stageMock
          .Setup (
              mock => mock.PrepareFromExpression (
                  It.Is<Expression> (e => e == joinClause.InnerSequence),
                  It.Is<ISqlPreparationContext> (c => c != _context),
                  OrderingExtractionPolicy.ExtractOrderingsIntoProjection))
          .Returns (preparedFromExpressionInfo)
          .Verifiable();

      var preparedExpression = new SqlTableReferenceExpression (preparedFromExpressionInfo.AppendedTable.SqlTable);
      _stageMock
          .Setup (
              mock => mock.PrepareWhereExpression (
                  It.Is<Expression> (
                      e => ((BinaryExpression)e).Left == joinClause.OuterKeySelector
                           && ((BinaryExpression)e).Right == joinClause.InnerKeySelector),
                  It.Is<ISqlPreparationContext> (c => c != _context)))
          .Returns (preparedExpression)
          .Verifiable();

      var result = _visitor.AddJoinClause (joinClause);

      _stageMock.Verify();
      Assert.That (result, Is.SameAs (preparedFromExpressionInfo.AppendedTable.SqlTable));
      Assert.That (_visitor.SqlStatementBuilder.WhereCondition, Is.SameAs (preparedExpression));
    }

    private static FromExpressionInfo CreateFromExpressionInfo (Expression whereCondition = null, Ordering[] orderings = null)
    {
      var preparedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable();
      return new FromExpressionInfo (
          preparedTable, 
          orderings ?? new Ordering[] { }, 
          new SqlTableReferenceExpression (preparedTable.SqlTable), 
          whereCondition);
    }
  }
}