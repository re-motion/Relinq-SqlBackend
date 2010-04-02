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
    private ResolvedJoinInfo _resolvedJoinInfo;
    private PropertyInfo _kitchenCookMember;
    private IMappingResolutionStage _stageMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<IMappingResolutionStage>();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver>();
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo();
      _generator = new UniqueIdentifierGenerator();

      _resolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();

      _kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
    }

    [Test]
    public void VisitSqlTableReferenceExpression_ResolvesExpression ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _generator))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (tableReferenceExpression, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (fakeResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlTableReferenceExpression_ResolvesExpression_AndRevisitsResult ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var unresolvedResult = new SqlTableReferenceExpression (_sqlTable);
      var resolvedResult = Expression.Constant (0);

      using (_resolverMock.GetMockRepository().Ordered())
      {
        _resolverMock
            .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _generator))
            .Return (unresolvedResult);
        _resolverMock
            .Expect (mock => mock.ResolveTableReferenceExpression (unresolvedResult, _generator))
            .Return (resolvedResult);
        _resolverMock
            .Expect (mock => mock.ResolveConstantExpression (resolvedResult))
            .Return (resolvedResult);
      }
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (tableReferenceExpression, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (resolvedResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlTableReferenceExpression_ResolvesExpression_AndRevisitsResult_OnlyIfDifferent ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _generator))
          .Return (tableReferenceExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (tableReferenceExpression, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (tableReferenceExpression));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlMemberExpression_CreatesSqlColumnExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var memberExpression = new SqlMemberExpression (_sqlTable, memberInfo);

      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (memberExpression, _generator))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (fakeResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlMemberExpression_ResolvesExpression_AndRevisitsResult ()
    {
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
      var sqlMemberExpression = new SqlMemberExpression (_sqlTable, memberInfo);

      var unresolvedResult = new SqlMemberExpression (_sqlTable, memberInfo);
      var resolvedResult = Expression.Constant (0);

      using (_resolverMock.GetMockRepository().Ordered())
      {
        _resolverMock
            .Expect (mock => mock.ResolveMemberExpression (sqlMemberExpression, _generator))
            .Return (unresolvedResult);
        _resolverMock
            .Expect (mock => mock.ResolveMemberExpression (unresolvedResult, _generator))
            .Return (resolvedResult);
        _resolverMock
            .Expect (mock => mock.ResolveConstantExpression (resolvedResult))
            .Return (resolvedResult);
      }
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlMemberExpression, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (resolvedResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlMemberExpression_ResolvesExpression_AndRevisitsResult_OnlyIfDifferent ()
    {
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
      var sqlMemberExpression = new SqlMemberExpression (_sqlTable, memberInfo);

      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (sqlMemberExpression, _generator))
          .Return (sqlMemberExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlMemberExpression, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (sqlMemberExpression));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_CreatesJoin ()
    {
      var sqlEntityRefMemberExpression = new SqlEntityRefMemberExpression (_sqlTable, _kitchenCookMember);

      StubResolveTableInfo();

      ResolvingExpressionVisitor.ResolveExpression (sqlEntityRefMemberExpression, _resolverMock, _generator, _stageMock);

      Assert.That (_sqlTable.GetJoin (_kitchenCookMember), Is.Not.Null);
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_ResolvesJoin ()
    {
      var sqlEntityRefMemberExpression = new SqlEntityRefMemberExpression (_sqlTable, _kitchenCookMember);

      _resolverMock
          .Expect (
          mock => mock.ResolveJoinInfo (
                      Arg<UnresolvedJoinInfo>.Matches (ts => ts.MemberInfo == _kitchenCookMember),
                      Arg.Is (_generator)))
          .Return (_resolvedJoinInfo)
          .WhenCalled (
          mi =>
          {
            var joinInfo = (UnresolvedJoinInfo) mi.Arguments[0];
            Assert.That (joinInfo.ItemType, Is.SameAs (typeof (Cook)));
            Assert.That (joinInfo.Cardinality, Is.EqualTo (JoinCardinality.One));
          });
      _resolverMock.Replay();

      ResolvingExpressionVisitor.ResolveExpression (sqlEntityRefMemberExpression, _resolverMock, _generator, _stageMock);

      _resolverMock.VerifyAllExpectations();
      var join = _sqlTable.GetJoin (_kitchenCookMember);
      Assert.That (join.JoinInfo, Is.SameAs (_resolvedJoinInfo));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_CreatesAndResolvesTableReference ()
    {
      var sqlEntityRefMemberExpression = new SqlEntityRefMemberExpression (_sqlTable, _kitchenCookMember);

      StubResolveTableInfo();

      var columnListExpression = new SqlEntityExpression (typeof (Cook), new SqlColumnExpression (typeof (int), "c", "ID"));
      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (Arg<SqlTableReferenceExpression>.Is.Anything, Arg.Is (_generator)))
          .Return (columnListExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlEntityRefMemberExpression, _resolverMock, _generator, _stageMock);
      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (columnListExpression));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_TableReferenceRefersToJoin ()
    {
      var sqlEntityRefMemberExpression = new SqlEntityRefMemberExpression (_sqlTable, _kitchenCookMember);
      var join = _sqlTable.GetOrAddJoin (_kitchenCookMember, JoinCardinality.One);

      StubResolveTableInfo();

      _resolverMock
          .Expect (
          mock =>
          mock.ResolveTableReferenceExpression (Arg<SqlTableReferenceExpression>.Matches (tableRef => tableRef.SqlTable == join), Arg.Is (_generator)))
          .Return (new SqlEntityExpression (typeof (Cook), new SqlColumnExpression (typeof (int), "c", "ID")));
      _resolverMock.Replay();

      ResolvingExpressionVisitor.ResolveExpression (sqlEntityRefMemberExpression, _resolverMock, _generator, _stageMock);
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void UnknownExpression ()
    {
      var unknownExpression = new CustomExpression (typeof (int));
      var result = ResolvingExpressionVisitor.ResolveExpression (unknownExpression, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (unknownExpression));
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      var expression = new SqlSubStatementExpression (sqlStatement, typeof (int));

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement));
      _stageMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (expression, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (expression));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlFunctionExpression ()
    {
      var prefixExpression = Expression.Constant ("test");
      var argumentExpression = Expression.Constant (1);
      var sqlFunctionExpression = new SqlFunctionExpression (typeof (int), "FUNCNAME", prefixExpression, argumentExpression);

      var resolvedExpression = Expression.Constant("resolved");
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (prefixExpression))
          .Return(resolvedExpression);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (argumentExpression))
          .Return (resolvedExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlFunctionExpression, _resolverMock, _generator, _stageMock);
      
      Assert.That (result, Is.TypeOf (typeof (SqlFunctionExpression)));
      Assert.That (((SqlFunctionExpression) result).Prefix, Is.SameAs (resolvedExpression));
      Assert.That (((SqlFunctionExpression) result).Args[0], Is.SameAs (resolvedExpression));
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
      _resolverMock.Replay ();

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlConvertExpression, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.TypeOf (typeof (SqlConvertExpression)));
      Assert.That (((SqlConvertExpression) result).Type, Is.EqualTo(typeof(int)) );
      Assert.That (((SqlConvertExpression) result).Source, Is.SameAs (resolvedExpression));
      _resolverMock.VerifyAllExpectations ();
    }

    private void StubResolveTableInfo ()
    {
      _resolverMock
          .Stub (stub => stub.ResolveJoinInfo (Arg<UnresolvedJoinInfo>.Is.Anything, Arg.Is (_generator)))
          .Return (_resolvedJoinInfo);
    }
  }
}