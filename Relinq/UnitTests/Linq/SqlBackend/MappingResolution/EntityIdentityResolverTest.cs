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
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class EntityIdentityResolverTest
  {
    private IMappingResolutionStage _stageMock;
    private MappingResolutionContext _context;
    private EntityIdentityResolver _resolver;

    private SqlEntityExpression _entityExpression;
    private SqlEntityConstantExpression _entityConstantExpression;
    private SqlEntityRefMemberExpression _entityRefMemberExpression;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage>();
      _context = new MappingResolutionContext();
      _resolver = new EntityIdentityResolver (_stageMock, _context);

      _entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), primaryKeyType: typeof (int));
      _entityConstantExpression = new SqlEntityConstantExpression (typeof (Cook), new Cook (), Expression.Constant (0));
      _entityRefMemberExpression = new SqlEntityRefMemberExpression (
          SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Kitchen)),
          typeof (Kitchen).GetProperty ("Cook"));
    }

    [Test]
    public void ResolvePotentialEntity_Entity ()
    {
      var result = _resolver.ResolvePotentialEntity (_entityExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (_entityExpression.GetIdentityExpression(), result);
    }

    [Test]
    public void ResolvePotentialEntity_EntityConstant ()
    {
      var result = _resolver.ResolvePotentialEntity (_entityConstantExpression);

      Assert.That (result, Is.SameAs (_entityConstantExpression.PrimaryKeyExpression));
    }

    [Test]
    public void ResolvePotentialEntity_EntityRefMember_Optimizable ()
    {
      var someTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo();
      var fakeResolvedJoinInfo = CreateJoinInfoWithKeyOnRightSide (someTableInfo);
      _stageMock
          .Expect (
              mock => mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (
                      e =>
                      e.OriginatingEntity == _entityRefMemberExpression.OriginatingEntity && e.MemberInfo == _entityRefMemberExpression.MemberInfo
                      && e.Cardinality == JoinCardinality.One),
                  Arg.Is (_context)))
          .Return (fakeResolvedJoinInfo);

      var result = _resolver.ResolvePotentialEntity (_entityRefMemberExpression);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResolvedJoinInfo.LeftKey));
    }

    [Test]
    public void ResolvePotentialEntity_EntityRefMember_NonOptimizable_NoPrimaryKeyOnRightSide ()
    {
      var someTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo ();
      var fakeResolvedJoinInfo = CreateJoinInfoWithKeyOnLeftSide (someTableInfo);
      _stageMock
          .Expect (
              mock => mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (
                      e =>
                      e.OriginatingEntity == _entityRefMemberExpression.OriginatingEntity && e.MemberInfo == _entityRefMemberExpression.MemberInfo
                      && e.Cardinality == JoinCardinality.One),
                  Arg.Is (_context)))
          .Return (fakeResolvedJoinInfo);

      var fakeResolvedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression();
      _stageMock
          .Expect (mock => mock.ResolveEntityRefMemberExpression (_entityRefMemberExpression, fakeResolvedJoinInfo, _context))
          .Return (fakeResolvedEntity);

      var result = _resolver.ResolvePotentialEntity (_entityRefMemberExpression);

      _stageMock.VerifyAllExpectations();
      ExpressionTreeComparer.CheckAreEqualTrees (fakeResolvedEntity.GetIdentityExpression(), result);
    }

    [Test]
    public void ResolvePotentialEntity_EntityRefMember_NonOptimizable_NoColumnOnRightSide ()
    {
      var someTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo ();
      var fakeResolvedJoinInfo = CreateJoinInfoWithConstantOnRightSide (someTableInfo);
      _stageMock
          .Expect (
              mock => mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (
                      e =>
                      e.OriginatingEntity == _entityRefMemberExpression.OriginatingEntity && e.MemberInfo == _entityRefMemberExpression.MemberInfo
                      && e.Cardinality == JoinCardinality.One),
                  Arg.Is (_context)))
          .Return (fakeResolvedJoinInfo);

      var fakeResolvedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression ();
      _stageMock
          .Expect (mock => mock.ResolveEntityRefMemberExpression (_entityRefMemberExpression, fakeResolvedJoinInfo, _context))
          .Return (fakeResolvedEntity);

      var result = _resolver.ResolvePotentialEntity (_entityRefMemberExpression);

      _stageMock.VerifyAllExpectations ();
      ExpressionTreeComparer.CheckAreEqualTrees (fakeResolvedEntity.GetIdentityExpression (), result);
    }

    // TODO 4878: ResolvePotentialEntity_SqlSubStatement_WithEntity/WithoutEntity
    // TODO 4878: ResolvePotentialEntityComparison_BinaryExpression_Yes/Converts/No
    // TODO 4878: ResolvePotentialEntityComparison_SqlInExpression_Yes/Converts/No
    // TODO 4878: ResolvePotentialEntityComparison_SqlIsNullExpression_Yes/Converts/No
    // TODO 4878: ResolvePotentialEntityComparison_SqlIsNotNullExpression_Yes/Converts/No

    private static ResolvedJoinInfo CreateJoinInfoWithKeyOnRightSide (ResolvedSimpleTableInfo someTableInfo)
    {
      var leftKey = Expression.Constant (0);
      var rightKey = new SqlColumnDefinitionExpression (typeof (int), "t0", "ID", true);
      var fakeResolvedJoinInfo = new ResolvedJoinInfo (someTableInfo, leftKey, rightKey);
      return fakeResolvedJoinInfo;
    }

    private static ResolvedJoinInfo CreateJoinInfoWithKeyOnLeftSide (ResolvedSimpleTableInfo someTableInfo)
    {
      var leftKey = new SqlColumnDefinitionExpression (typeof (int), "t0", "ID", true);
      var rightKey = new SqlColumnDefinitionExpression (typeof (int), "t0", "ID", false);
      var fakeResolvedJoinInfo = new ResolvedJoinInfo (someTableInfo, leftKey, rightKey);
      return fakeResolvedJoinInfo;
    }

    private static ResolvedJoinInfo CreateJoinInfoWithConstantOnRightSide (ResolvedSimpleTableInfo someTableInfo)
    {
      var leftKey = new SqlColumnDefinitionExpression (typeof (int), "t0", "ID", true);
      var rightKey = Expression.Constant (0);
      var fakeResolvedJoinInfo = new ResolvedJoinInfo (someTableInfo, leftKey, rightKey);
      return fakeResolvedJoinInfo;
    }
  }
}