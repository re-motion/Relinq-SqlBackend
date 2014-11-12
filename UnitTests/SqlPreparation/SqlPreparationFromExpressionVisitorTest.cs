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
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Remotion.Linq.SqlBackend.UnitTests.Utilities;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationFromExpressionVisitorTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private ISqlPreparationContext _context;
    private IMethodCallTransformerProvider _methodCallTransformerProvider;
    private OrderingExtractionPolicy _someOrderingExtractionPolicy;
    private Func<ITableInfo, SqlTable> _tableGenerator;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<ISqlPreparationStage>();
      _generator = new UniqueIdentifierGenerator();
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext();
      _methodCallTransformerProvider = CompoundMethodCallTransformerProvider.CreateDefault();

      _someOrderingExtractionPolicy = Some.Item (
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection,
          OrderingExtractionPolicy.DoNotExtractOrderings);

      _tableGenerator = info => new SqlTable (info, JoinSemantics.Inner);
    }

    [Test]
    public void GetTableForFromExpression_ConstantExpression_ReturnsUnresolvedTable ()
    {
      var expression = Expression.Constant (new Cook[0]);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression,
          _stageMock,
          _generator,
          _methodCallTransformerProvider,
          _context,
          _tableGenerator,
          _someOrderingExtractionPolicy);

      Assert.That (result.SqlTable, Is.TypeOf (typeof (SqlTable)));

      var tableInfo = result.SqlTable.TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (UnresolvedTableInfo)));

      Assert.That (tableInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void GetTableForFromExpression_SqlMemberExpression_ReturnsSqlTableWithJoinedTable ()
    {
      // from r in Restaurant => sqlTable 
      // from c in r.Cooks => MemberExpression (QSRExpression (r), "Cooks") => Join: sqlTable.Cooks

      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var memberExpression = Expression.MakeMemberAccess (Expression.Constant (new Restaurant()), memberInfo);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          memberExpression,
          _stageMock,
          _generator,
          _methodCallTransformerProvider,
          _context,
          _tableGenerator,
          _someOrderingExtractionPolicy);

      Assert.That (result.SqlTable.TableInfo, Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (((SqlJoinedTable) result.SqlTable.TableInfo).JoinSemantics, Is.EqualTo (JoinSemantics.Inner));

      var joinInfo = ((SqlJoinedTable) result.SqlTable.TableInfo).JoinInfo;

      Assert.That (joinInfo, Is.TypeOf (typeof (UnresolvedCollectionJoinInfo)));

      Assert.That (((UnresolvedCollectionJoinInfo) joinInfo).MemberInfo, Is.EqualTo (memberInfo));
      Assert.That (joinInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Error parsing expression 'CustomExpression'. Expressions of type 'Cook[]' cannot be used as the SqlTables of a from clause.")]
    public void GetTableForFromExpression_UnsupportedExpression_Throws ()
    {
      var customExpression = new CustomExpression (typeof (Cook[]));

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          customExpression, _stageMock, _generator, _methodCallTransformerProvider, _context, _tableGenerator, _someOrderingExtractionPolicy);
    }

    [ExpectedException (typeof (NotSupportedException))]
    [Test]
    public void VisitEntityRefMemberExpression_ThrowsNotSupportException ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Restaurant));
      var expression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _methodCallTransformerProvider, _context, _tableGenerator, _someOrderingExtractionPolicy);
    }

    [Test]
    public void VisitSqlSubStatementExpression_WithExtractOrderingsPolicy ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
                         {
                             SelectProjection = new NamedExpression ("test", Expression.Constant ("test")),
                             Orderings = { SqlStatementModelObjectMother.CreateOrdering() }
                         }.GetSqlStatement();

      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var stage = new DefaultSqlPreparationStage (_methodCallTransformerProvider, new ResultOperatorHandlerRegistry(), _generator);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          sqlSubStatementExpression,
          stage,
          _generator,
          _methodCallTransformerProvider,
          _context,
          _tableGenerator,
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      Assert.That (result.SqlTable.TableInfo, Is.InstanceOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((ResolvedSubStatementTableInfo) result.SqlTable.TableInfo).SqlStatement.Orderings.Count, Is.EqualTo (0));
      Assert.That (result.ExtractedOrderings, Is.Not.Empty);
    }

    [Test]
    public void VisitSqlSubStatementExpression_WithDoNotExtractOrderingsPolicy ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
                         {
                             SelectProjection = new NamedExpression ("test", Expression.Constant ("test")),
                             Orderings = { SqlStatementModelObjectMother.CreateOrdering() }
                         }.GetSqlStatement();

      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var stage = new DefaultSqlPreparationStage (_methodCallTransformerProvider, new ResultOperatorHandlerRegistry(), _generator);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          sqlSubStatementExpression,
          stage,
          _generator,
          _methodCallTransformerProvider,
          _context,
          _tableGenerator,
          OrderingExtractionPolicy.DoNotExtractOrderings);

      Assert.That (result.SqlTable.TableInfo, Is.InstanceOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((ResolvedSubStatementTableInfo) result.SqlTable.TableInfo).SqlStatement.Orderings.Count, Is.EqualTo (0));
      Assert.That (result.ExtractedOrderings, Is.Empty);
    }

    [Test]
    public void VisitSqlSubStatementExpression_WithNonBooleanSqlGroupingSelectExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
                         {
                             SelectProjection =
                                 new NamedExpression (
                                     "test", new SqlGroupingSelectExpression (Expression.Constant ("key"), Expression.Constant ("element")))
                         }.GetSqlStatement();

      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var stage = new DefaultSqlPreparationStage (_methodCallTransformerProvider, new ResultOperatorHandlerRegistry(), _generator);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          sqlSubStatementExpression,
          stage,
          _generator,
          _methodCallTransformerProvider,
          _context,
          _tableGenerator, 
          _someOrderingExtractionPolicy);

      Assert.That (result.SqlTable.TableInfo, Is.InstanceOf (typeof (ResolvedSubStatementTableInfo)));
    }

    [Test]
    public void VisitMemberExpression ()
    {
      var memberExpression = Expression.MakeMemberAccess (Expression.Constant (new Cook()), typeof (Cook).GetProperty ("IllnessDays"));
      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          memberExpression,
          _stageMock,
          _generator,
          _methodCallTransformerProvider,
          _context,
          _tableGenerator,
          _someOrderingExtractionPolicy);

      Assert.That (result.SqlTable, Is.TypeOf (typeof (SqlTable)));
      Assert.That (result.SqlTable.TableInfo, Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (((SqlJoinedTable) result.SqlTable.TableInfo).JoinInfo, Is.TypeOf (typeof (UnresolvedCollectionJoinInfo)));
      Assert.That (
          ((UnresolvedCollectionJoinInfo) ((SqlJoinedTable) result.SqlTable.TableInfo).JoinInfo).SourceExpression,
          Is.EqualTo (memberExpression.Expression));
      Assert.That (
          ((UnresolvedCollectionJoinInfo) ((SqlJoinedTable) result.SqlTable.TableInfo).JoinInfo).MemberInfo,
          Is.EqualTo (memberExpression.Member));
      var expectedWherecondition = new JoinConditionExpression (((SqlJoinedTable) result.SqlTable.TableInfo));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedWherecondition, result.WhereCondition);
    }

    [Test]
    public void VisitMemberExpression_InnerExpressionIsPrepared ()
    {
      var fakeQuerySource = MockRepository.GenerateStub<IQuerySource>();
      fakeQuerySource.Stub (stub => stub.ItemType).Return (typeof (Cook));

      var replacement = Expression.Constant (null, typeof (Cook));
      _context.AddExpressionMapping (new QuerySourceReferenceExpression (fakeQuerySource), replacement);

      var memberExpression = Expression.MakeMemberAccess (
          new QuerySourceReferenceExpression (fakeQuerySource), typeof (Cook).GetProperty ("IllnessDays"));
      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          memberExpression,
          _stageMock,
          _generator,
          _methodCallTransformerProvider,
          _context,
          _tableGenerator,
          _someOrderingExtractionPolicy);

      var sqlTable = result.SqlTable;
      var joinedTable = (SqlJoinedTable) sqlTable.TableInfo;
      Assert.That (((UnresolvedCollectionJoinInfo) joinedTable.JoinInfo).SourceExpression, Is.SameAs (replacement));
    }

    [Test]
    public void VisitSqlTableReferenceExpression_Grouping ()
    {
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (IGrouping<string, int>), "test", "t0"), JoinSemantics.Inner);
      var expression = new SqlTableReferenceExpression (sqlTable);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression,
          _stageMock,
          _generator,
          _methodCallTransformerProvider,
          _context,
          _tableGenerator,
          _someOrderingExtractionPolicy);

      Assert.That (result.SqlTable, Is.Not.SameAs (sqlTable));
      Assert.That (result.WhereCondition, Is.Null);
      Assert.That (result.ExtractedOrderings, Is.Empty);

      var expectedItemSelector = new SqlTableReferenceExpression (result.SqlTable);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedItemSelector, result.ItemSelector);

      var tableInfo = result.SqlTable.TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (UnresolvedGroupReferenceTableInfo)));
      var castTableInfo = (UnresolvedGroupReferenceTableInfo) tableInfo;
      Assert.That (castTableInfo.ItemType, Is.SameAs (typeof (int)));
      Assert.That (castTableInfo.ReferencedGroupSource, Is.SameAs (sqlTable));
      Assert.That (result.SqlTable.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityRefMemberExpression ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Restaurant));
      var expression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _methodCallTransformerProvider, _context, _tableGenerator, _someOrderingExtractionPolicy);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityConstantExpression ()
    {
      var expression = new SqlEntityConstantExpression (typeof (Cook), "test", new SqlLiteralExpression (12));

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, _stageMock, _generator, _methodCallTransformerProvider, _context, _tableGenerator, _someOrderingExtractionPolicy);
    }

    [Test]
    public void VisitQuerySourceReferenceExpression ()
    {
      var innerSequenceExpression = Expression.Constant (new[] { new Cook() });
      var joinClause = new JoinClause (
          "x",
          typeof (Cook[]),
          innerSequenceExpression,
          Expression.Constant (new Cook()),
          Expression.Constant (new Cook()));
      var groupJoinClause = new GroupJoinClause ("g", typeof (Cook[]), joinClause);
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (groupJoinClause);
      var fakeWhereExpression = Expression.Constant (true);

      _stageMock
          .Expect (mock => mock.PrepareWhereExpression (Arg<Expression>.Matches (e => e is BinaryExpression), Arg.Is (_context)))
          .WhenCalled (
              mi =>
              SqlExpressionTreeComparer.CheckAreEqualTrees (
                  Expression.Equal (groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause.JoinClause.InnerKeySelector),
                  (Expression) mi.Arguments[0]))
          .Return (fakeWhereExpression);
      _stageMock.Replay();

      var visitor = CreateTestableVisitor(_someOrderingExtractionPolicy);
      var result = visitor.VisitQuerySourceReferenceExpression (querySourceReferenceExpression);

      _stageMock.VerifyAllExpectations();

      Debug.Assert (visitor.FromExpressionInfo != null, "_visitor.FromExpressionInfo != null");
      var fromExpressionInfo = (FromExpressionInfo) visitor.FromExpressionInfo;

      SqlExpressionTreeComparer.CheckAreEqualTrees (
          new SqlTableReferenceExpression (fromExpressionInfo.SqlTable),
          _context.GetExpressionMapping (new QuerySourceReferenceExpression (groupJoinClause.JoinClause)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (fromExpressionInfo.ItemSelector, result);
      Assert.That (((UnresolvedTableInfo) fromExpressionInfo.SqlTable.TableInfo).ItemType, Is.EqualTo (typeof (Cook)));
      Assert.That (fromExpressionInfo.WhereCondition, Is.SameAs (fakeWhereExpression));
      Assert.That (fromExpressionInfo.ExtractedOrderings.Count, Is.EqualTo (0));
    }

    [Test]
    public void VisitQuerySourceReferenceExpression_WithNonNullWhereCondition ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Assistants");
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook)));
      var innerSequenceExpression =
          Expression.MakeMemberAccess (
              sqlTableReferenceExpression, memberInfo);
      var joinClause = new JoinClause (
          "x",
          typeof (Cook[]),
          innerSequenceExpression,
          Expression.Constant (new Cook()),
          Expression.Constant (new Cook()));
      var groupJoinClause = new GroupJoinClause ("g", typeof (Cook[]), joinClause);
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (groupJoinClause);
      var fakeWhereExpression = Expression.Constant (true);

      _stageMock
          .Expect (mock => mock.PrepareWhereExpression (Arg<Expression>.Matches (e => e is BinaryExpression), Arg.Is (_context)))
          .WhenCalled (
              mi =>
              SqlExpressionTreeComparer.CheckAreEqualTrees (
                  Expression.Equal (groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause.JoinClause.InnerKeySelector),
                  (Expression) mi.Arguments[0]))
          .Return (fakeWhereExpression);
      _stageMock.Replay();

      var visitor = CreateTestableVisitor(_someOrderingExtractionPolicy);
      visitor.VisitQuerySourceReferenceExpression (querySourceReferenceExpression);

      _stageMock.VerifyAllExpectations();

      Assert.That (visitor.FromExpressionInfo != null); // inline condition because of ReSharper
      var fromExpressionInfo = (FromExpressionInfo) visitor.FromExpressionInfo;

      Assert.That (fromExpressionInfo.WhereCondition, Is.AssignableTo (typeof (BinaryExpression)));
      Assert.That (fromExpressionInfo.WhereCondition.NodeType, Is.EqualTo(ExpressionType.AndAlso));
      Assert.That (((BinaryExpression) fromExpressionInfo.WhereCondition).Left, Is.TypeOf(typeof(JoinConditionExpression)));
      Assert.That (((JoinConditionExpression) ((BinaryExpression) fromExpressionInfo.WhereCondition).Left).JoinedTable.JoinInfo, Is.TypeOf (typeof (UnresolvedCollectionJoinInfo)));
      Assert.That (
          ((UnresolvedCollectionJoinInfo) ((JoinConditionExpression) ((BinaryExpression) fromExpressionInfo.WhereCondition).Left).JoinedTable.JoinInfo).SourceExpression, Is.SameAs(sqlTableReferenceExpression));
      Assert.That (
          ((UnresolvedCollectionJoinInfo) ((JoinConditionExpression) ((BinaryExpression) fromExpressionInfo.WhereCondition).Left).JoinedTable.JoinInfo).MemberInfo, Is.SameAs (memberInfo));
      Assert.That (((JoinConditionExpression) ((BinaryExpression) fromExpressionInfo.WhereCondition).Left).JoinedTable.JoinSemantics, Is.EqualTo(JoinSemantics.Inner));
      Assert.That (((BinaryExpression) fromExpressionInfo.WhereCondition).Right, Is.SameAs (fakeWhereExpression));
    }

    [Test]
    public void VisitQuerySourceReferenceExpression_WithOrderings_AndExtractOrdingsPolicy ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      var selectProjection = new NamedExpression("test", Expression.MakeMemberAccess (sqlTableReferenceExpression, typeof (Cook).GetProperty ("Name")));
      var orderingExpression = Expression.MakeMemberAccess (sqlTableReferenceExpression, typeof (Cook).GetProperty ("ID"));
      var sqlStatement = new SqlStatementBuilder
                         {
                             DataInfo = new StreamedSequenceInfo(typeof (DateTime[]), Expression.Constant (new DateTime (2000, 1, 1))),
                             SelectProjection = selectProjection,
                             SqlTables = { sqlTable },
                             Orderings =
                                 { new Ordering (orderingExpression, OrderingDirection.Asc) }
                         }.GetSqlStatement();
      var fakeSelectExpression = Expression.Constant (new KeyValuePair<string, int>("test", 5));
      var fakeWhereExpression = Expression.Constant (true);

      var innerSequenceExpression = new SqlSubStatementExpression (sqlStatement);
          
      var joinClause = new JoinClause (
          "x",
          typeof (Cook[]),
          innerSequenceExpression,
          Expression.Constant (new Cook ()),
          Expression.Constant (new Cook ()));
      var groupJoinClause = new GroupJoinClause ("g", typeof (Cook[]), joinClause);
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (groupJoinClause);

      _stageMock
          .Expect (mock => mock.PrepareSelectExpression(Arg<Expression>.Is.Anything, Arg<ISqlPreparationContext>.Is.Anything))
          .Return (fakeSelectExpression);
      _stageMock
          .Expect (mock => mock.PrepareWhereExpression (Arg<Expression>.Matches (e => e is BinaryExpression), Arg.Is (_context)))
          .WhenCalled (
              mi =>
              SqlExpressionTreeComparer.CheckAreEqualTrees (
                  Expression.Equal (groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause.JoinClause.InnerKeySelector),
                  (Expression) mi.Arguments[0]))
          .Return (fakeWhereExpression);
      _stageMock.Replay ();

      var visitor = CreateTestableVisitor(OrderingExtractionPolicy.ExtractOrderingsIntoProjection);
      visitor.VisitQuerySourceReferenceExpression (querySourceReferenceExpression);

      _stageMock.VerifyAllExpectations ();

      Assert.That (visitor.FromExpressionInfo != null); // inline condition because of ReSharper
      var fromExpressionInfo = (FromExpressionInfo) visitor.FromExpressionInfo;

      Assert.That (fromExpressionInfo.ExtractedOrderings.Count, Is.EqualTo(1));
      Assert.That (fromExpressionInfo.ExtractedOrderings[0].Expression, Is.AssignableTo(typeof(MemberExpression)));
      Assert.That (((MemberExpression) fromExpressionInfo.ExtractedOrderings[0].Expression).Expression, Is.TypeOf(typeof(SqlTableReferenceExpression)));
      Assert.That (
          ((SqlTableReferenceExpression) ((MemberExpression) fromExpressionInfo.ExtractedOrderings[0].Expression).Expression).SqlTable, Is.TypeOf (typeof (SqlTable)));
      Assert.That (
          ((SqlTable) ((SqlTableReferenceExpression) ((MemberExpression) fromExpressionInfo.ExtractedOrderings[0].Expression).Expression).SqlTable).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      var resolvedSubStatementtableInfo =
         (ResolvedSubStatementTableInfo) ((SqlTable) ((SqlTableReferenceExpression) ((MemberExpression) fromExpressionInfo.ExtractedOrderings[0].Expression).Expression).SqlTable).TableInfo;
      Assert.That (resolvedSubStatementtableInfo.SqlStatement.SelectProjection, Is.SameAs(fakeSelectExpression));
      Assert.That (resolvedSubStatementtableInfo.SqlStatement.Orderings.Count, Is.EqualTo (0));
      Assert.That (((MemberExpression) fromExpressionInfo.ExtractedOrderings[0].Expression).Member, Is.EqualTo(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), typeof(int)).GetProperty("Value")));
      Assert.That (fromExpressionInfo.ItemSelector, Is.AssignableTo (typeof (MemberExpression)));
      Assert.That (((MemberExpression) fromExpressionInfo.ItemSelector).Expression, Is.TypeOf(typeof(SqlTableReferenceExpression)));
      Assert.That (((SqlTableReferenceExpression) ((MemberExpression) fromExpressionInfo.ItemSelector).Expression).SqlTable, Is.TypeOf (typeof (SqlTable)));
      Assert.That (
          ((SqlTable) ((SqlTableReferenceExpression) ((MemberExpression) fromExpressionInfo.ItemSelector).Expression).SqlTable).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((MemberExpression) fromExpressionInfo.ItemSelector).Member, Is.EqualTo (typeof (KeyValuePair<,>).MakeGenericType (typeof (string), typeof (int)).GetProperty ("Key")));
    }

    [Test]
    public void VisitQuerySourceReferenceExpression_WithOrderings_AndDoNotExtractOrdingsPolicy ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      var sqlTableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      var selectProjection = new NamedExpression("test", Expression.MakeMemberAccess (sqlTableReferenceExpression, typeof (Cook).GetProperty ("Name")));
      var orderingExpression = Expression.MakeMemberAccess (sqlTableReferenceExpression, typeof (Cook).GetProperty ("ID"));
      var sqlStatement = new SqlStatementBuilder
                         {
                             DataInfo = new StreamedSequenceInfo(typeof (DateTime[]), Expression.Constant (new DateTime (2000, 1, 1))),
                             SelectProjection = selectProjection,
                             SqlTables = { sqlTable },
                             Orderings =
                                 { new Ordering (orderingExpression, OrderingDirection.Asc) }
                         }.GetSqlStatement();
      var fakeWhereExpression = Expression.Constant (true);

      var innerSequenceExpression = new SqlSubStatementExpression (sqlStatement);
          
      var joinClause = new JoinClause (
          "x",
          typeof (Cook[]),
          innerSequenceExpression,
          Expression.Constant (new Cook ()),
          Expression.Constant (new Cook ()));
      var groupJoinClause = new GroupJoinClause ("g", typeof (Cook[]), joinClause);
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (groupJoinClause);

      _stageMock
          .Expect (mock => mock.PrepareWhereExpression (Arg<Expression>.Matches (e => e is BinaryExpression), Arg.Is (_context)))
          .WhenCalled (
              mi =>
              SqlExpressionTreeComparer.CheckAreEqualTrees (
                  Expression.Equal (groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause.JoinClause.InnerKeySelector),
                  (Expression) mi.Arguments[0]))
          .Return (fakeWhereExpression);
      _stageMock.Replay ();

      var visitor = CreateTestableVisitor(OrderingExtractionPolicy.DoNotExtractOrderings);
      visitor.VisitQuerySourceReferenceExpression (querySourceReferenceExpression);

      _stageMock.VerifyAllExpectations ();

      Assert.That (visitor.FromExpressionInfo != null); // inline condition because of ReSharper
      var fromExpressionInfo = (FromExpressionInfo) visitor.FromExpressionInfo;

      Assert.That (fromExpressionInfo.ExtractedOrderings, Is.Empty);
    }

    private TestableSqlPreparationFromExpressionVisitor CreateTestableVisitor (OrderingExtractionPolicy? orderingExtractionPolicy = null)
    {
      return new TestableSqlPreparationFromExpressionVisitor (
          _generator,
          _stageMock,
          _methodCallTransformerProvider,
          _context,
          _tableGenerator,
          orderingExtractionPolicy ?? _someOrderingExtractionPolicy);
    }
  }
}