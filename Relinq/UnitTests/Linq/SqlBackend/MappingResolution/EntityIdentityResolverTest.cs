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
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
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
    public void ResolvePotentialEntity_Entity_ResolvesToIdentity ()
    {
      var result = _resolver.ResolvePotentialEntity (_entityExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (_entityExpression.GetIdentityExpression(), result);
    }

    [Test]
    public void ResolvePotentialEntity_Entity_StripsConversions ()
    {
      var result = _resolver.ResolvePotentialEntity (Expression.Convert (_entityExpression, typeof (object)));

      ExpressionTreeComparer.CheckAreEqualTrees (_entityExpression.GetIdentityExpression (), result);
    }

    [Test]
    public void ResolvePotentialEntity_EntityConstant_ResolvesToIdentity ()
    {
      var result = _resolver.ResolvePotentialEntity (_entityConstantExpression);

      Assert.That (result, Is.SameAs (_entityConstantExpression.PrimaryKeyExpression));
    }

    [Test]
    public void ResolvePotentialEntity_EntityConstant_StripsConversions ()
    {
      var result = _resolver.ResolvePotentialEntity (Expression.Convert (_entityConstantExpression, typeof (object)));

      Assert.That (result, Is.SameAs (_entityConstantExpression.PrimaryKeyExpression));
    }

    [Test]
    public void ResolvePotentialEntity_EntityRefMember_Optimizable_ResolvesToLeftKey ()
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
    public void ResolvePotentialEntity_EntityRefMember_NonOptimizable_NoPrimaryKeyOnRightSide_ResolvesToJoinAndIdentity ()
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
    public void ResolvePotentialEntity_EntityRefMember_NonOptimizable_NoColumnOnRightSide_ResolvesToJoinAndIdentity ()
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

    [Test]
    public void ResolvePotentialEntity_EntityRefMember_StripsConversions ()
    {
      var someTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo ();
      var fakeResolvedJoinInfo = CreateJoinInfoWithConstantOnRightSide (someTableInfo);
      _stageMock
          .Expect (mock => mock.ResolveJoinInfo (Arg<UnresolvedJoinInfo>.Is.Anything, Arg.Is (_context)))
          .Return (fakeResolvedJoinInfo);

      var fakeResolvedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression ();
      _stageMock
          .Expect (mock => mock.ResolveEntityRefMemberExpression (_entityRefMemberExpression, fakeResolvedJoinInfo, _context))
          .Return (fakeResolvedEntity);

      var result = _resolver.ResolvePotentialEntity (Expression.Convert (_entityRefMemberExpression, typeof (object)));

      _stageMock.VerifyAllExpectations ();
      ExpressionTreeComparer.CheckAreEqualTrees (fakeResolvedEntity.GetIdentityExpression (), result);
    }

    [Test]
    public void ResolvePotentialEntity_Substatement_WithEntity_ResolvesToSubstatement_WithIdentity ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (_entityExpression, SqlStatementModelObjectMother.CreateSqlTable());
      var subStatementExpression = new SqlSubStatementExpression (subStatement);

      var result = _resolver.ResolvePotentialEntity (subStatementExpression);

      Assert.That (result, Is.TypeOf<SqlSubStatementExpression>());
      var expectedSelectProjection = _entityExpression.GetIdentityExpression();
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, ((SqlSubStatementExpression) result).SqlStatement.SelectProjection);
      Assert.That (((SqlSubStatementExpression) result).SqlStatement.DataInfo.DataType, Is.SameAs (typeof (IQueryable<int>)));
    }

    [Test]
    public void ResolvePotentialEntity_Substatement_WithEntity_TrivialProjection_ResolvesToIdentityDirectly ()
    {
      // No SQL tables => the substatement can be completely eliminated.
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (_entityExpression);
      var subStatementExpression = new SqlSubStatementExpression (subStatement);

      var result = _resolver.ResolvePotentialEntity (subStatementExpression);

      var expectedSelectProjection = _entityExpression.GetIdentityExpression ();
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, result);
    }
    
    [Test]
    public void ResolvePotentialEntity_Substatement_WithoutEntity_ReturnsSameExpression ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (0));
      var subStatementExpression = new SqlSubStatementExpression (subStatement);

      var result = _resolver.ResolvePotentialEntity (subStatementExpression);

      Assert.That (result, Is.SameAs (subStatementExpression));
    }

    [Test]
    public void ResolvePotentialEntity_Substatement_StripsConversions ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Convert (_entityExpression, typeof (object)), SqlStatementModelObjectMother.CreateSqlTable ());
      var subStatementExpression = new SqlSubStatementExpression (subStatement);

      var result = _resolver.ResolvePotentialEntity (Expression.Convert (subStatementExpression, typeof (object)));

      Assert.That (result, Is.TypeOf<SqlSubStatementExpression> ());
      var expectedSelectProjection = _entityExpression.GetIdentityExpression ();
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, ((SqlSubStatementExpression) result).SqlStatement.SelectProjection);
      Assert.That (((SqlSubStatementExpression) result).SqlStatement.DataInfo.DataType, Is.SameAs (typeof (IQueryable<int>)));
    }

    [Test]
    public void ResolvePotentialEntity_NonEntity_ReturnsSameExpression ()
    {
      var expression = Expression.Convert (Expression.Constant (0), typeof (double));

      var result = _resolver.ResolvePotentialEntity (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_ResolvesEntitiesToIdentity ()
    {
      var binary = Expression.Equal (_entityExpression, _entityConstantExpression);

      var result = _resolver.ResolvePotentialEntityComparison (binary);

      var expected = Expression.Equal (_entityExpression.GetIdentityExpression(), _entityConstantExpression.PrimaryKeyExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_EntitiesAreResolvedToIdentity_LeftOnly_InsertsConversions ()
    {
      var binary = Expression.Equal (_entityExpression, Expression.Constant (null, typeof (Cook)));

      var result = _resolver.ResolvePotentialEntityComparison (binary);

      var expected = Expression.Equal (
          Expression.Convert (_entityExpression.GetIdentityExpression(), typeof (object)),
          Expression.Convert (Expression.Constant (null, typeof (Cook)), typeof (object)));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_EntitiesAreResolvedToIdentity_RightOnly_InsertsConversions ()
    {
      var binary = Expression.Equal (Expression.Constant (null, typeof (Cook)), _entityExpression);

      var result = _resolver.ResolvePotentialEntityComparison (binary);

      var expected = Expression.Equal (
          Expression.Convert (Expression.Constant (null, typeof (Cook)), typeof (object)),
          Expression.Convert (_entityExpression.GetIdentityExpression (), typeof (object)));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_RemovesOperatorMethod ()
    {
      var binary = Expression.Equal (
          _entityExpression, _entityConstantExpression, false, ReflectionUtility.GetMethod (() => FakeEqualityOperator (null, null)));

      var result = _resolver.ResolvePotentialEntityComparison (binary);

      var expected = Expression.Equal (_entityExpression.GetIdentityExpression (), _entityConstantExpression.PrimaryKeyExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_NonEntities_ReturnsSameExpression ()
    {
      var binary = Expression.Equal (Expression.Constant (0), Expression.Constant (0));

      var result = _resolver.ResolvePotentialEntityComparison (binary);

      Assert.That (result, Is.SameAs (binary));
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlInExpression_ResolvesEntitiesToIdentity ()
    {
      var sqlInExpression = new SqlInExpression (_entityExpression, _entityConstantExpression);

      var result = _resolver.ResolvePotentialEntityComparison (sqlInExpression);

      var expected = new SqlInExpression (_entityExpression.GetIdentityExpression (), _entityConstantExpression.PrimaryKeyExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlInExpression_EntitiesAreResolvedToIdentity_LeftOnly ()
    {
      var sqlInExpression = new SqlInExpression (_entityExpression, Expression.Constant (null, typeof (Cook)));

      var result = _resolver.ResolvePotentialEntityComparison (sqlInExpression);

      var expected = new SqlInExpression (_entityExpression.GetIdentityExpression (), Expression.Constant (null, typeof (Cook)));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlInExpression_EntitiesAreResolvedToIdentity_RightOnly ()
    {
      var sqlInExpression = new SqlInExpression (Expression.Constant (null, typeof (Cook)), _entityExpression);

      var result = _resolver.ResolvePotentialEntityComparison (sqlInExpression);

      var expected = new SqlInExpression (Expression.Constant (null, typeof (Cook)), _entityExpression.GetIdentityExpression ());
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlInExpression_NonEntities_ReturnsSameExpression ()
    {
      var sqlInExpression = new SqlInExpression (Expression.Constant (0), Expression.Constant (0));

      var result = _resolver.ResolvePotentialEntityComparison (sqlInExpression);

      Assert.That (result, Is.SameAs (sqlInExpression));
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlIsNullExpression_ResolvesEntitiesToIdentity ()
    {
      var sqlIsNullExpression = new SqlIsNullExpression (_entityExpression);

      var result = _resolver.ResolvePotentialEntityComparison (sqlIsNullExpression);

      var expected = new SqlIsNullExpression (_entityExpression.GetIdentityExpression ());
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlIsNullExpression_NonEntities_ReturnsSameExpression ()
    {
      var sqlIsNullExpression = new SqlIsNullExpression (Expression.Constant (0));

      var result = _resolver.ResolvePotentialEntityComparison (sqlIsNullExpression);

      Assert.That (result, Is.SameAs (sqlIsNullExpression));
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlIsNotNullExpression_ResolvesEntitiesToIdentity ()
    {
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (_entityExpression);

      var result = _resolver.ResolvePotentialEntityComparison (sqlIsNotNullExpression);

      var expected = new SqlIsNotNullExpression (_entityExpression.GetIdentityExpression ());
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlIsNotNullExpression_NonEntities_ReturnsSameExpression ()
    {
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (Expression.Constant (0));

      var result = _resolver.ResolvePotentialEntityComparison (sqlIsNotNullExpression);

      Assert.That (result, Is.SameAs (sqlIsNotNullExpression));
    }

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

    [UsedImplicitly]
    private static bool FakeEqualityOperator (Cook one, Cook two)
    {
      throw new NotImplementedException();
    }
  }
}