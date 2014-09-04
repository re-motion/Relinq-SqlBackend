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
    private SqlStatementBuilder _statementBuilder;

    [SetUp]
    public void SetUp ()
    {
      _statementBuilder = new SqlStatementBuilder { DataInfo = new TestStreamedValueInfo (typeof (int)) };
    }

    [Test]
    public void Initialization_WithExistingSqlStatement ()
    {
      var isDistinctQuery = BooleanObjectMother.GetRandomBoolean();
      var selectProjection = Expression.Constant ("select");
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant ("top");
      var alwaysUseOuterJoinSemantics = BooleanObjectMother.GetRandomBoolean();
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
      var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Desc);
      var rowNumberSelector = Expression.Constant("selector");
      var currentRowNumberOffset = Expression.Constant(1);
      var groupExpression = Expression.Constant ("group");

      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          selectProjection,
          alwaysUseOuterJoinSemantics,
          new[] { sqlTable },
          whereCondition,
          groupExpression,
          new[] { ordering },
          topExpression,
          isDistinctQuery,
          rowNumberSelector,
          currentRowNumberOffset);

      var testedBuilder = new SqlStatementBuilder (sqlStatement);

      Assert.That (testedBuilder.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (testedBuilder.TopExpression, Is.SameAs (topExpression));
      Assert.That (testedBuilder.AlwaysUseOuterJoinSemantics, Is.EqualTo (alwaysUseOuterJoinSemantics));
      Assert.That (testedBuilder.SqlTables[0], Is.SameAs (sqlTable));
      Assert.That (testedBuilder.Orderings[0], Is.SameAs (ordering));
      Assert.That (testedBuilder.WhereCondition, Is.EqualTo (whereCondition));
      Assert.That (testedBuilder.IsDistinctQuery, Is.EqualTo (isDistinctQuery));
      Assert.That (testedBuilder.DataInfo, Is.SameAs (sqlStatement.DataInfo));
      Assert.That (testedBuilder.RowNumberSelector, Is.SameAs (sqlStatement.RowNumberSelector));
      Assert.That (testedBuilder.CurrentRowNumberOffset, Is.SameAs (currentRowNumberOffset));
      Assert.That (testedBuilder.GroupByExpression, Is.SameAs (groupExpression));
    }

    [Test]
    public void Initialization_WithDefaultValues ()
    {
      var statementBuilder = new SqlStatementBuilder();

      Assert.That (statementBuilder.DataInfo, Is.Null);
      Assert.That (statementBuilder.TopExpression, Is.Null);
      Assert.That (statementBuilder.IsDistinctQuery, Is.False);
      Assert.That (statementBuilder.SelectProjection, Is.Null);
      Assert.That (statementBuilder.AlwaysUseOuterJoinSemantics, Is.False);
      Assert.That (statementBuilder.SqlTables, Is.Empty);
      Assert.That (statementBuilder.Orderings, Is.Empty);
      Assert.That (statementBuilder.WhereCondition, Is.Null);
      Assert.That (statementBuilder.RowNumberSelector, Is.Null);
      Assert.That (statementBuilder.CurrentRowNumberOffset, Is.Null);
      Assert.That (statementBuilder.GroupByExpression, Is.Null);

    }

    [Test]
    public void GetSqlStatement ()
    {
      var constantExpression = Expression.Constant ("test");
      _statementBuilder.SelectProjection = constantExpression;
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable();
      _statementBuilder.SqlTables.Add (sqlTable);

      var result = _statementBuilder.GetSqlStatement();

      Assert.That (result.SelectProjection, Is.SameAs (constantExpression));
      Assert.That (result.SqlTables.Count, Is.EqualTo (1));
      Assert.That (result.SqlTables[0], Is.SameAs (sqlTable));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void GetSqlStatement_NoSelectProjection ()
    {
      _statementBuilder.SelectProjection = null;
      _statementBuilder.GetSqlStatement();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "A DataInfo must be set before the SqlStatement can be retrieved.")]
    public void GetSqlStatement_NoDataInfoSet ()
    {
      _statementBuilder.DataInfo = null;
      _statementBuilder.GetSqlStatement();
    }

    [Test]
    public void GetSqlStatement_CheckProperties ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (Cook));
      var topExpression = ExpressionHelper.CreateExpression();
      var isDistinctQuery = BooleanObjectMother.GetRandomBoolean();
      var selectProjection = new AggregationExpression (typeof (int), Expression.Constant (1), AggregationModifier.Min);
      var whereCondition = Expression.Constant (true);
      var alwaysUseOuterJoinSemantics = BooleanObjectMother.GetRandomBoolean();
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
      var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Desc);
      var rowNumberSelector = Expression.Constant ("selector");
      var currentRowNumberOffset = Expression.Constant (1);
      var groupExpression = Expression.Constant ("group");

      var statementBuilder = new SqlStatementBuilder
                             {
                                 DataInfo = dataInfo,
                                 TopExpression = topExpression,
                                 IsDistinctQuery = isDistinctQuery,
                                 SelectProjection = selectProjection,
                                 SqlTables = { sqlTable },
                                 AlwaysUseOuterJoinSemantics = alwaysUseOuterJoinSemantics,
                                 WhereCondition = whereCondition,
                                 RowNumberSelector = rowNumberSelector,
                                 CurrentRowNumberOffset = currentRowNumberOffset,
                                 GroupByExpression = groupExpression,
                                 Orderings = { ordering }
                             };

      var sqlStatement = statementBuilder.GetSqlStatement();

      Assert.That (sqlStatement.DataInfo, Is.SameAs (dataInfo));
      Assert.That (sqlStatement.TopExpression, Is.SameAs (topExpression));
      Assert.That (sqlStatement.IsDistinctQuery, Is.EqualTo (isDistinctQuery));
      Assert.That (sqlStatement.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (sqlStatement.AlwaysUseOuterJoinSemantics, Is.EqualTo (alwaysUseOuterJoinSemantics));
      Assert.That (sqlStatement.SqlTables[0], Is.SameAs (sqlTable));
      Assert.That (sqlStatement.Orderings[0], Is.SameAs (ordering));
      Assert.That (sqlStatement.WhereCondition, Is.SameAs (whereCondition));
      Assert.That (sqlStatement.RowNumberSelector, Is.SameAs (rowNumberSelector));
      Assert.That (sqlStatement.CurrentRowNumberOffset, Is.SameAs (currentRowNumberOffset));
      Assert.That (sqlStatement.GroupByExpression, Is.SameAs (groupExpression));
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
                                 SqlTables = { new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner) },
                                 AlwaysUseOuterJoinSemantics = true,
                                 WhereCondition = Expression.Constant (true),
                                 RowNumberSelector = Expression.Constant ("selector"),
                                 CurrentRowNumberOffset = Expression.Constant (1),
                                 GroupByExpression = Expression.Constant ("group"),
                                 Orderings = { new Ordering (Expression.Constant ("order"), OrderingDirection.Desc) }
                             };
      var sqlStatement = statementBuilder.GetSqlStatement();

      var result = statementBuilder.GetStatementAndResetBuilder();

      Assert.That (result, Is.Not.SameAs (sqlStatement));
      Assert.That (result, Is.EqualTo (sqlStatement));

      Assert.That (statementBuilder.DataInfo, Is.Null);
      Assert.That (statementBuilder.TopExpression, Is.Null);
      Assert.That (statementBuilder.IsDistinctQuery, Is.False);
      Assert.That (statementBuilder.SelectProjection, Is.Null);
      Assert.That (statementBuilder.AlwaysUseOuterJoinSemantics, Is.False);
      Assert.That (statementBuilder.SqlTables, Is.Empty);
      Assert.That (statementBuilder.Orderings, Is.Empty);
      Assert.That (statementBuilder.WhereCondition, Is.Null);
      Assert.That (statementBuilder.RowNumberSelector, Is.Null);
      Assert.That (statementBuilder.CurrentRowNumberOffset, Is.Null);
      Assert.That (statementBuilder.GroupByExpression, Is.Null);
    }

    [Test]
    public void AddWhereCondition_SingleWhereCondition ()
    {
      var expression = Expression.Constant ("whereTest");
      _statementBuilder.AddWhereCondition (expression);

      Assert.That (_statementBuilder.WhereCondition, Is.EqualTo (expression));
    }

    [Test]
    public void AddWhereCondition_MultipleWhereCondition ()
    {
      var expression1 = Expression.Constant (true);
      _statementBuilder.AddWhereCondition (expression1);
      var expression2 = Expression.Constant (false);
      _statementBuilder.AddWhereCondition (expression2);

      Assert.That (((BinaryExpression) _statementBuilder.WhereCondition).Left, Is.EqualTo (expression1));
      Assert.That (((BinaryExpression) _statementBuilder.WhereCondition).Right, Is.EqualTo (expression2));
      Assert.That (_statementBuilder.WhereCondition.NodeType, Is.EqualTo (ExpressionType.AndAlso));
    }

    [Test]
    public void RecalculateDataInfo_StreamedSequenceInfo ()
    {
      var previousSelectProjection = Expression.Constant (typeof (Restaurant));
      _statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false);
      _statementBuilder.DataInfo = new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (typeof (Restaurant)), Expression.Constant (new Restaurant()));
      
      _statementBuilder.RecalculateDataInfo(previousSelectProjection);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) _statementBuilder.DataInfo).DataType, Is.EqualTo(typeof (IQueryable<>).MakeGenericType(typeof(string))));
    }

    [Test]
    public void RecalculateDataInfo_StreamedSingleValueInfo ()
    {
      var previousSelectProjection = Expression.Constant ("test");
      _statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (int), "c", "Length", false);
      _statementBuilder.DataInfo = new StreamedSingleValueInfo (typeof(string), false);

      _statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
      Assert.That (((StreamedSingleValueInfo) _statementBuilder.DataInfo).DataType, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void RecalculateDataInfo_StreamedScalarValueInfo_SameDataInfo ()
    {
      var previousSelectProjection = Expression.Constant ("test");
      _statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (int), "c", "Length", false);
      _statementBuilder.DataInfo = new StreamedScalarValueInfo(typeof(string));

      _statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (((StreamedScalarValueInfo) _statementBuilder.DataInfo).DataType, Is.EqualTo (typeof (string)));
    }

    [Test]
    public void RecalculateDataInfo_SameDataInfo ()
    {
      var previousSelectProjection = Expression.Constant ("test");
      _statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (int), "c", "Length", false);
     
      _statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (TestStreamedValueInfo)));
      Assert.That (((TestStreamedValueInfo) _statementBuilder.DataInfo).DataType, Is.EqualTo(typeof (int)));
    }

    [Test]
    public void RecalculateDataInfo_UnchangedProjectionType_SameDataInfo ()
    {
      var previousSelectProjection = Expression.Constant ("test");
      _statementBuilder.SelectProjection = new SqlColumnDefinitionExpression (typeof (string), "c", "Length", false);

      _statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (TestStreamedValueInfo)));
      Assert.That (((TestStreamedValueInfo) _statementBuilder.DataInfo).DataType, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void ToString_AllProperties ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Asc);
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant (10);
      var groupExpression = Expression.Constant ("group");

      var builder = new SqlStatementBuilder
      {
        DataInfo = dataInfo,
        SelectProjection = selectProjection,
        SqlTables = { sqlTable },
        Orderings = { ordering },
        WhereCondition = whereCondition,
        TopExpression = topExpression,
        IsDistinctQuery = true,
        GroupByExpression = groupExpression
      };

      var result = builder.ToString ();

      Assert.That (result, Is.EqualTo ("SELECT DISTINCT TOP (10) 1 FROM [CookTable] [c] WHERE True GROUP BY \"group\" ORDER BY \"ordering\" ASC"));
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