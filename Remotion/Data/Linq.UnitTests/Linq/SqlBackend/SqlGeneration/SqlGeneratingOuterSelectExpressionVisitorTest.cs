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
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratingOuterSelectExpressionVisitorTest
  {
    private SqlCommandBuilder _commandBuilder;
    private ISqlGenerationStage _stageMock;
    private NamedExpression _namedExpression;
    private SqlEntityDefinitionExpression _entityExpression;
    private ParameterExpression _expectedRowParameter;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<ISqlGenerationStage> ();
      _commandBuilder = new SqlCommandBuilder ();
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
    public void VisitNamedExpression ()
    {
      var result = SqlGeneratingOuterSelectExpressionVisitor.GenerateSql (_namedExpression, _commandBuilder, _stageMock);

      var expectedFullProjection = Expression.Lambda<Func<IDatabaseResultRow, object>> (
          Expression.Convert (GetExpectedProjectionForNamedExpression (_expectedRowParameter), typeof (object)),
          _expectedRowParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFullProjection, result);
    }

    [Test]
    public void VisitSqlEntityExpression ()
    {
      var result = SqlGeneratingOuterSelectExpressionVisitor.GenerateSql (_entityExpression, _commandBuilder, _stageMock);

      var expectedFullProjection = Expression.Lambda<Func<IDatabaseResultRow, object>> (
          Expression.Convert (GetExpectedProjectionForEntityExpression (_expectedRowParameter), typeof (object)),
          _expectedRowParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFullProjection, result);
    }

    [Test]
    public void VisitNewExpression_WithoutMemberNames ()
    {
      var newExpression = Expression.New (
          typeof (KeyValuePair<int, Cook>).GetConstructor (new[] { typeof(int), typeof (Cook)}), 
          new Expression[] { _namedExpression, _entityExpression });

      var result = SqlGeneratingOuterSelectExpressionVisitor.GenerateSql (newExpression, _commandBuilder, _stageMock);

      var expectedProjectionForNamedExpression = GetExpectedProjectionForNamedExpression (_expectedRowParameter);
      var expectedProjectionForEntityExpression = GetExpectedProjectionForEntityExpression (_expectedRowParameter);
      var expectedProjectionForNewExpression = GetExpectedProjectionForNewExpression (expectedProjectionForNamedExpression, expectedProjectionForEntityExpression);

      var expectedFullProjection = Expression.Lambda<Func<IDatabaseResultRow, object>> (
          Expression.Convert (expectedProjectionForNewExpression, typeof (object)), 
          _expectedRowParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFullProjection, result);
    }

    [Test]
    public void VisitNewExpression_WithMemberNames ()
    {
      var keyValueType = typeof (KeyValuePair<int, Cook>);
      var newExpression = Expression.New (
          keyValueType.GetConstructor (new[] { typeof (int), typeof (Cook) }),
          new Expression[] { _namedExpression, _entityExpression }, keyValueType.GetProperty("Key"), keyValueType.GetProperty("Value"));

      var result = SqlGeneratingOuterSelectExpressionVisitor.GenerateSql (newExpression, _commandBuilder, _stageMock);

      var expectedProjectionForNamedExpression = GetExpectedProjectionForNamedExpression (_expectedRowParameter);
      var expectedProjectionForEntityExpression = GetExpectedProjectionForEntityExpression (_expectedRowParameter);
      var expectedProjectionForNewExpression = GetExpectedProjectionForNewExpressionWithMembers (expectedProjectionForNamedExpression, expectedProjectionForEntityExpression);

      var expectedFullProjection = Expression.Lambda<Func<IDatabaseResultRow, object>> (
          Expression.Convert (expectedProjectionForNewExpression, typeof (object)),
          _expectedRowParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFullProjection, result);
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

    private MethodCallExpression GetExpectedProjectionForEntityExpression (ParameterExpression expectedRowParameter)
    {
      return Expression.Call (
          expectedRowParameter,
          typeof (IDatabaseResultRow).GetMethod ("GetEntity").MakeGenericMethod (typeof (Cook)),
          Expression.Constant (new[] { new ColumnID ("ID"), new ColumnID ("Name"), new ColumnID ("FirstName")}));
    }

    private MethodCallExpression GetExpectedProjectionForNamedExpression (ParameterExpression expectedRowParameter)
    {
      return Expression.Call (
          expectedRowParameter,
          typeof (IDatabaseResultRow).GetMethod ("GetValue").MakeGenericMethod (typeof (int)),
          Expression.Constant (new ColumnID ("test")));
    }
  }
}