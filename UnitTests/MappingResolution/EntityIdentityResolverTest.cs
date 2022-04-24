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
using Moq;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class EntityIdentityResolverTest
  {
    private Mock<IMappingResolutionStage> _stageMock;
    private Mock<IMappingResolver> _resolverMock;
    private MappingResolutionContext _context;
    private EntityIdentityResolver _entityIdentityResolver;

    private SqlEntityExpression _entityExpression;
    private SqlEntityConstantExpression _entityConstantExpression;
    private SqlEntityRefMemberExpression _entityRefMemberExpression;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<IMappingResolutionStage> (MockBehavior.Strict);
      _resolverMock = new Mock<IMappingResolver> (MockBehavior.Strict);
      _context = new MappingResolutionContext();

      _entityIdentityResolver = new EntityIdentityResolver (_stageMock.Object, _resolverMock.Object, _context);

      _entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), primaryKeyType: typeof (int));
      _entityConstantExpression = new SqlEntityConstantExpression (typeof (Cook), new Cook (), Expression.Constant (0));
      _entityRefMemberExpression = new SqlEntityRefMemberExpression (
          SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Kitchen)),
          typeof (Kitchen).GetProperty ("Cook"));
    }

    [Test]
    public void ResolvePotentialEntity_Entity_ResolvesToIdentity ()
    {
      var result = _entityIdentityResolver.ResolvePotentialEntity (_entityExpression);

      SqlExpressionTreeComparer.CheckAreEqualTrees (_entityExpression.GetIdentityExpression(), result);
    }

    [Test]
    public void ResolvePotentialEntity_Entity_StripsConversions_AndNames ()
    {
      var result = _entityIdentityResolver.ResolvePotentialEntity (Expression.Convert (_entityExpression, typeof (object)));

      SqlExpressionTreeComparer.CheckAreEqualTrees (_entityExpression.GetIdentityExpression (), result);
    }

    [Test]
    public void ResolvePotentialEntity_Entity_WithIdentityExpressionGeneratorReturningSelf_ThrowsInvalidOperationException ()
    {
      var expression = new SqlEntityDefinitionExpression (typeof (Cook), "t", "Cook", e => e);

      Assert.That (
          () => _entityIdentityResolver.ResolvePotentialEntity (expression),
          Throws.InvalidOperationException.With.Message.EqualTo ("SqlEntityExpression cannot be the same instance as its identity Expression."));
    }

    [Test]
    public void ResolvePotentialEntity_EntityConstant_ResolvesToIdentity ()
    {
      var result = _entityIdentityResolver.ResolvePotentialEntity (_entityConstantExpression);

      Assert.That (result, Is.SameAs (_entityConstantExpression.IdentityExpression));
    }

    [Test]
    public void ResolvePotentialEntity_EntityConstant_StripsConversions ()
    {
      var result = _entityIdentityResolver.ResolvePotentialEntity (Expression.Convert (_entityConstantExpression, typeof (object)));

      Assert.That (result, Is.SameAs (_entityConstantExpression.IdentityExpression));
    }

    [Test]
    public void ResolvePotentialEntity_EntityRefMember_OptimizesReferenceViaResolver ()
    {
      var fakeOptimizedIdentity = ExpressionHelper.CreateExpression();
      _resolverMock
          .Setup (mock => mock.TryResolveOptimizedIdentity (_entityRefMemberExpression))
          .Returns (fakeOptimizedIdentity)
          .Verifiable();
      
      var result = _entityIdentityResolver.ResolvePotentialEntity (_entityRefMemberExpression);

      _resolverMock.Verify();
      Assert.That (result, Is.SameAs (fakeOptimizedIdentity));
    }

    [Test]
    public void ResolvePotentialEntity_EntityRefMember_NonOptimizable_ResolvesToJoinAndIdentity ()
    {
      _resolverMock
        .Setup (mock => mock.TryResolveOptimizedIdentity (_entityRefMemberExpression))
        .Returns ((Expression) null)
        .Verifiable();
      
      var fakeResolvedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression ();
      _stageMock
          .Setup (mock => mock.ResolveEntityRefMemberExpression (_entityRefMemberExpression, _context))
          .Returns (fakeResolvedEntity)
          .Verifiable();

      var result = _entityIdentityResolver.ResolvePotentialEntity (_entityRefMemberExpression);

      _stageMock.Verify();
      SqlExpressionTreeComparer.CheckAreEqualTrees (fakeResolvedEntity.GetIdentityExpression(), result);
    }

    [Test]
    public void ResolvePotentialEntity_EntityRefMember_StripsConversions ()
    {
      var fakeOptimizedIdentity = ExpressionHelper.CreateExpression ();
      _resolverMock
          .Setup (mock => mock.TryResolveOptimizedIdentity (_entityRefMemberExpression))
          .Returns (fakeOptimizedIdentity)
          .Verifiable();

      var result = _entityIdentityResolver.ResolvePotentialEntity (Expression.Convert (_entityRefMemberExpression, typeof (object)));

      _resolverMock.Verify();
      Assert.That (result, Is.SameAs (fakeOptimizedIdentity));
    }

    [Test]
    public void ResolvePotentialEntity_EntityRefMemeber_WithResolverReturningSameExpression_IsLeftUnchanged ()
    {
      _resolverMock
          .Setup (mock => mock.TryResolveOptimizedIdentity (_entityRefMemberExpression))
          .Returns (_entityRefMemberExpression)
          .Verifiable();

      var expression = Expression.Convert (_entityRefMemberExpression, typeof (object));

      var result = _entityIdentityResolver.ResolvePotentialEntity (expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void ResolvePotentialEntity_Substatement_WithEntity_ResolvesToSubstatement_WithIdentity ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (_entityExpression, SqlStatementModelObjectMother.CreateSqlTable());
      var subStatementExpression = new SqlSubStatementExpression (subStatement);

      var result = _entityIdentityResolver.ResolvePotentialEntity (subStatementExpression);

      Assert.That (result, Is.TypeOf<SqlSubStatementExpression>());
      var expectedSelectProjection = _entityExpression.GetIdentityExpression();
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, ((SqlSubStatementExpression) result).SqlStatement.SelectProjection);
      Assert.That (((SqlSubStatementExpression) result).SqlStatement.DataInfo.DataType, Is.SameAs (typeof (IQueryable<int>)));
    }

    [Test]
    public void ResolvePotentialEntity_Substatement_WithEntity_TrivialProjection_ResolvesToIdentityDirectly ()
    {
      // No SQL tables => the substatement can be completely eliminated.
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (_entityExpression);
      var subStatementExpression = new SqlSubStatementExpression (subStatement);

      var result = _entityIdentityResolver.ResolvePotentialEntity (subStatementExpression);

      var expectedSelectProjection = _entityExpression.GetIdentityExpression ();
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, result);
    }
    
    [Test]
    public void ResolvePotentialEntity_Substatement_WithoutEntity_ReturnsSameExpression ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (0));
      var subStatementExpression = new SqlSubStatementExpression (subStatement);

      var result = _entityIdentityResolver.ResolvePotentialEntity (subStatementExpression);

      Assert.That (result, Is.SameAs (subStatementExpression));
    }

    [Test]
    public void ResolvePotentialEntity_Substatement_StripsConversions ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Convert (_entityExpression, typeof (object)), SqlStatementModelObjectMother.CreateSqlTable ());
      var subStatementExpression = new SqlSubStatementExpression (subStatement);

      var result = _entityIdentityResolver.ResolvePotentialEntity (Expression.Convert (subStatementExpression, typeof (object)));

      Assert.That (result, Is.TypeOf<SqlSubStatementExpression> ());
      var expectedSelectProjection = _entityExpression.GetIdentityExpression ();
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, ((SqlSubStatementExpression) result).SqlStatement.SelectProjection);
      Assert.That (((SqlSubStatementExpression) result).SqlStatement.DataInfo.DataType, Is.SameAs (typeof (IQueryable<int>)));
    }

    [Test]
    public void ResolvePotentialEntity_ConvertedSubstatement_WithNoEntity_IsLeftUnchanged ()
    {
      var selectProjectionExpression = Expression.Constant (0);
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (selectProjectionExpression, SqlStatementModelObjectMother.CreateSqlTable());
      var subStatementExpression = new SqlSubStatementExpression (subStatement);
      var expression = Expression.Convert (subStatementExpression, typeof (object));

      var result = _entityIdentityResolver.ResolvePotentialEntity (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void ResolvePotentialEntity_NonEntity_ReturnsSameExpression ()
    {
      var expression = Expression.Convert (Expression.Constant (0), typeof (double));

      var result = _entityIdentityResolver.ResolvePotentialEntity (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void ResolvePotentialEntity_LooksBehindNames ()
    {
      var result = _entityIdentityResolver.ResolvePotentialEntity (new NamedExpression ("X", _entityConstantExpression));

      SqlExpressionTreeComparer.CheckAreEqualTrees (new NamedExpression ("X", _entityConstantExpression.IdentityExpression), result);
    }

    [Test]
    public void ResolvePotentialEntity_NamedExpression_WithNoEntity_IsLeftUnchanged ()
    {
      var namedExpression = new NamedExpression ("X", Expression.Constant (0));

      var result = _entityIdentityResolver.ResolvePotentialEntity (namedExpression);

      Assert.That (result, Is.SameAs (namedExpression));
    }

    [Test]
    public void ResolvePotentialEntity_ConvertedNamedExpression_WithNoEntity_IsLeftUnchanged ()
    {
      var expression = Expression.Convert (new NamedExpression ("X", Expression.Constant (0)), typeof (object));

      var result = _entityIdentityResolver.ResolvePotentialEntity (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_ResolvesEntitiesToIdentity ()
    {
      var binary = Expression.Equal (_entityExpression, _entityConstantExpression);

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (binary);

      var expected = Expression.Equal (_entityExpression.GetIdentityExpression(), _entityConstantExpression.IdentityExpression);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_EntitiesAreResolvedToIdentity_LeftOnly_InsertsConversions ()
    {
      var binary = Expression.Equal (_entityExpression, Expression.Constant (null, typeof (Cook)));

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (binary);

      var expected = Expression.Equal (
          Expression.Convert (_entityExpression.GetIdentityExpression(), typeof (object)),
          Expression.Convert (Expression.Constant (null, typeof (Cook)), typeof (object)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_EntitiesAreResolvedToIdentity_RightOnly_InsertsConversions ()
    {
      var binary = Expression.Equal (Expression.Constant (null, typeof (Cook)), _entityExpression);

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (binary);

      var expected = Expression.Equal (
          Expression.Convert (Expression.Constant (null, typeof (Cook)), typeof (object)),
          Expression.Convert (_entityExpression.GetIdentityExpression (), typeof (object)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_RemovesOperatorMethod ()
    {
      var binary = Expression.Equal (
          _entityExpression, _entityConstantExpression, false, MemberInfoFromExpressionUtility.GetMethod (() => FakeEqualityOperator (null, null)));

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (binary);

      var expected = Expression.Equal (_entityExpression.GetIdentityExpression (), _entityConstantExpression.IdentityExpression);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_BinaryExpression_NonEntities_ReturnsSameExpression ()
    {
      var binary = Expression.Equal (Expression.Constant (0), Expression.Constant (0));

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (binary);

      Assert.That (result, Is.SameAs (binary));
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlInExpression_ResolvesEntitiesToIdentity ()
    {
      var sqlInExpression = new SqlInExpression (_entityExpression, _entityConstantExpression);

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (sqlInExpression);

      var expected = new SqlInExpression (_entityExpression.GetIdentityExpression (), _entityConstantExpression.IdentityExpression);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlInExpression_EntitiesAreResolvedToIdentity_LeftOnly ()
    {
      var sqlInExpression = new SqlInExpression (_entityExpression, Expression.Constant (null, typeof (Cook)));

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (sqlInExpression);

      var expected = new SqlInExpression (_entityExpression.GetIdentityExpression (), Expression.Constant (null, typeof (Cook)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlInExpression_EntitiesAreResolvedToIdentity_RightOnly ()
    {
      var sqlInExpression = new SqlInExpression (Expression.Constant (null, typeof (Cook)), _entityExpression);

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (sqlInExpression);

      var expected = new SqlInExpression (Expression.Constant (null, typeof (Cook)), _entityExpression.GetIdentityExpression ());
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlInExpression_NonEntities_ReturnsSameExpression ()
    {
      var sqlInExpression = new SqlInExpression (Expression.Constant (0), Expression.Constant (0));

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (sqlInExpression);

      Assert.That (result, Is.SameAs (sqlInExpression));
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlIsNullExpression_ResolvesEntitiesToIdentity ()
    {
      var sqlIsNullExpression = new SqlIsNullExpression (_entityExpression);

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (sqlIsNullExpression);

      var expected = new SqlIsNullExpression (_entityExpression.GetIdentityExpression ());
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlIsNullExpression_NonEntities_ReturnsSameExpression ()
    {
      var sqlIsNullExpression = new SqlIsNullExpression (Expression.Constant (0));

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (sqlIsNullExpression);

      Assert.That (result, Is.SameAs (sqlIsNullExpression));
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlIsNotNullExpression_ResolvesEntitiesToIdentity ()
    {
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (_entityExpression);

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (sqlIsNotNullExpression);

      var expected = new SqlIsNotNullExpression (_entityExpression.GetIdentityExpression ());
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolvePotentialEntityComparison_SqlIsNotNullExpression_NonEntities_ReturnsSameExpression ()
    {
      var sqlIsNotNullExpression = new SqlIsNotNullExpression (Expression.Constant (0));

      var result = _entityIdentityResolver.ResolvePotentialEntityComparison (sqlIsNotNullExpression);

      Assert.That (result, Is.SameAs (sqlIsNotNullExpression));
    }

    [UsedImplicitly]
    private static bool FakeEqualityOperator (Cook one, Cook two)
    {
      throw new NotImplementedException();
    }
  }
}