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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlStatementTest
  {
    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void WhereCondition_ChecksType ()
    {
      new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          Expression.Constant (1),
          new SqlTable[] { },
          new Ordering[] { },
          Expression.Constant (1),
          null,
          false);
    }

    [Test]
    public void WhereCondition_CanBeSetToNull ()
    {
      var sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)), Expression.Constant (1), new SqlTable[] { }, new Ordering[] { }, null, null, false);

      Assert.That (sqlStatement.WhereCondition, Is.Null);
    }

    [Test]
    public void Equals_EqualStatementsWithAllMembers ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Asc);
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant ("top");

      var sqlStatement1 = new SqlStatement (
          dataInfo, selectProjection, new[] { sqlTable }, new[] { ordering }, whereCondition, topExpression, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new[] { sqlTable }, new[] { ordering }, whereCondition, topExpression, false);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.True);
    }

    [Test]
    public void Equals_EqualStatementsWithMandatoryMembers ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);

      var sqlStatement1 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.True);
    }

    [Test]
    public void Equals_DifferentDataInfo ()
    {
      var dataInfo1 = new TestStreamedValueInfo (typeof (int));
      var dataInfo2 = new TestStreamedValueInfo (typeof (char));
      var selectProjection = Expression.Constant (1);

      var sqlStatement1 = new SqlStatement (
          dataInfo1, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo2, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentSelectProjection ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection1 = Expression.Constant (1);
      var selectProjection2 = Expression.Constant (2);


      var sqlStatement1 = new SqlStatement (
          dataInfo, selectProjection1, new SqlTable[] { }, new Ordering[] { }, null, null, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection2, new SqlTable[] { }, new Ordering[] { }, null, null, false);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentSqlTables ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var sqlTable1 = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var sqlTable2 = new SqlTable (new ResolvedSimpleTableInfo (typeof (Kitchen), "KitchenTable", "k"));

      var sqlStatement1 = new SqlStatement (
          dataInfo, selectProjection, new[] { sqlTable1 }, new Ordering[] { }, null, null, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new[] { sqlTable2 }, new Ordering[] { }, null, null, false);

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
          dataInfo, selectProjection, new SqlTable[] { }, new[] { ordering1 }, null, null, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new[] { ordering2 }, null, null, false);

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
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, whereCondition1, null, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, whereCondition2, null, false);

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
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, topExpression1, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, topExpression2, false);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void DifferentAggregationModifier ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var selectProjectionWithCountAggregation = new AggregationExpression(selectProjection, AggregationModifier.Count);
      
      var sqlStatement1 = new SqlStatement (
          dataInfo, selectProjectionWithCountAggregation, new SqlTable[] { }, new Ordering[] { }, null, null, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_DifferentDistinctCondition ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);

      var sqlStatement1 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, true);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.False);
    }

    [Test]
    public void Equals_ObjectIsNull ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var sqlStatement = new SqlStatement (
         dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);
     
      Assert.That (sqlStatement.Equals(null), Is.False);
    }

    [Test]
    public void Equals_ObjectIsNotASqlStatement ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var sqlStatement = new SqlStatement (
         dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);

      Assert.That (sqlStatement.Equals(new object()), Is.False);
    }

    [Test]
    public void Equals_AssertPropertyCount ()
    {
      Assert.That (typeof (SqlStatement).GetProperties().Count(), Is.EqualTo (7), "The implementation of Equals and GetHashCode has to be adapted.");
    }
    
    [Test]
    public void GetHashcode_EqualSqlStatementsWithAllMembers ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var ordering = new Ordering (Expression.Constant ("ordering"), OrderingDirection.Asc);
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant ("top");

      var sqlStatement1 = new SqlStatement (
          dataInfo, selectProjection, new[] { sqlTable }, new[] { ordering }, whereCondition, topExpression, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new[] { sqlTable }, new[] { ordering }, whereCondition, topExpression, false);

      Assert.That (sqlStatement1.GetHashCode(), Is.EqualTo(sqlStatement2.GetHashCode()));
    }

    [Test]
    public void GetHashCode_EqualStatementsWithMandatoryMembers ()
    {
      var dataInfo = new TestStreamedValueInfo (typeof (int));
      var selectProjection = Expression.Constant (1);

      var sqlStatement1 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);
      var sqlStatement2 = new SqlStatement (
          dataInfo, selectProjection, new SqlTable[] { }, new Ordering[] { }, null, null, false);

      Assert.That (sqlStatement1.Equals (sqlStatement2), Is.True);
    }

    [Test]
    public void CreateExpression_WithSqlTables ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ());
      var sqlStatement = sqlStatementBuilder.GetSqlStatement ();

      var result = sqlStatement.CreateExpression();

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void CreateExpression_HasAggregationModifier ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ());
      sqlStatementBuilder.SqlTables.Clear();
      var selectProjection = new AggregationExpression(sqlStatementBuilder.SelectProjection, AggregationModifier.Max);
      sqlStatementBuilder.SelectProjection = selectProjection;
      var sqlStatement = sqlStatementBuilder.GetSqlStatement ();

      var result = sqlStatement.CreateExpression ();

      Assert.That (result, Is.SameAs(selectProjection));
    }

    [Test]
    public void CreateExpression_IsDistinctQuery ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ());
      sqlStatementBuilder.SqlTables.Clear();
      sqlStatementBuilder.IsDistinctQuery = true;
      var sqlStatement = sqlStatementBuilder.GetSqlStatement ();

      var result = sqlStatement.CreateExpression ();

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (sqlStatement));
    }

    [Test]
    public void CreateExpression_WithNoSqlTablesAndNoDistinctQueryAndNoAggregationModifier ()
    {
      var sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ());
      sqlStatementBuilder.SqlTables.Clear();
      var sqlStatement = sqlStatementBuilder.GetSqlStatement ();

      var result = sqlStatement.CreateExpression ();

      Assert.That (result, Is.SameAs(sqlStatement.SelectProjection));
    }
    
  }
}