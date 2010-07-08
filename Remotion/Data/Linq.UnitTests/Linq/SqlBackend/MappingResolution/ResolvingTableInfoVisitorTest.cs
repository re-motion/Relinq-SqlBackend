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
using System.Collections.Generic;
using System.Linq;
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
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingTableInfoVisitorTest
  {
    private IMappingResolver _resolverMock;
    private UnresolvedTableInfo _unresolvedTableInfo;
    private UniqueIdentifierGenerator _generator;
    private IMappingResolutionStage _stageMock;
    private ResolvedSimpleTableInfo _resolvedTableInfo;
    private SqlStatement _sqlStatement;
    private MappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<IMappingResolutionStage>();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver>();
      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _resolvedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
      _generator = new UniqueIdentifierGenerator();
      _sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[]));
      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void ResolveTableInfo_Unresolved ()
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");
      _resolverMock.Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _generator)).Return (resolvedTableInfo);
      _resolverMock.Replay();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (resolvedTableInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (resolvedTableInfo));
    }

   [Test]
    public void ResolveTableInfo_Unresolved_RevisitsResult_OnlyIfDifferent ()
    {
      _resolverMock
          .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _generator))
          .Return (_resolvedTableInfo);
      _resolverMock.Replay();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (_unresolvedTableInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (_resolvedTableInfo));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveTableInfo_SubStatementTableInfo ()
    {
      _sqlStatement = new SqlStatementBuilder (_sqlStatement) { DataInfo = new StreamedSequenceInfo(typeof(IQueryable<Cook>), Expression.Constant(new Cook())) }.GetSqlStatement();
      
      var sqlSubStatementTableInfo = new ResolvedSubStatementTableInfo ("c", _sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (_sqlStatement, _mappingResolutionContext))
          .Return(_sqlStatement);
      _resolverMock.Replay();

      ResolvedSubStatementTableInfo result = (ResolvedSubStatementTableInfo) ResolvingTableInfoVisitor.ResolveTableInfo (sqlSubStatementTableInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      Assert.That (result.SqlStatement, Is.EqualTo (sqlSubStatementTableInfo.SqlStatement));
    }

    [Test]
    public void ResolveTableInfo_SimpleTableInfo ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (simpleTableInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (simpleTableInfo));
    }

    [Test]
    public void ResolveTableInfo_SqlJoinedTable ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var leftJoinInfo = new ResolvedJoinInfo (simpleTableInfo, new SqlLiteralExpression (1), new SqlLiteralExpression (1));
      var sqlJoinedTable = new SqlJoinedTable (leftJoinInfo, JoinSemantics.Left);

      _stageMock
          .Expect (mock => mock.ResolveJoinInfo(leftJoinInfo, _mappingResolutionContext))
          .Return(leftJoinInfo);
      _resolverMock.Replay();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (sqlJoinedTable, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (simpleTableInfo));
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
          .Expect (mock => mock.ResolveWhereExpression (Arg<Expression>.Is.Anything, Arg.Is (_mappingResolutionContext)))
          .WhenCalled (mi => ExpressionTreeComparer.CheckAreEqualTrees (expectedResultWhereCondition, (Expression) mi.Arguments[0]))
          .Return (fakeWhereCondition);
      _stageMock.Replay();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (tableInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf (typeof (ResolvedJoinedGroupingTableInfo)));

      var resultGroupingSelector = ((ResolvedJoinedGroupingTableInfo) result).AssociatedGroupingSelectExpression;
      Assert.That (resultGroupingSelector, Is.SameAs (groupingSelect));
      
      var resultSqlStatement = ((ResolvedJoinedGroupingTableInfo) result).SqlStatement;

      Assert.That (resultSqlStatement.SqlTables, Is.EqualTo (groupingSubStatement.SqlTables));
      Assert.That (resultSqlStatement.Orderings, Is.Empty);
      Assert.That (resultSqlStatement.GroupByExpression, Is.Null);
      
      ExpressionTreeComparer.CheckAreEqualTrees (
          Expression.AndAlso (groupingSubStatement.WhereCondition, fakeWhereCondition), 
          resultSqlStatement.WhereCondition);

      var expectedResultSelectProjection =
          Expression.MakeMemberAccess (new SqlTableReferenceExpression (resultSqlStatement.SqlTables[0]), typeof (Cook).GetProperty ("ID"));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResultSelectProjection, resultSqlStatement.SelectProjection);

      Assert.That (resultSqlStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (resultSqlStatement.DataInfo.DataType, Is.SameAs (typeof (IQueryable<int>)));

      var expectedItemExpression = resultSqlStatement.SelectProjection;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedItemExpression, ((StreamedSequenceInfo) resultSqlStatement.DataInfo).ItemExpression);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "This SQL generator only supports sequences in from expressions if they are members of an entity or if they come from a GroupBy operator.")]
    public void ResolveTableInfo_GroupReferenceTableInfo_NoSubStatement ()
    {
      var groupSource = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (IEnumerable<Cook>));
      var tableInfo = new UnresolvedGroupReferenceTableInfo (groupSource);

      ResolvingTableInfoVisitor.ResolveTableInfo (tableInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "When a sequence retrieved by a subquery is used in a from expression, the subquery must end with a GroupBy operator.")]
    public void ResolveTableInfo_GroupReferenceTableInfo_NoGorupingSubStatement ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (new int[0]));
      var groupSource = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSubStatementTableInfo ("q0", subStatement));
      var tableInfo = new UnresolvedGroupReferenceTableInfo (groupSource);

      ResolvingTableInfoVisitor.ResolveTableInfo (tableInfo, _resolverMock, _generator, _stageMock, _mappingResolutionContext);
    }
  }
}