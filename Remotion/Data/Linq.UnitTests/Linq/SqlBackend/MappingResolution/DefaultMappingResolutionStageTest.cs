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
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _resolverMock = MockRepository.GenerateMock<IMappingResolver>();
      _uniqueIdentifierGenerator = new UniqueIdentifierGenerator();

      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable (_unresolvedTableInfo);
      _fakeResolvedSimpleTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));

      _stage = new DefaultMappingResolutionStage (_resolverMock, _uniqueIdentifierGenerator);

      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void ResolveSelectExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveSelectExpression (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf(typeof(ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo(0));
    }

    [Test]
    public void ResolveWhereExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock.Replay ();

      var result = _stage.ResolveWhereExpression (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (((BinaryExpression) result).Left, Is.SameAs (fakeResult));
      Assert.That (((SqlLiteralExpression) ((BinaryExpression) result).Right).Value, Is.EqualTo(1));
    }

    [Test]
    public void ResolveOrderingExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveOrderingExpression (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (0));
    }

    [Test]
    public void ResolveTopExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveTopExpression (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (0));
    }

    [Test]
    public void ResolveTableInfo ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
      {
        DataInfo = new StreamedSequenceInfo(typeof(Cook[]), Expression.Constant(new Cook()))

      }.GetSqlStatement();
      var fakeResolvedSubStatementTableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);

      _resolverMock
          .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _uniqueIdentifierGenerator))
          .Return (fakeResolvedSubStatementTableInfo);
      _resolverMock.Replay();

      var result = _stage.ResolveTableInfo (_sqlTable.TableInfo, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.SameAs (fakeResolvedSubStatementTableInfo));
    }

    [Test]
    public void ResolveJoinInfo ()
    {
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Kitchen), "c", new SqlColumnExpression (typeof (string), "c", "Name", false));
      var unresolvedJoinInfo = new UnresolvedJoinInfo (entityExpression, memberInfo, JoinCardinality.One);
      var join = _sqlTable.GetOrAddLeftJoin (unresolvedJoinInfo, memberInfo);
      var joinInfo = (UnresolvedJoinInfo) join.JoinInfo;

      var fakeResolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));

      _resolverMock
          .Expect (mock => mock.ResolveJoinInfo (joinInfo, _uniqueIdentifierGenerator))
          .Return (fakeResolvedJoinInfo);
      _resolverMock.Replay();

      var result = _stage.ResolveJoinInfo (joinInfo, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResolvedJoinInfo));
    }

    [Test]
    public void ResolveSqlStatement ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo (typeof (Cook));
      var tableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)), tableReferenceExpression, new[] { sqlTable }, new Ordering[] { }, null, null, false);
      var fakeEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", new SqlColumnExpression (typeof (int), "c", "ID", false));

      _resolverMock
          .Expect (mock => mock.ResolveTableInfo ((UnresolvedTableInfo) ((SqlTable) sqlStatement.SqlTables[0]).TableInfo, _uniqueIdentifierGenerator))
          .Return (_fakeResolvedSimpleTableInfo);
      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _uniqueIdentifierGenerator))
          .Return (fakeEntityExpression);
      _resolverMock.Replay();

      var newSqlStatment = _stage.ResolveSqlStatement (sqlStatement, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (((SqlTable) newSqlStatment.SqlTables[0]).TableInfo, Is.SameAs (_fakeResolvedSimpleTableInfo));
      Assert.That (newSqlStatment.SelectProjection, Is.SameAs (fakeEntityExpression));
    }

    [Test]
    public void ResolveTableReferenceExpression ()
    {
      var expression = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo(typeof (Cook)));
      var fakeResult = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", new SqlColumnExpression (typeof (string), "c", "Name", false));

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (expression, _uniqueIdentifierGenerator))
          .Return (fakeResult);

      var result = _stage.ResolveTableReferenceExpression (expression, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveCollectionSourceExpression ()
    {
      var constantExpression = Expression.Constant (new Cook());
      var expression = Expression.MakeMemberAccess (constantExpression, typeof (Cook).GetProperty ("FirstName"));
      var sqlColumnExpression = new SqlColumnExpression (typeof (string), "c", "Name", false);
      var fakeResult = new SqlEntityDefinitionExpression (typeof (Cook), "c", sqlColumnExpression);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (constantExpression))
          .Return (sqlColumnExpression);
      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (sqlColumnExpression, expression.Member))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveCollectionSourceExpression (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveEntityRefMemberExpression ()
    {
      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", new SqlColumnExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, kitchenCookMember);
      var unresolvedJoinInfo = new UnresolvedJoinInfo (entityExpression, kitchenCookMember, JoinCardinality.One);
      var fakeJoinInfo = new ResolvedJoinInfo (
          new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"),
          new SqlColumnExpression (typeof (int), "k", "ID", true),
          new SqlColumnExpression (typeof (int), "c", "KitchenID", false));
      _mappingResolutionContext.AddSqlEntityMapping (entityExpression, new SqlTable(new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c")));

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

      var result = _stage.ResolveEntityRefMemberExpression (entityRefMemberExpression, unresolvedJoinInfo, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeEntityExpression));
      var sqlTable = _mappingResolutionContext.GetSqlTableForEntityExpression (entityRefMemberExpression.EntityExpression);
      Assert.That (sqlTable.GetJoin (kitchenCookMember), Is.Not.Null);
      Assert.That (sqlTable.GetJoin (kitchenCookMember).JoinInfo, Is.SameAs (fakeJoinInfo));
    }

    [Test]
    public void ApplyContext_Expression ()
    {
      var result = _stage.ApplyContext (Expression.Constant (false), SqlExpressionContext.PredicateRequired, _mappingResolutionContext);

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

      var result = _stage.ApplySelectionContext (sqlStatement, SqlExpressionContext.SingleValueRequired, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (result.SelectProjection, Is.TypeOf (typeof (SqlColumnExpression)));
    }

    [Test]
    public void ApplyContext_TableInfo ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                           SelectProjection = Expression.Constant(true),
                           DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var tableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);
      
      var result = _stage.ApplyContext (tableInfo, SqlExpressionContext.ValueRequired, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs(tableInfo));
      Assert.That (((ResolvedSubStatementTableInfo) result).SqlStatement.SelectProjection, Is.TypeOf(typeof(ConstantExpression)));
      Assert.That (((ConstantExpression) ((ResolvedSubStatementTableInfo) result).SqlStatement.SelectProjection).Value, Is.EqualTo(1));
    }

    [Test]
    public void ApplyContext_JoinInfo ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
      {
        SelectProjection = Expression.Constant (true),
        DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();
      var tableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);
      var joinInfo = new ResolvedJoinInfo (tableInfo, new SqlLiteralExpression (1), new SqlLiteralExpression (1));

      var result = _stage.ApplyContext (joinInfo, SqlExpressionContext.ValueRequired, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs(joinInfo));
      Assert.That (((ResolvedSubStatementTableInfo) ((ResolvedJoinInfo) result).ForeignTableInfo).SqlStatement.SelectProjection, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) ((ResolvedSubStatementTableInfo) ((ResolvedJoinInfo) result).ForeignTableInfo).SqlStatement.SelectProjection).Value, Is.EqualTo (1));
    }
    
  }
}