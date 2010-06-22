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
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class SumResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stage;
    private UniqueIdentifierGenerator _generator;
    private SumResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private SqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator ();
      _stage = new DefaultSqlPreparationStage (new MethodCallTransformerRegistry (), new ResultOperatorHandlerRegistry (), _generator);
      _handler = new SumResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo (typeof (int[]), Expression.Constant (5))
      };
      _context = new SqlPreparationContext ();
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

    [Test]
    public void HandleResultOperator_SumAfterTopExpression ()
    {
      _sqlStatementBuilder.TopExpression = Expression.Constant ("top");

      var resultOperator = new SumResultOperator ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
    }

    [Test]
    public void HandleResultOperator_SumAfterGroupExpression ()
    {
      _sqlStatementBuilder.GroupByExpression = Expression.Constant ("group");

      var resultOperator = new SumResultOperator ();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
    }
  }
}