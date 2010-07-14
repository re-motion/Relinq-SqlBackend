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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratingOuterSelectExpressionVisitorTest
  {
    private NamedExpression _namedExpression;
    private SqlEntityDefinitionExpression _entityExpression;
    private ParameterExpression _expectedRowParameter;
    private TestableSqlGeneratingOuterSelectExpressionVisitor _visitor;
    private ISqlGenerationStage _stageMock;
    private SqlCommandBuilder _commandBuilder;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<ISqlGenerationStage> ();
      _commandBuilder = new SqlCommandBuilder ();
      _visitor = new TestableSqlGeneratingOuterSelectExpressionVisitor (_commandBuilder, _stageMock);

      _namedExpression = new NamedExpression ("test", Expression.Constant (0));
      _entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook),
          "c",
          "test",
          new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true),
          new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true),
          new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false),
          new SqlColumnDefinitionExpression (typeof (string), "c", "FirstName", false)
          );
      _expectedRowParameter = Expression.Parameter (typeof (IDatabaseResultRow), "row");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Queries selecting collections are not supported because SQL is not well-suited to returning collections.",
        MatchType = MessageMatch.Contains)]
    public void GenerateSql_Collection ()
    {
      var expression = Expression.Constant (new Cook[] { });
      SqlGeneratingOuterSelectExpressionVisitor.GenerateSql (expression, _commandBuilder, _stageMock);
    }

    [Test]
    public void GenerateSql_CreatesFullInMemoryProjectionLambda ()
    {
      var result = SqlGeneratingOuterSelectExpressionVisitor.GenerateSql (_namedExpression, _commandBuilder, _stageMock);

      var expectedFullProjection = Expression.Lambda<Func<IDatabaseResultRow, object>> (
          Expression.Convert (GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0), typeof (object)),
          _expectedRowParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFullProjection, result);
    }

    [Test]
    public void VisitNamedExpression ()
    {
      Assert.That (_visitor.ColumnPosition, Is.EqualTo (0));

      _visitor.VisitNamedExpression (_namedExpression);

      Assert.That (_visitor.ColumnPosition, Is.EqualTo (1));

      var expectedProjection = GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedProjection, _visitor.ProjectionExpression);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("@1 AS [test]"));
    }

    [Test]
    public void VisitSqlEntityExpression ()
    {
      Assert.That (_visitor.ColumnPosition, Is.EqualTo (0));

      _visitor.VisitSqlEntityExpression (_entityExpression);

      Assert.That (_visitor.ColumnPosition, Is.EqualTo (3));

      var expectedProjection = GetExpectedProjectionForEntityExpression (_expectedRowParameter, 0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedProjection, _visitor.ProjectionExpression);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[c].[ID] AS [test_ID],[c].[Name] AS [test_Name],[c].[FirstName] AS [test_FirstName]"));
    }

    [Test]
    public void VisitNewExpression_WithoutMembers ()
    {
      Assert.That (_visitor.ColumnPosition, Is.EqualTo (0));

      var newExpression = Expression.New (
          typeof (KeyValuePair<int, Cook>).GetConstructor (new[] { typeof(int), typeof (Cook)}), 
          new Expression[] { _namedExpression, _entityExpression });
      _visitor.VisitNewExpression (newExpression);

      Assert.That (_visitor.ColumnPosition, Is.EqualTo (4));

      var expectedProjectionForNamedExpression = GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0);
      var expectedProjectionForEntityExpression = GetExpectedProjectionForEntityExpression (_expectedRowParameter, 1);
      
      var expectedProjectionForNewExpression = Expression.New (
          typeof (KeyValuePair<int, Cook>).GetConstructor (new[] { typeof (int), typeof (Cook) }),
          new Expression[] { expectedProjectionForNamedExpression, expectedProjectionForEntityExpression });

      Assert.That (
          _commandBuilder.GetCommandText (), 
          Is.EqualTo ("@1 AS [test],[c].[ID] AS [test_ID],[c].[Name] AS [test_Name],[c].[FirstName] AS [test_FirstName]"));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedProjectionForNewExpression, _visitor.ProjectionExpression);
    }

    [Test]
    public void VisitNewExpression_WithMembers ()
    {
      var keyValueType = typeof (KeyValuePair<int, Cook>);
      var newExpression = Expression.New (
          keyValueType.GetConstructor (new[] { typeof (int), typeof (Cook) }),
          new Expression[] { _namedExpression, _entityExpression },
          keyValueType.GetProperty("Key"),
          keyValueType.GetProperty("Value"));

      _visitor.VisitNewExpression (newExpression);

      var expectedProjectionForNamedExpression = GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0);
      var expectedProjectionForEntityExpression = GetExpectedProjectionForEntityExpression (_expectedRowParameter, 1);
      
      var expectedProjectionForNewExpression = Expression.New (
          keyValueType.GetConstructor (new[] { typeof (int), typeof (Cook) }),
          new Expression[] { expectedProjectionForNamedExpression, expectedProjectionForEntityExpression },
          keyValueType.GetProperty ("Key"), keyValueType.GetProperty ("Value"));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedProjectionForNewExpression, _visitor.ProjectionExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (_expectedRowParameter, _visitor.RowParameter);
    }

    [Test]
    public void VisitConvertedBooleanExpression_ProjectionExpressonIsNull ()
    {
      _visitor.ProjectionExpression = null;

      var expression = new ConvertedBooleanExpression (Expression.Constant (1));
      _visitor.VisitConvertedBooleanExpression (expression);

      Assert.That (_visitor.ProjectionExpression, Is.Null);
    }

    [Test]
    public void VisitConvertedBooleanExpression_ProjectionExpressonIsNotNull ()
    {
      _visitor.ProjectionExpression = null;

      var expression = new ConvertedBooleanExpression (_namedExpression);

      _visitor.VisitConvertedBooleanExpression (expression);

      var expectedProjection = Expression.Call (typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (int) }), GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedProjection, _visitor.ProjectionExpression);
    }

    [Test]
    public void VisitConvertExpression_ProjectionExpressonIsNull ()
    {
      var expression = Expression.Convert (Expression.Constant (1), typeof (double));
      _visitor.ProjectionExpression = null;

      _visitor.VisitUnaryExpression (expression);

      Assert.That (_visitor.ProjectionExpression, Is.Null);
    }

    [Test]
    public void VisitConvertExpression_ProjectionExpressonIsNotNull ()
    {
      _visitor.ProjectionExpression = null;

      var methodInfo = typeof (Convert).GetMethod ("ToDouble", new[] { typeof (int) });
      var expression = Expression.Convert (_namedExpression, typeof (double), methodInfo);

      _visitor.VisitUnaryExpression (expression);

      var expectedProjection = Expression.Convert (
          GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0),
          typeof (double),
          methodInfo);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedProjection, _visitor.ProjectionExpression);
    }

    [Test]
    public void VisitConvertCheckedExpression_ProjectionExpressonIsNull ()
    {
      var expression = Expression.ConvertChecked (Expression.Constant (1), typeof (double));
      _visitor.ProjectionExpression = null;

      _visitor.VisitUnaryExpression (expression);

      Assert.That (_visitor.ProjectionExpression, Is.Null);
    }

    [Test]
    public void VisitConvertCheckedExpression_ProjectionExpressonIsNotNull ()
    {
      _visitor.ProjectionExpression = null;

      var methodInfo = typeof (Convert).GetMethod ("ToDouble", new[] { typeof (int) });
      var expression = Expression.ConvertChecked (_namedExpression, typeof (double), methodInfo);

      _visitor.VisitUnaryExpression (expression);

      var expectedProjection = Expression.ConvertChecked (
          GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0),
          typeof (double),
          methodInfo);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedProjection, _visitor.ProjectionExpression);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "This SQL generator does not support queries returning groupings that result from a GroupBy operator because SQL is not suited to "
         + "efficiently return LINQ groupings. Use 'group into' and either return the items of the groupings by feeding them into an additional "
         + "from clause, or perform an aggregation on the groupings.", MatchType = MessageMatch.Contains)]
    public void VisitSqlGroupingSelectExpression ()
    {
      var expression = SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression ();
      _visitor.VisitSqlGroupingSelectExpression (expression);
    }

    private MethodCallExpression GetExpectedProjectionForEntityExpression (ParameterExpression expectedRowParameter, int columnPositionStart)
    {
      return Expression.Call (
          expectedRowParameter,
          typeof (IDatabaseResultRow).GetMethod ("GetEntity").MakeGenericMethod (typeof (Cook)),
          Expression.Constant (new[] { 
              new ColumnID ("test_ID", columnPositionStart), 
              new ColumnID ("test_Name", columnPositionStart + 1), 
              new ColumnID ("test_FirstName", columnPositionStart + 2) }));
    }

    private MethodCallExpression GetExpectedProjectionForNamedExpression (ParameterExpression expectedRowParameter, string name, int columnPosoitionStart)
    {
      return Expression.Call (
          expectedRowParameter,
          typeof (IDatabaseResultRow).GetMethod ("GetValue").MakeGenericMethod (typeof (int)),
          Expression.Constant (new ColumnID (name ?? "value", columnPosoitionStart)));
    }
  }
}