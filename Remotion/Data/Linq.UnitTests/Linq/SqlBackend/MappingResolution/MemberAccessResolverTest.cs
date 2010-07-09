// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
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
    public void VisitMemberExpression_OnEntity ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false));
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
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The member 'Cook.Courses' describes a collection and can only be used in places where collections are allowed.")]
    public void VisitMemberExpression_OnEntity_WithCollectionMember ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Courses");
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false));

      MemberAccessResolver.ResolveMemberAccess (sqlEntityExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);
    }

    [Test]
    public void VisitMemberExpression_OnEntityRefMemberExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      var fakeEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true));
      _stageMock
          .Expect (
              mock =>
              mock.ResolveEntityRefMemberExpression (
                  Arg.Is (entityRefMemberExpression),
                  Arg<UnresolvedJoinInfo>.Matches (j => 
                      j.OriginatingEntity == entityExpression 
                      && j.MemberInfo == memberInfo 
                      && j.Cardinality == JoinCardinality.One),
                  Arg.Is (_mappingResolutionContext)))
          .Return (fakeEntityExpression);
      _stageMock.Replay ();

      var fakeResult = Expression.Constant (0);
      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (fakeEntityExpression, memberInfo))
          .Return (fakeResult);
      _resolverMock.Replay ();

      var result = MemberAccessResolver.ResolveMemberAccess (entityRefMemberExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations ();
      _resolverMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitMemberExpression_OnUnaryExpression_Convert ()
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
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "Cannot resolve member 'LetterOfRecommendation' applied to expression '-[c].[ID]'; the expression type 'UnaryExpression' is not supported in "
        + "member expressions.")]
    public void VisitMemberExpression_OnUnaryExpression_NotConvert ()
    {
      var operand = new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false);
      var convertExpression = Expression.Negate (operand);
      var memberInfo = typeof (Chef).GetProperty ("LetterOfRecommendation");

      MemberAccessResolver.ResolveMemberAccess (convertExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);
    }

    [Test]
    public void VisitMemberExpression_OnNamedExpression ()
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
    public void VisitMemberExpression_OnNestedNamedConvertExpressions ()
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
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitMemberExpression_MemberAppliedToConstant ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var expression = Expression.Constant (new Cook ());
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock.Replay ();

      MemberAccessResolver.ResolveMemberAccess (expression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);
    }

    [Test]
    public void VisitMemberExpression_OnColumnExpression ()
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
    public void VisitMemberExpression_OnGroupingSelectExpression ()
    {
      var expression = SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression();
      var memberInfo = typeof (IGrouping<string, string>).GetProperty ("Key");

      var result = MemberAccessResolver.ResolveMemberAccess (expression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (expression.KeyExpression));
    }

    [Test]
    public void VisitMemberExpression_OnNewExpression_PropertyInfo ()
    {
      var constructorInfo = typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int), typeof (int) });
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

      Assert.That (result, Is.SameAs (newExpression.Arguments[1]));
    }

    [Test]
    public void VisitMemberExpression_OnNewExpression_FieldInfo ()
    {
      var constructorInfo = typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int), typeof (int) });
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

      Assert.That (result, Is.SameAs (newExpression.Arguments[0]));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'TypeForNewExpression.C' cannot be translated to SQL. "+
      "Expression: 'new TypeForNewExpression(1 AS value, 2 AS value)'")]
    public void VisitMemberExpression_OnNewExpression_NoMembers ()
    {
      var constructorInfo = typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int), typeof (int) });
      var newExpression = Expression.New (
          constructorInfo,
          new[] { new NamedExpression ("value", new SqlLiteralExpression (1)), new NamedExpression ("value", new SqlLiteralExpression (2)) });

      MemberAccessResolver.ResolveMemberAccess (
          newExpression,
          typeof (TypeForNewExpression).GetField ("C"),
          _resolverMock,
          _stageMock,
          _mappingResolutionContext);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot resolve member 'LetterOfRecommendation' applied to expression '([c].[ID] & [c].[ID])'; the expression type 'BinaryExpression' is not "
        + "supported in member expressions.")]
    public void VisitMemberExpression_OnUnsupportedExpression ()
    {
      var operand = new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false);
      var convertExpression = Expression.And (operand, operand);
      var memberInfo = typeof (Chef).GetProperty ("LetterOfRecommendation");

      MemberAccessResolver.ResolveMemberAccess (convertExpression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot resolve member 'FirstName' applied to expression 'TABLE-REF(TABLE(Cook))'; the expression type 'SqlTableReferenceExpression' is not "
        + "supported in member expressions.")]
    public void VisitMemberExpression_OnSqlTableReferenceExpression ()
    {
      var expression = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook)));
      var memberInfo = typeof (Cook).GetProperty ("FirstName");

      MemberAccessResolver.ResolveMemberAccess (expression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot resolve member 'FirstName' applied to expression 'ENTITY(14)'; the expression type 'SqlEntityConstantExpression' is not supported in "
        + "member expressions.")]
    public void VisitMemberExpression_OnSqlEntityConstantExpression ()
    {
      var expression = new SqlEntityConstantExpression (typeof (Cook), new Cook(), "14");
      var memberInfo = typeof (Cook).GetProperty ("FirstName");

      MemberAccessResolver.ResolveMemberAccess (expression, memberInfo, _resolverMock, _stageMock, _mappingResolutionContext);
    }
  }
}