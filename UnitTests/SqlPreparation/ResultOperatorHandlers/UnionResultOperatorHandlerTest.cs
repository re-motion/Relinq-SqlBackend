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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class UnionResultOperatorHandlerTest : ResultOperatorHandlerTestBase
  {
    private ISqlPreparationStage _stage;
    private UnionResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    public override void SetUp ()
    {
      base.SetUp();

      _stage = CreateDefaultSqlPreparationStage();
      _handler = new UnionResultOperatorHandler();

      var selectProjection = ExpressionHelper.CreateExpression (typeof (int));
      _sqlStatementBuilder = new SqlStatementBuilder
                             {
                                 DataInfo = new StreamedSequenceInfo (typeof (int[]), selectProjection),
                                 SelectProjection = selectProjection
                             };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext();
    }

    [Test]
    public void HandleResultOperator_CreatesUnionStatement ()
    {
      var resultOperator = CreateValidResultOperator();

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, UniqueIdentifierGenerator, _stage, _context);

      Assert.That (_sqlStatementBuilder.SetOperationCombinedStatements, Has.Count.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SetOperationCombinedStatements[0].SetOperation, Is.EqualTo (SetOperation.Union));
    }

    private static UnionResultOperator CreateValidResultOperator ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (Expression.Constant (0));
      return new UnionResultOperator ("x", typeof (int), new SqlSubStatementExpression (sqlStatement));
    }
  }
}