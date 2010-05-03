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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class DefaultIfEmptyResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private DefaultIfEmptyResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private QueryModel _queryModel;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _generator = new UniqueIdentifierGenerator();
      _handler = new DefaultIfEmptyResultOperatorHandler();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement())
                             {
                                 DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                             };
      _queryModel = new QueryModel (ExpressionHelper.CreateMainFromClause_Cook(), ExpressionHelper.CreateSelectClause());
    }

    [Test]
    public void HandleResultOperator ()
    {
      var resultOperator = new DefaultIfEmptyResultOperator (Expression.Constant (null));
      var sqlStatement = _sqlStatementBuilder.GetSqlStatement();

      _handler.HandleResultOperator (resultOperator, _queryModel, _sqlStatementBuilder, _generator, _stageMock);

      _stageMock.VerifyAllExpectations();

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SqlTables[0], Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (((SqlJoinedTable) _sqlStatementBuilder.SqlTables[0]).JoinInfo, Is.TypeOf (typeof (ResolvedLeftJoinInfo)));
      Assert.That (
          ((ResolvedLeftJoinInfo) ((SqlJoinedTable) _sqlStatementBuilder.SqlTables[0]).JoinInfo).ForeignTableInfo,
          Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (
          ((ResolvedLeftJoinInfo) ((SqlJoinedTable) _sqlStatementBuilder.SqlTables[0]).JoinInfo).LeftKey,
          Is.TypeOf (typeof (SqlLiteralExpression)));
      Assert.That (
          ((SqlLiteralExpression) ((ResolvedLeftJoinInfo) ((SqlJoinedTable) _sqlStatementBuilder.SqlTables[0]).JoinInfo).LeftKey).Value,
         Is.EqualTo(1));
      Assert.That (
          ((ResolvedLeftJoinInfo) ((SqlJoinedTable) _sqlStatementBuilder.SqlTables[0]).JoinInfo).RightKey,
          Is.TypeOf(typeof (SqlLiteralExpression)));
      Assert.That (
          ((SqlLiteralExpression) ((ResolvedLeftJoinInfo) ((SqlJoinedTable) _sqlStatementBuilder.SqlTables[0]).JoinInfo).RightKey).Value,
         Is.EqualTo (1));
      Assert.That (
          ((ResolvedSubStatementTableInfo) ((ResolvedLeftJoinInfo) ((SqlJoinedTable) _sqlStatementBuilder.SqlTables[0]).JoinInfo).ForeignTableInfo).
              SqlStatement,
          Is.EqualTo(sqlStatement));
    }
  }
}