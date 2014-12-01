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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class DefaultIfEmptyResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stage;
    private UniqueIdentifierGenerator _generator;
    private DefaultIfEmptyResultOperatorHandler _handler;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator ();
      _stage = new DefaultSqlPreparationStage (
          CompoundMethodCallTransformerProvider.CreateDefault(), ResultOperatorHandlerRegistry.CreateDefault(), _generator);
      _handler = new DefaultIfEmptyResultOperatorHandler();
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator_ThrowsForGivenDefaultValue ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder();
      var resultOperator = new DefaultIfEmptyResultOperator (Expression.Constant (null));

      Assert.That (
          () => _handler.HandleResultOperator (resultOperator, sqlStatementBuilder, _generator, _stage, _context),
          Throws.TypeOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The DefaultIfEmpty operator is not supported if a default value is specified. Use the overload without a specified default value."));
    }

    [Test]
    public void HandleResultOperator_WithSingleTable_ConvertsTableIntoLeftJoinedTable ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable();
      var selectProjection = new SqlTableReferenceExpression (sqlTable);
      var dataInfo = new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (selectProjection.Type), selectProjection);

      var sqlStatementBuilder = new SqlStatementBuilder
                                {
                                    SqlTables = { sqlTable },
                                    SelectProjection = selectProjection,
                                    DataInfo = dataInfo
                                };
      
      var resultOperator = new DefaultIfEmptyResultOperator (null);

      _handler.HandleResultOperator (resultOperator, sqlStatementBuilder, _generator, _stage, _context);

      // Everything is the same, but the table has been replaced.
      Assert.That (sqlStatementBuilder.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (sqlStatementBuilder.WhereCondition, Is.Null);
      Assert.That (sqlStatementBuilder.SqlTables, Has.Count.EqualTo (1));
      Assert.That (sqlStatementBuilder.SqlTables[0].JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
      Assert.That (sqlStatementBuilder.SqlTables[0].TableInfo, Is.TypeOf<ResolvedSubStatementTableInfo>());
      Assert.That (((ResolvedSubStatementTableInfo) sqlStatementBuilder.SqlTables[0].TableInfo).TableAlias, Is.EqualTo ("Empty"));

      // The new table now contains a dummy statement (SELECT NULL AS Empty)...
      var dummySubStatement = ((ResolvedSubStatementTableInfo) sqlStatementBuilder.SqlTables[0].TableInfo).SqlStatement;
      var expectedNullAsEmptyProjection = new NamedExpression ("Empty", SqlLiteralExpression.Null (typeof (object)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullAsEmptyProjection, dummySubStatement.SelectProjection);
      Assert.That (dummySubStatement.SqlTables, Is.Empty);
      Assert.That (dummySubStatement.DataInfo, Is.TypeOf<StreamedSequenceInfo>());
      Assert.That (dummySubStatement.DataInfo.DataType, Is.SameAs (typeof(IEnumerable<object>)));
      Assert.That (((StreamedSequenceInfo) dummySubStatement.DataInfo).ItemExpression, Is.SameAs (dummySubStatement.SelectProjection));

      // ... as well as a join for the original table ...
      Assert.That (sqlStatementBuilder.SqlTables[0].OrderedJoins, Has.Count.EqualTo (1));
      var join = sqlStatementBuilder.SqlTables[0].OrderedJoins.Single();
      Assert.That (join.JoinedTable, Is.SameAs (sqlTable));
      
      // ... with a dummy join condition.
      ExpressionTreeComparer.CheckAreEqualTrees (Expression.Equal (new SqlLiteralExpression (1), new SqlLiteralExpression (1)), join.JoinCondition);
    }

    [Test]
    public void HandleResultOperator_WithSingleTable_ConvertsTableIntoLeftJoinedTable_AndPutsWhereConditionIntoJoinCondition ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable();
      var selectProjection = new SqlTableReferenceExpression (sqlTable);
      var whereCondition = ExpressionHelper.CreateExpression (typeof (bool));
      var dataInfo = new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (selectProjection.Type), selectProjection);

      var sqlStatementBuilder = new SqlStatementBuilder
                                {
                                    SqlTables = { sqlTable },
                                    SelectProjection = selectProjection,
                                    WhereCondition = whereCondition,
                                    DataInfo = dataInfo
                                };
      
      var resultOperator = new DefaultIfEmptyResultOperator (null);

      _handler.HandleResultOperator (resultOperator, sqlStatementBuilder, _generator, _stage, _context);

      // Again, everything is the same, but the table has been replaced.
      Assert.That (sqlStatementBuilder.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (sqlStatementBuilder.SqlTables, Has.Count.EqualTo (1));
      Assert.That (sqlStatementBuilder.SqlTables[0].TableInfo, Is.TypeOf<ResolvedSubStatementTableInfo>());
      Assert.That (((ResolvedSubStatementTableInfo) sqlStatementBuilder.SqlTables[0].TableInfo).TableAlias, Is.EqualTo ("Empty"));

      // Here's the join where the original table was moved...
      Assert.That (sqlStatementBuilder.SqlTables[0].OrderedJoins, Has.Count.EqualTo (1));
      var join = sqlStatementBuilder.SqlTables[0].OrderedJoins.Single();
      Assert.That (join.JoinedTable, Is.SameAs (sqlTable));
      
      // ... with the where condition as a join condition. The outer where condition has been cleared.
      Assert.That (join.JoinCondition, Is.SameAs (whereCondition));
      Assert.That (sqlStatementBuilder.WhereCondition, Is.Null);
    }

    [Test]
    public void HandleResultOperator_WithMoreThanOneTable_MovesStatementToSubStatement_AsLeftJoinedTable ()
    {
      var sqlTable1 = SqlStatementModelObjectMother.CreateSqlTable();
      var sqlTable2 = SqlStatementModelObjectMother.CreateSqlTable();
      var selectProjection = new SqlTableReferenceExpression (sqlTable1);
      var dataInfo = new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (selectProjection.Type), selectProjection);

      var sqlStatementBuilder = new SqlStatementBuilder
                                {
                                    SqlTables = { sqlTable1, sqlTable2 },
                                    SelectProjection = selectProjection,
                                    DataInfo = dataInfo
                                };


      var resultOperator = new DefaultIfEmptyResultOperator (null);

      _handler.HandleResultOperator (resultOperator, sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (sqlStatementBuilder.SqlTables[0].JoinSemantics, Is.EqualTo (JoinSemantics.Left));
      
      var tableInfo = sqlStatementBuilder.SqlTables[0].TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (_context.GetExpressionMapping (((StreamedSequenceInfo) sqlStatementBuilder.DataInfo).ItemExpression), Is.Not.Null);
    }

    [Test]
    public void HandleResultOperator_WithSetOperation_MovesStatementToSubStatement_AsLeftJoinedTable ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable();
      var selectProjection = new SqlTableReferenceExpression (sqlTable);
      var dataInfo = new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (selectProjection.Type), selectProjection);

      var sqlStatementBuilder = new SqlStatementBuilder
                                {
                                    SqlTables = { sqlTable },
                                    SelectProjection = selectProjection,
                                    DataInfo = dataInfo,
                                    SetOperationCombinedStatements = { SqlStatementModelObjectMother.CreateSetOperationCombinedStatement() }
                                };
      
      var resultOperator = new DefaultIfEmptyResultOperator (null);

      _handler.HandleResultOperator (resultOperator, sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (sqlStatementBuilder.SqlTables[0].JoinSemantics, Is.EqualTo (JoinSemantics.Left));
      
      var tableInfo = sqlStatementBuilder.SqlTables[0].TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (_context.GetExpressionMapping (((StreamedSequenceInfo) sqlStatementBuilder.DataInfo).ItemExpression), Is.Not.Null);
    }
  }
}