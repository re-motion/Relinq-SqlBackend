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
    public void GenerateSql_VisitNamedExpression ()
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
      _visitor.VisitNamedExpression (_namedExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0), _visitor.ProjectionExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (_expectedRowParameter, _visitor.RowParameter);
    }

    [Test]
    public void VisitSqlEntityExpression ()
    {
      _visitor.VisitSqlEntityExpression (_entityExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (GetExpectedProjectionForEntityExpression (_expectedRowParameter, 0), _visitor.ProjectionExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (_expectedRowParameter, _visitor.RowParameter);
    }

    [Test]
    public void VisitNewExpression_WithoutMemberNames ()
    {
      var newExpression = Expression.New (
          typeof (KeyValuePair<int, Cook>).GetConstructor (new[] { typeof(int), typeof (Cook)}), 
          new Expression[] { _namedExpression, _entityExpression });

      _visitor.VisitNewExpression (newExpression);

      var expectedProjectionForNamedExpression = GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0);
      var expectedProjectionForEntityExpression = GetExpectedProjectionForEntityExpression (_expectedRowParameter, 1);
      var expectedProjectionForNewExpression = GetExpectedProjectionForNewExpression (expectedProjectionForNamedExpression, expectedProjectionForEntityExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedProjectionForNewExpression, _visitor.ProjectionExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (_expectedRowParameter, _visitor.RowParameter);
    }

    [Test]
    public void VisitNewExpression_WithMemberNames ()
    {
      var keyValueType = typeof (KeyValuePair<int, Cook>);
      var newExpression = Expression.New (
          keyValueType.GetConstructor (new[] { typeof (int), typeof (Cook) }),
          new Expression[] { _namedExpression, _entityExpression }, keyValueType.GetProperty("Key"), keyValueType.GetProperty("Value"));

      _visitor.VisitNewExpression (newExpression);

      var expectedProjectionForNamedExpression = GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0);
      var expectedProjectionForEntityExpression = GetExpectedProjectionForEntityExpression (_expectedRowParameter, 1);
      var expectedProjectionForNewExpression = GetExpectedProjectionForNewExpressionWithMembers (expectedProjectionForNamedExpression, expectedProjectionForEntityExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedProjectionForNewExpression, _visitor.ProjectionExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (_expectedRowParameter, _visitor.RowParameter);
    }

    [Test]
    public void VisitConvertedBooleanExpression_ProjectionExpressonIsNull ()
    {
      var expression = new ConvertedBooleanExpression (Expression.Constant (1));
      _visitor.ProjectionExpression = null;

      _visitor.VisitConvertedBooleanExpression (expression);

      Assert.That (_visitor.ProjectionExpression, Is.Null);
    }

    [Test]
    public void VisitConvertedBooleanExpression_ProjectionExpressonIsNotNull ()
    {
      var expression = new ConvertedBooleanExpression (Expression.Constant (1));
      var projectionExpression = GetExpectedProjectionForNamedExpression (_expectedRowParameter, "test", 0);
      _visitor.ProjectionExpression = projectionExpression;

      _visitor.VisitConvertedBooleanExpression (expression);

      var expectedProjection = Expression.Call(typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (int) }), projectionExpression);
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

    private NewExpression GetExpectedProjectionForNewExpression (MethodCallExpression expectedProjectionForNamedExpression, MethodCallExpression expectedProjectionForEntityExpression)
    {
      return Expression.New (
          typeof (KeyValuePair<int, Cook>).GetConstructor (new[] { typeof (int), typeof (Cook) }),
          new Expression[] { expectedProjectionForNamedExpression, expectedProjectionForEntityExpression });
    }

    private NewExpression GetExpectedProjectionForNewExpressionWithMembers (MethodCallExpression expectedProjectionForNamedExpression, MethodCallExpression expectedProjectionForEntityExpression)
    {
      var keyValueType = typeof (KeyValuePair<int, Cook>);
      return Expression.New (
          keyValueType.GetConstructor (new[] { typeof (int), typeof (Cook) }),
          new Expression[] { expectedProjectionForNamedExpression, expectedProjectionForEntityExpression },
          keyValueType.GetProperty ("Key"), keyValueType.GetProperty ("Value"));
    }

    private MethodCallExpression GetExpectedProjectionForEntityExpression (ParameterExpression expectedRowParameter, int columnPositionStart)
    {
      return Expression.Call (
          expectedRowParameter,
          typeof (IDatabaseResultRow).GetMethod ("GetEntity").MakeGenericMethod (typeof (Cook)),
          Expression.Constant (new[] { new ColumnID ("ID", columnPositionStart++), new ColumnID ("Name", columnPositionStart++), new ColumnID ("FirstName", columnPositionStart++)}));
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