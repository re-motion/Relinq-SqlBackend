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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class GroupAggregateSimplifierTest
  {
    private StreamedScalarValueInfo _dataInfo;

    private SqlGroupingSelectExpression _associatedGroupingSelectExpression;
    private SqlStatement _resolvedJoinedGroupingSubStatement;
    private SqlTable _resolvedJoinedGroupingTable;
    private SqlColumnDefinitionExpression _resolvedElementExpressionReference;
    private AggregationExpression _resolvedSelectProjection;
    private SqlStatement _simplifiableResolvedSqlStatement;
    private IMappingResolutionStage _stageMock;
    private MappingResolutionContext _context;

    [SetUp]
    public void SetUp ()
    {
      _dataInfo = new StreamedScalarValueInfo (typeof (int));

      _resolvedElementExpressionReference = new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false);
      _resolvedSelectProjection = new AggregationExpression (
          typeof (int), _resolvedElementExpressionReference, AggregationModifier.Min);

      _associatedGroupingSelectExpression = new SqlGroupingSelectExpression (
          new NamedExpression ("key", Expression.Constant ("k")),
          new NamedExpression ("element", Expression.Constant ("e")));

      _resolvedJoinedGroupingSubStatement = SqlStatementModelObjectMother.CreateSqlStatement (_associatedGroupingSelectExpression);
      _resolvedJoinedGroupingTable = new SqlTable (
          new ResolvedJoinedGroupingTableInfo (
              "q1",
              _resolvedJoinedGroupingSubStatement,
              _associatedGroupingSelectExpression,
              "q0"));

      _simplifiableResolvedSqlStatement = new SqlStatement (
          _dataInfo,
          _resolvedSelectProjection,
          new[] { _resolvedJoinedGroupingTable },
          null,
          null,
          new Ordering[0],
          null,
          false,
          Expression.Constant (0),
          Expression.Constant (0));

      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage> ();
      _context = new MappingResolutionContext();
    }
    
    [Test]
    public void IsSimplifiableGroupAggregate_True_WithReferenceToJoinGroupInAggregationExpression ()
    {
      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (_simplifiableResolvedSqlStatement), Is.True);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_True_WithNamedExpressionInAggregationExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        SelectProjection = new AggregationExpression (
          typeof (int), new NamedExpression ("value", _resolvedElementExpressionReference), AggregationModifier.Min)
      }.GetSqlStatement();
      
      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.True);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_True_WithNonReferenceExpressionInAggregationExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        SelectProjection = new AggregationExpression (typeof (int), Expression.Constant (0), AggregationModifier.Min)
      }.GetSqlStatement ();

      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.True);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_NoAggregationExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        SelectProjection = _resolvedElementExpressionReference
      }.GetSqlStatement ();
      
      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_WhereExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        WhereCondition = Expression.Constant (true)
      }.GetSqlStatement ();

      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_OrderingExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        Orderings = { new Ordering(Expression.Constant("order"), OrderingDirection.Asc) }
      }.GetSqlStatement ();

      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_GroupByExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        GroupByExpression = Expression.Constant(0)
      }.GetSqlStatement ();

      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_TooManySqlTables ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (_simplifiableResolvedSqlStatement);
      sqlStatementBuilder.SqlTables.Add (SqlStatementModelObjectMother.CreateSqlTable());
      var sqlStatement = sqlStatementBuilder.GetSqlStatement ();

      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_NoResolvedJoinedGroupingTableInfo ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (_simplifiableResolvedSqlStatement);
      sqlStatementBuilder.SqlTables.Clear();
      sqlStatementBuilder.SqlTables.Add (SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), "table", "t0")));
      var sqlStatement = sqlStatementBuilder.GetSqlStatement ();

      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_TopExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        TopExpression = Expression.Constant (0)
      }.GetSqlStatement ();

      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void IsSimplifiableGroupAggregate_False_DistinctQuery()
    {
      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        IsDistinctQuery = true
      }.GetSqlStatement ();

      Assert.That (GroupAggregateSimplifier.IsSimplifiableGroupAggregate (sqlStatement), Is.False);
    }

    [Test]
    public void SimplifyIfPossible_NonSimplifiableStatement ()
    {
      var resolvedSqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        IsDistinctQuery = true
      }.GetSqlStatement ();
      
      _stageMock.Replay();

      var result = GroupAggregateSimplifier.SimplifyIfPossible (resolvedSqlStatement, _stageMock, _context);

      _stageMock.VerifyAllExpectations();

      var expected = new SqlSubStatementExpression (resolvedSqlStatement);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void SimplifyIfPossible_SimplifiableStatement_AddsAggregationAndReturnsReference ()
    {
      Assert.That (_associatedGroupingSelectExpression.AggregationExpressions.Count, Is.EqualTo (0));

      _stageMock
          .Expect (
              mock => mock.ResolveTableReferenceExpression (
                  Arg<SqlTableReferenceExpression>.Matches (e => e.SqlTable == _resolvedJoinedGroupingTable),
                  Arg.Is (_context)))
          .Return (new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false));
      _stageMock.Replay();
      
      var result = GroupAggregateSimplifier.SimplifyIfPossible (_simplifiableResolvedSqlStatement, _stageMock, _context);

      _stageMock.VerifyAllExpectations();

      Assert.That (_associatedGroupingSelectExpression.AggregationExpressions.Count, Is.EqualTo (1));
      var expectedAggregate = new NamedExpression ("a0", new AggregationExpression (
          typeof (int), 
          new NamedExpression ("element", Expression.Constant ("e")), 
          AggregationModifier.Min));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedAggregate, _associatedGroupingSelectExpression.AggregationExpressions[0]);

      var expected = new SqlColumnDefinitionExpression (typeof (int), "q0", "a0", false);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void SimplifyIfPossible_WithInvalidReferenceInAggregationExpression_ReturnsOriginalStatement ()
    {
      Assert.That (_associatedGroupingSelectExpression.AggregationExpressions.Count, Is.EqualTo (0));

      var sqlStatement = new SqlStatementBuilder (_simplifiableResolvedSqlStatement)
      {
        SelectProjection = new AggregationExpression (
            typeof (int),
            new SqlColumnDefinitionExpression (typeof (int), "q3", "test", false),
            AggregationModifier.Min)
      }.GetSqlStatement ();

      _stageMock
          .Expect (
              mock => mock.ResolveTableReferenceExpression (
                  Arg<SqlTableReferenceExpression>.Matches (e => e.SqlTable == _resolvedJoinedGroupingTable),
                  Arg.Is (_context)))
          .Return (new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false));
      _stageMock.Replay ();

      var result = GroupAggregateSimplifier.SimplifyIfPossible (sqlStatement, _stageMock, _context);

      _stageMock.VerifyAllExpectations ();

      var expected = new SqlSubStatementExpression (sqlStatement);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitExpression_TransformsAggregationContainingColumn ()
    {
      var elementReference = new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false);
      var elementExpressionToBeUsed = Expression.Constant ("definition");
      var visitor = new TestableGroupAggregateSimplifier (elementReference, elementExpressionToBeUsed);

      var aggregationExpression = new AggregationExpression (
          typeof (int), 
          new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false), 
          AggregationModifier.Count);

      var result = visitor.VisitExpression (aggregationExpression);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.True);

      var expectedResult = new AggregationExpression (typeof (int), elementExpressionToBeUsed, AggregationModifier.Count);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitExpression_TransformsAggregationContainingDifferentColumn_TableAlias ()
    {
      var elementReference = new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false);
      var elementExpressionToBeUsed = Expression.Constant ("definition");
      var visitor = new TestableGroupAggregateSimplifier (elementReference, elementExpressionToBeUsed);

      var aggregationExpression = new AggregationExpression (
          typeof (int), 
          new SqlColumnDefinitionExpression (typeof (string), "q1", "element", false), 
          AggregationModifier.Count);

      visitor.VisitExpression (aggregationExpression);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.False);
    }

    [Test]
    public void VisitExpression_TransformsAggregationContainingColumn_ButElementReferenceIsNoColumn ()
    {
      var elementReference = Expression.Constant ("test");
      var elementExpressionToBeUsed = Expression.Constant ("definition");
      var visitor = new TestableGroupAggregateSimplifier (elementReference, elementExpressionToBeUsed);

      var aggregationExpression = new AggregationExpression (
          typeof (int),
          new SqlColumnDefinitionExpression (typeof (string), "q0", "element2", false),
          AggregationModifier.Count);

      visitor.VisitExpression (aggregationExpression);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.False);
    }

    [Test]
    public void VisitExpression_TransformsAggregationContainingEntity ()
    {
      var elementReference = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "q0");
      var elementExpressionToBeUsed = Expression.Constant ("definition");
      var visitor = new TestableGroupAggregateSimplifier (elementReference, elementExpressionToBeUsed);

      var aggregationExpression = new AggregationExpression (
          typeof (int),
          SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "q0"),
          AggregationModifier.Count);

      var result = visitor.VisitExpression (aggregationExpression);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.True);

      var expectedResult = new AggregationExpression (typeof (int), elementExpressionToBeUsed, AggregationModifier.Count);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitExpression_TransformsAggregationContainingDifferentEntity_TableAlias ()
    {
      var elementReference = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "q0");
      var elementExpressionToBeUsed = Expression.Constant ("definition");
      var visitor = new TestableGroupAggregateSimplifier (elementReference, elementExpressionToBeUsed);

      var aggregationExpression = new AggregationExpression (
          typeof (int),
          SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "q1"),
          AggregationModifier.Count);

      visitor.VisitExpression (aggregationExpression);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.False);
    }

    [Test]
    public void VisitExpression_TransformsAggregationContainingEntity_ButElementReferenceIsNoColumn ()
    {
      var elementReference = Expression.Constant ("test");
      var elementExpressionToBeUsed = Expression.Constant ("definition");
      var visitor = new TestableGroupAggregateSimplifier (elementReference, elementExpressionToBeUsed);

      var aggregationExpression = new AggregationExpression (
          typeof (int),
          SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "q0"),
          AggregationModifier.Count);

      visitor.VisitExpression (aggregationExpression);

      Assert.That (visitor.CanBeTransferredToGroupingSource, Is.False);
    }
  }
}