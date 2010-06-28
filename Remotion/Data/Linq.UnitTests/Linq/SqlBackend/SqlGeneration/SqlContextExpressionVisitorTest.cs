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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlContextExpressionVisitorTest
  {
    private TestableSqlContextExpressionVisitor _nonTopLevelVisitor;
    private IMappingResolutionStage _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage>();
      _mappingResolutionContext = new MappingResolutionContext();
      _nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, false, _stageMock, _mappingResolutionContext);
    }

    [Test]
    public void ApplyContext ()
    {
      var valueExpression = Expression.Constant (0);
      var predicateExpression = Expression.Constant (true);

      var convertedValue = SqlContextExpressionVisitor.ApplySqlExpressionContext (
          valueExpression, SqlExpressionContext.PredicateRequired, _stageMock, _mappingResolutionContext);
      var convertedPredicate = SqlContextExpressionVisitor.ApplySqlExpressionContext (
          predicateExpression, SqlExpressionContext.SingleValueRequired, _stageMock, _mappingResolutionContext);

      var expectedConvertedValue = Expression.Equal (valueExpression, new SqlLiteralExpression (1));
      var expectedConvertedPredicate = Expression.Constant (1);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedConvertedValue, convertedValue);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedConvertedPredicate, convertedPredicate);
    }

    [Test]
    public void VisitExpression_Null_Ignored ()
    {
      var result = _nonTopLevelVisitor.VisitExpression (null);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void VisitExpression_CallsNodeSpecificVisitMethods ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, true, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (entityExpression);

      Assert.That (result, Is.SameAs (entityExpression.PrimaryKeyColumn));
    }

    [Test]
    public void VisitExpression_ConvertsBool_ToValue ()
    {
      var expression = new CustomExpression (typeof (bool));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, true, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);

      var expected = new SqlCaseExpression (expression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));

      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitExpression_ConvertsBool_ToSingleValue ()
    {
      var expression = new CustomExpression (typeof (bool));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, true, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);

      var expected = new SqlCaseExpression (expression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));

      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitExpression_LeavesExistingValue ()
    {
      var expression = new CustomExpression (typeof (int));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, true, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExpression_LeavesExistingSingleValue ()
    {
      var expression = new CustomExpression (typeof (int));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, true, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExpression_LeavesExistingPredicate ()
    {
      var expression = new CustomExpression (typeof (bool));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, true, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExpression_ConvertsInt_ToPredicate ()
    {
      var expression = new CustomExpression (typeof (int));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, true, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (expression);

      var expected = Expression.Equal (expression, new SqlLiteralExpression (1));

      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot convert an expression of type 'System.String' to a boolean expression.")]
    public void VisitExpression_ThrowsOnNonConvertible_ToPredicate ()
    {
      var expression = new CustomExpression (typeof (string));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, true, _stageMock, _mappingResolutionContext);
      visitor.VisitExpression (expression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Invalid enum value: -1")]
    public void VisitExpression_ThrowsOnInvalidContext ()
    {
      var expression = new CustomExpression (typeof (string));

      var visitor = new TestableSqlContextExpressionVisitor ((SqlExpressionContext) (-1), true, _stageMock, _mappingResolutionContext);
      visitor.VisitExpression (expression);
    }

    [Test]
    public void VisitExpression_NonTopLevel_AlwaysAppliesSingleValueSemantics ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, false, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (entityExpression);

      Assert.That (result, Is.Not.SameAs (entityExpression));
      Assert.That (result, Is.SameAs (entityExpression.PrimaryKeyColumn));
    }

    [Test]
    public void VisitExpression_TopLevel_AppliesSpecifiedSemantics ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, true, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (entityExpression);

      Assert.That (result, Is.SameAs (entityExpression));
    }

    [Test]
    public void VisitExpression_ChildNode_GetsSingleValueSemantics ()
    {
      var childExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var parentExpression = new CustomCompositeExpression (typeof (bool), childExpression);

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, true, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitExpression (parentExpression);

      var expectedExpression = new CustomCompositeExpression (typeof (bool), childExpression.PrimaryKeyColumn);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitConstantExpression_BoolConstants ()
    {
      var constantTrue = Expression.Constant (true);
      var constantFalse = Expression.Constant (false);

      var resultTrue = _nonTopLevelVisitor.VisitExpression (constantTrue);
      var resultFalse = _nonTopLevelVisitor.VisitExpression (constantFalse);

      var expectedExpressionTrue = Expression.Constant (1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionTrue, resultTrue);

      var expectedExpressionFalse = Expression.Constant (0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpressionFalse, resultFalse);
    }

    [Test]
    public void VisitConstantExpression_OtherConstants ()
    {
      var constant = Expression.Constant ("hello");

      var result = _nonTopLevelVisitor.VisitExpression (constant);

      Assert.That (result, Is.SameAs (constant));
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_NoPrimaryColumn ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", false);

      var result = _nonTopLevelVisitor.VisitSqlColumnExpression (column);

      var expectedExpression = new SqlColumnDefinitionExpression (typeof (int), "x", "y", false);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlColumnExpression_BoolColumn_ConvertedToIntColumn_IsPrimaryColumn ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (bool), "x", "y", true);

      var result = _nonTopLevelVisitor.VisitSqlColumnExpression (column);

      var expectedExpression = new SqlColumnDefinitionExpression (typeof (int), "x", "y", true);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlColumnExpression_OtherColumn ()
    {
      var column = new SqlColumnDefinitionExpression (typeof (string), "x", "y", false);

      var result = _nonTopLevelVisitor.VisitSqlColumnExpression (column);

      Assert.That (result, Is.SameAs (column));
    }

    [Test]
    public void VisitSqlEntityExpression_WithSingleValueSemantics_ConvertsEntityToPrimaryKey ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitSqlEntityExpression (entityExpression);

      Assert.That (result, Is.SameAs (entityExpression.PrimaryKeyColumn));
    }

    [Test]
    public void VisitSqlEntityExpression_WithNonSingleValueSemantics_LeavesEntity ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var visitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.ValueRequired, false, _stageMock, _mappingResolutionContext);
      var result = visitor.VisitSqlEntityExpression (entityExpression);

      Assert.That (result, Is.SameAs (entityExpression));
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToSingleValue_ForEqual ()
    {
      var expression = Expression.Equal (Expression.Constant (true), Expression.Constant (false));

      var result = _nonTopLevelVisitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.Equal (Expression.Constant (1), Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToSingleValue_ForNotEqual ()
    {
      var expression = Expression.NotEqual (Expression.Constant (true), Expression.Constant (false));

      var result = _nonTopLevelVisitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.NotEqual (Expression.Constant (1), Expression.Constant (0));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAndAlso ()
    {
      var expression = Expression.AndAlso (Expression.Constant (true), Expression.Constant (false));

      var result = _nonTopLevelVisitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.AndAlso (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOrElse ()
    {
      var expression = Expression.OrElse (Expression.Constant (true), Expression.Constant (false));

      var result = _nonTopLevelVisitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.OrElse (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForAnd ()
    {
      var expression = Expression.And (Expression.Constant (true), Expression.Constant (false));

      var result = _nonTopLevelVisitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.And (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForOr ()
    {
      var expression = Expression.Or (Expression.Constant (true), Expression.Constant (false));

      var result = _nonTopLevelVisitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.Or (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_ConvertsLeftRightToPredicate_ForExclusiveOr ()
    {
      var expression = Expression.ExclusiveOr (Expression.Constant (true), Expression.Constant (false));

      var result = _nonTopLevelVisitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.ExclusiveOr (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_BinaryBoolExpression_PassesMethod ()
    {
      var operatorMethod = typeof (SqlContextExpressionVisitorTest).GetMethod ("FakeAndOperator");
      var expression = Expression.And (Expression.Constant (true), Expression.Constant (false), operatorMethod);
      Assert.That (expression.Method, Is.Not.Null);

      var result = _nonTopLevelVisitor.VisitBinaryExpression (expression);

      var expectedExpression = Expression.And (
          Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)),
          Expression.Equal (Expression.Constant (0), new SqlLiteralExpression (1)),
          operatorMethod);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitBinaryExpression_NonBoolBinaryExpression_Unchanged ()
    {
      var binary = BinaryExpression.And (Expression.Constant (5), Expression.Constant (5));

      var result = _nonTopLevelVisitor.VisitBinaryExpression (binary);

      Assert.That (result, Is.SameAs (binary));
    }

    [Test]
    public void VisitBinaryExpression_NonBoolBinaryExpression_OperandsGetSingleValueSemantics ()
    {
      var binary = BinaryExpression.And (Expression.Convert (Expression.Not (Expression.Constant (true)), typeof (int)), Expression.Constant (5));

      var result = _nonTopLevelVisitor.VisitBinaryExpression (binary);

      var expectedExpression =
          BinaryExpression.And (
              Expression.Convert (
                  new SqlCaseExpression (
                      Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1))),
                      new SqlLiteralExpression (1),
                      new SqlLiteralExpression (0)),
                  typeof (int)),
              Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnaryExpression_UnaryBoolExpression_OperandConvertedToPredicate ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (true));

      var result = _nonTopLevelVisitor.VisitUnaryExpression (unaryExpression);

      var expectedExpression = Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitUnaryExpression_ConvertExpression_OperandChanged ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var unaryExpression =
          Expression.Convert (
              new SqlEntityDefinitionExpression (typeof (Cook), "c", "CookTable", new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true)),
              typeof (object));

      var result = nonTopLevelVisitor.VisitUnaryExpression (unaryExpression);

      Assert.That (result, Is.Not.SameAs (unaryExpression));
      Assert.That (((UnaryExpression) result).Operand, Is.TypeOf (typeof (SqlColumnDefinitionExpression)));
    }

    [Test]
    public void VisitUnaryExpression_ConvertExpression_SameOperand ()
    {
      var unaryExpression =
          Expression.Convert (
              new SqlEntityDefinitionExpression (typeof (Cook), "c", "CookTable", new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true)),
              typeof (object));

      var result = _nonTopLevelVisitor.VisitUnaryExpression (unaryExpression);

      Assert.That (result, Is.SameAs (unaryExpression));
    }

    [Test]
    public void VisitUnaryExpression_OtherUnaryExpression_Unchanged ()
    {
      var unaryExpression = Expression.Not (Expression.Constant (5));

      var result = _nonTopLevelVisitor.VisitUnaryExpression (unaryExpression);

      Assert.That (result, Is.SameAs (unaryExpression));
    }

    [Test]
    public void VisitUnaryExpression_OtherUnaryExpression_OperandConvertedToSingleValue ()
    {
      var unaryExpression = // ValueRequired
          Expression.Not (
              // ValueRequired
              Expression.Convert (
                  Expression.Not (
                      Expression.Constant (true)
                      ),
                  typeof (int)));

      var result = _nonTopLevelVisitor.VisitUnaryExpression (unaryExpression);

      var expectedExpression =
          Expression.Not (
              Expression.Convert (
                  new SqlCaseExpression (
                      Expression.Not (Expression.Equal (Expression.Constant (1), new SqlLiteralExpression (1))),
                      new SqlLiteralExpression (1),
                      new SqlLiteralExpression (0)),
                  typeof (int)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitSqlIsNullExpression_AppliesSingleValueSemantics ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var sqlIsNullExpressionWithValue = new SqlIsNullExpression (Expression.Constant (1));
      var sqlIsNullExpressionWithEntity = new SqlIsNullExpression (entityExpression);

      var resultWithValue = _nonTopLevelVisitor.VisitSqlIsNullExpression (sqlIsNullExpressionWithValue);
      var resultWithEntity = _nonTopLevelVisitor.VisitSqlIsNullExpression (sqlIsNullExpressionWithEntity);

      Assert.That (resultWithValue, Is.SameAs (sqlIsNullExpressionWithValue));
      Assert.That (resultWithEntity, Is.Not.SameAs (sqlIsNullExpressionWithValue));

      var expectedResultWithEntity = new SqlIsNullExpression (entityExpression.PrimaryKeyColumn);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResultWithEntity, resultWithEntity);
    }

    [Test]
    public void VisitSqlIsNotNullExpression_AppliesSingleValueSemantics ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var sqlIsNotNullExpressionWithValue = new SqlIsNotNullExpression (Expression.Constant (1));
      var sqlIsNotNullExpressionWithEntity = new SqlIsNotNullExpression (entityExpression);

      var resultWithValue = _nonTopLevelVisitor.VisitSqlIsNotNullExpression (sqlIsNotNullExpressionWithValue);
      var resultWithEntity = _nonTopLevelVisitor.VisitSqlIsNotNullExpression (sqlIsNotNullExpressionWithEntity);

      Assert.That (resultWithValue, Is.SameAs (sqlIsNotNullExpressionWithValue));
      Assert.That (resultWithEntity, Is.Not.SameAs (sqlIsNotNullExpressionWithValue));

      var expectedResultWithEntity = new SqlIsNotNullExpression (entityExpression.PrimaryKeyColumn);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResultWithEntity, resultWithEntity);
    }

    [Test]
    public void VisitSqlEntityConstantExpression_ValueRequired ()
    {
      var expression = new SqlEntityConstantExpression (typeof (int), 5, 1);
      var result = _nonTopLevelVisitor.VisitSqlEntityConstantExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitSqlEntityConstantExpression_SingleValueRequired ()
    {
      var expression = new SqlEntityConstantExpression (typeof (int), 5, 1);

      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var result = nonTopLevelVisitor.VisitSqlEntityConstantExpression (expression);

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();
      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);
      var fakeResult = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      _stageMock
          .Expect (mock => mock.ApplySelectionContext (sqlStatement, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _nonTopLevelVisitor.VisitSqlSubStatementExpression (sqlSubStatementExpression);

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.Not.SameAs (sqlStatement));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_ValueSemantic ()
    {
      var resolvedSimpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "KitchenTable", "k");
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", true);
      var foreignKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var fakeJoinInfo = new ResolvedJoinInfo (resolvedSimpleTableInfo, primaryKeyColumn, foreignKeyColumn);
      var fakeEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, primaryKeyColumn, primaryKeyColumn);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (ji => ji.MemberInfo == memberInfo && ji.OriginatingEntity.Type == typeof (Cook)),
                  Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (fakeJoinInfo);
      _stageMock
          .Expect (mock => mock.ResolveEntityRefMemberExpression (entityRefMemberExpression, fakeJoinInfo, _mappingResolutionContext))
          .Return (fakeEntityExpression);
      _stageMock.Replay();

      var result = _nonTopLevelVisitor.VisitSqlEntityRefMemberExpression (entityRefMemberExpression);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeEntityExpression));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_SingleValueSemantic_PrimaryKeyColumnOnLeftSide ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var resolvedSimpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "KitchenTable", "k");
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", true);
      var foreignKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var fakeJoinInfo = new ResolvedJoinInfo (resolvedSimpleTableInfo, primaryKeyColumn, foreignKeyColumn);
      var fakeEntityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, primaryKeyColumn, primaryKeyColumn);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (ji => ji.MemberInfo == memberInfo && ji.OriginatingEntity.Type == typeof (Cook)),
                  Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (fakeJoinInfo);
      _stageMock
          .Expect (mock => mock.ResolveEntityRefMemberExpression (entityRefMemberExpression, fakeJoinInfo, _mappingResolutionContext))
          .Return (fakeEntityExpression);
      _stageMock.Replay();

      var result = nonTopLevelVisitor.VisitSqlEntityRefMemberExpression (entityRefMemberExpression);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (primaryKeyColumn));
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression_SingleValueSemantic_PrimaryKeyColumnOnRightSide ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var resolvedSimpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "KitchenTable", "k");
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "k", "ID", true);
      var foreignKeyColumn = new SqlColumnDefinitionExpression (typeof (int), "c", "KitchenID", false);
      var fakeJoinInfo = new ResolvedJoinInfo (resolvedSimpleTableInfo, foreignKeyColumn, primaryKeyColumn);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (
                  Arg<UnresolvedJoinInfo>.Matches (ji => ji.MemberInfo == memberInfo && ji.OriginatingEntity.Type == typeof (Cook)),
                  Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (fakeJoinInfo);
      _stageMock.Replay();

      var result = nonTopLevelVisitor.VisitSqlEntityRefMemberExpression (entityRefMemberExpression);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (foreignKeyColumn));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityRefMemberExpression_PredicateSemantic ()
    {
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var memberInfo = typeof (Cook).GetProperty ("ID");
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      _stageMock
          .Expect (
              mock =>
              mock.ResolveJoinInfo (Arg<UnresolvedJoinInfo>.Is.Anything, Arg<IMappingResolutionContext>.Matches (c => c == _mappingResolutionContext)))
          .Return (null);
      _stageMock.Replay();

      SqlContextExpressionVisitor.ApplySqlExpressionContext (
          entityRefMemberExpression, SqlExpressionContext.PredicateRequired, _stageMock, _mappingResolutionContext);
    }

    [Test]
    public void VisitNamedExpression_NoSqlEntityExpression_SameExpression ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var expression = new NamedExpression ("test", Expression.Constant ("test"));

      var result = nonTopLevelVisitor.VisitNamedExpression (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNamedExpression_ReturnsNamedExpression ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var expression = new NamedExpression ("outer", new NamedExpression ("inner", Expression.Constant ("test")));

      var result = nonTopLevelVisitor.VisitNamedExpression (expression);

      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("outer_inner"));
    }

    [Test]
    public void VisitNamedExpression_ReturnsNamedExpression_InnerNameIsNull ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var expression = new NamedExpression ("outer", new NamedExpression (null, Expression.Constant ("test")));

      var result = nonTopLevelVisitor.VisitNamedExpression (expression);

      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("outer"));
    }

    [Test]
    public void VisitNamedExpression_ReturnsNamedExpression_OuterNameIsNull ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var expression = new NamedExpression (null, new NamedExpression ("inner", Expression.Constant ("test")));

      var result = nonTopLevelVisitor.VisitNamedExpression (expression);

      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("inner"));
    }

    [Test]
    public void VisitNamedExpression_NoSqlEntityExpression_DifferentExpression ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var expression = new NamedExpression ("test", Expression.Constant (true));

      var result = nonTopLevelVisitor.VisitNamedExpression (expression);

      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
    }

    [Test]
    public void VisitNamedExpression_SqlEntityExpression ()
    {
      var valueRequiredVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.ValueRequired,
          false,
          _stageMock,
          _mappingResolutionContext);
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), "test2");
      var namedExpression = new NamedExpression ("test", entityExpression);

      var tableRegisteredForEntity = SqlStatementModelObjectMother.CreateSqlTable();
      _mappingResolutionContext.AddSqlEntityMapping (entityExpression, tableRegisteredForEntity);

      var result = valueRequiredVisitor.VisitNamedExpression (namedExpression);

      Assert.That (result, Is.Not.SameAs (namedExpression));
      Assert.That (result, Is.TypeOf (typeof (SqlEntityDefinitionExpression)));
      Assert.That (((SqlEntityDefinitionExpression) result).Name, Is.EqualTo ("test_test2"));
      Assert.That (_mappingResolutionContext.GetSqlTableForEntityExpression ((SqlEntityExpression) result), Is.SameAs (tableRegisteredForEntity));
    }

    [Test]
    public void VisitNamedExpression_NewExpression ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var expression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }),
          new[] { Expression.Constant (0) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));
      var namedExpression = new NamedExpression ("test", expression);

      var result = nonTopLevelVisitor.VisitNamedExpression (namedExpression);

      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NewExpression) result).Members[0].Name, Is.EqualTo ("A"));
      Assert.That (((NewExpression) result).Members.Count, Is.EqualTo (1));
    }

    [Test]
    public void VisitNamedExpression_NewExpression_NamedExpressionsInsideConstructorArgumentsCombined ()
    {
      var expression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }),
          new[] { new NamedExpression ("inner", Expression.Constant (0)) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));
      var namedExpression = new NamedExpression ("outer", expression);

      var result = _nonTopLevelVisitor.VisitNamedExpression (namedExpression);

      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) ((NewExpression) result).Arguments[0]).Name, Is.EqualTo ("outer_inner"));
    }

    [Test]
    public void VisitNamedExpression_AppliesContextToInnerExpression ()
    {
      var innermostExpression = new TestExtensionExpressionWithoutChildren(typeof(bool));
      var expression = new NamedExpression ("test", new NamedExpression ("test2",  innermostExpression));

      var predicateRequiredVisitor = new TestableSqlContextExpressionVisitor (SqlExpressionContext.PredicateRequired, false, _stageMock, _mappingResolutionContext);

      var result = predicateRequiredVisitor.VisitNamedExpression (expression);

      var expectedResult = new NamedExpression ("test_test2", innermostExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitNamedExpression_NewExpression_NoMembers ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var expression = Expression.New (typeof (TypeForNewExpression).GetConstructor(new[]{typeof(int)}), new[] { Expression.Constant (0) });
      var namedExpression = new NamedExpression ("test", expression);

      var result = nonTopLevelVisitor.VisitNamedExpression (namedExpression);

      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NewExpression) result).Members, Is.Null);
    }

    [Test]
    public void VisitNewExpression ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var expression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor(new[]{typeof(int)}),
          new[] { Expression.Constant (0) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      var result = nonTopLevelVisitor.VisitNewExpression (expression);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((NewExpression) result).Members[0].Name, Is.EqualTo ("A"));
      Assert.That (((NewExpression) result).Members.Count, Is.EqualTo (1));
    }

    [Test]
    public void VisitNewExpression_NoMembers ()
    {
      var nonTopLevelVisitor = new TestableSqlContextExpressionVisitor (
          SqlExpressionContext.SingleValueRequired, false, _stageMock, _mappingResolutionContext);
      var expression = Expression.New (typeof (TypeForNewExpression).GetConstructor(new[]{typeof(int)}), new[] { Expression.Constant (0) });
      var result = nonTopLevelVisitor.VisitNewExpression (expression);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((NewExpression) result).Members, Is.Null);
    }

    public static bool FakeAndOperator (bool operand1, bool operand2)
    {
      throw new NotImplementedException();
    }
  }
}