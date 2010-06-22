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
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestUtilities;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class DefaultSqlGenerationStageTest
  {
    private SqlStatement _sqlStatement;
    private DefaultSqlGenerationStage _stageMock;
    private SqlCommandBuilder _commandBuilder;
    private SqlEntityExpression _columnListExpression;

    [SetUp]
    public void SetUp ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo();
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "t", "ID", true);
      _columnListExpression = new SqlEntityDefinitionExpression (
          typeof(string),
          "t", null,
          primaryKeyColumn,
          new[]
          {
              primaryKeyColumn,
              new SqlColumnDefinitionExpression (typeof (int), "t", "Name", false),
              new SqlColumnDefinitionExpression (typeof (int), "t", "City", false)
          });

      _sqlStatement = new SqlStatement (new TestStreamedValueInfo (typeof (int)), _columnListExpression, new[] { sqlTable }, null, null, new Ordering[] { }, null, false, null, null);
      _commandBuilder = new SqlCommandBuilder();

      _stageMock = MockRepository.GeneratePartialMock<DefaultSqlGenerationStage>();
    }

    [Test]
    public void GenerateTextForFromTable ()
    {
      _stageMock.GenerateTextForFromTable (_commandBuilder, _sqlStatement.SqlTables[0], true);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[Table] AS [t]"));
    }

    [Test]
    public void GenerateTextForSelectExpression ()
    {
      _stageMock
          .Expect (mock => CallGenerateTextForExpression (mock, _sqlStatement.SelectProjection, SqlGenerationMode.SelectExpression))
          .WhenCalled (c => _commandBuilder.Append ("[t].[ID],[t].[Name],[t].[City]"));

      _stageMock.Replay();

      _stageMock.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Subquery selects a collection where a single value is expected.")]
    public void GenerateTextForSelectExpression_CollectionInSelectProjection_ThrowsException ()
    {
      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (int)),
          Expression.Constant (new Cook[] { }),
          new SqlTable[] { }, null, null, new Ordering[] { }, null, false, null, null);

      _stageMock.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection);
    }

    [Test]
    public void  GenerateTextForSelectExpression_CollectionInSelectProjection_StringsNotDetectedAsCollections ()
    {
      _sqlStatement = new SqlStatement (
          new TestStreamedValueInfo (typeof (string)),
          Expression.Constant ("test"),
          new SqlTable[] { }, null, null, new Ordering[] { }, null, false, null, null);

      _stageMock.GenerateTextForSelectExpression (_commandBuilder, _sqlStatement.SelectProjection);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("@1"));
    }

    [Test]
    public void GenerateTextForTopExpression ()
    {
      var sqlStatement =
          new SqlStatementBuilder { DataInfo = new TestStreamedValueInfo(typeof(int)), SelectProjection = _columnListExpression, TopExpression = Expression.Constant (5) }.GetSqlStatement();

      _stageMock
          .Expect ( mock => CallGenerateTextForExpression (mock, sqlStatement.TopExpression, SqlGenerationMode.NonSelectExpression))
          .WhenCalled (c => _commandBuilder.Append ("test"));
      _stageMock.Replay();

      _stageMock.GenerateTextForTopExpression (_commandBuilder, sqlStatement.TopExpression);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForWhereExpression ()
    {
      var sqlStatement = new SqlStatement (new TestStreamedValueInfo (typeof (int)), _columnListExpression,
          new SqlTable[] { }, Expression.AndAlso (Expression.Constant (true), Expression.Constant (true)), null, new Ordering[] { }, null, false, null, null);

      _stageMock
          .Expect (mock => CallGenerateTextForExpression (mock, sqlStatement.WhereCondition, SqlGenerationMode.NonSelectExpression))
          .WhenCalled (c => _commandBuilder.Append ("test"));
      _stageMock.Replay();

      _stageMock.GenerateTextForWhereExpression (_commandBuilder, sqlStatement.WhereCondition);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForOrderByExpression_ConstantExpression ()
    {
      var expression = Expression.Constant (1);

      _stageMock
          .Expect (mock => CallGenerateTextForExpression (mock, expression, SqlGenerationMode.NonSelectExpression))
          .WhenCalled (c => _commandBuilder.Append ("test"));
      _stageMock.Replay();

      _stageMock.GenerateTextForOrderByExpression (_commandBuilder, expression);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void GenerateTextForOrdering ()
    {
      var ordering = new Ordering(Expression.Constant (1),OrderingDirection.Asc);

      _stageMock.GenerateTextForOrdering(_commandBuilder, ordering);

      _stageMock.VerifyAllExpectations ();
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("(SELECT @1) ASC"));
    }

    [Test]
    public void GenerateTextForSqlStatement ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (
          _columnListExpression, new[] { new SqlTable (new ResolvedSimpleTableInfo (typeof (int), "Table", "t")) });

      _stageMock.GenerateTextForSqlStatement (_commandBuilder, sqlStatement);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }

    [Test]
    public void GenerateTextForJoinKeyExpression ()
    {
      var expression = new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false);

      _stageMock
          .Expect (mock => CallGenerateTextForExpression (mock, expression, SqlGenerationMode.NonSelectExpression))
          .WhenCalled (c => _commandBuilder.Append ("test"));
      _stageMock.Replay();

      _stageMock.GenerateTextForJoinKeyExpression (_commandBuilder, expression);

      _stageMock.VerifyAllExpectations();
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    private void CallGenerateTextForExpression (DefaultSqlGenerationStage mock, Expression expression, SqlGenerationMode sqlGenerationMode)
    {
      PrivateInvoke.InvokeNonPublicMethod (mock, "GenerateTextForExpression", _commandBuilder, expression, sqlGenerationMode);
    }
  }
}