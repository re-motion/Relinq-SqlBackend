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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class DefaultMappingResolutionStageTest
  {
    private IMappingResolver _resolverMock;
    private UniqueIdentifierGenerator _uniqueIdentifierGenerator;
    private UnresolvedTableInfo _unresolvedTableInfo;
    private SqlTable _sqlTable;
    private ResolvedSimpleTableInfo _fakeResolvedSimpleTableInfo;
    private DefaultMappingResolutionStage _stage;

    [SetUp]
    public void SetUp ()
    {
      _resolverMock = MockRepository.GenerateMock<IMappingResolver>();
      _uniqueIdentifierGenerator = new UniqueIdentifierGenerator();

      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable (_unresolvedTableInfo);
      _fakeResolvedSimpleTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));

      _stage = new DefaultMappingResolutionStage (_resolverMock, _uniqueIdentifierGenerator);
    }

    [Test]
    public void ResolveSelectExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (expression, _uniqueIdentifierGenerator))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveSelectExpression (expression);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveWhereExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (expression, _uniqueIdentifierGenerator))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveWhereExpression (expression);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveOrderingExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (expression, _uniqueIdentifierGenerator))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveOrderingExpression (expression);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveTopExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (expression, _uniqueIdentifierGenerator))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveTopExpression (expression);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveTableInfo ()
    {
      _resolverMock
          .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _uniqueIdentifierGenerator))
          .Return (_fakeResolvedSimpleTableInfo);
      _resolverMock.Replay();

      var result = _stage.ResolveTableInfo (_sqlTable.TableInfo);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (_fakeResolvedSimpleTableInfo));
    }

    [Test]
    public void ResolveJoinInfo ()
    {
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var join = _sqlTable.GetOrAddJoin (memberInfo, JoinCardinality.One);
      var joinInfo = (UnresolvedJoinInfo) join.JoinInfo;

      var fakeResolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));

      _resolverMock
          .Expect (mock => mock.ResolveJoinInfo (joinInfo, _uniqueIdentifierGenerator))
          .Return (fakeResolvedJoinInfo);
      _resolverMock.Replay();

      var result = _stage.ResolveJoinInfo (joinInfo);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResolvedJoinInfo));
    }

    [Test]
    public void ResolveSqlStatement ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo (typeof (Cook));
      var tableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)), tableReferenceExpression, new[] { sqlTable }, new Ordering[] { }, null, null, false, false);
      var fakeEntityExpression = new SqlEntityExpression (sqlTable, new SqlColumnExpression (typeof (int), "c", "ID", false));

      _resolverMock
          .Expect (mock => mock.ResolveTableInfo ((UnresolvedTableInfo) ((SqlTable) sqlStatement.SqlTables[0]).TableInfo, _uniqueIdentifierGenerator))
          .Return (_fakeResolvedSimpleTableInfo);
      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _uniqueIdentifierGenerator))
          .Return (fakeEntityExpression);
      _resolverMock.Replay();

      var newSqlStatment = _stage.ResolveSqlStatement (sqlStatement);

      _resolverMock.VerifyAllExpectations();
      Assert.That (((SqlTable) newSqlStatment.SqlTables[0]).TableInfo, Is.SameAs (_fakeResolvedSimpleTableInfo));
      Assert.That (newSqlStatment.SelectProjection, Is.SameAs (fakeEntityExpression));
    }

    [Test]
    public void ResolveSqlSubStatement ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo (typeof (Cook));
      var tableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)), tableReferenceExpression, new[] { sqlTable }, new Ordering[] { }, null, null, false, false);
      var fakeEntityExpression = new SqlEntityExpression (sqlTable, new SqlColumnExpression (typeof (int), "c", "ID", false));

      _resolverMock
          .Expect (mock => mock.ResolveTableInfo ((UnresolvedTableInfo) ((SqlTable) sqlStatement.SqlTables[0]).TableInfo, _uniqueIdentifierGenerator))
          .Return (_fakeResolvedSimpleTableInfo);
      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _uniqueIdentifierGenerator))
          .Return (fakeEntityExpression);
      _resolverMock.Replay();

      var newSqlStatment = _stage.ResolveSqlSubStatement (sqlStatement);

      _resolverMock.VerifyAllExpectations();
      Assert.That (((SqlTable) newSqlStatment.SqlTables[0]).TableInfo, Is.SameAs (_fakeResolvedSimpleTableInfo));
      Assert.That (newSqlStatment.SelectProjection, Is.SameAs (fakeEntityExpression));
    }

    [Test]
    public void ResolveCollectionSourceExpression ()
    {
      var constantExpression = Expression.Constant (new Cook());
      var expression = Expression.MakeMemberAccess (constantExpression, typeof (Cook).GetProperty ("FirstName"));
      var fakeResult = new SqlColumnExpression (typeof (string), "c", "Name", false);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (constantExpression))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (fakeResult, expression.Member))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveCollectionSourceExpression (expression);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveEntityRefMemberExpression ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo();
      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (sqlTable, kitchenCookMember);
      var fakeJoinInfo = new ResolvedJoinInfo (
          new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"),
          new SqlColumnExpression (typeof (int), "k", "ID", true),
          new SqlColumnExpression (typeof (int), "c", "KitchenID", false));
      var fakeEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityExpression (typeof (Cook));

      _resolverMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (joinInfo => joinInfo.MemberInfo == kitchenCookMember), Arg<UniqueIdentifierGenerator>.Is.Anything))
          .Return (fakeJoinInfo);

      _resolverMock
          .Expect (
              mock =>
              mock.ResolveTableReferenceExpression (
                  Arg<SqlTableReferenceExpression>.Matches (expr => ((SqlJoinedTable) expr.SqlTable).JoinInfo == fakeJoinInfo),
                  Arg<UniqueIdentifierGenerator>.Is.Anything))
          .Return (fakeEntityExpression);

      var result = _stage.ResolveEntityRefMemberExpression (entityRefMemberExpression);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeEntityExpression));
    }

    [Test]
    public void ApplyContext_Expression ()
    {
      var result = _stage.ApplyContext (Expression.Constant (false), SqlExpressionContext.PredicateRequired);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((ConstantExpression) ((BinaryExpression) result).Left).Value, Is.EqualTo (0));
      Assert.That (((SqlLiteralExpression) ((BinaryExpression) result).Right).Value, Is.EqualTo (1));
    }

    [Test]
    public void ApplyContext_SqlStatement ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
                         {
                             SelectProjection = SqlStatementModelObjectMother.CreateSqlEntityExpression (typeof (Cook))
                         }.GetSqlStatement();

      var result = _stage.ApplyContext (sqlStatement, SqlExpressionContext.SingleValueRequired);

      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (result.SelectProjection, Is.TypeOf (typeof (SqlColumnExpression)));
    }

    [Test]
    public void ApplyContext_Table ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
                         {
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();

      var subStatementTableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);
      var sqlTable = new SqlTable (subStatementTableInfo);
      _stage.ApplyContext (sqlTable, SqlExpressionContext.ValueRequired);

      Assert.That (sqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement, Is.Not.SameAs (sqlStatement));
    }
  }
}