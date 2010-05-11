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
using System.Linq;
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
  public class SqlContextStatementVisitorTest
  {
    private IMappingResolutionStage _stageMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<IMappingResolutionStage>();
    }

    [Test]
    public void VisitSqlStatement_NoExpressionChanged_SameSqlStatementIsReturned ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (sqlStatement.SelectProjection);
      _stageMock.Replay();

      var result = SqlContextStatementVisitor.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result.SelectProjection, Is.TypeOf(typeof(NamedExpression)));
      Assert.That (((NamedExpression) result.SelectProjection).Expression, Is.SameAs (sqlStatement.SelectProjection));
      Assert.That (result.DataInfo, Is.SameAs (sqlStatement.DataInfo));
    }

    [Test]
    public void VisitSqlStatement_ExpressionsAndStreamedSequenceDataTypeChanged ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      builder.TopExpression = Expression.Constant ("top");
      builder.WhereCondition = Expression.Constant (true);
      builder.Orderings.Add (new Ordering (Expression.Constant ("ordering"), OrderingDirection.Asc));
      builder.DataInfo = new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (builder.SelectProjection.Type), builder.SelectProjection);
      var sqlStatement = builder.GetSqlStatement();

      var fakeResult = Expression.Constant ("test");
      var fakeWhereResult = Expression.Equal (fakeResult, fakeResult);

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (fakeResult);
      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.TopExpression, SqlExpressionContext.SingleValueRequired))
          .Return (fakeResult);
      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.WhereCondition, SqlExpressionContext.PredicateRequired))
          .Return (fakeWhereResult);
      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.Orderings[0].Expression, SqlExpressionContext.SingleValueRequired))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = SqlContextStatementVisitor.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (((NamedExpression) result.SelectProjection).Expression, Is.SameAs (fakeResult));
      Assert.That (result.WhereCondition, Is.SameAs (fakeWhereResult));
      Assert.That (result.TopExpression, Is.SameAs (fakeResult));
      Assert.That (result.Orderings[0].Expression, Is.SameAs (fakeResult));
      Assert.That (result.Orderings[0].OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      Assert.That (result.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) result.DataInfo).ItemExpression.Type, Is.EqualTo (typeof (string)));
      Assert.That (result.DataInfo.DataType, Is.EqualTo (typeof (IQueryable<>).MakeGenericType (fakeResult.Type)));
    }

    [Test]
    public void VisitSqlStatement_SelectExpressionAndStreamedSingleValueTypeChanged_NoEntityExpression ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      builder.DataInfo = new StreamedSingleValueInfo (builder.SelectProjection.Type, true);
      var sqlStatement = builder.GetSqlStatement();
      var fakeResult = Expression.Constant ("fake");

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = SqlContextStatementVisitor.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (((NamedExpression) result.SelectProjection).Expression, Is.SameAs (fakeResult));
      Assert.That (result.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
      Assert.That (result.DataInfo.DataType, Is.EqualTo (fakeResult.Type));
    }

    [Test]
    public void VisitSqlStatement_SelectExpressionAndStreamedSingleValueTypeChanged_EntityExpression ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook ());
      builder.DataInfo = new StreamedSingleValueInfo (builder.SelectProjection.Type, true);
      var sqlStatement = builder.GetSqlStatement ();
      var fakeResult = new SqlEntityExpression (
          SqlStatementModelObjectMother.CreateSqlTable(), new SqlColumnExpression (typeof (string), "c", "Name", false));

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (fakeResult);
      _stageMock.Replay ();

      var result = SqlContextStatementVisitor.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (result.SelectProjection, Is.SameAs (fakeResult));
      Assert.That (result.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
      Assert.That (result.DataInfo.DataType, Is.EqualTo (fakeResult.Type));
    }

    [Test]
    public void VisitSqlStatement_SelectExpressionAndStreamedScalarValueTypeChanged ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      builder.DataInfo = new StreamedScalarValueInfo (builder.SelectProjection.Type);
      var sqlStatement = builder.GetSqlStatement();
      var fakeResult = Expression.Constant (new Cook());

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = SqlContextStatementVisitor.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (((NamedExpression) result.SelectProjection).Expression, Is.SameAs (fakeResult));
      Assert.That (result.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataInfo.DataType, Is.EqualTo (fakeResult.Type));
    }

    [Test]
    public void VisitSqlStatement_SqlTablesAreVisited ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      var sqlStatement = builder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.SingleValueRequired))
          .Return (sqlStatement.SelectProjection);
      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SqlTables[0], SqlExpressionContext.ValueRequired));
      _stageMock.Replay();

      var result = SqlContextStatementVisitor.ApplyContext (sqlStatement, SqlExpressionContext.SingleValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result.SqlTables[0], Is.SameAs (sqlStatement.SqlTables[0]));
    }

    [Test]
    public void VisitSqlStatement_CopiesIsCountQueryFlag ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
                    { AggregationModifier = AggregationModifier.Count };
      var sqlStatement = builder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (sqlStatement.SelectProjection);
      _stageMock.Replay();

      var result = SqlContextStatementVisitor.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (result.AggregationModifier == AggregationModifier.Count, Is.True);
    }

    [Test]
    public void VisitSqlStatement_CopiesIsDistinctQueryFlag ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook()) { IsDistinctQuery = true };
      var sqlStatement = builder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (sqlStatement.SelectProjection);
      _stageMock.Replay();

      var result = SqlContextStatementVisitor.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result.IsDistinctQuery, Is.True);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "A SqlStatement cannot be used as a predicate.")]
    public void VisitSqlStatement_PrdicateRequired_ThrowsException ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      SqlContextStatementVisitor.ApplyContext (sqlStatement, SqlExpressionContext.PredicateRequired, _stageMock);
    }
  }
}