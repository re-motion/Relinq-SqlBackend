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
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class ResolvingSelectExpressionVisitorTest
  {
    private IMappingResolutionStage _stageMock;
    private IMappingResolver _resolverMock;
    private IMappingResolutionContext _mappingResolutionContext;
    private UniqueIdentifierGenerator _generator;
    private IGroupAggregateSimplifier _groupAggregateSimplifier;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage>();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver>();
      _mappingResolutionContext = new MappingResolutionContext();
      _generator = new UniqueIdentifierGenerator();
      _groupAggregateSimplifier = MockRepository.GenerateStrictMock<IGroupAggregateSimplifier> ();
    }

    [Test]
    public void VisitSqlSubStatementExpression_LeavesSqlSubStatementExpression_ForStreamedSequenceInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (new Cook()));
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (sqlStatement);
      _groupAggregateSimplifier
          .Expect (mock => mock.SimplifyIfPossible (expression, sqlStatement.SelectProjection))
          .Return (expression);

      var visitor = CreateVisitor (new SqlStatementBuilder (sqlStatement));
      var result = visitor.VisitExpression (expression);

      _stageMock.VerifyAllExpectations();
      _groupAggregateSimplifier.VerifyAllExpectations ();
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void VisitSqlSubStatementExpression_LeavesSqlSubStatementExpression_ForStreamedScalarInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Scalar();
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (sqlStatement);
      _groupAggregateSimplifier
          .Expect (mock => mock.SimplifyIfPossible (expression, sqlStatement.SelectProjection))
          .Return (expression);

      var visitor = CreateVisitor (new SqlStatementBuilder (sqlStatement));
      var result = visitor.VisitExpression (expression);

      _stageMock.VerifyAllExpectations();
      _groupAggregateSimplifier.VerifyAllExpectations ();
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void VisitSqlSubStatementExpression_ConvertsToSqlTable_ForStreamedSingleValueInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Single();
      var fakeResolvedSqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Single();
      var expression = new SqlSubStatementExpression (sqlStatement);

      var resolvedReference = Expression.Constant ("fake");
      SqlTable sqlTable = null;

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (fakeResolvedSqlStatement);
      _groupAggregateSimplifier
          .Expect (
              mock => mock.SimplifyIfPossible (
                  Arg<SqlSubStatementExpression>.Matches (e => e.SqlStatement == fakeResolvedSqlStatement), Arg.Is (sqlStatement.SelectProjection)))
          .Return (null)
          .WhenCalled (mi => mi.ReturnValue = mi.Arguments[0]);
      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (Arg<SqlTableReferenceExpression>.Is.Anything, Arg.Is (_mappingResolutionContext)))
          .WhenCalled (
              mi =>
              {
                var expectedStatement =
                    new SqlStatementBuilder (fakeResolvedSqlStatement)
                    { DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<int>), fakeResolvedSqlStatement.SelectProjection) }.GetSqlStatement();
                sqlTable = (SqlTable) ((SqlTableReferenceExpression) mi.Arguments[0]).SqlTable;
                Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement, Is.EqualTo (expectedStatement));
              })
          .Return (resolvedReference);

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (resolvedReference)).Return (resolvedReference);

      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var visitor = CreateVisitor (sqlStatementBuilder);

      Assert.That (sqlStatementBuilder.SqlTables.Count, Is.EqualTo (0));
      
      var result = visitor.VisitExpression (expression);

      _stageMock.VerifyAllExpectations();
      _groupAggregateSimplifier.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (resolvedReference));
      Assert.That (sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (sqlStatementBuilder.SqlTables[0], Is.EqualTo (sqlTable));
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
          .Stub (stub => stub.TryResolveOptimizedIdentity (entityRefMemberExpression))
          .Return (fakeOptimizedRefIdentity);

      var entity = CreateEntityExpressionWithIdentity (typeof (Cook), typeof (int));
      var binary = Expression.Equal (entityRefMemberExpression, entity);
      var sqlStatementBuilder = new SqlStatementBuilder ();

      var result = ResolvingSelectExpressionVisitor.ResolveExpression (
          binary, _resolverMock, _stageMock, _mappingResolutionContext, _generator, sqlStatementBuilder);

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
      var sqlStatementBuilder = new SqlStatementBuilder();

      var fakeResult = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      _stageMock
          .Expect (
              mock => mock.ResolveEntityRefMemberExpression (
                  Arg.Is (entityRefMemberExpression),
                  Arg<UnresolvedJoinInfo>.Matches (
                      i => i.OriginatingEntity == entityExpression && i.MemberInfo == memberInfo && i.Cardinality == JoinCardinality.One),
                  Arg.Is (_mappingResolutionContext)))
          .Return (fakeResult);

      var result = ResolvingSelectExpressionVisitor.ResolveExpression (
          entityRefMemberExpression, _resolverMock, _stageMock, _mappingResolutionContext, _generator, sqlStatementBuilder);

      Assert.That (result, Is.SameAs (fakeResult));
    }

    private SqlEntityExpression CreateEntityExpressionWithIdentity (Type entityType, Type identityType)
    {
      return SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (entityType, null, "t0", identityType);
    }

    private ResolvingSelectExpressionVisitor CreateVisitor (SqlStatementBuilder statementBuilder)
    {
      return (ResolvingSelectExpressionVisitor)
             Activator.CreateInstance (
                 typeof (ResolvingSelectExpressionVisitor),
                 BindingFlags.Instance | BindingFlags.NonPublic,
                 null,
                 new object[]
                 {
                     _resolverMock, 
                     _stageMock, 
                     _mappingResolutionContext, 
                     _generator, 
                     MockRepository.GenerateStub<IEntityIdentityResolver>(),
                     MockRepository.GenerateStub<ICompoundExpressionComparisonSplitter>(),
                     MockRepository.GenerateStub<INamedExpressionCombiner>(), 
                     _groupAggregateSimplifier, 
                     false, 
                     statementBuilder
                 },
                 null);
    }
  }
}