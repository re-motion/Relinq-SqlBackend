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
      // TODO Review 3097: Use an ordinary context for this, not a mock.
      _mappingResolutionContextMock = MockRepository.GenerateMock<IMappingResolutionContext>();
      _generator = new UniqueIdentifierGenerator();
    }

    [Test]
    public void VisitSqlSubStatementExpression_LeavesSqlSubStatementExpression_ForStreamedSequenceInfo ()
    {
      // TODO Review 3097: Use SqlStatementObjectMother to create this
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
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContextMock))
          .Return (sqlStatement);
      _stageMock.Replay();

      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var result = ResolvingSelectExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContextMock, _generator, sqlStatementBuilder);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf(typeof(SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void VisitSqlSubStatementExpression_LeavesSqlSubStatementExpression_ForStreamedScalarInfo ()
    {
      // TODO Review 3097: Use SqlStatementObjectMother to create this. (Add a CreateSqlStatement_Scalar method.)
      var sqlStatement = new SqlStatement (
         new StreamedScalarValueInfo (typeof (int)),
         Expression.Constant (1),
         new SqlTable[0],
         null,
         null,
         new Ordering[0],
         null,
         false,
         null,
         null);
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContextMock))
          .Return (sqlStatement);
      _stageMock.Replay ();

      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var result = ResolvingSelectExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContextMock, _generator, sqlStatementBuilder);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void VisitSqlSubStatementExpression_ConvertsToSqlTable_ForStreamedSingleValueInfo ()
    {
      // TODO Review 3097: Use SqlStatementObjectMother to create this. (CreateSqlStatement_Single)
      var sqlStatement = new SqlStatement (
          new StreamedSingleValueInfo (typeof (int), false),
          Expression.Constant (1),
          new[] { SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook)) },
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null);
      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var expression = new SqlSubStatementExpression (sqlStatement);

      var resolvedReference = Expression.Constant ("fake");

      _mappingResolutionContextMock
          .Expect (mock => mock.AddSqlTable (Arg<SqlTable>.Is.Anything, Arg<SqlStatementBuilder>.Is.Anything));
      _mappingResolutionContextMock.Replay ();

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContextMock))
          .Return (sqlStatement); // TODO Review 3097: Return a separate "resolvedSqlStatement" and make sure that resolved statement is used below
      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (Arg<SqlTableReferenceExpression>.Is.Anything, Arg.Is (_mappingResolutionContextMock))) // TODO Review 3097: Use WhenCalled to ensure that the reference is to the newly added table which contains the "resolvedSqlStatement": var expectedStatement = new SqlStatementBuilder (resolvedSqlStatement) { DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<int>), resolvedSqlStatement.SelectProjection) }.GetSqlStatement(); Assert.That (..., Is.EqualTo (expectedStatement));
          .Return (resolvedReference);
      _stageMock.Replay ();

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (resolvedReference)).Return (resolvedReference);
      _resolverMock.Replay();

      var result = ResolvingSelectExpressionVisitor.ResolveExpression (
          expression,
          _resolverMock,
          _stageMock,
          _mappingResolutionContextMock,
          _generator,
          sqlStatementBuilder);

      _mappingResolutionContextMock.VerifyAllExpectations ();
      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (resolvedReference));
      // TODO Review 3097: When _mappingResolutionContextMock is changed to an ordinary context, assert that the table was added.
    }
  }
}