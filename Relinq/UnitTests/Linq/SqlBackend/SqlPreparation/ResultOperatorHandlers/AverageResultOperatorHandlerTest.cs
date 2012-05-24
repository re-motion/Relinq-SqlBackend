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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.UnitTests.Linq.Core;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class AverageResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stage;
    private UniqueIdentifierGenerator _generator;
    private AverageResultOperatorHandler _handler;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator ();
      _stage = new DefaultSqlPreparationStage (
          CompoundMethodCallTransformerProvider.CreateDefault(), ResultOperatorHandlerRegistry.CreateDefault(), _generator);
      _handler = new AverageResultOperatorHandler ();
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator_WithMatchingTypes ()
    {
      var sqlStatementBuilder = CreateSqlStatementBuilder (typeof (decimal));

      var averageResultOperator = new AverageResultOperator ();
      Assert.That (averageResultOperator.GetOutputDataInfo (sqlStatementBuilder.DataInfo).DataType, Is.SameAs (typeof (decimal)));

      var previousSelectExpression = sqlStatementBuilder.SelectProjection;

      _handler.HandleResultOperator (averageResultOperator, sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (sqlStatementBuilder.SelectProjection, Is.TypeOf<AggregationExpression> ());
      Assert.That (((AggregationExpression) sqlStatementBuilder.SelectProjection).AggregationModifier, Is.EqualTo (AggregationModifier.Average));
      Assert.That (((AggregationExpression) sqlStatementBuilder.SelectProjection).Expression, Is.SameAs (previousSelectExpression));

      Assert.That (sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (((StreamedScalarValueInfo) sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (decimal)));
    }

    [Test]
    public void HandleResultOperator_WithNonMatchingTypes ()
    {
      var sqlStatementBuilder = CreateSqlStatementBuilder (typeof (int));

      var averageResultOperator = new AverageResultOperator ();
      Assert.That (averageResultOperator.GetOutputDataInfo (sqlStatementBuilder.DataInfo).DataType, Is.SameAs (typeof (double)));

      var previousSelectExpression = sqlStatementBuilder.SelectProjection;

      _handler.HandleResultOperator (averageResultOperator, sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (sqlStatementBuilder.SelectProjection, Is.TypeOf<AggregationExpression> ());
      Assert.That (((AggregationExpression) sqlStatementBuilder.SelectProjection).AggregationModifier, Is.EqualTo (AggregationModifier.Average));
      Assert.That (((AggregationExpression) sqlStatementBuilder.SelectProjection).Expression, Is.TypeOf<SqlConvertExpression> ());

      var sqlConvertExpression = (SqlConvertExpression) ((AggregationExpression) sqlStatementBuilder.SelectProjection).Expression;
      Assert.That (sqlConvertExpression.Type, Is.SameAs (typeof (double)));
      Assert.That (sqlConvertExpression.Source, Is.SameAs (previousSelectExpression));

      Assert.That (sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (((StreamedScalarValueInfo) sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (double)));
    }

    private SqlStatementBuilder CreateSqlStatementBuilder (Type sequenceItemType)
    {
      var selectProjection = ExpressionHelper.CreateExpression (sequenceItemType);
      return new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo (sequenceItemType.MakeArrayType (), selectProjection),
        SelectProjection = selectProjection
      };
    }
  }
}