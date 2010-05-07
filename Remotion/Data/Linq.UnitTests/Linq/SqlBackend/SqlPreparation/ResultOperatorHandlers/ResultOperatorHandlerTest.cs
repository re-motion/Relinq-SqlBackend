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
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.ResultOperators;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class ResultOperatorHandlerTest
  {
    private TestableResultOperatorHandler _handler;
    private TestChoiceResultOperator _resultOperator;
    private SqlStatementBuilder _statementBuilder;
    private UniqueIdentifierGenerator _generator;
    private ISqlPreparationStage _stageMock;
    private SqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _handler = new TestableResultOperatorHandler();
      _resultOperator = new TestChoiceResultOperator (false);
      _statementBuilder = new SqlStatementBuilder();
      _statementBuilder.SelectProjection = Expression.Constant ("select");
      _statementBuilder.DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()));
      _generator = new UniqueIdentifierGenerator();
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _context = new SqlPreparationContext();
    }

    [Test]
    public void MoveCurrentStatementToSqlTable ()
    {
      var originalStatement = _statementBuilder.GetSqlStatement ();

      _handler.MoveCurrentStatementToSqlTable (_statementBuilder, _generator, _context, info => new SqlTable (info));

      CheckStatementMovedToSqlTable (originalStatement);
    }

    [Test]
    public void EnsureNoTopExpressionAndSetDataInfo_WithTopExpression ()
    {
      _statementBuilder.TopExpression = Expression.Constant ("top");
      var originalStatement = _statementBuilder.GetSqlStatement();

      _handler.EnsureNoTopExpression (_resultOperator, _statementBuilder, _generator, _stageMock, _context);

      Assert.That (originalStatement, Is.Not.EqualTo (_statementBuilder.GetSqlStatement()));

      CheckStatementMovedToSqlTable (originalStatement);
    }

    [Test]
    public void EnsureNoTopExpressionAndSetDataInfo_WithoutTopExpression ()
    {
      var sqlStatement = _statementBuilder.GetSqlStatement();

      _handler.EnsureNoTopExpression (_resultOperator, _statementBuilder, _generator, _stageMock, _context);

      Assert.That (sqlStatement, Is.EqualTo (_statementBuilder.GetSqlStatement()));
    }

    [Test]
    public void EnsureNoDistinctQuery_DistinctQuery ()
    {
      var resultOperator = new TestChoiceResultOperator (false);
      _statementBuilder.IsDistinctQuery = true;
      var sqlStatement = _statementBuilder.GetSqlStatement ();

      _handler.EnsureNoDistinctQuery(resultOperator, _statementBuilder, _generator, _stageMock, _context);

      Assert.That (sqlStatement, Is.Not.EqualTo (_statementBuilder.GetSqlStatement ()));
      Assert.That (_context.TryGetContextMappingFromHierarchy (((StreamedSequenceInfo) sqlStatement.DataInfo).ItemExpression), Is.Not.Null);
      Assert.That (
          ((ResolvedSubStatementTableInfo) ((SqlTable)
                                            ((SqlTableReferenceExpression)
                                             _context.TryGetContextMappingFromHierarchy (
                                                 ((StreamedSequenceInfo) sqlStatement.DataInfo).ItemExpression)).SqlTable).TableInfo).SqlStatement,
          Is.EqualTo (sqlStatement));
    }

    [Test]
    public void EnsureDistinctQuery_NoDistinctQuery ()
    {
      var resultOperator = new TestChoiceResultOperator (false);
      _statementBuilder.IsDistinctQuery = false;
      var sqlStatement = _statementBuilder.GetSqlStatement ();

      _handler.EnsureNoDistinctQuery(resultOperator, _statementBuilder, _generator, _stageMock, _context);

      Assert.That (sqlStatement, Is.EqualTo (_statementBuilder.GetSqlStatement ()));
    }

    [Test]
    public void UpdateDataInfo ()
    {
      var streamDataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()));

      _handler.UpdateDataInfo (_resultOperator, _statementBuilder, streamDataInfo);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
    }

    private void CheckStatementMovedToSqlTable (SqlStatement originalStatement)
    {
      Assert.That (((SqlTable) _statementBuilder.SqlTables[0]).TableInfo, Is.InstanceOfType (typeof (ResolvedSubStatementTableInfo)));
      var subStatement = ((ResolvedSubStatementTableInfo) ((SqlTable) _statementBuilder.SqlTables[0]).TableInfo).SqlStatement;

      Assert.That (subStatement, Is.EqualTo (originalStatement));
      Assert.That (_statementBuilder.SelectProjection, Is.InstanceOfType (typeof (SqlTableReferenceExpression)));
      Assert.That (((SqlTableReferenceExpression) _statementBuilder.SelectProjection).SqlTable, Is.SameAs (_statementBuilder.SqlTables[0]));

      var mappedItemExpression = _context.TryGetContextMappingFromHierarchy (((StreamedSequenceInfo) originalStatement.DataInfo).ItemExpression);
      Assert.That (mappedItemExpression, Is.Not.Null);
      Assert.That (mappedItemExpression, Is.InstanceOfType (typeof (SqlTableReferenceExpression)));
      Assert.That (((SqlTableReferenceExpression) mappedItemExpression).SqlTable, Is.SameAs (_statementBuilder.SqlTables[0]));
    }
  }
}