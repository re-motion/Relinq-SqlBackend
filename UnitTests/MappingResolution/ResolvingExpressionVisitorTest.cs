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
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class ResolvingExpressionVisitorTest
  {
    private Mock<IMappingResolver> _resolverMock;
    private SqlTable _sqlTable;
    private Mock<IMappingResolutionStage> _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;
    private UniqueIdentifierGenerator _generator;
    private Mock<IEntityIdentityResolver> _entityIdentityResolverMock;
    private Mock<ICompoundExpressionComparisonSplitter> _compoundComparisonSplitterMock;
    private Mock<INamedExpressionCombiner> _namedExpressionCombinerMock;
    private Mock<IGroupAggregateSimplifier> _groupAggregateSimplifierMock;

    private ResolvingExpressionVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<IMappingResolutionStage> (MockBehavior.Strict);
      _resolverMock = new Mock<IMappingResolver> (MockBehavior.Strict);
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (Cook));
      _mappingResolutionContext = new MappingResolutionContext();
      _generator = new UniqueIdentifierGenerator();
      _entityIdentityResolverMock = new Mock<IEntityIdentityResolver> (MockBehavior.Strict);
      _compoundComparisonSplitterMock = new Mock<ICompoundExpressionComparisonSplitter> (MockBehavior.Strict);
      _namedExpressionCombinerMock = new Mock<INamedExpressionCombiner> (MockBehavior.Strict);
      _groupAggregateSimplifierMock = new Mock<IGroupAggregateSimplifier> (MockBehavior.Strict);

      _visitor = CreateVisitor (true);
    }

    [Test]
    public void VisitConstantExpression ()
    {
      var constantExpression = Expression.Constant (0);

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (constantExpression))
          .Returns (constantExpression)
          .Verifiable();

      var result = _visitor.Visit (constantExpression);

      _stageMock.Verify();
      _resolverMock.Verify();
      Assert.That (result, Is.SameAs (constantExpression));
    }

    [Test]
    public void VisitConstantExpression_RevisitsResult ()
    {
      var constantExpression = Expression.Constant (0);
      var fakeResult = Expression.Constant (1);

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (constantExpression))
          .Returns (fakeResult)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult))
          .Returns (fakeResult)
          .Verifiable();

      var result = _visitor.Visit (constantExpression);

      _stageMock.Verify();
      _resolverMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitSqlTableReferenceExpression_ResolvesExpression ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Setup (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _mappingResolutionContext))
          .Returns (fakeResult)
          .Verifiable();

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult))
          .Returns (fakeResult)
          .Verifiable();

      var result = _visitor.Visit (tableReferenceExpression);

      _stageMock.Verify();
      _resolverMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitSqlTableReferenceExpression_RevisitsResult ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Setup (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _mappingResolutionContext))
          .Returns (fakeResult)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult))
          .Returns (fakeResult)
          .Verifiable();

      var result = _visitor.Visit (tableReferenceExpression);

      _stageMock.Verify();
      _resolverMock.Verify();
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
          .Setup (mock => mock.ResolveMemberAccess (expression, memberInfo, _resolverMock.Object, _mappingResolutionContext))
          .Returns (fakeResolvedExpression)
          .Verifiable();

      var result = ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock.Object, _stageMock.Object, _mappingResolutionContext, _generator);

      _stageMock.Verify();
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
          .Setup (mock => mock.ResolveConstantExpression (expression))
          .Returns (fakeResolvedSourceExpression)
          .Verifiable();

      var fakeResolvedExpression = new SqlLiteralExpression (1);
      _stageMock
          .Setup (mock => mock.ResolveMemberAccess (fakeResolvedSourceExpression, memberInfo, _resolverMock.Object, _mappingResolutionContext))
          .Returns (fakeResolvedExpression)
          .Verifiable();

      _visitor.Visit (memberExpression);

      _resolverMock.Verify();
      _stageMock.Verify();
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
          .Setup (mock => mock.ResolveMemberAccess (expression, memberInfo, _resolverMock.Object, _mappingResolutionContext))
          .Returns (fakeResult1)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult1))
          .Returns (fakeResult2)
          .Verifiable();

      var result = _visitor.Visit (memberExpression);

      _stageMock.Verify();
      _resolverMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult2));
    }

    [Test]
    public void UnknownExpression ()
    {
      var unknownExpression = new CustomExpression (typeof (int));
      var result = _visitor.Visit (unknownExpression);

      Assert.That (result, Is.SameAs (unknownExpression));
    }

    [Test]
    public void VisitSqlSubStatementExpression_NoChanges ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Setup (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Returns (sqlStatement)
          .Verifiable();
      _groupAggregateSimplifierMock
          .Setup (mock => mock.SimplifyIfPossible (expression, expression.SqlStatement.SelectProjection))
          .Returns (expression)
          .Verifiable();

      var result = _visitor.Visit (expression);

      _stageMock.Verify();
      _groupAggregateSimplifierMock.Verify();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlSubStatementExpression_WithChanges ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (0));
      var expression = new SqlSubStatementExpression (sqlStatement);

      var fakeResolvedStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (1));
      _stageMock
          .Setup (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Returns (fakeResolvedStatement)
          .Verifiable();
      var fakeSimplifiedExpression = Expression.Constant (0);
      _groupAggregateSimplifierMock
          .Setup (
              mock => mock.SimplifyIfPossible (
                  It.Is<SqlSubStatementExpression> (e => ReferenceEquals (e.SqlStatement, fakeResolvedStatement)),
                  expression.SqlStatement.SelectProjection))
         .Returns (fakeSimplifiedExpression)
         .Verifiable();

      var result = _visitor.Visit (expression);

      _stageMock.Verify();
      _groupAggregateSimplifierMock.Verify();
      Assert.That (result, Is.SameAs (fakeSimplifiedExpression));
    }

    [Test]
    public void VisitTypeBinaryExpression ()
    {
      var expression = Expression.Constant ("select");
      var typeBinaryExpression = Expression.TypeIs (expression, typeof (Chef));
      var resolvedTypeExpression = Expression.Constant ("resolved");
      var resolvedRevisitedResult = new SqlLiteralExpression (0);

      _resolverMock.Setup (mock => mock.ResolveConstantExpression (expression)).Returns (expression).Verifiable();
      _resolverMock.Setup (mock => mock.ResolveTypeCheck (expression, typeof (Chef))).Returns (resolvedTypeExpression).Verifiable();
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (resolvedTypeExpression)).Returns (resolvedRevisitedResult).Verifiable();

      var result = _visitor.Visit (typeBinaryExpression);

      _resolverMock.Verify();

      Assert.That (result, Is.SameAs (resolvedRevisitedResult));
    }

    [Test]
    public void VisitBinaryExpression ()
    {
      var left = Expression.Constant (0);
      var right = Expression.Constant (1);
      var binary = Expression.Equal (left, right);

      var fakeResolvedLeft = new SqlLiteralExpression (2);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (left)).Returns (fakeResolvedLeft).Verifiable();
      var fakeResolvedRight = new SqlLiteralExpression (3);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (right)).Returns (fakeResolvedRight).Verifiable();

      var fakeResolvedEntityComparison = Expression.Equal (Expression.Constant (2), Expression.Constant (3));
      _entityIdentityResolverMock
          .Setup (
              mock => mock.ResolvePotentialEntityComparison (
                  It.Is<BinaryExpression> (e => e.Left == fakeResolvedLeft && e.Right == fakeResolvedRight)))
         .Returns (fakeResolvedEntityComparison)
         .Verifiable();

      var fakeSplitComparison = Expression.Equal (Expression.Constant (4), Expression.Constant (5));
      _compoundComparisonSplitterMock
          .Setup (mock => mock.SplitPotentialCompoundComparison (fakeResolvedEntityComparison))
          .Returns (fakeSplitComparison)
          .Verifiable();

      // Result is revisited
      _resolverMock.Setup (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeSplitComparison.Left)).Returns (fakeSplitComparison.Left).Verifiable();
      _resolverMock.Setup (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeSplitComparison.Right)).Returns (fakeSplitComparison.Right).Verifiable();
      _entityIdentityResolverMock.Setup (mock => mock.ResolvePotentialEntityComparison (fakeSplitComparison)).Returns (fakeSplitComparison).Verifiable();
      _compoundComparisonSplitterMock.Setup (mock => mock.SplitPotentialCompoundComparison (fakeSplitComparison)).Returns (fakeSplitComparison).Verifiable();

      var result = _visitor.Visit (binary);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();
      _compoundComparisonSplitterMock.Verify();

      Assert.That (result, Is.SameAs (fakeSplitComparison));
    }

    [Test]
    public void VisitBinaryExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges ()
    {
      var left = Expression.Constant (0);
      var right = Expression.Constant (1);
      var binary = Expression.Equal (left, right);

      var fakeResolvedLeft = new SqlLiteralExpression (2);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (left)).Returns (fakeResolvedLeft).Verifiable();
      var fakeResolvedRight = new SqlLiteralExpression (3);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (right)).Returns (fakeResolvedRight).Verifiable();

      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntityComparison (It.IsAny<BinaryExpression>()))
          .Returns ((BinaryExpression param1) => param1)
          .Verifiable();

      _compoundComparisonSplitterMock
          .Setup (mock => mock.SplitPotentialCompoundComparison (It.IsAny<BinaryExpression>()))
          .Returns ((BinaryExpression param1) => param1)
          .Verifiable();

      // No revisiting

      var result = _visitor.Visit (binary);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();
      _compoundComparisonSplitterMock.Verify();

      Assert.That (result, Is.AssignableTo<BinaryExpression> ());
      Assert.That (((BinaryExpression) result).Left, Is.SameAs (fakeResolvedLeft));
      Assert.That (((BinaryExpression) result).Right, Is.SameAs (fakeResolvedRight));
    }

    [Test]
    public void VisitJoinConditionExpression ()
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var leftKey = new SqlColumnDefinitionExpression (typeof (Cook), "c", "ID", false);
      var rightKey = new SqlColumnDefinitionExpression (typeof (Cook), "a", "FK", false);
      var joinInfo = new ResolvedJoinInfo (resolvedTableInfo, Expression.Equal (leftKey, rightKey));
      var sqlTable = new SqlJoinedTable (joinInfo, JoinSemantics.Left);
      var joinConditionExpression = new JoinConditionExpression (sqlTable);

      _entityIdentityResolverMock
          .Setup (
              mock => mock.ResolvePotentialEntityComparison (
                  It.Is<BinaryExpression> (b => b.Left == leftKey && b.Right == rightKey)))
         .Returns ((BinaryExpression param1) => param1)
         .Verifiable();
      _compoundComparisonSplitterMock
          .Setup (
              mock => mock.SplitPotentialCompoundComparison (
                  It.Is<BinaryExpression> (b => b.Left == leftKey && b.Right == rightKey)))
         .Returns ((BinaryExpression param1) => param1)
         .Verifiable();

      var result = _visitor.Visit (joinConditionExpression);

      _entityIdentityResolverMock.Verify();
      _compoundComparisonSplitterMock.Verify();

      var expectedExpression = Expression.Equal (leftKey, rightKey);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
    
    [Test]
    public void VisitNamedExpression ()
    {
      var innerExpression = Expression.Constant (0);
      var namedExpression = new NamedExpression ("Name", innerExpression);

      var fakeResolvedInnerExpression = new SqlLiteralExpression (1);
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (innerExpression)).Returns (fakeResolvedInnerExpression).Verifiable();

      var fakeCombinedExpression = new NamedExpression ("Name2", Expression.Constant (2));
      _namedExpressionCombinerMock
          .Setup (mock => mock.ProcessNames (It.Is<NamedExpression> (e => e.Name == "Name" && e.Expression == fakeResolvedInnerExpression)))
          .Returns (fakeCombinedExpression)
          .Verifiable();

      // Result is revisited.
      var fakeResolvedInnerExpression2 = new SqlLiteralExpression (3);
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeCombinedExpression.Expression))
          .Returns (fakeResolvedInnerExpression2)
          .Verifiable();
      _namedExpressionCombinerMock
          .Setup (mock => mock.ProcessNames (It.Is<NamedExpression> (e => e.Name == "Name2" && e.Expression == fakeResolvedInnerExpression2)))
          .Returns ((NamedExpression param1) => param1)
          .Verifiable();

      var result = _visitor.Visit (namedExpression);

      _resolverMock.Verify();
      _namedExpressionCombinerMock.Verify();

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
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (innerExpression)).Returns (fakeResolvedInnerExpression).Verifiable();

      _namedExpressionCombinerMock
          .Setup (mock => mock.ProcessNames (It.IsAny<NamedExpression>()))
          .Returns ((NamedExpression param1) => param1)
          .Verifiable();

      // No revisiting

      var result = _visitor.Visit (namedExpression);

      _resolverMock.Verify();
      _namedExpressionCombinerMock.Verify();

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
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (inner)).Returns (fakeResolvedInner).Verifiable();

      var fakeResolvedEntityIdentity = Expression.Constant (1);
      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntity (fakeResolvedInner))
          .Returns (fakeResolvedEntityIdentity)
          .Verifiable();

      // Result is revisited
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (fakeResolvedEntityIdentity)).Returns (fakeResolvedEntityIdentity).Verifiable();
      _entityIdentityResolverMock.Setup (mock => mock.ResolvePotentialEntity (fakeResolvedEntityIdentity)).Returns (fakeResolvedEntityIdentity).Verifiable();

      var result = _visitor.Visit (existsExpression);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();

      Assert.That (result, Is.TypeOf<SqlExistsExpression>());
      Assert.That (((SqlExistsExpression) result).Expression, Is.SameAs (fakeResolvedEntityIdentity));
    }

    [Test]
    public void VisitSqlExistsExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges ()
    {
      var inner = Expression.Constant (0);
      var existsExpression = new SqlExistsExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (inner)).Returns (fakeResolvedInner).Verifiable();

      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntity (fakeResolvedInner))
          .Returns (fakeResolvedInner)
          .Verifiable();

      // No revisiting!

      var result = _visitor.Visit (existsExpression);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();

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
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (left)).Returns (fakeResolvedLeft).Verifiable();
      var fakeResolvedRight = new SqlLiteralExpression (3);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (right)).Returns (fakeResolvedRight).Verifiable();

      var fakeResolvedInExpression = new SqlInExpression (Expression.Constant (4), Expression.Constant (5));
      _entityIdentityResolverMock
          .Setup (
              mock => mock.ResolvePotentialEntityComparison (
                  It.Is<SqlInExpression> (e => e.LeftExpression == fakeResolvedLeft && e.RightExpression == fakeResolvedRight)))
         .Returns (fakeResolvedInExpression)
         .Verifiable();
      
      // Result is revisited
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeResolvedInExpression.LeftExpression))
          .Returns (fakeResolvedInExpression.LeftExpression)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeResolvedInExpression.RightExpression))
          .Returns (fakeResolvedInExpression.RightExpression)
          .Verifiable();
      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntityComparison (fakeResolvedInExpression))
          .Returns (fakeResolvedInExpression)
          .Verifiable();

      var result = _visitor.Visit (inExpression);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();

      Assert.That (result, Is.SameAs (fakeResolvedInExpression));
    }

    [Test]
    public void VisitSqlInExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges  ()
    {
      var left = Expression.Constant (0);
      var right = Expression.Constant (1);
      var inExpression = new SqlInExpression (left, right);

      var fakeResolvedLeft = new SqlLiteralExpression (2);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (left)).Returns (fakeResolvedLeft).Verifiable();
      var fakeResolvedRight = new SqlLiteralExpression (3);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (right)).Returns (fakeResolvedRight).Verifiable();

      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntityComparison (It.IsAny<SqlInExpression>()))
          .Returns ((SqlInExpression param1) => param1)
          .Verifiable();

      // No revisiting

      var result = _visitor.Visit (inExpression);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();

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
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (inner)).Returns (fakeResolvedInner).Verifiable();

      var fakeResolvedEntityComparison = new SqlIsNullExpression (Expression.Constant (2));
      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntityComparison (It.Is<SqlIsNullExpression> (e => e.Expression == fakeResolvedInner)))
          .Returns (fakeResolvedEntityComparison)
          .Verifiable();

      var fakeSplitCompoundComparison = new SqlIsNullExpression (Expression.Constant (3));
      _compoundComparisonSplitterMock
          .Setup (mock => mock.SplitPotentialCompoundComparison (fakeResolvedEntityComparison))
          .Returns (fakeSplitCompoundComparison)
          .Verifiable();

      // Result is revisited
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeSplitCompoundComparison.Expression))
          .Returns (fakeSplitCompoundComparison.Expression)
          .Verifiable();
      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntityComparison (fakeSplitCompoundComparison))
          .Returns (fakeSplitCompoundComparison)
          .Verifiable();
      _compoundComparisonSplitterMock
          .Setup (mock => mock.SplitPotentialCompoundComparison (fakeSplitCompoundComparison))
          .Returns (fakeSplitCompoundComparison)
          .Verifiable();
     
      var result = _visitor.Visit (isNullExpression);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();
      _compoundComparisonSplitterMock.Verify();

      Assert.That (result, Is.SameAs (fakeSplitCompoundComparison));
    }
    
    [Test]
    public void VisitSqlIsNullExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges  ()
    {
      var inner = Expression.Constant (0);
      var isNullExpression = new SqlIsNullExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (inner)).Returns (fakeResolvedInner).Verifiable();

      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntityComparison (It.IsAny<SqlIsNullExpression>()))
          .Returns ((SqlIsNullExpression param1) => param1)
          .Verifiable();
      _compoundComparisonSplitterMock
          .Setup (mock => mock.SplitPotentialCompoundComparison (It.IsAny<SqlIsNullExpression>()))
          .Returns ((SqlIsNullExpression param1) => param1)
          .Verifiable();

      // No revisiting

      var result = _visitor.Visit (isNullExpression);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();
      _compoundComparisonSplitterMock.Verify();

      Assert.That (result, Is.TypeOf<SqlIsNullExpression> ());
      Assert.That (((SqlIsNullExpression) result).Expression, Is.SameAs (fakeResolvedInner));
    }

    [Test]
    public void VisitSqlIsNotNullExpression ()
    {
      var inner = Expression.Constant (0);
      var isNotNullExpression = new SqlIsNotNullExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (inner)).Returns (fakeResolvedInner).Verifiable();

      var fakeResolvedEntityComparison = new SqlIsNotNullExpression (Expression.Constant (2));
      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntityComparison (It.Is<SqlIsNotNullExpression> (e => e.Expression == fakeResolvedInner)))
          .Returns (fakeResolvedEntityComparison)
          .Verifiable();

      var fakeSplitCompoundComparison = new SqlIsNotNullExpression (Expression.Constant (3));
      _compoundComparisonSplitterMock
          .Setup (mock => mock.SplitPotentialCompoundComparison (fakeResolvedEntityComparison))
          .Returns (fakeSplitCompoundComparison)
          .Verifiable();

      // Result is revisited
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeSplitCompoundComparison.Expression))
          .Returns (fakeSplitCompoundComparison.Expression)
          .Verifiable();
      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntityComparison (fakeSplitCompoundComparison))
          .Returns (fakeSplitCompoundComparison)
          .Verifiable();
      _compoundComparisonSplitterMock
          .Setup (mock => mock.SplitPotentialCompoundComparison (fakeSplitCompoundComparison))
          .Returns (fakeSplitCompoundComparison)
          .Verifiable();

      var result = _visitor.Visit (isNotNullExpression);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();
      _compoundComparisonSplitterMock.Verify();

      Assert.That (result, Is.SameAs (fakeSplitCompoundComparison));
    }

    [Test]
    public void VisitSqlIsNotNullExpression_RevisitingTerminatesAfterInnerChanges_WithoutOuterChanges  ()
    {
      var inner = Expression.Constant (0);
      var isNotNullExpression = new SqlIsNotNullExpression (inner);

      var fakeResolvedInner = new SqlLiteralExpression (1);
      _resolverMock.Setup (mock => mock.ResolveConstantExpression (inner)).Returns (fakeResolvedInner).Verifiable();

      _entityIdentityResolverMock
          .Setup (mock => mock.ResolvePotentialEntityComparison (It.IsAny<SqlIsNotNullExpression>()))
          .Returns ((SqlIsNotNullExpression param1) => param1)
          .Verifiable();
      _compoundComparisonSplitterMock
          .Setup (mock => mock.SplitPotentialCompoundComparison (It.IsAny<SqlIsNotNullExpression>()))
          .Returns ((SqlIsNotNullExpression param1) => param1)
          .Verifiable();


      // No revisiting

      var result = _visitor.Visit (isNotNullExpression);

      _resolverMock.Verify();
      _entityIdentityResolverMock.Verify();
      _compoundComparisonSplitterMock.Verify();

      Assert.That (result, Is.TypeOf<SqlIsNotNullExpression> ());
      Assert.That (((SqlIsNotNullExpression) result).Expression, Is.SameAs (fakeResolvedInner));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_ResolveFlagFalse ()
    {
      var entityRefMemberExpression = SqlStatementModelObjectMother.CreateSqlEntityRefMemberExpression();

      var visitor = CreateVisitor (false);
      var result = visitor.Visit (entityRefMemberExpression);

      Assert.That (result, Is.SameAs (entityRefMemberExpression));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_ResolveFlagTrue ()
    {
      var entityRefMemberExpression = SqlStatementModelObjectMother.CreateSqlEntityRefMemberExpression ();

      var fakeResolvedExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression();
      _stageMock
          .Setup (
              mock => mock.ResolveEntityRefMemberExpression (
                  entityRefMemberExpression,
                  It.Is<UnresolvedJoinInfo> (
                      ji => ji.OriginatingEntity == entityRefMemberExpression.OriginatingEntity
                            && ji.MemberInfo == entityRefMemberExpression.MemberInfo
                            && ji.Cardinality == JoinCardinality.One),
                  _mappingResolutionContext))
         .Returns (fakeResolvedExpression)
         .Verifiable();

      var visitor = CreateVisitor (true);
      var result = visitor.Visit (entityRefMemberExpression);

      Assert.That (result, Is.SameAs (fakeResolvedExpression));
    }

    [Test]
    public void ResolveExpression_OptimizesEntityRefMemberComparisons ()
    {
      // This test proves that the first stage (without resolving SqlEntityRefMemberExpressions) is executed.
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      var fakeOptimizedRefIdentity = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      _resolverMock
          .Setup (stub => stub.TryResolveOptimizedIdentity (entityRefMemberExpression))
          .Returns (fakeOptimizedRefIdentity);

      var entity = CreateEntityExpressionWithIdentity (typeof (Cook), typeof (int));
      var binary = Expression.Equal (entityRefMemberExpression, entity);

      var result = ResolvingExpressionVisitor.ResolveExpression (binary, _resolverMock.Object, _stageMock.Object, _mappingResolutionContext, _generator);

      var expected = Expression.Equal (fakeOptimizedRefIdentity, entity.GetIdentityExpression ());
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
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
          .Setup (
              mock => mock.ResolveEntityRefMemberExpression (
                  entityRefMemberExpression,
                  It.Is<UnresolvedJoinInfo> (
                      i => i.OriginatingEntity == entityExpression && i.MemberInfo == memberInfo && i.Cardinality == JoinCardinality.One),
                  _mappingResolutionContext))
         .Returns (fakeResult)
         .Verifiable();

      var result = ResolvingExpressionVisitor.ResolveExpression (
          entityRefMemberExpression, _resolverMock.Object, _stageMock.Object, _mappingResolutionContext, _generator);

      Assert.That (result, Is.SameAs (fakeResult));
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
                     _resolverMock.Object, _stageMock.Object, _mappingResolutionContext, _generator, _entityIdentityResolverMock.Object,
                     _compoundComparisonSplitterMock.Object,
                     _namedExpressionCombinerMock.Object,
                     _groupAggregateSimplifierMock.Object,
                     resolveEntityRefMemberExpressions
                 },
                 null);
    }
  }
}