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
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingSelectExpressionVisitorTest
  {
    private IMappingResolutionStage _stageMock;
    private IMappingResolver _resolverMock;
    private SqlTable _sqlTable;
    private MappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage> ();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver> ();
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (Cook));
      _mappingResolutionContext = new MappingResolutionContext ();

      
    }

    [Test]
    public void VisitSqlSubStatementExpression_ReturnsNoSqlSubStatementExpression ()
    {
      var dataInfo = new StreamedScalarValueInfo (typeof (int));
      var resolvedElementExpressionReference = new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false);
      var resolvedSelectProjection = new NamedExpression (
          null,
          new AggregationExpression (typeof (int), resolvedElementExpressionReference, AggregationModifier.Min));
      var associatedGroupingSelectExpression = new SqlGroupingSelectExpression (
          new NamedExpression ("key", Expression.Constant ("k")),
          new NamedExpression ("element", Expression.Constant ("e")));
      var resolvedJoinedGroupingSubStatement = SqlStatementModelObjectMother.CreateSqlStatement (associatedGroupingSelectExpression);
      var resolvedJoinedGroupingTable = new SqlTable (
          new ResolvedJoinedGroupingTableInfo (
              "q1",
              resolvedJoinedGroupingSubStatement,
              associatedGroupingSelectExpression,
              "q0"), JoinSemantics.Inner);
      var simplifiableResolvedSqlStatement = new SqlStatement (
         dataInfo,
         resolvedSelectProjection,
         new[] { resolvedJoinedGroupingTable },
         null,
         null,
         new Ordering[0],
         null,
         false,
         Expression.Constant (0),
         Expression.Constant (0));
      var fakeSelectExpression = Expression.Constant ("fake");

      var expression = new SqlSubStatementExpression (simplifiableResolvedSqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (simplifiableResolvedSqlStatement, _mappingResolutionContext))
          .Return (simplifiableResolvedSqlStatement);
      _stageMock
          .Expect (mock => mock.ResolveAggregationExpression(Arg<Expression>.Is.Anything, Arg<IMappingResolutionContext>.Is.Anything))
          .Return (fakeSelectExpression);
      _stageMock.Replay();

      var result = ResolvingSelectExpressionVisitor.ResolveExpression(expression, _resolverMock, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (SqlColumnDefinitionExpression)));
    }

    [Test]
    public void VisitSqlSubStatementExpression_ReturnsSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (sqlStatement);
      _stageMock.Replay();

      var result = ResolvingSelectExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
    }


  }
}