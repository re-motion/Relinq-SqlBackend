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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class SqlStatementBuilderTest
  {
    [Test]
    public void Initialization_WithExistingSqlStatement ()
    {
      var isDistinctQuery = BooleanObjectMother.GetRandomBoolean();
      var selectProjection = Expression.Constant ("select");
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant ("top");
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable();
      var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Desc);
      var rowNumberSelector = Expression.Constant("selector");
      var currentRowNumberOffset = Expression.Constant(1);
      var groupExpression = Expression.Constant ("group");
      var setOperationCombinedStatement = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();

      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          selectProjection,
          new[] { appendedTable },
          whereCondition,
          groupExpression,
          new[] { ordering },
          topExpression,
          isDistinctQuery,
          rowNumberSelector,
          currentRowNumberOffset,
          new[] { setOperationCombinedStatement });

      var testedBuilder = new SqlStatementBuilder (sqlStatement);

      Assert.That (testedBuilder.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (testedBuilder.TopExpression, Is.SameAs (topExpression));
      Assert.That (testedBuilder.SqlTables, Is.EqualTo (new[] { appendedTable }));
      Assert.That (testedBuilder.Orderings, Is.EqualTo (new[] { ordering }));
      Assert.That (testedBuilder.WhereCondition, Is.EqualTo (whereCondition));
      Assert.That (testedBuilder.IsDistinctQuery, Is.EqualTo (isDistinctQuery));
      Assert.That (testedBuilder.DataInfo, Is.SameAs (sqlStatement.DataInfo));
      Assert.That (testedBuilder.RowNumberSelector, Is.SameAs (sqlStatement.RowNumberSelector));
      Assert.That (testedBuilder.CurrentRowNumberOffset, Is.SameAs (currentRowNumberOffset));
      Assert.That (testedBuilder.GroupByExpression, Is.SameAs (groupExpression));
      Assert.That (testedBuilder.SetOperationCombinedStatements, Is.EqualTo (new[] { setOperationCombinedStatement }));
    }

    [Test]
    public void Initialization_WithDefaultValues ()
    {
      var statementBuilder = new SqlStatementBuilder();

      Assert.That (statementBuilder.DataInfo, Is.Null);
      Assert.That (statementBuilder.TopExpression, Is.Null);
      Assert.That (statementBuilder.IsDistinctQuery, Is.False);
      Assert.That (statementBuilder.SelectProjection, Is.Null);
      Assert.That (statementBuilder.SqlTables, Is.Empty);
      Assert.That (statementBuilder.Orderings, Is.Empty);
      Assert.That (statementBuilder.WhereCondition, Is.Null);
      Assert.That (statementBuilder.RowNumberSelector, Is.Null);
      Assert.That (statementBuilder.CurrentRowNumberOffset, Is.Null);
      Assert.That (statementBuilder.GroupByExpression, Is.Null);
      Assert.That (statementBuilder.SetOperationCombinedStatements, Is.Empty);
    }

    [Test]
    public void GetSqlStatement ()
    {
      var statementBuilder = new SqlStatementBuilder ();
      statementBuilder.DataInfo = new TestStreamedValueInfo (typeof (int));

      var constantExpression = Expression.Constant ("test");
      statementBuilder.SelectProjection = constantExpression;
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable();
      statementBuilder.SqlTables.Add (appendedTable);

      var result = statementBuilder.GetSqlStatement();

      Assert.That (result.SelectProjection, Is.SameAs (constantExpression));
      Assert.That (result.SqlTables.Count, Is.EqualTo (1));
      Assert.That (result.SqlTables[0], Is.SameAs (appendedTable));
    }

    [Test]
    public void GetSqlStatement_NoDataInfoSet ()
    {
      var statementBuilder = new SqlStatementBuilder();
      statementBuilder.DataInfo = null;
      statementBuilder.SelectProjection = ExpressionHelper.CreateExpression();
      Assert.That (
          () => statementBuilder.GetSqlStatement(),
          Throws.InvalidOperationException.With.Message.EqualTo ("A DataInfo must be set before the SqlStatement can be retrieved."));
    }

    [Test]
    public void GetSqlStatement_NoSelectProjection ()
    {
      var statementBuilder = new SqlStatementBuilder();
      statementBuilder.DataInfo = new TestStreamedValueInfo (typeof (int));
      statementBuilder.SelectProjection = null;
      Assert.That (
          () => statementBuilder.GetSqlStatement(),
          Throws.InvalidOperationException.With.Message.EqualTo ("A SelectProjection must be set before the SqlStatement can be retrieved."));
    }

    [Test]
    public void GetSqlStatement_CheckProperties ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (Cook));
      var topExpression = ExpressionHelper.CreateExpression();
      var isDistinctQuery = BooleanObjectMother.GetRandomBoolean();
      var selectProjection = new AggregationExpression (typeof (int), Expression.Constant (1), AggregationModifier.Min);
      var whereCondition = Expression.Constant (true);
      var appendedTable = SqlStatementModelObjectMother.CreateSqlAppendedTable();
      var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Desc);
      var rowNumberSelector = Expression.Constant ("selector");
      var currentRowNumberOffset = Expression.Constant (1);
      var groupExpression = Expression.Constant ("group");
      var setOperationCombinedStatement = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();
      
      var statementBuilder = new SqlStatementBuilder
                             {
                                 DataInfo = dataInfo,
                                 TopExpression = topExpression,
                                 IsDistinctQuery = isDistinctQuery,
                                 SelectProjection = selectProjection,
                                 SqlTables = { appendedTable },
                                 WhereCondition = whereCondition,
                                 RowNumberSelector = rowNumberSelector,
                                 CurrentRowNumberOffset = currentRowNumberOffset,
                                 GroupByExpression = groupExpression,
                                 Orderings = { ordering },
                                 SetOperationCombinedStatements = { setOperationCombinedStatement }
                             };

      var sqlStatement = statementBuilder.GetSqlStatement();

      Assert.That (sqlStatement.DataInfo, Is.SameAs (dataInfo));
      Assert.That (sqlStatement.TopExpression, Is.SameAs (topExpression));
      Assert.That (sqlStatement.IsDistinctQuery, Is.EqualTo (isDistinctQuery));
      Assert.That (sqlStatement.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (sqlStatement.SqlTables, Is.EqualTo (new[] { appendedTable }));
      Assert.That (sqlStatement.Orderings, Is.EqualTo (new[] { ordering }));
      Assert.That (sqlStatement.WhereCondition, Is.SameAs (whereCondition));
      Assert.That (sqlStatement.RowNumberSelector, Is.SameAs (rowNumberSelector));
      Assert.That (sqlStatement.CurrentRowNumberOffset, Is.SameAs (currentRowNumberOffset));
      Assert.That (sqlStatement.GroupByExpression, Is.SameAs (groupExpression));
      Assert.That (sqlStatement.SetOperationCombinedStatements, Is.EqualTo (new[] { setOperationCombinedStatement }));
    }

    [Test]
    public void GetStatementAndResetBuilder ()
    {
      var statementBuilder = new SqlStatementBuilder
                             {
                                 DataInfo = new TestStreamedValueInfo (typeof (Cook)),
                                 TopExpression =  ExpressionHelper.CreateExpression(),
                                 IsDistinctQuery = true,
                                 SelectProjection = new AggregationExpression(typeof(int), Expression.Constant (1),AggregationModifier.Min),
                                 SqlTables = { SqlStatementModelObjectMother.CreateSqlAppendedTable() },
                                 WhereCondition = Expression.Constant (true),
                                 RowNumberSelector = Expression.Constant ("selector"),
                                 CurrentRowNumberOffset = Expression.Constant (1),
                                 GroupByExpression = Expression.Constant ("group"),
                                 Orderings = { new Ordering (Expression.Constant ("order"), OrderingDirection.Desc) },
                                 SetOperationCombinedStatements = { SqlStatementModelObjectMother.CreateSetOperationCombinedStatement() }
                             };
      var sqlStatement = statementBuilder.GetSqlStatement();

      var result = statementBuilder.GetStatementAndResetBuilder();

      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (result, Is.EqualTo (sqlStatement));

      Assert.That (statementBuilder.DataInfo, Is.Null);
      Assert.That (statementBuilder.TopExpression, Is.Null);
      Assert.That (statementBuilder.IsDistinctQuery, Is.False);
      Assert.That (statementBuilder.SelectProjection, Is.Null);
      Assert.That (statementBuilder.SqlTables, Is.Empty);
      Assert.That (statementBuilder.Orderings, Is.Empty);
      Assert.That (statementBuilder.WhereCondition, Is.Null);
      Assert.That (statementBuilder.RowNumberSelector, Is.Null);
      Assert.That (statementBuilder.CurrentRowNumberOffset, Is.Null);
      Assert.That (statementBuilder.GroupByExpression, Is.Null);
      Assert.That (statementBuilder.SetOperationCombinedStatements, Is.Empty);
    }

    [Test]
    public void AddWhereCondition_SingleWhereCondition ()
    {
      var statementBuilder = new SqlStatementBuilder ();

      var expression = Expression.Constant ("whereTest");
      statementBuilder.AddWhereCondition (expression);

      Assert.That (statementBuilder.WhereCondition, Is.EqualTo (expression));
    }

    [Test]
    public void AddWhereCondition_MultipleWhereCondition ()
    {
      var statementBuilder = new SqlStatementBuilder ();

      var expression1 = Expression.Constant (true);
      statementBuilder.AddWhereCondition (expression1);
      var expression2 = Expression.Constant (false);
      statementBuilder.AddWhereCondition (expression2);

      Assert.That (((BinaryExpression) statementBuilder.WhereCondition).Left, Is.EqualTo (expression1));
      Assert.That (((BinaryExpression) statementBuilder.WhereCondition).Right, Is.EqualTo (expression2));
      Assert.That (statementBuilder.WhereCondition.NodeType, Is.EqualTo (ExpressionType.AndAlso));
    }

    [Test]
    public void RecalculateDataInfo_StreamedSequenceInfo ()
    {
      var statementBuilder = new SqlStatementBuilder ();

      var previousSelectProjection = Expression.Constant (typeof (Restaurant));
      statementBuilder.DataInfo = new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (typeof (Restaurant)), Expression.Constant (new Restaurant()));
      statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);

      statementBuilder.RecalculateDataInfo(previousSelectProjection);

      Assert.That (statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) statementBuilder.DataInfo).DataType, Is.EqualTo(typeof (IQueryable<>).MakeGenericType(typeof(string))));
    }

    [Test]
    public void RecalculateDataInfo_StreamedSingleValueInfo ()
    {
      var statementBuilder = new SqlStatementBuilder ();

      var previousSelectProjection = Expression.Constant ("test");
      statementBuilder.DataInfo = new StreamedSingleValueInfo (typeof(string), false);
      statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (int), "c", "Length", false);

      statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
      Assert.That (((StreamedSingleValueInfo) statementBuilder.DataInfo).DataType, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void RecalculateDataInfo_StreamedScalarValueInfo_ReturnsSameDataInfo ()
    {
      var statementBuilder = new SqlStatementBuilder ();

      var previousSelectProjection = Expression.Constant ("test");
      statementBuilder.DataInfo = new StreamedScalarValueInfo(typeof(string));
      statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (int), "c", "Length", false);

      statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (((StreamedScalarValueInfo) statementBuilder.DataInfo).DataType, Is.EqualTo (typeof (string)));
    }

    [Test]
    public void RecalculateDataInfo_WithOtherDataInfo_ReturnsSameDataInfo ()
    {
      var statementBuilder = new SqlStatementBuilder ();

      var previousSelectProjection = Expression.Constant ("test");
      var originalDataInfo = new TestStreamedValueInfo(typeof(string));
      statementBuilder.DataInfo = originalDataInfo;
      statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (int), "c", "Length", false);

      statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (statementBuilder.DataInfo, Is.SameAs (originalDataInfo));
    }

    [Test]
    public void RecalculateDataInfo_UnchangedProjectionType_SameDataInfo ()
    {
      var statementBuilder = new SqlStatementBuilder ();

      var previousSelectProjection = Expression.Constant ("test");
      var originalDataInfo = new StreamedSingleValueInfo (typeof(string), false);
      statementBuilder.DataInfo = originalDataInfo;
      statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (string), "c", "Length", false);

      statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (statementBuilder.DataInfo, Is.SameAs (originalDataInfo));
    }

    [Test]
    public void ToString_AllProperties ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var appendedTable1 =
          SqlStatementModelObjectMother.CreateSqlAppendedTable (
              new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner),
              JoinSemantics.Inner);
      var appendedTable2 =
          SqlStatementModelObjectMother.CreateSqlAppendedTable (
              new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "k"), JoinSemantics.Left),
              JoinSemantics.Left);
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Asc);
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant (10);
      var groupExpression = Expression.Constant ("group");
      var setOperationCombinedStatement = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();

      var builder = new SqlStatementBuilder
                    {
                        DataInfo = dataInfo,
                        SelectProjection = selectProjection,
                        SqlTables = { appendedTable1, appendedTable2 },
                        Orderings = { ordering },
                        WhereCondition = whereCondition,
                        TopExpression = topExpression,
                        IsDistinctQuery = true,
                        GroupByExpression = groupExpression,
                        SetOperationCombinedStatements = { setOperationCombinedStatement }
                    };

      var result = builder.ToString();

      Assert.That (
          result,
          Is.EqualTo (
              "SELECT DISTINCT TOP (10) 1 FROM CROSS APPLY [CookTable] [c] OUTER APPLY [KitchenTable] [k] WHERE True GROUP BY \"group\" "
              + "ORDER BY \"ordering\" ASC "
              + "UNION (" + setOperationCombinedStatement.SqlStatement + ")"));
    }

    [Test]
    public void ToString_SingleTable ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var appendedTable =
          SqlStatementModelObjectMother.CreateSqlAppendedTable (
              new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner),
              JoinSemantics.Inner);

      var builder = new SqlStatementBuilder
      {
        DataInfo = dataInfo,
        SelectProjection = selectProjection,
        SqlTables = { appendedTable },
      };

      var result = builder.ToString ();

      Assert.That (result, Is.EqualTo ("SELECT 1 FROM CROSS APPLY [CookTable] [c]"));
    }

    [Test]
    public void ToString_NoProperties ()
    {
      var builder = new SqlStatementBuilder();

      var result = builder.ToString ();

      Assert.That (result, Is.EqualTo ("SELECT "));
    }
   
  }
}