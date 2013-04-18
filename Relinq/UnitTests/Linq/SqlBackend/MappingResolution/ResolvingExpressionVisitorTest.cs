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
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingExpressionVisitorTest
  {
    private IMappingResolver _resolverMock;
    private SqlTable _sqlTable;
    private IMappingResolutionStage _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;
    private UniqueIdentifierGenerator _generator;
    private IEntityIdentityResolver _entityIdentityResolverMock;
    private ICompoundExpressionComparisonSplitter _compoundComparisonSplitterMock;
    private INamedExpressionCombiner _namedExpressionCombinerMock;
    private IGroupAggregateSimplifier _groupAggregateSimplifierMock;

    private ResolvingExpressionVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage>();
      _resolverMock = MockRepository.GenerateStrictMock<IMappingResolver>();
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (Cook));
      _mappingResolutionContext = new MappingResolutionContext();
      _generator = new UniqueIdentifierGenerator();
      _entityIdentityResolverMock = MockRepository.GenerateStrictMock<IEntityIdentityResolver> ();
      _compoundComparisonSplitterMock = MockRepository.GenerateStrictMock<ICompoundExpressionComparisonSplitter> ();
      _namedExpressionCombinerMock = MockRepository.GenerateStrictMock<INamedExpressionCombiner>();
      _groupAggregateSimplifierMock = MockRepository.GenerateStrictMock<IGroupAggregateSimplifier>();

      _visitor = CreateVisitor (true);
    }

    [Test]
    public void VisitConstantExpression ()
    {
      var constantExpression = Expression.Constant (0);

      _stageMock.Replay();
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (constantExpression))
          .Return (constantExpression);
      _resolverMock.Replay ();

      var result = _visitor.VisitExpression (constantExpression);

      _stageMock.VerifyAllExpectations ();
      _resolverMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (constantExpression));
    }

    [Test]
    public void VisitConstantExpression_RevisitsResult ()
    {
      var constantExpression = Expression.Constant (0);
      var fakeResult = Expression.Constant (1);

      _stageMock.Replay ();
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (constantExpression))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay ();

      var result = _visitor.VisitExpression (constantExpression);

      _stageMock.VerifyAllExpectations ();
      _resolverMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitSqlTableReferenceExpression_ResolvesExpression ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _visitor.VisitExpression (tableReferenceExpression);

      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitSqlTableReferenceExpression_RevisitsResult ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _visitor.VisitExpression (tableReferenceExpression);

      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitMemberExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("ID");
      var expression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);

      var fakeResolvedExpression = new SqlLiteralExpression (1);
      _stageMock
          .Expect (mock => mock.ResolveMemberAccess (expression, memberInfo, _resolverMock, _mappingResolutionContext))
          .Return (fakeResolvedExpression);
      _stageMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock, _stageMock, _mappingResolutionContext, _generator);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResolvedExpression));
    }

    [Test]
    public void VisitMemberExpression_ResolvesSourceExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("ID");
      var expression = Expression.Constant (null, typeof (Cook));
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);

      var fakeResolvedSourceExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResolvedSourceExpression);
      _resolverMock.Replay();

      var fakeResolvedExpression = new SqlLiteralExpression (1);
      _stageMock
          .Expect (mock => mock.ResolveMemberAccess (fakeResolvedSourceExpression, memberInfo, _resolverMock, _mappingResolutionContext))
          .Return (fakeResolvedExpression);
      _stageMock.Replay();

      _visitor.VisitExpression (memberExpression);

      _resolverMock.VerifyAllExpectations();
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberExpression_RevisitsResult ()
    {
      var memberInfo = typeof (Cook).GetProperty ("ID");
      var expression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);

      var fakeResult1 = Expression.Constant (1);
      var fakeResult2 = new SqlLiteralExpression (7);
      _stageMock
          .Expect (mock => mock.ResolveMemberAccess (expression, memberInfo, _resolverMock, _mappingResolutionContext))
          .Return (fakeResult1);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult1))
          .Return (fakeResult2);

      var result = _visitor.VisitExpression (memberExpression);

      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult2));
    }

    [Test]
    public void UnknownExpression ()
    {
      var unknownExpression = new CustomExpression (typeof (int));
      var result = _visitor.VisitExpression (unknownExpression);

      Assert.That (result, Is.SameAs (unknownExpression));
    }

    [Test]
    public void VisitSqlSubStatementExpression_NoChanges ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (sqlStatement);
      _groupAggregateSimplifierMock
          .Expect (mock => mock.SimplifyIfPossible (expression, expression.SqlStatement.SelectProjection))
          .Return (expression);

      var result = _visitor.VisitExpression (expression);

      _stageMock.VerifyAllExpectations ();
      _groupAggregateSimplifierMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlSubStatementExpression_WithChanges ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (0));
      var expression = new SqlSubStatementExpression (sqlStatement);

      var fakeResolvedStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (1));
      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (fakeResolvedStatement);
      var fakeSimplifiedExpression = Expression.Constant (0);
      _groupAggregateSimplifierMock
          .Expect (
              mock => mock.SimplifyIfPossible (
                  Arg<SqlSubStatementExpression>.Matches (e => ReferenceEquals (e.SqlStatement, fakeResolvedStatement)),
                  Arg.Is (expression.SqlStatement.SelectProjection)))
          .Return (fakeSimplifiedExpression);

      var result = _visitor.VisitExpression (expression);

      _stageMock.VerifyAllExpectations ();
      _groupAggregateSimplifierMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeSimplifiedExpression));
    }

    [Test]
    public void VisitTypeBinaryExpression ()
    {
      var expression = Expression.Constant ("select");
      var typeBinaryExpression = Expression.TypeIs (expression, typeof (Chef));
      var resolvedTypeExpression = Expression.Constant ("resolved");
      var resolvedRevisitedResult = new SqlLiteralExpression (0);

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (expression);
      _resolverMock.Expect (mock => mock.ResolveTypeCheck (expression, typeof (Chef)))
          .Return (resolvedTypeExpression);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (resolvedTypeExpression))
          .Return (resolvedRevisitedResult);
      _resolverMock.Replay();

      var result = _visitor.VisitExpression (typeBinaryExpression);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (resolvedRevisitedResult));
    }

    [Test]
    public void VisitBinaryExpression()
    {
      var left = Expression.Constant (0);
      var right = Expression.Constant (1);
      var binary = Expression.Equal (left, right);

      var fakeResolvedLeft = new SqlLiteralExpression (2);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (left)).Return (fakeResolvedLeft);
      var fakeResolvedRight = new SqlLiteralExpression (3);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (right)).Return (fakeResolvedRight);

      var fakeResolvedEntityComparison = Expression.Equal (Expression.Constant (2), Expression.Constant (3));
      _entityIdentityResolverMock
          .Expect (
              mock => mock.ResolvePotentialEntityComparison (
                  Arg<BinaryExpression>.Matches (e => e.Left == fakeResolvedLeft && e.Right == fakeResolvedRight)))
          .Return (fakeResolvedEntityComparison);

      var fakeSplitComparison = Expression.Equal (Expression.Constant (4), Expression.Constant (5));
      _compoundComparisonSplitterMock
          .Expect (mock => mock.SplitPotentialCompoundComparison (fakeResolvedEntityComparison))
          .Return (fakeSplitComparison);

      // Result is revisited
      _resolverMock.Expect (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeSplitComparison.Left)).Return (fakeSplitComparison.Left);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeSplitComparison.Right)).Return (fakeSplitComparison.Right);
      _entityIdentityResolverMock.Expect (mock => mock.ResolvePotentialEntityComparison (fakeSplitComparison)).Return (fakeSplitComparison);
      _compoundComparisonSplitterMock.Expect (mock => mock.SplitPotentialCompoundComparison (fakeSplitComparison)).Return (fakeSplitComparison);

      var result = _visitor.VisitExpression (binary);

      _resolverMock.VerifyAllExpectations();
      _entityIdentityResolverMock.VerifyAllExpectations();
      _compoundComparisonSplitterMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeSplitComparison));
    }

    [Test]
    public void VisitBinaryExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges ()
    {
      var left = Expression.Constant (0);
      var right = Expression.Constant (1);
      var binary = Expression.Equal (left, right);

      var fakeResolvedLeft = new SqlLiteralExpression (2);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (left)).Return (fakeResolvedLeft);
      var fakeResolvedRight = new SqlLiteralExpression (3);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (right)).Return (fakeResolvedRight);

      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntityComparison (Arg<BinaryExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);

      _compoundComparisonSplitterMock
          .Expect (mock => mock.SplitPotentialCompoundComparison (Arg<BinaryExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);

      // No revisiting

      var result = _visitor.VisitExpression (binary);

      _resolverMock.VerifyAllExpectations ();
      _entityIdentityResolverMock.VerifyAllExpectations ();
      _compoundComparisonSplitterMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf<BinaryExpression> ());
      Assert.That (((BinaryExpression) result).Left, Is.SameAs (fakeResolvedLeft));
      Assert.That (((BinaryExpression) result).Right, Is.SameAs (fakeResolvedRight));
    }

    [Test]
    public void VisitJoinConditionExpression ()
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var leftKey = new SqlColumnDefinitionExpression (typeof (Cook), "c", "ID", false);
      var rightKey = new SqlColumnDefinitionExpression (typeof (Cook), "a", "FK", false);
      var joinInfo = new ResolvedJoinInfo (resolvedTableInfo, leftKey, rightKey);
      var sqlTable = new SqlJoinedTable (joinInfo, JoinSemantics.Left);
      var joinConditionExpression = new JoinConditionExpression (sqlTable);

      _entityIdentityResolverMock
          .Expect (
              mock => mock.ResolvePotentialEntityComparison (
                  Arg<BinaryExpression>.Matches (b => b.Left == joinInfo.LeftKey && b.Right == joinInfo.RightKey)))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);
      _compoundComparisonSplitterMock
          .Expect (
              mock => mock.SplitPotentialCompoundComparison (
                  Arg<BinaryExpression>.Matches (b => b.Left == joinInfo.LeftKey && b.Right == joinInfo.RightKey)))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);

      var result = _visitor.VisitExpression (joinConditionExpression);

      _entityIdentityResolverMock.VerifyAllExpectations();
      _compoundComparisonSplitterMock.VerifyAllExpectations();

      var expectedExpression = Expression.Equal (joinInfo.LeftKey, joinInfo.RightKey);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
    
    [Test]
    public void VisitJoinConditionExpression_LiftsOperandsIfNecessary ()
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var leftKey = new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false);
      var rightKey = new SqlColumnDefinitionExpression (typeof (int?), "a", "FK", false);
      var joinInfo = new ResolvedJoinInfo (resolvedTableInfo, leftKey, rightKey);

      var sqlTable = new SqlJoinedTable (joinInfo, JoinSemantics.Left);
      var joinConditionExpression = new JoinConditionExpression (sqlTable);

      _entityIdentityResolverMock
          .Stub (mock => mock.ResolvePotentialEntityComparison (Arg<BinaryExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);
      _compoundComparisonSplitterMock
          .Stub (mock => mock.SplitPotentialCompoundComparison (Arg<BinaryExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);

      var result = _visitor.VisitExpression (joinConditionExpression);

      var expectedExpression = Expression.Equal (Expression.Convert (leftKey, typeof (int?)), rightKey);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitNamedExpression ()
    {
      var innerExpression = Expression.Constant (0);
      var namedExpression = new NamedExpression ("Name", innerExpression);

      var fakeResolvedInnerExpression = new SqlLiteralExpression (1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (innerExpression)).Return (fakeResolvedInnerExpression);

      var fakeCombinedExpression = new NamedExpression ("Name2", Expression.Constant (2));
      _namedExpressionCombinerMock
          .Expect (mock => mock.ProcessNames (Arg<NamedExpression>.Matches (e => e.Name == "Name" && e.Expression == fakeResolvedInnerExpression)))
          .Return (fakeCombinedExpression);

      // Result is revisited.
      var fakeResolvedInnerExpression2 = new SqlLiteralExpression (3);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeCombinedExpression.Expression))
          .Return (fakeResolvedInnerExpression2);
      _namedExpressionCombinerMock
          .Expect (mock => mock.ProcessNames (Arg<NamedExpression>.Matches (e => e.Name == "Name2" && e.Expression == fakeResolvedInnerExpression2)))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);

      var result = _visitor.VisitExpression (namedExpression);

      _resolverMock.VerifyAllExpectations ();
      _namedExpressionCombinerMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf<NamedExpression> ());
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("Name2"));
      Assert.That (((NamedExpression) result).Expression, Is.SameAs (fakeResolvedInnerExpression2));
    }

    [Test]
    public void VisitNamedExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges ()
    {
      var innerExpression = Expression.Constant (0);
      var namedExpression = new NamedExpression ("Name", innerExpression);

      var fakeResolvedInnerExpression = new SqlLiteralExpression (1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (innerExpression)).Return (fakeResolvedInnerExpression);

      _namedExpressionCombinerMock
          .Expect (mock => mock.ProcessNames (Arg<NamedExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);

      // No revisiting

      var result = _visitor.VisitExpression (namedExpression);

      _resolverMock.VerifyAllExpectations ();
      _namedExpressionCombinerMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf<NamedExpression> ());
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("Name"));
      Assert.That (((NamedExpression) result).Expression, Is.SameAs (fakeResolvedInnerExpression));
    }

    [Test]
    public void VisitSqlExistsExpression ()
    {
      var inner = Expression.Constant (0);
      var existsExpression = new SqlExistsExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (inner)).Return (fakeResolvedInner);

      var fakeResolvedEntityIdentity = Expression.Constant (1);
      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntity (fakeResolvedInner))
          .Return (fakeResolvedEntityIdentity);

      // Result is revisited
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (fakeResolvedEntityIdentity)).Return (fakeResolvedEntityIdentity);
      _entityIdentityResolverMock.Expect (mock => mock.ResolvePotentialEntity (fakeResolvedEntityIdentity)).Return (fakeResolvedEntityIdentity);

      var result = _visitor.VisitExpression (existsExpression);

      _resolverMock.VerifyAllExpectations ();
      _entityIdentityResolverMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf<SqlExistsExpression>());
      Assert.That (((SqlExistsExpression) result).Expression, Is.SameAs (fakeResolvedEntityIdentity));
    }

    [Test]
    public void VisitSqlExistsExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges ()
    {
      var inner = Expression.Constant (0);
      var existsExpression = new SqlExistsExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (inner)).Return (fakeResolvedInner);

      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntity (fakeResolvedInner))
          .Return (fakeResolvedInner);

      // No revisiting!

      var result = _visitor.VisitExpression (existsExpression);

      _resolverMock.VerifyAllExpectations ();
      _entityIdentityResolverMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf<SqlExistsExpression> ());
      Assert.That (((SqlExistsExpression) result).Expression, Is.SameAs (fakeResolvedInner));
    }

    [Test]
    public void VisitSqlInExpression ()
    {
      var left = Expression.Constant (0);
      var right = Expression.Constant (1);
      var inExpression = new SqlInExpression (left, right);

      var fakeResolvedLeft = new SqlLiteralExpression (2);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (left)).Return (fakeResolvedLeft);
      var fakeResolvedRight = new SqlLiteralExpression (3);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (right)).Return (fakeResolvedRight);

      var fakeResolvedInExpression = new SqlInExpression (Expression.Constant (4), Expression.Constant (5));
      _entityIdentityResolverMock
          .Expect (
              mock => mock.ResolvePotentialEntityComparison (
                  Arg<SqlInExpression>.Matches (e => e.LeftExpression == fakeResolvedLeft && e.RightExpression == fakeResolvedRight)))
          .Return (fakeResolvedInExpression);
      
      // Result is revisited
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeResolvedInExpression.LeftExpression))
          .Return (fakeResolvedInExpression.LeftExpression);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeResolvedInExpression.RightExpression))
          .Return (fakeResolvedInExpression.RightExpression);
      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntityComparison (fakeResolvedInExpression))
          .Return (fakeResolvedInExpression);

      var result = _visitor.VisitExpression (inExpression);

      _resolverMock.VerifyAllExpectations ();
      _entityIdentityResolverMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (fakeResolvedInExpression));
    }

    [Test]
    public void VisitSqlInExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges  ()
    {
      var left = Expression.Constant (0);
      var right = Expression.Constant (1);
      var inExpression = new SqlInExpression (left, right);

      var fakeResolvedLeft = new SqlLiteralExpression (2);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (left)).Return (fakeResolvedLeft);
      var fakeResolvedRight = new SqlLiteralExpression (3);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (right)).Return (fakeResolvedRight);

      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntityComparison (Arg<SqlInExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);

      // No revisiting

      var result = _visitor.VisitExpression (inExpression);

      _resolverMock.VerifyAllExpectations ();
      _entityIdentityResolverMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf<SqlInExpression> ());
      Assert.That (((SqlInExpression) result).LeftExpression, Is.SameAs (fakeResolvedLeft));
      Assert.That (((SqlInExpression) result).RightExpression, Is.SameAs (fakeResolvedRight));
    }

    [Test]
    public void VisitSqlIsNullExpression ()
    {
      var inner = Expression.Constant (0);
      var isNullExpression = new SqlIsNullExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (inner)).Return (fakeResolvedInner);

      var fakeResolvedEntityComparison = new SqlIsNullExpression (Expression.Constant (2));
      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntityComparison (Arg<SqlIsNullExpression>.Matches (e => e.Expression == fakeResolvedInner)))
          .Return (fakeResolvedEntityComparison);

      var fakeSplitCompoundComparison = new SqlIsNullExpression (Expression.Constant (3));
      _compoundComparisonSplitterMock
          .Expect (mock => mock.SplitPotentialCompoundComparison (fakeResolvedEntityComparison))
          .Return (fakeSplitCompoundComparison);

      // Result is revisited
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeSplitCompoundComparison.Expression))
          .Return (fakeSplitCompoundComparison.Expression);
      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntityComparison (fakeSplitCompoundComparison))
          .Return (fakeSplitCompoundComparison);
      _compoundComparisonSplitterMock
          .Expect (mock => mock.SplitPotentialCompoundComparison (fakeSplitCompoundComparison))
          .Return (fakeSplitCompoundComparison);
     
      var result = _visitor.VisitExpression (isNullExpression);

      _resolverMock.VerifyAllExpectations ();
      _entityIdentityResolverMock.VerifyAllExpectations ();
      _compoundComparisonSplitterMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (fakeSplitCompoundComparison));
    }
    
    [Test]
    public void VisitSqlIsNullExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges  ()
    {
      var inner = Expression.Constant (0);
      var isNullExpression = new SqlIsNullExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (inner)).Return (fakeResolvedInner);

      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntityComparison (Arg<SqlIsNullExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);
      _compoundComparisonSplitterMock
          .Expect (mock => mock.SplitPotentialCompoundComparison (Arg<SqlIsNullExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);

      // No revisiting

      var result = _visitor.VisitExpression (isNullExpression);

      _resolverMock.VerifyAllExpectations ();
      _entityIdentityResolverMock.VerifyAllExpectations ();
      _compoundComparisonSplitterMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf<SqlIsNullExpression> ());
      Assert.That (((SqlIsNullExpression) result).Expression, Is.SameAs (fakeResolvedInner));
    }

    [Test]
    public void VisitSqlIsNotNullExpression ()
    {
      var inner = Expression.Constant (0);
      var isNotNullExpression = new SqlIsNotNullExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (inner)).Return (fakeResolvedInner);

      var fakeResolvedEntityComparison = new SqlIsNotNullExpression (Expression.Constant (2));
      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntityComparison (Arg<SqlIsNotNullExpression>.Matches (e => e.Expression == fakeResolvedInner)))
          .Return (fakeResolvedEntityComparison);

      var fakeSplitCompoundComparison = new SqlIsNotNullExpression (Expression.Constant (3));
      _compoundComparisonSplitterMock
          .Expect (mock => mock.SplitPotentialCompoundComparison (fakeResolvedEntityComparison))
          .Return (fakeSplitCompoundComparison);

      // Result is revisited
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeSplitCompoundComparison.Expression))
          .Return (fakeSplitCompoundComparison.Expression);
      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntityComparison (fakeSplitCompoundComparison))
          .Return (fakeSplitCompoundComparison);
      _compoundComparisonSplitterMock
          .Expect (mock => mock.SplitPotentialCompoundComparison (fakeSplitCompoundComparison))
          .Return (fakeSplitCompoundComparison);

      var result = _visitor.VisitExpression (isNotNullExpression);

      _resolverMock.VerifyAllExpectations ();
      _entityIdentityResolverMock.VerifyAllExpectations ();
      _compoundComparisonSplitterMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (fakeSplitCompoundComparison));
    }

    [Test]
    public void VisitSqlIsNotNullExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges  ()
    {
      var inner = Expression.Constant (0);
      var isNotNullExpression = new SqlIsNotNullExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (inner)).Return (fakeResolvedInner);

      _entityIdentityResolverMock
          .Expect (mock => mock.ResolvePotentialEntityComparison (Arg<SqlIsNotNullExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);
      _compoundComparisonSplitterMock
          .Expect (mock => mock.SplitPotentialCompoundComparison (Arg<SqlIsNotNullExpression>.Is.Anything))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);


      // No revisiting

      var result = _visitor.VisitExpression (isNotNullExpression);

      _resolverMock.VerifyAllExpectations ();
      _entityIdentityResolverMock.VerifyAllExpectations ();
      _compoundComparisonSplitterMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf<SqlIsNotNullExpression> ());
      Assert.That (((SqlIsNotNullExpression) result).Expression, Is.SameAs (fakeResolvedInner));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_ResolveFlagFalse ()
    {
      var entityRefMemberExpression = SqlStatementModelObjectMother.CreateSqlEntityRefMemberExpression();

      var visitor = CreateVisitor (false);
      var result = visitor.VisitExpression (entityRefMemberExpression);

      Assert.That (result, Is.SameAs (entityRefMemberExpression));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_ResolveFlagTrue ()
    {
      var entityRefMemberExpression = SqlStatementModelObjectMother.CreateSqlEntityRefMemberExpression ();

      var fakeResolvedExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression();
      _stageMock
          .Expect (
              mock => mock.ResolveEntityRefMemberExpression (
                  Arg.Is (entityRefMemberExpression),
                  Arg<UnresolvedJoinInfo>.Matches (
                      ji =>
                      ji.OriginatingEntity == entityRefMemberExpression.OriginatingEntity && ji.MemberInfo == entityRefMemberExpression.MemberInfo
                      && ji.Cardinality == JoinCardinality.One),
                  Arg.Is (_mappingResolutionContext)))
          .Return (fakeResolvedExpression);

      var visitor = CreateVisitor (true);
      var result = visitor.VisitExpression (entityRefMemberExpression);

      Assert.That (result, Is.SameAs (fakeResolvedExpression));
    }

    [Test]
    public void ResolveExpression_OptimizesEntityRefMemberComparisons ()
    {
      // This test proves that the first stage (without resolving SqlEntityRefMemberExpressions) is executed.
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      var fakeOptimizedRefIdentity = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var entityRefMemberExpression = CreateEntityRefMemberExpressionAndStubOptimizedIdentity (entityExpression, memberInfo, fakeOptimizedRefIdentity);
      var entity = CreateEntityExpressionWithIdentity (typeof (Cook), typeof (int));
      var binary = Expression.Equal (entityRefMemberExpression, entity);

      var result = ResolvingExpressionVisitor.ResolveExpression (binary, _resolverMock, _stageMock, _mappingResolutionContext, _generator);

      var expected = Expression.Equal (fakeOptimizedRefIdentity, entity.GetIdentityExpression ());
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolveExpression_ResolvesSqlEntityRefMemberExpressions ()
    {
      // This test proves that the second stage (resolving SqlEntityRefMemberExpressions) is executed.
      var memberInfo = typeof (Cook).GetProperty ("Kitchen");
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      var fakeResult = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      _stageMock
          .Expect (
              mock => mock.ResolveEntityRefMemberExpression (
                  Arg.Is (entityRefMemberExpression),
                  Arg<UnresolvedJoinInfo>.Matches (
                      i => i.OriginatingEntity == entityExpression && i.MemberInfo == memberInfo && i.Cardinality == JoinCardinality.One),
                  Arg.Is (_mappingResolutionContext)))
          .Return (fakeResult);

      var result = ResolvingExpressionVisitor.ResolveExpression (
          entityRefMemberExpression, _resolverMock, _stageMock, _mappingResolutionContext, _generator);

      Assert.That (result, Is.SameAs (fakeResult));
    }

    private SqlEntityRefMemberExpression CreateEntityRefMemberExpressionAndStubOptimizedIdentity (
        SqlEntityExpression originatingEntity, PropertyInfo memberInfo, Expression optimizedIdentity)
    {
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (originatingEntity, memberInfo);

      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", true);
      var resolvedSimpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "KitchenTable", "k");
      var fakeJoinInfoWithPrimaryKeyOnRightSide = new ResolvedJoinInfo (resolvedSimpleTableInfo, optimizedIdentity, primaryKeyColumn);

      _stageMock
          .Stub (
              mock => mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (ji => ji.MemberInfo == memberInfo && ji.OriginatingEntity.Type == typeof (Kitchen)),
                  Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (fakeJoinInfoWithPrimaryKeyOnRightSide);
      return entityRefMemberExpression;
    }

    private SqlEntityExpression CreateEntityExpressionWithIdentity (Type entityType, Type identityType)
    {
      return SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (entityType, null, "t0", identityType);
    }

    private ResolvingExpressionVisitor CreateVisitor (bool resolveEntityRefMemberExpressions)
    {
      return (ResolvingExpressionVisitor)
             Activator.CreateInstance (
                 typeof (ResolvingExpressionVisitor),
                 BindingFlags.Instance | BindingFlags.NonPublic,
                 null,
                 new object[]
                 {
                     _resolverMock, _stageMock, _mappingResolutionContext, _generator, _entityIdentityResolverMock,
                     _compoundComparisonSplitterMock,
                     _namedExpressionCombinerMock,
                     _groupAggregateSimplifierMock,
                     resolveEntityRefMemberExpressions
                 },
                 null);
    }
  }
}