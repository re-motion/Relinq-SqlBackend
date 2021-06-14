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
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class MemberAccessResolverTest
  {
    private IMappingResolutionStage _stageMock;
    private IMappingResolver _resolverMock;
    private MappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage> ();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver> ();
      _mappingResolutionContext = new MappingResolutionContext ();
    }

    [Test]
    public void ResolveMemberAccess_OnEntity ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var sqlEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (sqlEntityExpression, memberInfo))
          .Return (fakeResult);
     _resolverMock.Replay ();

     var result = MemberAccessResolver.ResolveMemberAccess (sqlEntityExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveMemberAccess_OnEntity_WithCollectionMember ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Courses");
      var sqlEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "c");
      Assert.That (
          () => MemberAccessResolver.ResolveMemberAccess (sqlEntityExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The member 'Cook.Courses' describes a collection and can only be used in places where collections are allowed. Expression: '[c]'"));
    }

    [Test]
    public void ResolveMemberAccess_OnEntityRefMemberExpression_NoOptimization ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var entityRefMemberExpression = SqlStatementModelObjectMother.CreateSqlEntityRefMemberExpression ();

      _resolverMock
          .Stub (mock => mock.TryResolveOptimizedMemberExpression (entityRefMemberExpression, memberInfo))
          .Return (null);

      var fakeEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      _stageMock
          .Expect (
              mock =>
              mock.ResolveEntityRefMemberExpression (
                  Arg.Is (entityRefMemberExpression),
                  Arg<UnresolvedJoinInfo>.Matches (j => 
                      j.OriginatingEntity == entityRefMemberExpression.OriginatingEntity 
                      && j.MemberInfo == entityRefMemberExpression.MemberInfo
                      && j.Cardinality == JoinCardinality.One),
                  Arg.Is (_mappingResolutionContext)))
          .Return (fakeEntityExpression);

      var fakeResult = Expression.Constant (0);
      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (fakeEntityExpression, memberInfo))
          .Return (fakeResult);

      var result = MemberAccessResolver.ResolveMemberAccess (entityRefMemberExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations ();
      _resolverMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveMemberAccess_OnEntityRefMemberExpression_WithSuccessfulOptimization ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var entityRefMemberExpression = SqlStatementModelObjectMother.CreateSqlEntityRefMemberExpression ();

      var fakeResolvedExpression = ExpressionHelper.CreateExpression ();
      _resolverMock
          .Expect (mock => mock.TryResolveOptimizedMemberExpression (entityRefMemberExpression, memberInfo))
          .Return (fakeResolvedExpression);

      var result = MemberAccessResolver.ResolveMemberAccess (entityRefMemberExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations ();
      _stageMock.AssertWasNotCalled (
          mock => mock.ResolveEntityRefMemberExpression (
              Arg<SqlEntityRefMemberExpression>.Is.Anything, Arg<IJoinInfo>.Is.Anything, Arg<IMappingResolutionContext>.Is.Anything));
      Assert.That (result, Is.SameAs (fakeResolvedExpression));
    }


    [Test]
    public void ResolveMemberAccess_OnUnaryExpression_Convert ()
    {
      var operand = new SqlColumnDefinitionExpression (typeof (Cook), "c", "ID", false);
      var convertExpression = Expression.Convert (Expression.Convert (operand, typeof (Chef)), typeof (Chef));
      var memberInfo = typeof (Chef).GetProperty ("LetterOfRecommendation");
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (operand, memberInfo))
          .Return (fakeResult);
      _resolverMock.Replay ();
      
      var result = MemberAccessResolver.ResolveMemberAccess(convertExpression, memberInfo,  _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveMemberAccess_OnUnaryExpression_NotConvert ()
    {
      var operand = new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false);
      var convertExpression = Expression.Negate (operand);
      var memberInfo = typeof (Chef).GetProperty ("LetterOfRecommendation");
      Assert.That (
          () => MemberAccessResolver.ResolveMemberAccess (convertExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "Cannot resolve member 'LetterOfRecommendation' applied to expression '-[c].[ID]'; the expression type 'UnaryExpression' is not supported in "
                  + "member expressions."));
    }

    [Test]
    public void ResolveMemberAccess_OnNamedExpression ()
    {
      var operand = new SqlColumnDefinitionExpression (typeof (Cook), "c", "ID", false);
      var namedExpression = new NamedExpression ("two", new NamedExpression ("one", operand));
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
     
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (operand, memberInfo))
          .Return (fakeResult);
      _resolverMock.Replay ();

      var result = MemberAccessResolver.ResolveMemberAccess (namedExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveMemberAccess_OnNestedNamedConvertExpressions ()
    {
      var operand = new SqlColumnDefinitionExpression (typeof (Cook), "c", "ID", false);
      var innerMostNamedExpression = new NamedExpression ("inner", operand);
      var innerConvertExpression = Expression.Convert (innerMostNamedExpression, typeof (Chef));
      var outerNamedExpression = new NamedExpression ("outer", innerConvertExpression);
      var outerMostConvertExpression = Expression.Convert (outerNamedExpression, typeof (Cook));

      var memberInfo = typeof (Cook).GetProperty ("FirstName");

      var fakeResult = Expression.Constant ("empty");

      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (operand, memberInfo))
          .Return (fakeResult);
      _resolverMock.Replay ();

      var result = MemberAccessResolver.ResolveMemberAccess (outerMostConvertExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveMemberAccess_MemberAppliedToConstant ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var expression = Expression.Constant (new Cook ());
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock.Replay ();
      Assert.That (
          () => MemberAccessResolver.ResolveMemberAccess (expression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext),
          Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void ResolveMemberAccess_OnColumnExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var constantExpression = Expression.Constant ("test");
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);

      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (columnExpression, memberInfo))
          .Return (constantExpression);
      _resolverMock.Replay ();

      var result = MemberAccessResolver.ResolveMemberAccess (columnExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (constantExpression));
    }

    [Test]
    public void ResolveMemberAccess_OnGroupingSelectExpression ()
    {
      var expression = SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression();
      var memberInfo = typeof (IGrouping<string, string>).GetProperty ("Key");

      var result = MemberAccessResolver.ResolveMemberAccess (expression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (expression.KeyExpression));
    }

    [Test]
    public void ResolveMemberAccess_OnGroupingSelectExpression_StripsNames ()
    {
      var expression = new SqlGroupingSelectExpression (
          new NamedExpression ("k", Expression.Constant ("key")), 
          new NamedExpression ("e", Expression.Constant ("element")));
      var memberInfo = typeof (IGrouping<string, string>).GetProperty ("Key");

      var result = MemberAccessResolver.ResolveMemberAccess (expression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (((NamedExpression) expression.KeyExpression).Expression));
    }

    [Test]
    public void ResolveMemberAccess_OnNewExpression_PropertyInfo ()
    {
      var constructorInfo = TypeForNewExpression.GetConstructor (typeof (int), typeof (int));
      var newExpression = Expression.New (
          constructorInfo,
          new[] { new NamedExpression ("value", new SqlLiteralExpression (1)), new NamedExpression ("value", new SqlLiteralExpression (2)) },
          typeof (TypeForNewExpression).GetMethod ("get_A"), typeof (TypeForNewExpression).GetMethod ("get_B"));

      var result = MemberAccessResolver.ResolveMemberAccess (
          newExpression, 
          typeof (TypeForNewExpression).GetProperty ("B"), 
          _resolverMock, 
          _stageMock,
          _mappingResolutionContext);

      Assert.That (result, Is.SameAs (((NamedExpression) newExpression.Arguments[1]).Expression));
    }

    [Test]
    public void ResolveMemberAccess_OnNewExpression_FieldInfo ()
    {
      var constructorInfo = TypeForNewExpression.GetConstructor (typeof (int), typeof (int));
      var newExpression = Expression.New (
          constructorInfo,
          new[] { new NamedExpression ("value", new SqlLiteralExpression (1)), new NamedExpression ("value", new SqlLiteralExpression (2)) },
          typeof (TypeForNewExpression).GetField("C"), typeof (TypeForNewExpression).GetMethod ("get_B"));

      var result = MemberAccessResolver.ResolveMemberAccess (
          newExpression,
          typeof (TypeForNewExpression).GetField ("C"),
          _resolverMock,
          _stageMock,
          _mappingResolutionContext);

      Assert.That (result, Is.SameAs (((NamedExpression) newExpression.Arguments[0]).Expression));
    }

    [Test]
    public void ResolveMemberAccess_OnNewExpression_NoMembers ()
    {
      var constructorInfo = TypeForNewExpression.GetConstructor (typeof (int), typeof (int));
      var newExpression = Expression.New (
          constructorInfo,
          new[] { new NamedExpression ("value", new SqlLiteralExpression (1)), new NamedExpression ("value", new SqlLiteralExpression (2)) });
      Assert.That (
          () => MemberAccessResolver.ResolveMemberAccess (
          newExpression,
          typeof (TypeForNewExpression).GetField ("C"),
          _resolverMock,
          _stageMock,
          _mappingResolutionContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The member 'TypeForNewExpression.C' cannot be translated to SQL. "+
                  "Expression: 'new TypeForNewExpression(1 AS value, 2 AS value)'"));
    }

    [Test]
    public void ResolveMemberAccess_OnUnsupportedExpression ()
    {
      var operand = new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false);
      var convertExpression = Expression.And (operand, operand);
      var memberInfo = typeof (Chef).GetProperty ("LetterOfRecommendation");
      Assert.That (
          () => MemberAccessResolver.ResolveMemberAccess (convertExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "Cannot resolve member 'LetterOfRecommendation' applied to expression '([c].[ID] & [c].[ID])'; the expression type 'SimpleBinaryExpression' is not "
                  + "supported in member expressions."));
    }

    [Test]
    public void ResolveMemberAccess_OnSqlTableReferenceExpression ()
    {
      var expression = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook)));
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
      Assert.That (
          () => MemberAccessResolver.ResolveMemberAccess (expression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "Cannot resolve member 'FirstName' applied to expression 'TABLE-REF(UnresolvedTableInfo(Cook))'; "
                  +"the expression type 'SqlTableReferenceExpression' is not supported in member expressions."));
    }

    [Test]
    public void ResolveMemberAccess_OnSqlEntityConstantExpression ()
    {
      var expression = new SqlEntityConstantExpression (typeof (Cook), new Cook(), Expression.Constant (14));
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
      Assert.That (
          () => MemberAccessResolver.ResolveMemberAccess (expression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "Cannot resolve member 'FirstName' applied to expression 'ENTITY(14)'; the expression type 'SqlEntityConstantExpression' is not supported in "
                  + "member expressions."));
    }
  }
}