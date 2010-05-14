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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class SqlContextSelectionAdjusterTest
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

      var result = SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result.SelectProjection, Is.TypeOf(typeof(SqlTableReferenceExpression)));
      Assert.That (result.DataInfo, Is.SameAs (sqlStatement.DataInfo));
    }

    // TODO Review 2765: only select projection needed for this test
    [Test]
    public void VisitSqlStatement_ExpressionsAndStreamedSequenceDataTypeChanged ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      var topExpression = Expression.Constant ("top");
      builder.TopExpression = topExpression;
      var whereCondition = Expression.Constant (true);
      builder.WhereCondition = whereCondition;
      var orderingExpression = Expression.Constant ("ordering");
      builder.Orderings.Add (new Ordering (orderingExpression, OrderingDirection.Asc));
      builder.DataInfo = new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (builder.SelectProjection.Type), builder.SelectProjection);
      var sqlStatement = builder.GetSqlStatement();

      var fakeResult = Expression.Constant ("test");
      
      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (fakeResult);
     _stageMock.Replay();

      var result = SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (result.SelectProjection, Is.SameAs (fakeResult));
      Assert.That (result.WhereCondition, Is.SameAs (whereCondition));
      Assert.That (result.TopExpression, Is.SameAs (topExpression));
      Assert.That (result.Orderings[0].Expression, Is.SameAs (orderingExpression));
      Assert.That (result.Orderings[0].OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      Assert.That (result.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) result.DataInfo).ItemExpression.Type, Is.EqualTo (typeof (string)));
      Assert.That (result.DataInfo.DataType, Is.EqualTo (typeof (IQueryable<>).MakeGenericType (fakeResult.Type)));
    }

    [Test]
    public void VisitSqlStatement_CopiesIsCountQueryFlag () // TODO Review 2765: there is no count flag any more, remove test
    {
      var sqlStatementWithCook = SqlStatementModelObjectMother.CreateSqlStatementWithCook();
      var builder = new SqlStatementBuilder (sqlStatementWithCook)
                    { SelectProjection = new AggregationExpression(sqlStatementWithCook.SelectProjection, AggregationModifier.Count) };
      var sqlStatement = builder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (sqlStatement.SelectProjection);
      _stageMock.Replay();

      var result = SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (((AggregationExpression) result.SelectProjection).AggregationModifier, Is.EqualTo (AggregationModifier.Count));
    }

    [Test]
    public void VisitSqlStatement_CopiesIsDistinctQueryFlag () // TODO Review 2765: write one test that checks that everything is copied (all expressions + flags) when the select expression is changed
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook()) { IsDistinctQuery = true };
      var sqlStatement = builder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired))
          .Return (sqlStatement.SelectProjection);
      _stageMock.Replay();

      var result = SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result.IsDistinctQuery, Is.True);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "A SqlStatement cannot be used as a predicate.")]
    public void VisitSqlStatement_PrdicateRequired_ThrowsException ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.PredicateRequired, _stageMock);
    }
  }
}