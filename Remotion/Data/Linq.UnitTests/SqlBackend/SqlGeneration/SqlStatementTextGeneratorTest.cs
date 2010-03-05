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
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlStatementTextGeneratorTest
  {
    private SqlStatement _sqlStatement;

    [SetUp]
    public void SetUp ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTableWithConstantTableSource ();
      sqlTable.TableSource = new SqlTableSource (typeof (int), "Table", "t");
      var tableReferenceExpression = new SqlTableReferenceExpression (sqlTable);
      var columnListExpression = new SqlColumnListExpression (
          tableReferenceExpression.Type,
          new[]
          {
              new SqlColumnExpression (typeof (int), "t", "ID"),
              new SqlColumnExpression (typeof (int), "t", "Name"),
              new SqlColumnExpression (typeof (int), "t", "City")
          });

      _sqlStatement = new SqlStatement (columnListExpression, sqlTable, new UniqueIdentifierGenerator ());
    }

    [Test]
    public void Build ()
    {
      var generator = new SqlStatementTextGenerator();
      var result = generator.Build (_sqlStatement);
      Assert.That (result.CommandText, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Build_WithCountAndTop_ThrowsException ()
    {
      _sqlStatement.Count = true;
      _sqlStatement.TopExpression = Expression.Constant (1);

      var generator = new SqlStatementTextGenerator ();
      generator.Build (_sqlStatement);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Build_WithCountAndDistinct_ThrowsException ()
    {
      _sqlStatement.Count = true;
      _sqlStatement.Distinct = true;

      var generator = new SqlStatementTextGenerator ();
      generator.Build (_sqlStatement);
    }

    [Test]
    public void Build_WithCountIsTrue ()
    {
      _sqlStatement.Count = true;

      var generator = new SqlStatementTextGenerator ();
      var result = generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT COUNT(*) FROM [Table] AS [t]"));
    }

    [Test]
    public void Build_WithDistinctIsTrue ()
    {
      _sqlStatement.Distinct = true;

      var generator = new SqlStatementTextGenerator ();
      var result = generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT DISTINCT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }

    [Test]
    public void Build_WithTopExpression ()
    {
      _sqlStatement.TopExpression = Expression.Constant(5);

      var generator = new SqlStatementTextGenerator ();
      var result = generator.Build (_sqlStatement);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT TOP(@1) [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }
  }
}