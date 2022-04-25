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
using Moq;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class DefaultMappingResolutionStageTest
  {
    private Mock<IMappingResolver> _resolverMock;
    private UniqueIdentifierGenerator _uniqueIdentifierGenerator;
    private UnresolvedTableInfo _unresolvedTableInfo;
    private SqlTable _sqlTable;
    private ResolvedSimpleTableInfo _fakeResolvedSimpleTableInfo;
    private DefaultMappingResolutionStage _stage;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _resolverMock = new Mock<IMappingResolver>();
      _uniqueIdentifierGenerator = new UniqueIdentifierGenerator();

      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable (_unresolvedTableInfo);
      _fakeResolvedSimpleTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));

      _stage = new DefaultMappingResolutionStage (_resolverMock.Object, _uniqueIdentifierGenerator);

      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void ResolveSelectExpression ()
    {
      var expression = Expression.Constant (true);
      var sqlStatementBuilder = new SqlStatementBuilder();
      var fakeResult = Expression.Constant (0);
     
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (expression))
          .Returns (fakeResult)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult))
          .Returns (fakeResult)
          .Verifiable();

      var result = _stage.ResolveSelectExpression (expression, sqlStatementBuilder, _mappingResolutionContext);

      _resolverMock.Verify();

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
          .Setup (mock => mock.ResolveConstantExpression (((ConstantExpression) sqlStatement.SelectProjection)))
          .Returns (sqlStatement.SelectProjection)
          .Verifiable();

      var result = _stage.ResolveSelectExpression (expression, sqlStatementBuilder, _mappingResolutionContext);

      _resolverMock.Verify();
      var expectedDataInfo = new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (sqlStatement.DataInfo.DataType), sqlStatement.SelectProjection);
      var expectedSqlStatement = new SqlStatementBuilder (sqlStatement) { DataInfo = expectedDataInfo }.GetSqlStatement ();

      Assert.That (sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (((ResolvedSubStatementTableInfo) sqlStatementBuilder.SqlTables[0].SqlTable.TableInfo).SqlStatement, Is.EqualTo (expectedSqlStatement));
      Assert.That (result, Is.SameAs (sqlStatement.SelectProjection));
    }

    [Test]
    public void ResolveWhereExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = new SqlConvertedBooleanExpression (Expression.Constant (0));

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (expression))
          .Returns (fakeResult)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression ((ConstantExpression) fakeResult.Expression))
          .Returns (fakeResult.Expression)
          .Verifiable();

      var result = _stage.ResolveWhereExpression (expression, _mappingResolutionContext);

      _resolverMock.Verify();

      Assert.That (((BinaryExpression) result).Left, Is.SameAs (fakeResult.Expression));
      Assert.That (((SqlLiteralExpression) ((BinaryExpression) result).Right).Value, Is.EqualTo (1));
    }

    [Test]
    public void ResolveGroupByExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (expression))
          .Returns (fakeResult)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult))
          .Returns (fakeResult)
          .Verifiable();

      var result = _stage.ResolveGroupByExpression (expression, _mappingResolutionContext);

      _resolverMock.Verify();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveGroupByExpression_EntityRetained ()
    {
      var expression = Expression.Constant (true);
      var fakeEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression();

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (expression))
          .Returns (fakeEntityExpression)
          .Verifiable();

      var result = _stage.ResolveGroupByExpression (expression, _mappingResolutionContext);

      _resolverMock.Verify();

      Assert.That (result, Is.SameAs (fakeEntityExpression));
    }

    [Test]
    public void ResolveOrderingExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (expression))
          .Returns (fakeResult)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult))
          .Returns (fakeResult)
          .Verifiable();

      var result = _stage.ResolveOrderingExpression (expression, _mappingResolutionContext);

      _resolverMock.Verify();

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (0));
    }

    [Test]
    public void ResolveTopExpression ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (expression))
          .Returns (fakeResult)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult))
          .Returns (fakeResult)
          .Verifiable();

      var result = _stage.ResolveTopExpression (expression, _mappingResolutionContext);

      _resolverMock.Verify();

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (0));
    }

    [Test]
    public void ResolveAggregationExpression ()
    {
      var expression = Expression.Constant (1);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (expression))
          .Returns (fakeResult)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult))
          .Returns (fakeResult)
          .Verifiable();

      var result = _stage.ResolveAggregationExpression(expression, _mappingResolutionContext);

      _resolverMock.Verify();

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (0));
    }

    [Test]
    public void ResolveTableInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var fakeResolvedSubStatementTableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);

      _resolverMock
          .Setup (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _uniqueIdentifierGenerator))
          .Returns (fakeResolvedSubStatementTableInfo)
          .Verifiable();

      var result = _stage.ResolveTableInfo (_sqlTable.TableInfo, _mappingResolutionContext);

      _resolverMock.Verify();
      Assert.That (result, Is.SameAs (fakeResolvedSubStatementTableInfo));
    }

    [Test]
    public void ResolveJoinCondition_ResolvesExpression_AndAppliesPredicateContext ()
    {
      var expression = Expression.Constant (true);
      var fakeResult = Expression.Constant (false);

      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (expression))
          .Returns (fakeResult)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveConstantExpression (fakeResult))
          .Returns (fakeResult)
          .Verifiable();

      var result = _stage.ResolveJoinCondition (expression, _mappingResolutionContext);

      _resolverMock.Verify();

      var expected = ExpressionObjectMother.CreateExpectedPredicateExpressionForBooleanFalse();
      SqlExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void ResolveSqlStatement ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo (typeof (Cook));
      var tableReferenceExpression = new SqlTableReferenceExpression (new SqlTable (_fakeResolvedSimpleTableInfo, JoinSemantics.Inner));
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement(tableReferenceExpression, new[] { sqlTable});
      var fakeEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      _resolverMock
          .Setup (mock => mock.ResolveTableInfo ((UnresolvedTableInfo) sqlStatement.SqlTables[0].SqlTable.TableInfo, _uniqueIdentifierGenerator))
          .Returns (_fakeResolvedSimpleTableInfo)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveSimpleTableInfo (_fakeResolvedSimpleTableInfo))
          .Returns (fakeEntityExpression)
          .Verifiable();

      var newSqlStatment = _stage.ResolveSqlStatement (sqlStatement, _mappingResolutionContext);

      _resolverMock.Verify();
      Assert.That (newSqlStatment.SqlTables[0].SqlTable.TableInfo, Is.SameAs (_fakeResolvedSimpleTableInfo));
      Assert.That (newSqlStatment.SelectProjection, Is.SameAs (fakeEntityExpression));
    }

    [Test]
    public void ResolveTableReferenceExpression ()
    {
      var resolvedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
      var expression = new SqlTableReferenceExpression (SqlStatementModelObjectMother.CreateSqlTable (resolvedTableInfo));
      var fakeResult = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      _resolverMock
          .Setup (mock => mock.ResolveSimpleTableInfo (resolvedTableInfo))
          .Returns (fakeResult)
          .Verifiable();

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
          .Setup (mock => mock.ResolveConstantExpression (constantExpression))
          .Returns (sqlColumnExpression)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveMemberExpression (sqlColumnExpression, expression.Member))
          .Returns (fakeResult)
          .Verifiable();

      var result = _stage.ResolveCollectionSourceExpression (expression, _mappingResolutionContext);

      _resolverMock.Verify();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveEntityRefMemberExpression ()
    {
      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Kitchen));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, kitchenCookMember);
      var fakeJoinedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
      _mappingResolutionContext.AddSqlEntityMapping (entityExpression, SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook)));

      var fakeEntityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var fakeJoinConditionExpression = new CustomExpression (typeof (bool)); // Use CustomExpression so the expression isn't passed to the resolver again

      _resolverMock
          .Setup (mock => mock.ResolveJoinTableInfo (It.Is<UnresolvedJoinTableInfo> (joinInfo => joinInfo.MemberInfo == kitchenCookMember), It.IsAny<UniqueIdentifierGenerator>()))
          .Returns (fakeJoinedTableInfo)
          .Verifiable();
      _resolverMock
          .Setup (mock => mock.ResolveSimpleTableInfo (It.IsAny<ResolvedSimpleTableInfo>()))
          .Returns (fakeEntityExpression)
          .Verifiable();

      _resolverMock
          .Setup (mock => mock.ResolveJoinCondition (entityExpression, kitchenCookMember, fakeJoinedTableInfo))
          .Returns (fakeJoinConditionExpression)
          .Verifiable();

      var result = _stage.ResolveEntityRefMemberExpression (entityRefMemberExpression, _mappingResolutionContext);

      _resolverMock.Verify();

      Assert.That (result, Is.SameAs (fakeEntityExpression));
      var sqlTable = _mappingResolutionContext.GetSqlTableForEntityExpression (entityRefMemberExpression.OriginatingEntity);
      Assert.That (sqlTable.GetJoinByMember (kitchenCookMember), Is.Not.Null);
      Assert.That (sqlTable.GetJoinByMember (kitchenCookMember).JoinedTable.TableInfo, Is.SameAs (fakeJoinedTableInfo));
      Assert.That (sqlTable.GetJoinByMember (kitchenCookMember).JoinCondition, Is.SameAs (fakeJoinConditionExpression));
    }

    [Test]
    public void ResolveMemberAccess ()
    {
      var sourceExpression = new SqlColumnDefinitionExpression (typeof (Cook), "c", "Substitution", false);
      var memberInfo = typeof (Cook).GetProperty ("Name");
      var fakeResolvedExpression = new SqlLiteralExpression ("Hugo");

      _resolverMock
          .Setup (mock => mock.ResolveMemberExpression (sourceExpression, memberInfo))
          .Returns (fakeResolvedExpression)
          .Verifiable();

      var result = _stage.ResolveMemberAccess (sourceExpression, memberInfo, _resolverMock.Object, _mappingResolutionContext);

      _resolverMock.Verify();
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
  }
}