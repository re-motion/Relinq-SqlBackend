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
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class SingleResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stage;
    private UniqueIdentifierGenerator _generator;
    private SingleResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator ();
      _stage = new DefaultSqlPreparationStage (
          CompoundMethodCallTransformerProvider.CreateDefault(), ResultOperatorHandlerRegistry.CreateDefault(), _generator);
      _handler = new SingleResultOperatorHandler();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement())
                             {
                                 DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                             };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator ()
    {
      var resultOperator = new SingleResultOperator (false);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (((SqlLiteralExpression) _sqlStatementBuilder.TopExpression).Value, Is.EqualTo(2));
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
      Assert.That (((StreamedSingleValueInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (Cook)));
    }

    [Test]
    public void HandleResultOperator_SingleAfterTopExpression ()
    {
      _sqlStatementBuilder.TopExpression = Expression.Constant ("top");

      var resultOperator = new SingleResultOperator (false);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (((SqlLiteralExpression) _sqlStatementBuilder.TopExpression).Value, Is.EqualTo (2));
      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SqlTables[0].SqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
    }

    [Test]
    public void HandleResultOperator_SingleAfterGroupExpression ()
    {
      _sqlStatementBuilder.TopExpression = Expression.Constant ("group");

      var resultOperator = new SingleResultOperator (false);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (((SqlLiteralExpression) _sqlStatementBuilder.TopExpression).Value, Is.EqualTo (2));
      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SqlTables[0].SqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
    }

    [Test]
    public void HandleResultOperator_SingleAfterSkipExpression ()
    {
      _sqlStatementBuilder.WhereCondition = null;
      _sqlStatementBuilder.RowNumberSelector = Expression.Constant (5);
      _sqlStatementBuilder.CurrentRowNumberOffset = Expression.Constant (3);

      var resultOperator = new SingleResultOperator(false);

      var expectedWhereCondition = Expression.LessThanOrEqual (
                _sqlStatementBuilder.RowNumberSelector, Expression.Add (_sqlStatementBuilder.CurrentRowNumberOffset, new SqlLiteralExpression(2)));

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedWhereCondition, _sqlStatementBuilder.WhereCondition);
    }
  }
}