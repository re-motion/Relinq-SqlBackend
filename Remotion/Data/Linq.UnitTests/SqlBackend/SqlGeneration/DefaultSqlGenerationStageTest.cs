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
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;
using NUnit.Framework.SyntaxHelpers;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class DefaultSqlGenerationStageTest
  {
    private SqlStatement _sqlStatement;
    private DefaultSqlGenerationStage _stage;
    private SqlCommandBuilder _commandBuilder;
    private SqlEntityExpression _columnListExpression;

    [SetUp]
    public void SetUp ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo();
      var primaryKeyColumn = new SqlColumnExpression (typeof (int), "t", "ID");
      _columnListExpression = new SqlEntityExpression (
          typeof (Cook),
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnExpression (typeof (int), "t", "Name"),
              new SqlColumnExpression (typeof (int), "t", "City")
          });

      _stage = new DefaultSqlGenerationStage();
      _sqlStatement = new SqlStatement (_columnListExpression, new[] { sqlTable }, new Ordering[]{});
      _commandBuilder = new SqlCommandBuilder ();
    }
    

    [Test]
    public void GenerateTextForFromTable ()
    {
      _stage.GenerateTextForFromTable (_commandBuilder, _sqlStatement.SqlTables[0], true);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    public void GenerateTextForSelectExpression ()
    {
      _stage.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void GenerateTextForTopExpression ()
    {
      _sqlStatement.TopExpression = Expression.Constant (5);
      _stage.GenerateTextForTopExpression (_commandBuilder, _sqlStatement.TopExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1"));
      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo (5));
    }

    [Test]
    public void GenerateTextForWhereExpression ()
    {
      _sqlStatement.WhereCondition = Expression.AndAlso (Expression.Constant (true), Expression.Constant (true));

      _stage.GenerateTextForWhereExpression (_commandBuilder, _sqlStatement.WhereCondition);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("((@1 = 1) AND (@2 = 1))"));
      Assert.That (_commandBuilder.GetCommandParameters ().Length, Is.EqualTo (2));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters ()[1].Value, Is.EqualTo (1));
    }

    [Test]
    public void GenerateTextForOrderByExpression_ConstantExpression ()
    {
      var expression = Expression.Constant (1);
      
      _stage.GenerateTextForOrderByExpression (_commandBuilder, expression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo("@1"));
      Assert.That (_commandBuilder.GetCommandParameters ().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo (1));
    }

    [Test]
    public void GenerateTextForOrderByExpression_SqlColumnExpression ()
    {
      var expression = new SqlColumnExpression (typeof (int), "c", "ID");

      _stage.GenerateTextForOrderByExpression (_commandBuilder, expression);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[c].[ID]"));
    }

    [Test]
    public void GenerateTextForSqlStatement ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      sqlStatement.SelectProjection = _columnListExpression;
      sqlStatement.SqlTables[0].TableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");

      _stage.GenerateTextForSqlStatement (_commandBuilder, sqlStatement);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }
  }
}