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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
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
      var sqlStatementBuilder = new SqlStatementBuilder();
      var fakeResult = Expression.Constant (0);
     
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveSelectExpression (expression, sqlStatementBuilder, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (0));
    }

    [Test]
    public void ResolveSelectExpression_SqlSubStatementExpressionWithStreamedSingleValueInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Single();
      var expression = new SqlSubStatementExpression (sqlStatement);
      var sqlStatementBuilder = new SqlStatementBuilder ();

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (((ConstantExpression) sqlStatement.SelectProjection)))
          .Return (sqlStatement.SelectProjection);
      _resolverMock.Replay ();

      var result = _stage.ResolveSelectExpression (expression, sqlStatementBuilder, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations ();
      var expectedDataInfo = new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (sqlStatement.DataInfo.DataType), sqlStatement.SelectProjection);
      var expectedSqlStatement = new SqlStatementBuilder (sqlStatement) { DataInfo = expectedDataInfo }.GetSqlStatement ();

      Assert.That (sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (((ResolvedSubStatementTableInfo) ((SqlTable) sqlStatementBuilder.SqlTables[0]).TableInfo).SqlStatement, Is.EqualTo (expectedSqlStatement));
      Assert.That (result, Is.SameAs (sqlStatement.SelectProjection));
    }

    [Test]
    public void ResolveWhereExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = new SqlConvertedBooleanExpression (Expression.Constant (0));

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeResult.Expression))
          .Return (fakeResult.Expression);
      _resolverMock.Replay();

      var result = _stage.ResolveWhereExpression (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (((BinaryExpression) result).Left, Is.SameAs (fakeResult.Expression));
      Assert.That (((SqlLiteralExpression) ((BinaryExpression) result).Right).Value, Is.EqualTo (1));
    }

    [Test]
    public void ResolveGroupByExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveGroupByExpression (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveGroupByExpression_EntityRetained ()
    {
      var expression = Expression.Constant (true);
      var fakeEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression();

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeEntityExpression);
      _resolverMock.Replay();

      var result = _stage.ResolveGroupByExpression (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeEntityExpression));
    }

    [Test]
    public void ResolveOrderingExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
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
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _stage.ResolveTopExpression (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (0));
    }

    [Test]
    public void ResolveAggregationExpression ()
    {
      var expression = Expression.Constant (1);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay ();

      var result = _stage.ResolveAggregationExpression(expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (0));
    }

    [Test]
    public void ResolveTableInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var fakeResolvedSubStatementTableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);

      _resolverMock
          .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _uniqueIdentifierGenerator))
          .Return (fakeResolvedSubStatementTableInfo);
      _resolverMock.Replay();

      var result = _stage.ResolveTableInfo (_sqlTable.TableInfo, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResolvedSubStatementTableInfo));
    }

    [Test]
    public void ResolveJoinInfo ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();

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
    public void ResolveJoinCondition_ResolvesExpression_AndAppliesPredicateContext ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (false);

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResult);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);

      var result = _stage.ResolveJoinCondition (expression, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations ();

      var expected = Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolveSqlStatement ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo (typeof (Cook));
      var tableReferenceExpression = new SqlTableReferenceExpression (new SqlTable (_fakeResolvedSimpleTableInfo, JoinSemantics.Inner));
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement(tableReferenceExpression, new[] { sqlTable});
      var fakeEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      _resolverMock
          .Expect (mock => mock.ResolveTableInfo ((UnresolvedTableInfo) ((SqlTable) sqlStatement.SqlTables[0]).TableInfo, _uniqueIdentifierGenerator))
          .Return (_fakeResolvedSimpleTableInfo);
      _resolverMock
          .Expect (mock => mock.ResolveSimpleTableInfo (tableReferenceExpression.SqlTable.GetResolvedTableInfo(), _uniqueIdentifierGenerator))
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
      var expression = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (Cook)));
      var fakeResult = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      _resolverMock
          .Expect (mock => mock.ResolveSimpleTableInfo (expression.SqlTable.GetResolvedTableInfo(), _uniqueIdentifierGenerator))
          .Return (fakeResult);

      var result = _stage.ResolveTableReferenceExpression (expression, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveCollectionSourceExpression ()
    {
      var constantExpression = Expression.Constant (new Cook());
      var expression = Expression.MakeMemberAccess (constantExpression, typeof (Cook).GetProperty ("FirstName"));
      var sqlColumnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      var fakeResult = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

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
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, kitchenCookMember);
      var unresolvedJoinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();
      var fakeJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));
      _mappingResolutionContext.AddSqlEntityMapping (entityExpression, SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook)));

      var fakeEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      _resolverMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (joinInfo => joinInfo.MemberInfo == kitchenCookMember), Arg<UniqueIdentifierGenerator>.Is.Anything))
          .Return (fakeJoinInfo);

      _resolverMock
          .Expect (
              mock =>
              mock.ResolveSimpleTableInfo (
                  Arg<IResolvedTableInfo>.Is.Anything,
                  Arg<UniqueIdentifierGenerator>.Is.Anything))
          .Return (fakeEntityExpression);

      var result = _stage.ResolveEntityRefMemberExpression (entityRefMemberExpression, unresolvedJoinInfo, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeEntityExpression));
      var sqlTable = _mappingResolutionContext.GetSqlTableForEntityExpression (entityRefMemberExpression.OriginatingEntity);
      Assert.That (sqlTable.GetJoin (kitchenCookMember), Is.Not.Null);
      Assert.That (sqlTable.GetJoin (kitchenCookMember).JoinInfo, Is.SameAs (fakeJoinInfo));
    }

    [Test]
    public void ResolveMemberAccess ()
    {
      var sourceExpression = new SqlColumnDefinitionExpression (typeof (Cook), "c", "Substitution", false);
      var memberInfo = typeof (Cook).GetProperty ("Name");
      var fakeResolvedExpression = new SqlLiteralExpression ("Hugo");

      _resolverMock
          .Expect (mock => mock.ResolveMemberExpression (sourceExpression, memberInfo))
          .Return (fakeResolvedExpression);
      _resolverMock.Replay();

      var result = _stage.ResolveMemberAccess (sourceExpression, memberInfo, _resolverMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResolvedExpression));
    }
    
    [Test]
    public void ApplyContext_Expression ()
    {
      var result = _stage.ApplyContext (Expression.Constant (false), SqlExpressionContext.PredicateRequired, _mappingResolutionContext);

      Assert.That (result, Is.AssignableTo (typeof (BinaryExpression)));
      Assert.That (((ConstantExpression) ((BinaryExpression) result).Left).Value, Is.EqualTo (0));
      Assert.That (((SqlLiteralExpression) ((BinaryExpression) result).Right).Value, Is.EqualTo (1));
    }

    [Test]
    public void ApplySelectionContext_SqlStatement ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
                         {
                             SelectProjection = Expression.Constant (true)
                         }.GetSqlStatement();

      var result = _stage.ApplySelectionContext (sqlStatement, SqlExpressionContext.SingleValueRequired, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (result.SelectProjection, Is.TypeOf (typeof (SqlConvertedBooleanExpression)));
    }

    [Test]
    public void ApplyContext_TableInfo ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             SelectProjection = Expression.Constant (true),
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var tableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);

      var result = _stage.ApplyContext (tableInfo, SqlExpressionContext.ValueRequired, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs (tableInfo));
      Assert.That (((ResolvedSubStatementTableInfo) result).SqlStatement.SelectProjection, Is.TypeOf (typeof (SqlConvertedBooleanExpression)));
      Assert.That (
          ((ConstantExpression) ((SqlConvertedBooleanExpression) ((ResolvedSubStatementTableInfo) result).SqlStatement.SelectProjection).Expression).
              Value,
          Is.EqualTo (1));
    }

    [Test]
    public void ApplyContext_JoinInfo ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             SelectProjection = Expression.Constant (true),
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var tableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);
      var joinInfo = new ResolvedJoinInfo (tableInfo, Expression.Equal (new SqlLiteralExpression (1), new SqlLiteralExpression (1)));

      var result = _stage.ApplyContext (joinInfo, SqlExpressionContext.ValueRequired, _mappingResolutionContext);

      Assert.That (result, Is.Not.SameAs (joinInfo));
      Assert.That (
          ((ResolvedSubStatementTableInfo) ((ResolvedJoinInfo) result).ForeignTableInfo).SqlStatement.SelectProjection,
          Is.TypeOf (typeof (SqlConvertedBooleanExpression)));
      Assert.That (
          ((ConstantExpression)
           ((SqlConvertedBooleanExpression) ((ResolvedSubStatementTableInfo) ((ResolvedJoinInfo) result).ForeignTableInfo).SqlStatement.SelectProjection)
               .Expression).Value,
          Is.EqualTo (1));
    }
  }
}