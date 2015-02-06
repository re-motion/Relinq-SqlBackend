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
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class SqlContextSelectionAdjusterTest
  {
    private Mock<IMappingResolutionStage> _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = new Mock<IMappingResolutionStage>();
      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void VisitSqlStatement_NoExpressionChanged_SameSqlStatementIsReturned ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      _stageMock
          .Setup (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
          .Returns (sqlStatement.SelectProjection)
          .Verifiable();

      var result = SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify();
      Assert.That (result.SelectProjection, Is.TypeOf(typeof(SqlTableReferenceExpression)));
      Assert.That (result.DataInfo, Is.SameAs (sqlStatement.DataInfo));
    }

    [Test]
    public void VisitSqlStatement_ExpressionsAndStreamedSequenceDataTypeChanged ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      builder.DataInfo = new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (builder.SelectProjection.Type), builder.SelectProjection);
      var sqlStatement = builder.GetSqlStatement();

      var fakeResult = Expression.Constant ("test");
      
      _stageMock
          .Setup (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
          .Returns (fakeResult)
          .Verifiable();

      var result = SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify();
      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (result.SelectProjection, Is.SameAs (fakeResult));
      Assert.That (result.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) result.DataInfo).ResultItemType, Is.EqualTo (typeof (string)));
      Assert.That (result.DataInfo.DataType, Is.EqualTo (typeof (IQueryable<>).MakeGenericType (fakeResult.Type)));
    }

    [Test]
    public void VisitSqlStatement_CopiesIsDistinctQueryFlag ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook()) { IsDistinctQuery = true };
      var sqlStatement = builder.GetSqlStatement();

      _stageMock
          .Setup (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
          .Returns (sqlStatement.SelectProjection)
          .Verifiable();

      var result = SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify();
      Assert.That (result.IsDistinctQuery, Is.True);
    }

    [Test]
    public void VisitSqlStatement_EverthingIsCopiedIfSelectionProjectionHasChanged ()
    {
      var selectProjection = Expression.Constant("select");
      var whereCondition = Expression.Constant(true);
      var topExpression = Expression.Constant("top");
      var dataInfo = new StreamedSequenceInfo(typeof(Cook[]), Expression.Constant(new Cook()));
      var builder = new SqlStatementBuilder
                    {  
        SelectProjection = selectProjection,
        WhereCondition = whereCondition,
        TopExpression = topExpression,
        IsDistinctQuery = true,
        DataInfo = dataInfo
      };
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable();
      builder.SqlTables.Add (appendedTable);
      var ordering = new Ordering (Expression.Constant ("order"),OrderingDirection.Asc);
      builder.Orderings.Add (ordering);
      var sqlStatement = builder.GetSqlStatement();
      var fakeResult = Expression.Constant ("fake");

      _stageMock
          .Setup (mock => mock.ApplyContext (sqlStatement.SelectProjection, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
          .Returns (fakeResult)
          .Verifiable();

      var result = SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired, _stageMock.Object, _mappingResolutionContext);

      _stageMock.Verify();
      Assert.That (result.SelectProjection, Is.SameAs (fakeResult));
      Assert.That (result.DataInfo, Is.SameAs (dataInfo));
      Assert.That (result.WhereCondition, Is.SameAs (whereCondition));
      Assert.That (result.TopExpression, Is.SameAs (topExpression));
      Assert.That (result.SqlTables[0], Is.SameAs (appendedTable));
      Assert.That (result.Orderings[0], Is.SameAs (ordering));
      Assert.That (result.IsDistinctQuery, Is.True);
    }

    [Test]
    public void VisitSqlStatement_PrdicateRequired_ThrowsException ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();
      Assert.That (
          () => SqlContextSelectionAdjuster.ApplyContext (sqlStatement, SqlExpressionContext.PredicateRequired, _stageMock.Object, _mappingResolutionContext),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "A SqlStatement cannot be used as a predicate."));
    }
  }
}