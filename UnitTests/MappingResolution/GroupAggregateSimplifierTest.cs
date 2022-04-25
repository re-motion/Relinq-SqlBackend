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
using Moq;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.NUnit;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class GroupAggregateSimplifierTest
  {
    private StreamedScalarValueInfo _dataInfo;

    private SqlGroupingSelectExpression _associatedGroupingSelectExpression;
    private SqlStatement _resolvedJoinedGroupingSubStatement;
    private SqlTable _resolvedJoinedGroupingTable;
    private SqlColumnDefinitionExpression _resolvedElementExpressionReference;
    private NamedExpression _resolvedSelectProjection;
    private SqlStatement _simplifiableResolvedSqlStatement;
    private AggregationExpression _simplifiableUnresolvedProjection;

    private Mock<IMappingResolutionStage> _stageMock;
    private MappingResolutionContext _context;

    private GroupAggregateSimplifier _groupAggregateSimplifier;

    [SetUp]
    public void SetUp ()
    {
      _dataInfo = new StreamedScalarValueInfo (typeof (int));

      _resolvedElementExpressionReference = new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false);
      _resolvedSelectProjection = new NamedExpression (
          null, 
          new AggregationExpression (typeof (int), _resolvedElementExpressionReference, AggregationModifier.Min));

      _associatedGroupingSelectExpression = new SqlGroupingSelectExpression (
          new NamedExpression ("key", Expression.Constant ("k")),
          new NamedExpression ("element", Expression.Constant ("e")));

      _resolvedJoinedGroupingSubStatement = SqlStatementModelObjectMother.CreateSqlStatement (_associatedGroupingSelectExpression);
      _resolvedJoinedGroupingTable = new SqlTable (
          new ResolvedJoinedGroupingTableInfo (
              "q1",
              _resolvedJoinedGroupingSubStatement,
              _associatedGroupingSelectExpression,
              "q0"), JoinSemantics.Inner);

      _simplifiableResolvedSqlStatement = SqlStatementModelObjectMother.CreateMinimalSqlStatement (
          new SqlStatementBuilder
          {
              DataInfo = _dataInfo,
              SelectProjection = _resolvedSelectProjection,
              SqlTables = { SqlStatementModelObjectMother.CreateSqlAppendedTable (_resolvedJoinedGroupingTable) }
          });

      _simplifiableUnresolvedProjection = new AggregationExpression (
          typeof (int),
          new SqlTableReferenceExpression (_resolvedJoinedGroupingTable),
          AggregationModifier.Count);

      _stageMock = new Mock<IMappingResolutionStage> (MockBehavior.Strict);
      _context = new MappingResolutionContext();

      _groupAggregateSimplifier = new GroupAggregateSimplifier (_stageMock.Object, _context);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_True_WithReferenceToJoinGroupInAggregationExpression ()
    {
      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (_simplifiableResolvedSqlStatement), Is.True);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_True_WithNamedExpressionInAggregationExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        SelectProjection = new AggregationExpression (
          typeof (int), new NamedExpression ("value", _resolvedElementExpressionReference), AggregationModifier.Min)
      }.GetSqlStatement();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.True);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_True_WithAggregationExpressionInNamedExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        SelectProjection = new NamedExpression (
            null, 
            new AggregationExpression (typeof (int), _resolvedElementExpressionReference, AggregationModifier.Min))
      }.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.True);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_True_WithNonReferenceExpressionInAggregationExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        SelectProjection = new AggregationExpression (typeof (int), Expression.Constant (0), AggregationModifier.Min)
      }.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.True);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_NoAggregationExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        SelectProjection = _resolvedElementExpressionReference
      }.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_WhereExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        WhereCondition = Expression.Constant (true)
      }.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_OrderingExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        Orderings = { new Ordering(Expression.Constant("order"), OrderingDirection.Asc) }
      }.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_GroupByExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        GroupByExpression = Expression.Constant(0)
      }.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_TooManySqlTables ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (_simplifiableResolvedSqlStatement);
      sqlStatementBuilder.SqlTables.Add (SqlStatementModelObjectMother.CreateSqlAppendedTable(_resolvedJoinedGroupingTable));
      var sqlStatement = sqlStatementBuilder.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_NoResolvedJoinedGroupingTableInfo ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (_simplifiableResolvedSqlStatement);
      sqlStatementBuilder.SqlTables.Clear();
      sqlStatementBuilder.SqlTables.Add (
          SqlStatementModelObjectMother.CreateSqlAppendedTable (
              SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), "table", "t0"))));
      var sqlStatement = sqlStatementBuilder.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_TopExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        TopExpression = Expression.Constant (0)
      }.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_DistinctQuery()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        IsDistinctQuery = true
      }.GetSqlStatement ();

      Assert.That (_groupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void SimplifyIfPossible_NonSimplifiableStatement ()
    {
      var resolvedSqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        IsDistinctQuery = true
      }.GetSqlStatement ();
      var expression = new SqlSubStatementExpression (resolvedSqlStatement);

      var result = _groupAggregateSimplifier.SimplifyIfPossible (expression, _simplifiableUnresolvedProjection);

      _stageMock.Verify();

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void SimplifyIfPossible_SimplifiableStatement_AddsAggregationAndReturnsReference ()
    {
      Assert.That (_associatedGroupingSelectExpression.AggregationExpressions.Count, Is.EqualTo (0));

      var sqlSubStatementExpression = new SqlSubStatementExpression (_simplifiableResolvedSqlStatement);

      var preparedResolvedAggregate = new AggregationExpression (
          typeof (int),
          new NamedExpression ("element", Expression.Constant ("e")),
          AggregationModifier.Count);
      _stageMock
          .Setup (mock => mock.ResolveAggregationExpression(It.IsAny<Expression>(), _context))
          .Returns (preparedResolvedAggregate)
          .Callback ((Expression expressionArgument, IMappingResolutionContext context) => {
            var expectedReplacedAggregate = new AggregationExpression (
                typeof (int),
                ((NamedExpression) _associatedGroupingSelectExpression.ElementExpression).Expression,
                AggregationModifier.Count);
            SqlExpressionTreeComparer.CheckAreEqualTrees (expectedReplacedAggregate, expressionArgument);
          })
          .Verifiable();

      var result = _groupAggregateSimplifier.SimplifyIfPossible (sqlSubStatementExpression, _simplifiableUnresolvedProjection);

      _stageMock.Verify();

      Assert.That (_associatedGroupingSelectExpression.AggregationExpressions.Count, Is.EqualTo (1));
      Assert.That (
          ((NamedExpression) _associatedGroupingSelectExpression.AggregationExpressions[0]).Expression, 
          Is.SameAs (preparedResolvedAggregate));

      var expected = new SqlColumnDefinitionExpression (typeof (int), "q0", "a0", false);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void SimplifyIfPossible_WithNonSimplifiableProjection_ReturnsOriginalStatement ()
    {
      Assert.That (_associatedGroupingSelectExpression.AggregationExpressions.Count, Is.EqualTo (0));
      var expression = new SqlSubStatementExpression (_simplifiableResolvedSqlStatement);

      var nonSimplifiableProjection = new AggregationExpression (
          typeof (int), 
          new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable ()), AggregationModifier.Count);
      var result = _groupAggregateSimplifier.SimplifyIfPossible (expression, nonSimplifiableProjection);

      _stageMock.Verify();

      var expected = new SqlSubStatementExpression (_simplifiableResolvedSqlStatement);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);

      Assert.That (_associatedGroupingSelectExpression.AggregationExpressions.Count, Is.EqualTo (0));
    }

    [Test]
    public void SimplifyIfPossible_WithUnresolvedProjection_NotMatchingResolvedOned_NoAggregation ()
    {
      var expression = new SqlSubStatementExpression (_simplifiableResolvedSqlStatement);

      var nonSimplifiableProjection = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable ());
      Assert.That (
          () => _groupAggregateSimplifier.SimplifyIfPossible (expression, nonSimplifiableProjection),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "The unresolved projection doesn't match the resolved statement: it has no aggregation.", "unresolvedSelectProjection"));
    }

    [Test]
    public void Visit_ReferenceToRightTable ()
    {
      var visitor = new GroupAggregateSimplifier.SimplifyingVisitor (_resolvedJoinedGroupingTable, _associatedGroupingSelectExpression.ElementExpression);

      var input = new SqlTableReferenceExpression (_resolvedJoinedGroupingTable);
      var result = visitor.Visit (input);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.True);
      Assert.That (result, Is.SameAs (_associatedGroupingSelectExpression.ElementExpression));
    }

    [Test]
    public void Visit_ReferenceToRightTable_Nested ()
    {
      var visitor = new GroupAggregateSimplifier.SimplifyingVisitor (_resolvedJoinedGroupingTable, _associatedGroupingSelectExpression.ElementExpression);

      var input = Expression.Equal (
          new SqlTableReferenceExpression (_resolvedJoinedGroupingTable), 
          new SqlTableReferenceExpression (_resolvedJoinedGroupingTable));

      var result = visitor.Visit (input);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.True);
      var expectedResult = Expression.ReferenceEqual (
          _associatedGroupingSelectExpression.ElementExpression,
          _associatedGroupingSelectExpression.ElementExpression);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Visit_ReferenceToOtherTable ()
    {
      var visitor = new GroupAggregateSimplifier.SimplifyingVisitor (_resolvedJoinedGroupingTable, _associatedGroupingSelectExpression.ElementExpression);

      var input = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable());
      visitor.Visit (input);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.False);
    }

    [Test]
    public void Visit_AnyOtherExpression ()
    {
      var visitor = new GroupAggregateSimplifier.SimplifyingVisitor (_resolvedJoinedGroupingTable, _associatedGroupingSelectExpression.ElementExpression);

      var input = Expression.Constant (0);
      var result = visitor.Visit (input);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.True);
      Assert.That (result, Is.SameAs (input));
    }
  }
}