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
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingSelectExpressionVisitorTest
  {
    private IMappingResolutionStage _stageMock;
    private IMappingResolver _resolverMock;
    private IMappingResolutionContext _mappingResolutionContext;
    private UniqueIdentifierGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage>();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver>();
      _mappingResolutionContext = new MappingResolutionContext();
      _generator = new UniqueIdentifierGenerator();
    }

    [Test]
    public void VisitSqlSubStatementExpression_LeavesSqlSubStatementExpression_ForStreamedSequenceInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (new Cook()));
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (sqlStatement);
      _stageMock.Replay();

      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var result = ResolvingSelectExpressionVisitor.ResolveExpression (
          expression, _resolverMock, _stageMock, _mappingResolutionContext, _generator, sqlStatementBuilder);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void VisitSqlSubStatementExpression_LeavesSqlSubStatementExpression_ForStreamedScalarInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Scalar();
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (sqlStatement);
      _stageMock.Replay();

      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var result = ResolvingSelectExpressionVisitor.ResolveExpression (
          expression, _resolverMock, _stageMock, _mappingResolutionContext, _generator, sqlStatementBuilder);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void VisitSqlSubStatementExpression_ConvertsToSqlTable_ForStreamedSingleValueInfo ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Single();
      var fakeResolvedSqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Single();
      var sqlStatementBuilder = new SqlStatementBuilder (sqlStatement);
      var expression = new SqlSubStatementExpression (sqlStatement);

      var resolvedReference = Expression.Constant ("fake");
      SqlTable sqlTable = null;

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (fakeResolvedSqlStatement);
      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (Arg<SqlTableReferenceExpression>.Is.Anything, Arg.Is (_mappingResolutionContext)))
          .WhenCalled (
              mi =>
              {
                var expectedStatement =
                    new SqlStatementBuilder (fakeResolvedSqlStatement)
                    { DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<int>), fakeResolvedSqlStatement.SelectProjection) }.GetSqlStatement();
                sqlTable = (SqlTable) ((SqlTableReferenceExpression) mi.Arguments[0]).SqlTable;
                Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement, Is.EqualTo (expectedStatement));
              })
          .Return (resolvedReference);
      _stageMock.Replay();

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (resolvedReference)).Return (resolvedReference);
      _resolverMock.Replay();

      Assert.That (sqlStatementBuilder.SqlTables.Count, Is.EqualTo (0));
      
      var result = ResolvingSelectExpressionVisitor.ResolveExpression (
          expression,
          _resolverMock,
          _stageMock,
          _mappingResolutionContext,
          _generator,
          sqlStatementBuilder);

      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (resolvedReference));
      Assert.That (sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (sqlStatementBuilder.SqlTables[0], Is.EqualTo (sqlTable));
    }
  }
}