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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class ResolvingJoinInfoVisitorTest
  {
    private Mock<IMappingResolver> _resolverMock;
    private UnresolvedJoinInfo _unresolvedJoinInfo;
    private UniqueIdentifierGenerator _generator;
    private Mock<IMappingResolutionStage> _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;
    
    [SetUp]
    public void SetUp ()
    {
      _resolverMock = new Mock<IMappingResolver> (MockBehavior.Strict);
      _unresolvedJoinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();
      _generator = new UniqueIdentifierGenerator();
      _stageMock = new Mock<IMappingResolutionStage> (MockBehavior.Strict);
      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesUnresolvedJoinInfo ()
    {
      var foreignTableInfo = new ResolvedSimpleTableInfo (typeof (string), "Cook", "c");
      var condition = ExpressionHelper.CreateExpression (typeof (bool));

      var resolvedJoinInfo = new ResolvedJoinInfo (foreignTableInfo, condition);

      _resolverMock
          .Setup (mock => mock.ResolveJoinInfo (_unresolvedJoinInfo, _generator))
          .Returns (resolvedJoinInfo)
          .Verifiable();

      _stageMock
          .Setup (mock => mock.ResolveTableInfo (foreignTableInfo, _mappingResolutionContext))
          .Returns (foreignTableInfo)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (condition, _mappingResolutionContext))
          .Returns (condition)
          .Verifiable();
      
      var result = ResolvingJoinInfoVisitor.ResolveJoinInfo (_unresolvedJoinInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (resolvedJoinInfo));
      _resolverMock.Verify();
      _stageMock.Verify();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesUnresolvedJoinInfo_AndResolvesJoinedTable ()
    {
      var foreignTableInfo = new ResolvedSimpleTableInfo (typeof (string), "Cook", "c");
      var condition = ExpressionHelper.CreateExpression (typeof (bool));

      var resolvedJoinInfo = new ResolvedJoinInfo (foreignTableInfo, condition);
      _resolverMock
          .Setup (mock => mock.ResolveJoinInfo (_unresolvedJoinInfo, _generator))
          .Returns (resolvedJoinInfo)
          .Verifiable();

      var fakeResolvedForeignTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (string));
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (foreignTableInfo, _mappingResolutionContext))
          .Returns (fakeResolvedForeignTableInfo)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (condition, _mappingResolutionContext))
          .Returns (condition)
          .Verifiable();

      var result = ResolvingJoinInfoVisitor.ResolveJoinInfo (_unresolvedJoinInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs (resolvedJoinInfo));
      Assert.That (result.ForeignTableInfo, Is.SameAs (fakeResolvedForeignTableInfo));
      Assert.That (result.JoinCondition, Is.SameAs (condition));
      _resolverMock.Verify();
      _stageMock.Verify();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesUnresolvedJoinInfo_AndResolvesCondition ()
    {
      var foreignTableInfo = new ResolvedSimpleTableInfo (typeof (string), "Cook", "c");
      var condition = ExpressionHelper.CreateExpression (typeof (bool));

      var resolvedJoinInfo = new ResolvedJoinInfo (foreignTableInfo, condition);

      _resolverMock
          .Setup (mock => mock.ResolveJoinInfo (_unresolvedJoinInfo, _generator))
          .Returns (resolvedJoinInfo)
          .Verifiable();

      _stageMock
          .Setup (mock => mock.ResolveTableInfo (foreignTableInfo, _mappingResolutionContext))
          .Returns (foreignTableInfo)
          .Verifiable();
      var fakeResolvedCondition = ExpressionHelper.CreateExpression (typeof (bool));
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (condition, _mappingResolutionContext))
          .Returns (fakeResolvedCondition)
          .Verifiable();

      var result = ResolvingJoinInfoVisitor.ResolveJoinInfo (_unresolvedJoinInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs (resolvedJoinInfo));
      Assert.That (result.ForeignTableInfo, Is.SameAs (foreignTableInfo));
      Assert.That (result.JoinCondition, Is.SameAs (fakeResolvedCondition));
      _resolverMock.Verify();
      _stageMock.Verify();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesCollectionJoinInfo ()
    {
      var memberInfo = typeof (Cook).GetProperty ("IllnessDays");
      var unresolvedCollectionJoinInfo = new UnresolvedCollectionJoinInfo (Expression.Constant (new Cook()), memberInfo);

      var sqlEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var fakeResolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();

      _stageMock
          .Setup (mock => mock.ResolveCollectionSourceExpression (unresolvedCollectionJoinInfo.SourceExpression, _mappingResolutionContext))
          .Returns (sqlEntityExpression)
          .Verifiable();

      _resolverMock
          .Setup (
              mock => mock.ResolveJoinInfo (
                  It.Is<UnresolvedJoinInfo> (a => a.MemberInfo == memberInfo && a.OriginatingEntity.Type == typeof (Cook)),
                  _generator))
          .Returns (fakeResolvedJoinInfo)
          .Verifiable();

      _stageMock
          .Setup (mock => mock.ResolveTableInfo (fakeResolvedJoinInfo.ForeignTableInfo, _mappingResolutionContext))
          .Returns (fakeResolvedJoinInfo.ForeignTableInfo)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (fakeResolvedJoinInfo.JoinCondition, _mappingResolutionContext))
          .Returns (fakeResolvedJoinInfo.JoinCondition)
          .Verifiable();
      
      var resolvedJoinInfo = ResolvingJoinInfoVisitor.ResolveJoinInfo (unresolvedCollectionJoinInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      Assert.That (resolvedJoinInfo, Is.SameAs (fakeResolvedJoinInfo));

      _stageMock.Verify();
      _resolverMock.Verify();
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
          .Setup (mock => mock.ResolveCollectionSourceExpression (unresolvedCollectionJoinInfo.SourceExpression, _mappingResolutionContext))
          .Returns (fakeUnaryExpression)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveCollectionSourceExpression (fakeUnaryExpression.Operand, _mappingResolutionContext))
          .Returns (sqlEntityExpression)
          .Verifiable();

      _resolverMock
          .Setup (
              mock => mock.ResolveJoinInfo (
                  It.Is<UnresolvedJoinInfo> (a => a.MemberInfo == memberInfo && a.OriginatingEntity.Type == typeof (Cook)),
                  _generator))
          .Returns (fakeResolvedJoinInfo)
          .Verifiable();

      _stageMock
          .Setup (mock => mock.ResolveTableInfo (fakeResolvedJoinInfo.ForeignTableInfo, _mappingResolutionContext))
          .Returns (fakeResolvedJoinInfo.ForeignTableInfo)
          .Verifiable();
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (fakeResolvedJoinInfo.JoinCondition, _mappingResolutionContext))
          .Returns (fakeResolvedJoinInfo.JoinCondition)
          .Verifiable();

      var resolvedJoinInfo = ResolvingJoinInfoVisitor.ResolveJoinInfo (unresolvedCollectionJoinInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      Assert.That (resolvedJoinInfo, Is.SameAs (fakeResolvedJoinInfo));

      _stageMock.Verify();
      _resolverMock.Verify();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesCollectionJoinInfo_NoEntity ()
    {
      var memberInfo = typeof (Cook).GetProperty ("IllnessDays");
      var unresolvedCollectionJoinInfo = new UnresolvedCollectionJoinInfo (Expression.Constant (new Cook ()), memberInfo);
      var fakeExpression = Expression.Constant (1);

      _stageMock
          .Setup (mock => mock.ResolveCollectionSourceExpression (unresolvedCollectionJoinInfo.SourceExpression, _mappingResolutionContext))
          .Returns (fakeExpression)
          .Verifiable();
      Assert.That (
          () => ResolvingJoinInfoVisitor.ResolveJoinInfo (unresolvedCollectionJoinInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "Only entities can be used as the collection source in from expressions, '1' cannot. Member: 'Int32[] IllnessDays'"));
    }
    
    [Test]
    public void ResolveJoinInfo_WithResolvedJoinInfo_ReresolvesTableInfo_AndKeys ()
    {
      var resolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();

      var fakeResolvedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo();
      _stageMock
          .Setup (mock => mock.ResolveTableInfo (resolvedJoinInfo.ForeignTableInfo, _mappingResolutionContext))
          .Returns (fakeResolvedTableInfo)
          .Verifiable();
      var fakeResolvedJoinCondition = ExpressionHelper.CreateExpression (typeof (bool));
      _stageMock
          .Setup (mock => mock.ResolveJoinCondition (resolvedJoinInfo.JoinCondition, _mappingResolutionContext))
          .Returns (fakeResolvedJoinCondition)
          .Verifiable();

      var result = ResolvingJoinInfoVisitor.ResolveJoinInfo (resolvedJoinInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify();
      Assert.That (result.ForeignTableInfo, Is.SameAs (fakeResolvedTableInfo));
      Assert.That (result.JoinCondition, Is.SameAs (fakeResolvedJoinCondition));
    }
  }
}