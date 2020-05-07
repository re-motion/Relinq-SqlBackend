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
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class SqlContextJoinInfoVisitorTest
  {
    private Mock<IMappingResolutionStage> _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _mappingResolutionContext = new MappingResolutionContext();
      _stageMock = MockRepository.GenerateMock<IMappingResolutionStage> ();
    }

    [Test]
    public void ApplyContext_VisitUnresolvedJoinInfo ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var unresolvedJoinInfo = new UnresolvedJoinInfo (entityExpression, typeof (Cook).GetProperty ("ID"), JoinCardinality.One);
      
      var result = SqlContextJoinInfoVisitor.ApplyContext (unresolvedJoinInfo, SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (unresolvedJoinInfo));
    }

    [Test]
    public void ApplyContext_VisitUnresolvedCollectionJoinInfo ()
    {
      var unresolvedJoinInfo = new UnresolvedCollectionJoinInfo (Expression.Constant (new Cook ()), typeof (Cook).GetProperty ("IllnessDays"));
      
      var result = SqlContextJoinInfoVisitor.ApplyContext (unresolvedJoinInfo, SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (unresolvedJoinInfo));
    }

    [Test]
    public void ApplyContext_VisitResolvedJoinInfo_SameForeignTableInfo ()
    {
      var resolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();
      
      _stageMock
         .Setup (mock => mock.ApplyContext (resolvedJoinInfo.ForeignTableInfo, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
         .Returns (resolvedJoinInfo.ForeignTableInfo)
         .Verifiable ();

      var result = SqlContextJoinInfoVisitor.ApplyContext (resolvedJoinInfo, SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify ();
      Assert.That (result, Is.SameAs (resolvedJoinInfo));
    }

    [Test]
    public void ApplyContext_VisitResolvedJoinInfo_NewForeignTableInfo ()
    {
      var resolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo ();
      var fakeTableInfo = new ResolvedSimpleTableInfo (typeof (Restaurant), "RestaurantTable", "r");

      _stageMock
         .Setup (mock => mock.ApplyContext (resolvedJoinInfo.ForeignTableInfo, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
         .Returns (fakeTableInfo)
         .Verifiable ();

      var result = SqlContextJoinInfoVisitor.ApplyContext (resolvedJoinInfo, SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify ();
      Assert.That (((ResolvedJoinInfo) result).ForeignTableInfo, Is.SameAs (fakeTableInfo));
      Assert.That (((ResolvedJoinInfo) result).JoinCondition, Is.SameAs (resolvedJoinInfo.JoinCondition));
    }
  }
}