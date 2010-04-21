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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.StreamedData;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlStatementBuilderTest
  {
    private SqlStatementBuilder _statementBuilder;

    [SetUp]
    public void SetUp ()
    {
      _statementBuilder = new SqlStatementBuilder ();
      _statementBuilder.DataInfo = new TestStreamedValueInfo (typeof (int));
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
      Assert.That (result.SqlTables[0], Is.SameAs(sqlTable));
    }

    // TODO Review 2616: Also add a test GetSqlStatement_NoSelectProjection set. Expect an InvalidOperationException: "A SelectProjection must be set before the SqlStatement can be retrieved." Adapt implementation.

    [Test]
    [ExpectedException (typeof (ArgumentNullException))] // TODO Review 2616: Change to expect an InvalidOperationException: "A DataInfo must be set before the SqlStatement can be retrieved." Adapt implementation.
    public void GetSqlStatement_NoDataInfoSet ()
    {
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
    public void GetSqlStatement_CheckProperties()
    {
      var selectProjection = Expression.Constant ("select");
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant ("top");
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Desc);

      var statementBuilder = new SqlStatementBuilder()
                             {
                                 DataInfo = new TestStreamedValueInfo(typeof(Cook)),
                                 SelectProjection = selectProjection,
                                 WhereCondition = whereCondition,
                                 TopExpression = topExpression,
                                 IsCountQuery = false,
                                 IsDistinctQuery = true
                             };
      statementBuilder.SqlTables.Add (sqlTable);
      statementBuilder.Orderings.Add (ordering);

      var sqlStatement = statementBuilder.GetSqlStatement();

      Assert.That (sqlStatement.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (sqlStatement.TopExpression, Is.SameAs (topExpression));
      Assert.That (sqlStatement.SqlTables[0], Is.SameAs (sqlTable));
      Assert.That (sqlStatement.Orderings[0], Is.SameAs (ordering));
      Assert.That (sqlStatement.WhereCondition, Is.EqualTo (whereCondition));
      Assert.That (sqlStatement.IsDistinctQuery, Is.True);
      Assert.That (sqlStatement.IsCountQuery, Is.False);
      // TODO Review 2616: Also check DataInfo
    }

    [Test]
    // TODO Review 2616: Rename this test to Initialization_WithExistingSqlStatement - we always name ctor tests like this -, and move to the top (after Setup method)
    public void CreateSqlStatementBuilder_WithExistingSqlStatement ()
    {
      var selectProjection = Expression.Constant ("select");
      var whereCondition = Expression.Constant (true);
      var topExpression = Expression.Constant ("top");
      var sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
       var ordering = new Ordering (Expression.Constant ("order"), OrderingDirection.Desc);

       var sqlStatement = new SqlStatement (new TestStreamedValueInfo (typeof (int)), selectProjection, new SqlTable[] { sqlTable }, new Ordering[] { ordering }, whereCondition, topExpression, false, true);

      var testedBuilder = new SqlStatementBuilder (sqlStatement);

      Assert.That (testedBuilder.SelectProjection, Is.SameAs (selectProjection));
      Assert.That (testedBuilder.TopExpression, Is.SameAs (topExpression));
      Assert.That (testedBuilder.SqlTables[0], Is.SameAs (sqlTable));
      Assert.That (testedBuilder.Orderings[0], Is.SameAs (ordering));
      Assert.That (testedBuilder.WhereCondition, Is.EqualTo (whereCondition));
      Assert.That (testedBuilder.IsDistinctQuery, Is.True);
      Assert.That (testedBuilder.IsCountQuery, Is.False);

      // TODO Review 2616: Also check that DataInfo is taken from existing statement
      
    }
  }
}