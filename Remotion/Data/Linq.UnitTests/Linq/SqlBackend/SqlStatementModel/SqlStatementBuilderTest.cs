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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlStatementBuilderTest
  {
    private SqlStatementBuilder _statementBuilder;

    [SetUp]
    public void SetUp ()
    {
      _statementBuilder = new SqlStatementBuilder();
      _statementBuilder.DataInfo = new TestStreamedValueInfo (typeof (int));
    }

    [Test]
    public void Initialization_WithExistingSqlStatement ()
    {
      var selectProjection = Expression.Constant ("select");
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant ("top");
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Desc);

      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          selectProjection,
          new [] { sqlTable },
          new [] { ordering },
          whereCondition,
          topExpression,
          false);

      var testedBuilder = new SqlStatementBuilder (sqlStatement);

      Assert.That (testedBuilder.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (testedBuilder.TopExpression, Is.SameAs (topExpression));
      Assert.That (testedBuilder.SqlTables[0], Is.SameAs (sqlTable));
      Assert.That (testedBuilder.Orderings[0], Is.SameAs (ordering));
      Assert.That (testedBuilder.WhereCondition, Is.EqualTo (whereCondition));
      Assert.That (testedBuilder.IsDistinctQuery, Is.False);
      Assert.That (testedBuilder.DataInfo, Is.SameAs (sqlStatement.DataInfo));
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
    public void GetSqlStatement_CheckProperties ()
    {
      var selectProjection = new AggregationExpression(Expression.Constant ("select"),AggregationModifier.Min);
      var whereCondition = Expression.Constant (true);
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Desc);

      var statementBuilder = new SqlStatementBuilder
                             {
                                 DataInfo = new TestStreamedValueInfo (typeof (Cook)),
                                 SelectProjection = selectProjection,
                                 WhereCondition = whereCondition,
                                 TopExpression = null,
                                 IsDistinctQuery = false
                             };
      statementBuilder.SqlTables.Add (sqlTable);
      statementBuilder.Orderings.Add (ordering);

      var sqlStatement = statementBuilder.GetSqlStatement();

      Assert.That (sqlStatement.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (sqlStatement.TopExpression, Is.Null);
      Assert.That (sqlStatement.SqlTables[0], Is.SameAs (sqlTable));
      Assert.That (sqlStatement.Orderings[0], Is.SameAs (ordering));
      Assert.That (sqlStatement.WhereCondition, Is.EqualTo (whereCondition));
      Assert.That (sqlStatement.IsDistinctQuery, Is.False);
      Assert.That (((AggregationExpression) sqlStatement.SelectProjection).AggregationModifier, Is.EqualTo(AggregationModifier.Min));
      Assert.That (sqlStatement.DataInfo, Is.TypeOf (typeof (TestStreamedValueInfo)));
    }

    [Test]
    public void GetStatementAndResetBuilder ()
    {
      var selectProjection = new AggregationExpression(Expression.Constant ("select"),AggregationModifier.Count);
      var originalBuilder = new SqlStatementBuilder
                            {
                                DataInfo = new TestStreamedValueInfo (typeof (Cook)),
                                SelectProjection = selectProjection,
                                IsDistinctQuery = true
                            };
      var sqlStatement = originalBuilder.GetSqlStatement();

      var result = originalBuilder.GetStatementAndResetBuilder();

      Assert.That (result, Is.Not.SameAs (sqlStatement));
    }

    [Test]
    public void RecalculateDataInfo_StreamedSequenceInfo ()
    {
      var previousSelectProjection = Expression.Constant (typeof (Restaurant));
      _statementBuilder.SelectProjection = new SqlColumnExpression(typeof(string), "c", "Name", false);
      _statementBuilder.DataInfo = new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (typeof (Restaurant)), Expression.Constant (new Restaurant()));
      
      _statementBuilder.RecalculateDataInfo(previousSelectProjection);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) _statementBuilder.DataInfo).DataType, Is.EqualTo(typeof (IQueryable<>).MakeGenericType(typeof(string))));
    }

    [Test]
    public void RecalculateDataInfo_StreamedSingleValueInfo ()
    {
      var previousSelectProjection = Expression.Constant ("test");
      _statementBuilder.SelectProjection = new SqlColumnExpression (typeof (int), "c", "Length", false);
      _statementBuilder.DataInfo = new StreamedSingleValueInfo (typeof(string), false);

      _statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSingleValueInfo)));
      Assert.That (((StreamedSingleValueInfo) _statementBuilder.DataInfo).DataType, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void RecalculateDataInfo_SameDataInfo ()
    {
      var previousSelectProjection = Expression.Constant ("test");
      _statementBuilder.SelectProjection = new SqlColumnExpression (typeof (int), "c", "Length", false);
     
      _statementBuilder.RecalculateDataInfo (previousSelectProjection);

      Assert.That (_statementBuilder.DataInfo, Is.TypeOf (typeof (TestStreamedValueInfo)));
      Assert.That (((TestStreamedValueInfo) _statementBuilder.DataInfo).DataType, Is.EqualTo(typeof (int)));
    }
  }
}