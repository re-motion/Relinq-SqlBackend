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
using System.Linq;
using System.Linq.Expressions;
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
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class ResolvingTableInfoVisitorTest
  {
    private Mock<IMappingResolver> _resolverMock;
    private UnresolvedTableInfo _unresolvedTableInfo;
    private UniqueIdentifierGenerator _generator;
    private Mock<IMappingResolutionStage> _stageMock;
    private ResolvedSimpleTableInfo _resolvedTableInfo;
    private SqlStatement _sqlStatement;
    private MappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<IMappingResolutionStage>();
      _resolverMock = new Mock<IMappingResolver>();
      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _resolvedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
      _generator = new UniqueIdentifierGenerator();
      _sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void ResolveTableInfo_Unresolved ()
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");
      _resolverMock
         .Setup (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _generator)).Returns (resolvedTableInfo).Verifiable ();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (resolvedTableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (resolvedTableInfo));
    }

    [Test]
    public void ResolveTableInfo_Unresolved_RevisitsResult_OnlyIfDifferent ()
    {
      _resolverMock
         .Setup (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _generator))
         .Returns (_resolvedTableInfo)
         .Verifiable ();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (_unresolvedTableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (_resolvedTableInfo));
      _resolverMock.Verify();
    }

    [Test]
    public void ResolveTableInfo_SubStatementTableInfo_SubStatementUnmodified ()
    {
      var sqlSubStatementTableInfo = new ResolvedSubStatementTableInfo ("c", _sqlStatement);

      _stageMock
         .Setup (mock => mock.ResolveSqlStatement (_sqlStatement, _mappingResolutionContext))
         .Returns (_sqlStatement)
         .Verifiable ();

      var result = (ResolvedSubStatementTableInfo) ResolvingTableInfoVisitor.ResolveTableInfo (sqlSubStatementTableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify ();
      Assert.That (result, Is.SameAs (sqlSubStatementTableInfo));
    }

    [Test]
    public void ResolveTableInfo_SubStatementTableInfo_SubStatementModified ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook ();

      var sqlSubStatementTableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);

      _stageMock
         .Setup (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
         .Returns (_sqlStatement)
         .Verifiable ();

      var result = (ResolvedSubStatementTableInfo) ResolvingTableInfoVisitor.ResolveTableInfo (sqlSubStatementTableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify ();
      Assert.That (result, Is.Not.SameAs (sqlSubStatementTableInfo));
      Assert.That (result.SqlStatement, Is.SameAs (_sqlStatement));
      Assert.That (result.TableAlias, Is.EqualTo (sqlSubStatementTableInfo.TableAlias));
    }

    [Test]
    public void ResolveTableInfo_JoinedGroupingTableInfo_SubStatementUnmodified ()
    {
      var sqlJoinedGroupingTableInfo = SqlStatementModelObjectMother.CreateResolvedJoinedGroupingTableInfo (_sqlStatement);

      _stageMock
         .Setup (mock => mock.ResolveSqlStatement (_sqlStatement, _mappingResolutionContext))
         .Returns (_sqlStatement)
         .Verifiable ();

      var result = (ResolvedJoinedGroupingTableInfo) ResolvingTableInfoVisitor.ResolveTableInfo (sqlJoinedGroupingTableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify ();
      Assert.That (result, Is.SameAs (sqlJoinedGroupingTableInfo));
    }

    [Test]
    public void ResolveTableInfo_JoinedGroupingTableInfo_SubStatementModified ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook ();

      var sqlJoinedGroupingTableInfo = SqlStatementModelObjectMother.CreateResolvedJoinedGroupingTableInfo (sqlStatement);

      _stageMock
         .Setup (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
         .Returns (_sqlStatement)
         .Verifiable ();

      var result = (ResolvedJoinedGroupingTableInfo) ResolvingTableInfoVisitor.ResolveTableInfo (sqlJoinedGroupingTableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify ();
      Assert.That (result, Is.Not.SameAs (sqlJoinedGroupingTableInfo));
      Assert.That (result.SqlStatement, Is.SameAs (_sqlStatement));
      Assert.That (result.TableAlias, Is.EqualTo (sqlJoinedGroupingTableInfo.TableAlias));
      Assert.That (result.AssociatedGroupingSelectExpression, Is.SameAs (sqlJoinedGroupingTableInfo.AssociatedGroupingSelectExpression));
      Assert.That (result.GroupSourceTableAlias, Is.EqualTo (sqlJoinedGroupingTableInfo.GroupSourceTableAlias));
    }

    [Test]
    public void ResolveTableInfo_SimpleTableInfo ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (simpleTableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify();
      Assert.That (result, Is.SameAs (simpleTableInfo));
    }

    [Test]
    public void ResolveTableInfo_SqlJoinedTable ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();
      var sqlJoinedTable = new SqlJoinedTable (joinInfo, JoinSemantics.Left);

      _stageMock
         .Setup (mock => mock.ResolveJoinInfo(joinInfo, _mappingResolutionContext))
         .Returns (joinInfo)
         .Verifiable();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (sqlJoinedTable, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify();
      Assert.That (result, Is.SameAs (joinInfo.ForeignTableInfo));
    }

    [Test]
    public void ResolveTableInfo_GroupReferenceTableInfo ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      var groupingSelect = new SqlGroupingSelectExpression (
          Expression.MakeMemberAccess (new SqlTableReferenceExpression (sqlTable), typeof (Cook).GetProperty ("Name")),
          Expression.MakeMemberAccess (new SqlTableReferenceExpression (sqlTable), typeof (Cook).GetProperty ("ID")));
      var dataInfo = new StreamedSequenceInfo (
          typeof (IEnumerable<>).MakeGenericType (groupingSelect.Type), 
          Expression.Constant (null, groupingSelect.Type));
      var whereCondition = Expression.Constant (false);
      var groupByExpression = groupingSelect.KeyExpression;
      var groupingSubStatement = new SqlStatementBuilder
                                 {
                                   DataInfo = dataInfo,
                                   SelectProjection = groupingSelect,
                                   SqlTables = { sqlTable },
                                   WhereCondition = whereCondition,
                                   GroupByExpression = groupByExpression
                                 }.GetSqlStatement();
      var groupSource = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSubStatementTableInfo ("q0", groupingSubStatement));
      var tableInfo = new UnresolvedGroupReferenceTableInfo (groupSource);

      var expectedKeyViaElement = Expression.MakeMemberAccess (new SqlTableReferenceExpression (sqlTable), typeof (Cook).GetProperty ("Name"));
      var expectedKeyViaGroupSource = Expression.MakeMemberAccess (new SqlTableReferenceExpression (groupSource), groupSource.ItemType.GetProperty ("Key"));
      
      var expectedResultWhereCondition =
          Expression.OrElse (
              Expression.AndAlso (new SqlIsNullExpression (expectedKeyViaElement), new SqlIsNullExpression (expectedKeyViaGroupSource)),
              Expression.AndAlso (
                  Expression.AndAlso (new SqlIsNotNullExpression (expectedKeyViaElement), new SqlIsNotNullExpression (expectedKeyViaGroupSource)),
                  Expression.Equal (expectedKeyViaElement, expectedKeyViaGroupSource)));
      
      var fakeWhereCondition = Expression.Constant (false);
      _stageMock
         .Expect (mock => mock.ResolveWhereExpression (It.IsAny<TEMPLATE>(), It.Is<TEMPLATE> (param => param == _mappingResolutionContext)))
         .Callback (mi => SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResultWhereCondition, (Expression) mi.Arguments[0]))
         .Return (fakeWhereCondition);

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (tableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify();

      Assert.That (result, Is.TypeOf (typeof (ResolvedJoinedGroupingTableInfo)));

      var castResult = ((ResolvedJoinedGroupingTableInfo) result);
      
      var resultGroupingSelector = castResult.AssociatedGroupingSelectExpression;
      Assert.That (resultGroupingSelector, Is.SameAs (groupingSelect));

      Assert.That (castResult.GroupSourceTableAlias, Is.EqualTo ("q0"));
      
      var resultSqlStatement = castResult.SqlStatement;

      Assert.That (resultSqlStatement.SqlTables, Is.EqualTo (groupingSubStatement.SqlTables));
      Assert.That (resultSqlStatement.Orderings, Is.Empty);
      Assert.That (resultSqlStatement.GroupByExpression, Is.Null);
      
      SqlExpressionTreeComparer.CheckAreEqualTrees (
          Expression.AndAlso (groupingSubStatement.WhereCondition, fakeWhereCondition), 
          resultSqlStatement.WhereCondition);

      var expectedResultSelectProjection =
          Expression.MakeMemberAccess (new SqlTableReferenceExpression (resultSqlStatement.SqlTables[0]), typeof (Cook).GetProperty ("ID"));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResultSelectProjection, resultSqlStatement.SelectProjection);

      Assert.That (resultSqlStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (resultSqlStatement.DataInfo.DataType, Is.SameAs (typeof (IQueryable<int>)));

      var expectedItemExpression = resultSqlStatement.SelectProjection;
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedItemExpression, ((StreamedSequenceInfo) resultSqlStatement.DataInfo).ItemExpression);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "This SQL generator only supports sequences in from expressions if they are members of an entity or if they come from a GroupBy operator. "
        + "Sequence: 'GROUP-REF-TABLE(TABLE-REF(t))'")]
    public void ResolveTableInfo_GroupReferenceTableInfo_NoSubStatement ()
    {
      var groupSource = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (IEnumerable<Cook>));
      var tableInfo = new UnresolvedGroupReferenceTableInfo (groupSource);

      ResolvingTableInfoVisitor.ResolveTableInfo (tableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "When a sequence retrieved by a subquery is used in a from expression, the subquery must end with a GroupBy operator.")]
    public void ResolveTableInfo_GroupReferenceTableInfo_NoGorupingSubStatement ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (new int[0]));
      var groupSource = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSubStatementTableInfo ("q0", subStatement));
      var tableInfo = new UnresolvedGroupReferenceTableInfo (groupSource);

      ResolvingTableInfoVisitor.ResolveTableInfo (tableInfo, _resolverMock.Object, _generator, _stageMock.Object, _mappingResolutionContext);
    }
  }
}