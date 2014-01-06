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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class SumResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stage;
    private UniqueIdentifierGenerator _generator;
    private SumResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator ();
      _stage = new DefaultSqlPreparationStage (new CompoundMethodCallTransformerProvider (), new ResultOperatorHandlerRegistry (), _generator);
      _handler = new SumResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo (typeof (int[]), Expression.Constant (5))
      };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator ()
    {
      _sqlStatementBuilder.SelectProjection = new NamedExpression (null, _sqlStatementBuilder.SelectProjection);
      var averageResultOperator = new SumResultOperator ();

      _handler.HandleResultOperator (averageResultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (((AggregationExpression) _sqlStatementBuilder.SelectProjection).AggregationModifier, Is.EqualTo (AggregationModifier.Sum));
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (((StreamedScalarValueInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (int)));
    }
    
  }
}