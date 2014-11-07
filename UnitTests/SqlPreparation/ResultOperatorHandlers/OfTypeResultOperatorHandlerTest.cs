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
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class OfTypeResultOperatorHandlerTest : ResultOperatorHandlerTestBase
  {
    private ISqlPreparationStage _stage;
    private OfTypeResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    public override void SetUp ()
    {
      base.SetUp();

      _stage = CreateDefaultSqlPreparationStage();
      _handler = new OfTypeResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator ()
    {
      var originalSelectProjection = _sqlStatementBuilder.SelectProjection;
      var resultOperator = new OfTypeResultOperator (typeof (Chef));

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      Assert.That (_sqlStatementBuilder.WhereCondition, Is.TypeOf (typeof (TypeBinaryExpression)));
      Assert.That (((TypeBinaryExpression) _sqlStatementBuilder.WhereCondition).TypeOperand, Is.EqualTo (typeof (Chef)));

      var expectedSelectProjection = Expression.Convert (originalSelectProjection, typeof (Chef));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, _sqlStatementBuilder.SelectProjection);
      
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (IQueryable<>).MakeGenericType (typeof (Chef))));
    }

    [Test]
    public void HandleResultOperator_AfterGroupExpression_CreatesSubStatement ()
    {
      _sqlStatementBuilder.GroupByExpression = Expression.Constant ("group");

      var resultOperator = new OfTypeResultOperator(typeof(Chef));

      _handler.HandleResultOperator(resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }

    [Test]
    public void HandleResultOperator_AfterSetOperations_CreatesSubStatement ()
    {
      _sqlStatementBuilder.SetOperationCombinedStatements.Add(SqlStatementModelObjectMother.CreateSetOperationCombinedStatement());

      var resultOperator = new OfTypeResultOperator(typeof(Chef));

      _handler.HandleResultOperator(resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      AssertStatementWasMovedToSubStatement (_sqlStatementBuilder);
    }
  }
}