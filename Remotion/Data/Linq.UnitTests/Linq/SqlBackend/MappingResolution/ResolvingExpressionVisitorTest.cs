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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingExpressionVisitorTest
  {
    private IMappingResolver _resolverMock;
    private SqlTable _sqlTable;
    private UniqueIdentifierGenerator _generator;
    private IMappingResolutionStage _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage>();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver>();
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (Cook));
      _generator = new UniqueIdentifierGenerator();
      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void VisitSqlTableReferenceExpression_ResolvesExpression ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (tableReferenceExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitSqlTableReferenceExpression_RevisitsResult ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (tableReferenceExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitMemberExpression_OnEntity ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var expression = Expression.Constant (new Cook());
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false));

      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (sqlEntityExpression);
      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (sqlEntityExpression, memberInfo, _generator))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (fakeResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberExpression_OnEntityRefMemberExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);
      var fakeEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true));
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveEntityRefMemberExpression (
                  Arg<SqlEntityRefMemberExpression>.Matches (e => e == expression),
                  Arg<UnresolvedJoinInfo>.Matches (j => j.OriginatingEntity.Type == typeof(Cook)), Arg<IMappingResolutionContext>.Matches(c=>c==_mappingResolutionContext)))
          .Return (fakeEntityExpression);
      _stageMock.Replay();

      _resolverMock
          .Expect (
              mock =>
              mock.ResolveMemberExpression (
                  Arg<SqlEntityExpression>.Matches (e => e.Type == typeof(Cook)),
                  Arg<MemberInfo>.Matches (mi => mi == memberInfo),
                  Arg<UniqueIdentifierGenerator>.Matches (g => g == _generator)))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (fakeResult));
      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberExpression_OnConvertExpression ()
    {
      var operand = new SqlColumnDefinitionExpression (typeof (Cook), "c", "ID", false);
      var convertExpression = Expression.Convert (operand, typeof (Chef));
      var memberExpression = Expression.MakeMemberAccess (convertExpression, typeof (Chef).GetProperty ("LetterOfRecommendation"));

      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (operand, memberExpression.Member))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();


      var result = ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (fakeResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitMemberExpression_MemberAppliedToConstant ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var expression = Expression.Constant (new Cook());
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);

      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock.Replay();

      ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);
    }

    [Test]
    public void VisitMemberExpression_OnColumnExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var expression = Expression.Constant (new Cook());
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);
      var constantExpression = Expression.Constant ("test");
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (columnExpression);
      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (columnExpression, memberInfo))
          .Return (constantExpression);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (constantExpression))
          .Return (constantExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (constantExpression));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitNamedExpression_SqlEntityExression ()
    {
      var constantExpression = Expression.Constant (5);
      var namedExpression = new NamedExpression ("test", constantExpression);
      var fakeResult = new SqlEntityDefinitionExpression (typeof (Cook), "c", new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (constantExpression))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (namedExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitNamedExpression_SqlEntityRefMemberExpression ()
    {
      var constantExpression = Expression.Constant (5);
      var namedExpression = new NamedExpression ("test", constantExpression);
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var fakeResult = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var expectedResult = new NamedExpression ("test", new SqlEntityRefMemberExpression (entityExpression, memberInfo));

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (constantExpression))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (namedExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitNamedExpression_NoSqlEntityExpressionAndNoSqlEntityRefMemberExpression ()
    {
      var constantExpression = Expression.Constant (5);
      var namedExpression = new NamedExpression ("test", constantExpression);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (constantExpression))
          .Return (constantExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (namedExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (namedExpression));
      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo (namedExpression.Name));
    }

    [Test]
    public void VisitNamedExpression_NoSqlEntityExpressionAndNoSqlEntityRefMemberExpression_ReturnsDifferentNamedExpression ()
    {
      var constantExpression = Expression.Constant (5);
      var namedExpression = new NamedExpression ("test", constantExpression);
      var fakeResult = Expression.Constant ("test");
      var expectedResult = new NamedExpression (namedExpression.Name, fakeResult);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (constantExpression))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (namedExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void UnknownExpression ()
    {
      var unknownExpression = new CustomExpression (typeof (int));
      var result = ResolvingExpressionVisitor.ResolveExpression (unknownExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (unknownExpression));
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (sqlStatement);
      _stageMock.Replay();

      SqlSubStatementExpression result =
          (SqlSubStatementExpression) ResolvingExpressionVisitor.ResolveExpression (expression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result.SqlStatement, Is.EqualTo (expression.SqlStatement));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlFunctionExpression ()
    {
      var prefixExpression = Expression.Constant ("test");
      var argumentExpression = Expression.Constant (1);
      var sqlFunctionExpression = new SqlFunctionExpression (typeof (int), "FUNCNAME", prefixExpression, argumentExpression);

      var resolvedExpression = Expression.Constant ("resolved");
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (prefixExpression))
          .Return (resolvedExpression);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (argumentExpression))
          .Return (resolvedExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlFunctionExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.TypeOf (typeof (SqlFunctionExpression)));
      Assert.That (((SqlFunctionExpression) result).Args[0], Is.SameAs (resolvedExpression));
      Assert.That (((SqlFunctionExpression) result).Args[1], Is.SameAs (resolvedExpression));
      _resolverMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitSqlConvertExpression ()
    {
      var expression = Expression.Constant (1);
      var sqlConvertExpression = new SqlConvertExpression (typeof (int), expression);

      var resolvedExpression = Expression.Constant ("1");
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (resolvedExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlConvertExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.TypeOf (typeof (SqlConvertExpression)));
      Assert.That (result.Type, Is.EqualTo (typeof (int)));
      Assert.That (((SqlConvertExpression) result).Source, Is.SameAs (resolvedExpression));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitTypeBinaryExpression ()
    {
      var expression = Expression.Constant ("select");
      var typeBinaryExpression = Expression.TypeIs (expression, typeof (Chef));
      var resolvedTypeExpression = Expression.Constant ("resolved");

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (expression);
      _resolverMock.Expect (mock => mock.ResolveTypeCheck (expression, typeof (Chef)))
          .Return (resolvedTypeExpression);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (resolvedTypeExpression))
          .Return (resolvedTypeExpression);
      _resolverMock.Replay();

      ResolvingExpressionVisitor.ResolveExpression (typeBinaryExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression ()
    {
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var memberInfo = typeof (Cook).GetProperty ("ID");
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefmemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      var result = ResolvingExpressionVisitor.ResolveExpression (entityRefmemberExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (entityRefmemberExpression));
    }

    [Test]
    public void VisitSqlEntityConstantExpression ()
    {
      var sqlEntityConstantExpression = new SqlEntityConstantExpression (typeof (Cook), "test", "key");

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlEntityConstantExpression, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (sqlEntityConstantExpression));
    }
    
  }
}