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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class SqlStatementTest
  {
    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Parameter 'whereCondition' has type 'System.Int32' when type 'System.Boolean' was expected.\r\nParameter name: whereCondition")]
    public void WhereCondition_ChecksType ()
    {
      new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          Expression.Constant (1),
          new SqlTable[0],
          Expression.Constant (1),
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);
    }

    [Test]
    public void WhereCondition_CanBeSetToNull ()
    {
      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          Expression.Constant (1),
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement.WhereCondition, Is.Null);
    }

    [Test]
    public void Equals_EqualStatementsWithAllMembers ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var isDistinctQuery = BooleanObjectMother.GetRandomBoolean();
      var selectProjection = Expression.Constant (1);
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Asc);
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant ("top");
      var rowNumberSelector = Expression.Constant ("selector");
      var currentRowNumberOffset = Expression.Constant (1);
      var groupByExpression = Expression.Constant ("group");
      var setOperationCombinedStatement = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new[] { sqlTable },
          whereCondition,
          groupByExpression,
          new[] { ordering },
          topExpression,
          isDistinctQuery,
          rowNumberSelector,
          currentRowNumberOffset,
          new[] { setOperationCombinedStatement });

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new[] { sqlTable },
          whereCondition,
          groupByExpression,
          new[] { ordering },
          topExpression,
          isDistinctQuery,
          rowNumberSelector,
          currentRowNumberOffset,
          new[] { setOperationCombinedStatement });

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.True);
    }

    [Test]
    public void Equals_EqualStatementsWithMandatoryMembers ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.True);
    }

    [Test]
    public void Equals_DifferentDataInfo ()
    {
      var dataInfo1 = new TestStreamedValueInfo (typeof (int));
      var dataInfo2 = new TestStreamedValueInfo (typeof (char));
      var selectProjection = Expression.Constant (1);

      var sqlStatement1 = new SqlStatement (
          dataInfo1,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo2,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentSelectProjection ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection1 = Expression.Constant (1);
      var selectProjection2 = Expression.Constant (2);

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection1,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection2,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentSqlTables ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var sqlTable1 = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
      var sqlTable2 = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "k"), JoinSemantics.Inner);

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new[] { sqlTable1 },
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new[] { sqlTable2 },
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentOrderings ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var ordering1 = new Ordering (Expression.Constant ("ordering1"), OrderingDirection.Asc);
      var ordering2 = new Ordering (Expression.Constant ("ordering2"), OrderingDirection.Desc);

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new[] { ordering1 },
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new[] { ordering2 },
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentWhereCondition ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var whereCondition1 = Expression.Constant (true);
      var whereCondition2 = Expression.Constant (false);

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          whereCondition1,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          whereCondition2,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentTopExpression ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var topExpression1 = Expression.Constant ("top1");
      var topExpression2 = Expression.Constant ("top2");

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          topExpression1,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          topExpression2,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void DifferentAggregationModifier ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var selectProjectionWithCountAggregation = new AggregationExpression (typeof (int), selectProjection, AggregationModifier.Count);

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjectionWithCountAggregation,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentDistinctCondition ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var isDistinctQuery = BooleanObjectMother.GetRandomBoolean();

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          isDistinctQuery,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          !isDistinctQuery,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentRowNumberSelector ()
    {
      var rowNumberSelector1 = Expression.Constant ("selector1");
      var rowNumberSelector2 = Expression.Constant ("selector2");
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          rowNumberSelector1,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          rowNumberSelector2,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentCurrentRowNumberOffset ()
    {
      var currentRowNumberOffset1 = Expression.Constant (1);
      var currentRowNumberOffset2 = Expression.Constant (2);
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          currentRowNumberOffset1,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          currentRowNumberOffset2,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentGroupByExpression ()
    {
      var groupByExpression1 = Expression.Constant ("group1");
      var groupByExpression2 = Expression.Constant ("group2");
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          groupByExpression1,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          groupByExpression2,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new SetOperationCombinedStatement[0]);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentSetOperationCombinedStatements ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var setOperationCombinedStatement1 = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();
      var setOperationCombinedStatement2 = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new[] { setOperationCombinedStatement1 });

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new SqlTable[0],
          null,
          null,
          new Ordering[0],
          null,
          false,
          null,
          null,
          new[] { setOperationCombinedStatement2 });

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_ObjectIsNull ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();

      Assert.That (sqlStatement.Equals (null), Is.False);
    }

    [Test]
    public void Equals_ObjectIsNotASqlStatement ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();

      Assert.That (sqlStatement.Equals (new object()), Is.False);
    }

    [Test]
    public void Equals_AssertPropertyCount ()
    {
      Assert.That (typeof (SqlStatement).GetProperties().Count(), Is.EqualTo (11), "The implementation of Equals and GetHashCode has to be adapted.");
    }

    [Test]
    public void GetHashcode_EqualSqlStatementsWithAllMembers ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var isDistinctQuery = BooleanObjectMother.GetRandomBoolean();
      var selectProjection = Expression.Constant (1);
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Asc);
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant ("top");
      var rowNumberSelector = Expression.Constant ("selector1");
      var currentRowNumberOffset = Expression.Constant (1);
      var groupByExpression = Expression.Constant ("group");
      var setOperationCombinedStatement = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();

      var sqlStatement1 = new SqlStatement (
          dataInfo,
          selectProjection,
          new[] { sqlTable },
          whereCondition,
          groupByExpression,
          new[] { ordering },
          topExpression,
          isDistinctQuery,
          rowNumberSelector,
          currentRowNumberOffset,
          new[] { setOperationCombinedStatement });

      var sqlStatement2 = new SqlStatement (
          dataInfo,
          selectProjection,
          new[] { sqlTable },
          whereCondition,
          groupByExpression,
          new[] { ordering },
          topExpression,
          isDistinctQuery,
          rowNumberSelector,
          currentRowNumberOffset,
          new[] { setOperationCombinedStatement });

      Assert.That (sqlStatement1.GetHashCode(), Is.EqualTo (sqlStatement2.GetHashCode()));
    }

    [Test]
    public void CreateExpression_WithSqlTables ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement());
      var sqlStatement = sqlStatementBuilder.GetSqlStatement();

      var result = sqlStatement.CreateExpression();

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void CreateExpression_HasAggregationModifier ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement());
      sqlStatementBuilder.SqlTables.Clear();
      var selectProjection = new AggregationExpression (typeof (double), sqlStatementBuilder.SelectProjection, AggregationModifier.Max);
      sqlStatementBuilder.SelectProjection = selectProjection;
      var sqlStatement = sqlStatementBuilder.GetSqlStatement();

      var result = sqlStatement.CreateExpression();

      Assert.That (result, Is.SameAs (selectProjection));
    }

    [Test]
    public void CreateExpression_IsDistinctQuery ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement());
      sqlStatementBuilder.SqlTables.Clear();
      sqlStatementBuilder.IsDistinctQuery = true;
      var sqlStatement = sqlStatementBuilder.GetSqlStatement();

      var result = sqlStatement.CreateExpression();

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void CreateExpression_WithNoSqlTablesAndNoDistinctQueryAndNoAggregationModifier ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement());
      sqlStatementBuilder.SqlTables.Clear();
      var sqlStatement = sqlStatementBuilder.GetSqlStatement();

      var result = sqlStatement.CreateExpression();

      Assert.That (result, Is.SameAs (sqlStatement.SelectProjection));
    }

    [Test]
    public void ToString_AllProperties ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var sqlTable1 = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
      var sqlTable2 = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "k"), JoinSemantics.Left);
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Asc);
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant (10);
      var groupExpression = Expression.Constant ("group");
      var setOperationCombinedStatement = SqlStatementModelObjectMother.CreateSetOperationCombinedStatement();

      var sqlStatement = new SqlStatement (
          dataInfo,
          selectProjection,
          new[] { sqlTable1, sqlTable2 },
          whereCondition,
          groupExpression,
          new[] { ordering },
          topExpression,
          true,
          null,
          null,
          new[] { setOperationCombinedStatement });

      var result = sqlStatement.ToString();

      Assert.That (
          result,
          Is.EqualTo (
              "SELECT DISTINCT TOP (10) 1 FROM [CookTable] [c], [KitchenTable] [k] WHERE True GROUP BY \"group\" ORDER BY \"ordering\" ASC UNION ("
              + setOperationCombinedStatement.SqlStatement + ")"));
    }
  }
}