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
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlStatementTextGeneratorTest
  {
    private SqlStatement _sqlStatement;
    private SqlStatementTextGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo();
      var primaryKeyColumn = new SqlColumnExpression (typeof (int), "t", "ID");
      var columnListExpression = new SqlEntityExpression (
          typeof (Cook),
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnExpression (typeof (int), "t", "Name"),
              new SqlColumnExpression (typeof (int), "t", "City")
          });

      _generator = new SqlStatementTextGenerator();
      _sqlStatement = new SqlStatement (columnListExpression, new[] { sqlTable });
    }

    [Test]
    public void Build_WithSelectAndFrom ()
    {
      var result = _generator.Build (_sqlStatement);
      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void Build_WithCountAndTop_ThrowsException ()
    {
      _sqlStatement.IsCountQuery = true;
      _sqlStatement.TopExpression = Expression.Constant (1);

      _generator.Build (_sqlStatement);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void Build_WithCountAndDistinct_ThrowsException ()
    {
      _sqlStatement.IsCountQuery = true;
      _sqlStatement.IsDistinctQuery = true;

      _generator.Build (_sqlStatement);
    }

    [Test]
    public void Build_WithCountIsTrue ()
    {
      _sqlStatement.IsCountQuery = true;

      var result = _generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT COUNT(*) FROM [Table] AS [t]"));
    }

    [Test]
    public void Build_WithDistinctIsTrue ()
    {
      _sqlStatement.IsDistinctQuery = true;

      var result = _generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT DISTINCT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }

    [Test]
    public void Build_WithTopExpression ()
    {
      _sqlStatement.TopExpression = Expression.Constant(5);

      var result = _generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT TOP (@1) [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }

    [Test]
    public void Build_WithDistinctAndTopExpression ()
    {
      _sqlStatement.IsDistinctQuery = true;
      _sqlStatement.TopExpression = Expression.Constant (5);

      var result = _generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT DISTINCT TOP (@1) [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }

    [Test]
    public void Build_Select_HasValueSemantics ()
    {
      _sqlStatement.SelectProjection = Expression.Equal (Expression.Constant (0), Expression.Constant (1));

      var result = _generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT CASE WHEN (@1 = @2) THEN 1 ELSE 0 END FROM [Table] AS [t]"));
    }

    [Test]
    public void Build_WithSingleWhereCondition_PredicateSemantics ()
    {
      _sqlStatement.WhereCondition = Expression.Constant (true);

      var result = _generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] WHERE (@1 = 1)"));
    }

    [Test]
    public void Build_WithSingleOrderByClause ()
    {
      var columnExpression = new SqlColumnExpression (typeof (string), "t", "Name");
      var orderByClause = new Ordering (columnExpression, OrderingDirection.Asc);
      _sqlStatement.OrderByClauses.Add (orderByClause);

      var result = _generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] ORDER BY [t].[Name] ASC"));
    }

    [Test]
    public void Build_WithMultipleOrderByClauses ()
    {
      var columnExpression1 = new SqlColumnExpression (typeof (string), "t", "ID");
      var orderByClause1 = new Ordering (columnExpression1, OrderingDirection.Asc);
      _sqlStatement.OrderByClauses.Add (orderByClause1);
      var columnExpression2 = new SqlColumnExpression (typeof (string), "t", "Name");
      var orderByClause2 = new Ordering (columnExpression2, OrderingDirection.Desc);
      _sqlStatement.OrderByClauses.Add (orderByClause2);
      var columnExpression3 = new SqlColumnExpression (typeof (string), "t", "City");
      var orderByClause3 = new Ordering (columnExpression3, OrderingDirection.Desc);
      _sqlStatement.OrderByClauses.Add (orderByClause3);

      var result = _generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t] "
                                                    + "ORDER BY [t].[ID] ASC, [t].[Name] DESC, [t].[City] DESC"));
    }

    [Test]
    [Ignore ("TODO: 2364")]
    public void GenerateSqlGeneratorRegistry ()
    {
      Assert.Fail();
    }
  }
}