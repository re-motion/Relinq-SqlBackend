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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
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
    private IMappingResolutionContext _mappingResolutionContextMock;
    private UniqueIdentifierGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage> ();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver> ();
      _mappingResolutionContextMock = MockRepository.GenerateMock<IMappingResolutionContext>();
      _generator = new UniqueIdentifierGenerator();
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
          .Expect (mock => mock.ResolveSqlStatement (simplifiableResolvedSqlStatement, _mappingResolutionContextMock))
          .Return (simplifiableResolvedSqlStatement);
      _stageMock
          .Expect (mock => mock.ResolveAggregationExpression(Arg<Expression>.Is.Anything, Arg<IMappingResolutionContext>.Is.Anything))
          .Return (fakeSelectExpression);
      _stageMock.Replay();

      var result = ResolvingSelectExpressionVisitor.ResolveExpression(expression, _resolverMock, _stageMock, _mappingResolutionContextMock, _generator);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (SqlColumnDefinitionExpression)));
    }

    [Test]
    public void VisitSqlSubStatementExpression_ReturnsSqlSubStatementExpression_StreamedSequenceInfo ()
    {
      var sqlStatement = new SqlStatement (
          new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ())),
          Expression.Constant ("select"),
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null);
      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContextMock))
          .Return (sqlStatement);
      _stageMock.Replay();

      var result = ResolvingSelectExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContextMock, _generator, sqlStatementBuilder);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf(typeof(SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void VisitSqlSubStatementExpression_ReturnsSqlSubStatementExpression_StreamedScalarInfo ()
    {
      var selectProjection = Expression.Constant (1);
      var sqlStatement = new SqlStatement (
         new StreamedScalarValueInfo (typeof (int)),
         selectProjection,
         new SqlTable[0],
         null,
         null,
         new Ordering[0],
         null,
         false,
         null,
         null);
      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContextMock))
          .Return (sqlStatement);
      _stageMock.Replay ();

      var result = ResolvingSelectExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContextMock, _generator, sqlStatementBuilder);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void VisitSqlSubStatementExpression_StreamedSingleValueInfo ()
    {
      var selectProjection = Expression.Constant (1);
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      var sqlStatement = new SqlStatement (
          new StreamedSingleValueInfo (typeof (int), false),
          selectProjection,
          new[] { sqlTable },
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null);
      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var expression = new SqlSubStatementExpression (sqlStatement);

      var fakeResult = Expression.Constant ("fake");

      _mappingResolutionContextMock
          .Expect (mock => mock.AddSqlTable (Arg<SqlTable>.Is.Anything, Arg<SqlStatementBuilder>.Is.Anything));
      _mappingResolutionContextMock.Replay ();

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContextMock))
          .Return (sqlStatement);
      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (Arg<SqlTableReferenceExpression>.Is.Anything, Arg.Is (_mappingResolutionContextMock)))
          .Return (fakeResult);
      _stageMock.Replay ();

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (fakeResult)).Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingSelectExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContextMock, _generator, sqlStatementBuilder);

      _mappingResolutionContextMock.VerifyAllExpectations ();
      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs(fakeResult));
    }

    

  }
}