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
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class ResolvingJoinInfoVisitorTest
  {
    private IMappingResolver _resolverMock;
    private UnresolvedJoinInfo _unresolvedJoinInfo;
    private UniqueIdentifierGenerator _generator;
    private IMappingResolutionStage _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;
    
    [SetUp]
    public void SetUp ()
    {
      _resolverMock = MockRepository.GenerateStrictMock<IMappingResolver>();
      _unresolvedJoinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();
      _generator = new UniqueIdentifierGenerator();
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage> ();
      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesUnresolvedJoinInfo ()
    {
      var foreignTableInfo = new ResolvedSimpleTableInfo (typeof (string), "Cook", "c");
      var condition = ExpressionHelper.CreateExpression (typeof (bool));

      var resolvedJoinInfo = new ResolvedJoinInfo (foreignTableInfo, condition);

      _resolverMock
          .Expect (mock => mock.ResolveJoinInfo (_unresolvedJoinInfo, _generator))
          .Return (resolvedJoinInfo);

      _stageMock
          .Expect (mock => mock.ResolveTableInfo (foreignTableInfo, _mappingResolutionContext))
          .Return (foreignTableInfo);
      _stageMock
          .Expect (mock => mock.ResolveJoinCondition (condition, _mappingResolutionContext))
          .Return (condition);
      
      var result = ResolvingJoinInfoVisitor.ResolveJoinInfo (_unresolvedJoinInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (resolvedJoinInfo));
      _resolverMock.VerifyAllExpectations();
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesUnresolvedJoinInfo_AndResolvesJoinedTable ()
    {
      var foreignTableInfo = new ResolvedSimpleTableInfo (typeof (string), "Cook", "c");
      var condition = ExpressionHelper.CreateExpression (typeof (bool));

      var resolvedJoinInfo = new ResolvedJoinInfo (foreignTableInfo, condition);
      _resolverMock
          .Expect (mock => mock.ResolveJoinInfo (_unresolvedJoinInfo, _generator))
          .Return (resolvedJoinInfo);

      var fakeResolvedForeignTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (string));
      _stageMock
          .Expect (mock => mock.ResolveTableInfo (foreignTableInfo, _mappingResolutionContext))
          .Return (fakeResolvedForeignTableInfo);
      _stageMock
          .Expect (mock => mock.ResolveJoinCondition (condition, _mappingResolutionContext))
          .Return (condition);

      var result = ResolvingJoinInfoVisitor.ResolveJoinInfo (_unresolvedJoinInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs (resolvedJoinInfo));
      Assert.That (result.ForeignTableInfo, Is.SameAs (fakeResolvedForeignTableInfo));
      Assert.That (result.JoinCondition, Is.SameAs (condition));
      _resolverMock.VerifyAllExpectations ();
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesUnresolvedJoinInfo_AndResolvesCondition ()
    {
      var foreignTableInfo = new ResolvedSimpleTableInfo (typeof (string), "Cook", "c");
      var condition = ExpressionHelper.CreateExpression (typeof (bool));

      var resolvedJoinInfo = new ResolvedJoinInfo (foreignTableInfo, condition);

      _resolverMock
          .Expect (mock => mock.ResolveJoinInfo (_unresolvedJoinInfo, _generator))
          .Return (resolvedJoinInfo);

      _stageMock
          .Expect (mock => mock.ResolveTableInfo (foreignTableInfo, _mappingResolutionContext))
          .Return (foreignTableInfo);
      var fakeResolvedCondition = ExpressionHelper.CreateExpression (typeof (bool));
      _stageMock
          .Expect (mock => mock.ResolveJoinCondition (condition, _mappingResolutionContext))
          .Return (fakeResolvedCondition);

      var result = ResolvingJoinInfoVisitor.ResolveJoinInfo (_unresolvedJoinInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs (resolvedJoinInfo));
      Assert.That (result.ForeignTableInfo, Is.SameAs (foreignTableInfo));
      Assert.That (result.JoinCondition, Is.SameAs (fakeResolvedCondition));
      _resolverMock.VerifyAllExpectations ();
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesCollectionJoinInfo ()
    {
      var memberInfo = typeof (Cook).GetProperty ("IllnessDays");
      var unresolvedCollectionJoinInfo = new UnresolvedCollectionJoinInfo (Expression.Constant (new Cook()), memberInfo);

      var sqlEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var fakeResolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();

      _stageMock
          .Expect (mock => mock.ResolveCollectionSourceExpression (unresolvedCollectionJoinInfo.SourceExpression, _mappingResolutionContext))
          .Return (sqlEntityExpression);

      _resolverMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (a => a.MemberInfo == memberInfo && a.OriginatingEntity.Type == typeof(Cook)),
                  Arg.Is (_generator)))
          .Return (fakeResolvedJoinInfo);

      _stageMock
          .Expect (mock => mock.ResolveTableInfo (fakeResolvedJoinInfo.ForeignTableInfo, _mappingResolutionContext))
          .Return (fakeResolvedJoinInfo.ForeignTableInfo);
      _stageMock
          .Expect (mock => mock.ResolveJoinCondition (fakeResolvedJoinInfo.JoinCondition, _mappingResolutionContext))
          .Return (fakeResolvedJoinInfo.JoinCondition);
      
      var resolvedJoinInfo = ResolvingJoinInfoVisitor.ResolveJoinInfo (unresolvedCollectionJoinInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (resolvedJoinInfo, Is.SameAs (fakeResolvedJoinInfo));

      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesCollectionJoinInfo_UnaryExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("IllnessDays");
      var unresolvedCollectionJoinInfo = new UnresolvedCollectionJoinInfo (Expression.Constant (new Cook ()), memberInfo);

      var sqlEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var fakeUnaryExpression = Expression.Not(Expression.Constant(1));

      var fakeResolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();

      _stageMock
          .Expect (mock => mock.ResolveCollectionSourceExpression (unresolvedCollectionJoinInfo.SourceExpression, _mappingResolutionContext))
          .Return (fakeUnaryExpression);
      _stageMock
          .Expect (mock => mock.ResolveCollectionSourceExpression (fakeUnaryExpression.Operand, _mappingResolutionContext))
          .Return (sqlEntityExpression);

      _resolverMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (a => a.MemberInfo == memberInfo && a.OriginatingEntity.Type == typeof (Cook)),
                  Arg.Is (_generator)))
          .Return (fakeResolvedJoinInfo);

      _stageMock
          .Expect (mock => mock.ResolveTableInfo (fakeResolvedJoinInfo.ForeignTableInfo, _mappingResolutionContext))
          .Return (fakeResolvedJoinInfo.ForeignTableInfo);
      _stageMock
          .Expect (mock => mock.ResolveJoinCondition (fakeResolvedJoinInfo.JoinCondition, _mappingResolutionContext))
          .Return (fakeResolvedJoinInfo.JoinCondition);

      var resolvedJoinInfo = ResolvingJoinInfoVisitor.ResolveJoinInfo (unresolvedCollectionJoinInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (resolvedJoinInfo, Is.SameAs (fakeResolvedJoinInfo));

      _stageMock.VerifyAllExpectations ();
      _resolverMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Only entities can be used as the collection source in from expressions, '1' cannot. Member: 'Int32[] IllnessDays'")]
    public void ResolveJoinInfo_ResolvesCollectionJoinInfo_NoEntity ()
    {
      var memberInfo = typeof (Cook).GetProperty ("IllnessDays");
      var unresolvedCollectionJoinInfo = new UnresolvedCollectionJoinInfo (Expression.Constant (new Cook ()), memberInfo);
      var fakeExpression = Expression.Constant (1);

      _stageMock
          .Expect (mock => mock.ResolveCollectionSourceExpression (unresolvedCollectionJoinInfo.SourceExpression, _mappingResolutionContext))
          .Return (fakeExpression);

      ResolvingJoinInfoVisitor.ResolveJoinInfo (unresolvedCollectionJoinInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);
    }
    
    [Test]
    public void ResolveJoinInfo_WithResolvedJoinInfo_ReresolvesTableInfo_AndKeys ()
    {
      var resolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();

      var fakeResolvedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo();
      _stageMock
          .Expect (mock => mock.ResolveTableInfo (resolvedJoinInfo.ForeignTableInfo, _mappingResolutionContext))
          .Return (fakeResolvedTableInfo);
      var fakeResolvedJoinCondition = ExpressionHelper.CreateExpression (typeof (bool));
      _stageMock
          .Expect (mock => mock.ResolveJoinCondition (resolvedJoinInfo.JoinCondition, _mappingResolutionContext))
          .Return (fakeResolvedJoinCondition);

      var result = ResolvingJoinInfoVisitor.ResolveJoinInfo (resolvedJoinInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      Assert.That (result.ForeignTableInfo, Is.SameAs (fakeResolvedTableInfo));
      Assert.That (result.JoinCondition, Is.SameAs (fakeResolvedJoinCondition));
    }
  }
}