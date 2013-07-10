// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationExpressionVisitorTest
  {
    private ISqlPreparationContext _context;
    private MainFromClause _cookMainFromClause;
    private QuerySourceReferenceExpression _cookQuerySourceReferenceExpression;
    private SqlTable _sqlTable;
    private ISqlPreparationStage _stageMock;
    private IMethodCallTransformerProvider _methodCallTransformerProvider;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
      _cookMainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      _cookQuerySourceReferenceExpression = new QuerySourceReferenceExpression (_cookMainFromClause);
      var source = new UnresolvedTableInfo (_cookMainFromClause.ItemType);
      _sqlTable = new SqlTable (source, JoinSemantics.Inner);
      _context.AddExpressionMapping (new QuerySourceReferenceExpression (_cookMainFromClause), new SqlTableReferenceExpression (_sqlTable));
      _methodCallTransformerProvider = CompoundMethodCallTransformerProvider.CreateDefault();
    }

    [Test]
    public void VisitExpression ()
    {
      var visitor = new TestableSqlPreparationExpressionVisitorTest (_context, _stageMock, _methodCallTransformerProvider);
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      _context.AddExpressionMapping (_cookQuerySourceReferenceExpression, tableReferenceExpression);

      var result = visitor.VisitExpression (_cookQuerySourceReferenceExpression);

      Assert.That (result, Is.SameAs (tableReferenceExpression));
    }

    [Test]
    public void VisitQuerySourceReferenceExpression_CreatesSqlTableReferenceExpression ()
    {
      var result = SqlPreparationExpressionVisitor.TranslateExpression (_cookQuerySourceReferenceExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((SqlTableReferenceExpression) result).SqlTable, Is.SameAs (_sqlTable));
      Assert.That (result.Type, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void VisitNotSupportedExpression_ThrowsNotImplentedException ()
    {
      var expression = new CustomExpression (typeof (int));
      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.EqualTo (expression));
    }

    [Test]
    public void VisitSubqueryExpressionTest_WithNoSqlTables ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      var querModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      var expression = new SubQueryExpression (querModel);
      var fakeSqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement())
                                    {
                                        IsDistinctQuery = false
                                    };
      fakeSqlStatementBuilder.SqlTables.Clear();
      var fakeSqlStatement = fakeSqlStatementBuilder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.PrepareSqlStatement (querModel, _context))
          .Return (fakeSqlStatement);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.SameAs (fakeSqlStatement.SelectProjection));
    }

    [Test]
    public void VisitSubqueryExpressionTest_WithSqlTables ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      var querModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      var expression = new SubQueryExpression (querModel);
      var fakeSqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement());
      fakeSqlStatementBuilder.SqlTables.Add (_sqlTable);
      var fakeSqlStatement = fakeSqlStatementBuilder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.PrepareSqlStatement (querModel, _context))
          .Return (fakeSqlStatement);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (fakeSqlStatement));
    }

    [Test]
    public void VisitSubqueryExpressionTest_IsCountQuery ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      var querModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      var expression = new SubQueryExpression (querModel);
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      var fakeSqlStatementBuilder = new SqlStatementBuilder (sqlStatement)
                                    {
                                        SelectProjection =
                                            new AggregationExpression (typeof (int), sqlStatement.SelectProjection, AggregationModifier.Count)
                                    };

      var fakeSqlStatement = fakeSqlStatementBuilder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.PrepareSqlStatement (querModel, _context))
          .Return (fakeSqlStatement);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (fakeSqlStatement));
      Assert.That (
          ((AggregationExpression) ((SqlSubStatementExpression) result).SqlStatement.SelectProjection).AggregationModifier,
          Is.EqualTo (AggregationModifier.Count));
    }

    [Test]
    public void VisitSubqueryExpressionTest_IsDistinctQuery ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      var querModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      var expression = new SubQueryExpression (querModel);
      var fakeSqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement())
                                    {
                                        IsDistinctQuery = true
                                    };

      var fakeSqlStatement = fakeSqlStatementBuilder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.PrepareSqlStatement (querModel, _context))
          .Return (fakeSqlStatement);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (fakeSqlStatement));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement.IsDistinctQuery, Is.True);
    }

    [Test]
    public void VisitSubqueryExpressionTest_RevisitsResult ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      var querModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      var expression = new SubQueryExpression (querModel);
      var fakeSqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement())
                                    {
                                        TopExpression = null
                                    };
      fakeSqlStatementBuilder.Orderings.Add (new Ordering (Expression.Constant ("order"), OrderingDirection.Asc));
      fakeSqlStatementBuilder.SqlTables.Add (SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook)));
      var fakeSqlStatement = fakeSqlStatementBuilder.GetSqlStatement();
      fakeSqlStatementBuilder.Orderings.Clear();
      var expectedStatement = fakeSqlStatementBuilder.GetSqlStatement();

      _stageMock
          .Expect (mock => mock.PrepareSqlStatement (querModel, _context))
          .Return (fakeSqlStatement);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.EqualTo (expectedStatement));
    }

    [Test]
    public void VisitSqlSubStatmentExpression_NoTopExpression_ReturnsSame ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      builder.Orderings.Clear();
      var sqlStatement = builder.GetSqlStatement();

      var subStatementExpression = new SqlSubStatementExpression (sqlStatement);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (subStatementExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.SameAs (subStatementExpression));
    }

    [Test]
    public void VisitSqlSubStatmentExpression_HasTopExpression_ReturnsNew ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      builder.Orderings.Add (new Ordering (Expression.Constant ("order"), OrderingDirection.Asc));
      var sqlStatement = builder.GetSqlStatement();
      builder.Orderings.Clear();
      var expectedStatement = builder.GetSqlStatement();

      var subStatementExpression = new SqlSubStatementExpression (sqlStatement);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (subStatementExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.Not.SameAs (subStatementExpression));
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.EqualTo (expectedStatement));
    }

    [Test]
    public void VisitMemberExpression_WithNoPoperty ()
    {
      var memberExpression = Expression.Field (Expression.Constant (new TypeWithMember<int> (5)), "Field");

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.SameAs (memberExpression));
    }

    [Test]
    public void VisitMemberExpression_WithProperty_NotRegistered ()
    {
      var memberExpression = Expression.Property (Expression.Constant (new TypeWithMember<int> (5)), "Property");

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.SameAs (memberExpression));
    }

    [Test]
    public void VisitMemberExpression_WithProperty_Registered ()
    {
      var stringExpression = Expression.Constant ("test");
      var memberExpression = Expression.Property (stringExpression, "Length");

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expectedResult = new SqlLengthExpression (stringExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMemberExpression_WithInnerSqlCaseExpression ()
    {
      var testPredicate1 = Expression.Constant (true);
      var testPredicate2 = Expression.Constant (false);
      var value1 = Expression.Constant (new TypeWithMember<int> (1));
      var value2 = Expression.Constant (new TypeWithMember<int> (2));
      var elseValue = Expression.Constant (new TypeWithMember<int> (3));

      var caseExpression = new SqlCaseExpression (
          typeof (TypeWithMember<int>),
          new[] { new SqlCaseExpression.CaseWhenPair (testPredicate1, value1), new SqlCaseExpression.CaseWhenPair (testPredicate2, value2) },
          elseValue);
      var memberExpression = Expression.Property (caseExpression, "Property");

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expectedResult = new SqlCaseExpression (
          typeof (int),
          new[]
          {
              new SqlCaseExpression.CaseWhenPair (testPredicate1, Expression.Property (value1, "Property")),
              new SqlCaseExpression.CaseWhenPair (testPredicate2, Expression.Property (value2, "Property"))
          },
          Expression.Property (elseValue, "Property"));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMemberExpression_WithInnerSqlCaseExpression_NoElse ()
    {
      var predicate = Expression.Constant (true);
      var valueTypeValue = Expression.Constant (new TypeWithMember<int> (1));
      var nullableValueTypeValue = Expression.Constant (new TypeWithMember<int?> (1));
      var referenceTypeValue = Expression.Constant (new TypeWithMember<string> ("hoy"));

      var valueTypeMemberExpression = CreateMemberExpressionWithInnerSqlCaseExpression<int> (predicate, valueTypeValue, null);
      var nullableValueTypeMemberExpression = CreateMemberExpressionWithInnerSqlCaseExpression<int?> (predicate, nullableValueTypeValue, null);
      var referenceTypeMemberExpression = CreateMemberExpressionWithInnerSqlCaseExpression<string> (predicate, referenceTypeValue, null);

      var valueTypeResult = SqlPreparationExpressionVisitor.TranslateExpression (
          valueTypeMemberExpression, _context, _stageMock, _methodCallTransformerProvider);
      var nullableValueTypeResult = SqlPreparationExpressionVisitor.TranslateExpression (
          nullableValueTypeMemberExpression, _context, _stageMock, _methodCallTransformerProvider);
      var referenceTypeResult = SqlPreparationExpressionVisitor.TranslateExpression (
          referenceTypeMemberExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expectedValueTypeResult = CreateSqlCaseExpressionWithInnerMemberExpressionNoElse<int?> (predicate, valueTypeValue);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedValueTypeResult, valueTypeResult);
      var expectedNullableValueTypeResult = CreateSqlCaseExpressionWithInnerMemberExpressionNoElse<int?> (predicate, nullableValueTypeValue);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNullableValueTypeResult, nullableValueTypeResult);
      var expectedReferenceTypeResult = CreateSqlCaseExpressionWithInnerMemberExpressionNoElse<string> (predicate, referenceTypeValue);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReferenceTypeResult, referenceTypeResult);
    }

    [Test]
    public void VisitMemberExpression_WithInnerSqlCaseExpression_RevisitsResult ()
    {
      var predicate = Expression.Constant (true);
      var value = Expression.Constant ("value1");
      var elseValue = Expression.Constant ("value2");

      var caseExpression = new SqlCaseExpression (typeof (string), new[] { new SqlCaseExpression.CaseWhenPair (predicate, value) }, elseValue);
      var memberExpression = Expression.Property (caseExpression, "Length");

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expectedResult = new SqlCaseExpression (
          typeof (int),
          new[] { new SqlCaseExpression.CaseWhenPair (predicate, new SqlLengthExpression (value)) },
          new SqlLengthExpression (elseValue));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMemberExpression_WithInnerCoalesceExpression ()
    {
      var left = Expression.Constant (new TypeWithMember<int> (1));
      var right = Expression.Constant (new TypeWithMember<int> (2));
      var coalesceExpression = Expression.Coalesce (left, right);
      var memberExpression = Expression.Property (coalesceExpression, "Property");

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expectedResult = SqlCaseExpression.CreateIfThenElse (
          typeof (int),
          new SqlIsNotNullExpression (coalesceExpression.Left),
          Expression.Property (coalesceExpression.Left, "Property"),
          Expression.Property (coalesceExpression.Right, "Property"));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMemberExpression_WithInnerCoalesceExpression_RevisitsResult ()
    {
      var left = Expression.Constant("left");
      var right = Expression.Constant("right");
      var coalesceExpression = Expression.Coalesce (left, right);
      var memberExpression = Expression.Property (coalesceExpression, "Length");

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expectedResult = SqlCaseExpression.CreateIfThenElse (
          typeof (int),
          new SqlIsNotNullExpression (coalesceExpression.Left), 
          new SqlLengthExpression(coalesceExpression.Left), 
          new SqlLengthExpression (coalesceExpression.Right));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMemberExpression_WithInnerSqlSubStatementExpression()
    {
      var selectProjection = Expression.Constant (new Cook());
      var fakeStatement = SqlStatementModelObjectMother.CreateSqlStatement (
          new NamedExpression("test", selectProjection), new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"),JoinSemantics.Left));
      var memberInfo = typeof (Cook).GetProperty ("Name");
      var queryModel = ExpressionHelper.CreateQueryModel_Cook();
      queryModel.ResultOperators.Add (new FirstResultOperator (false));
      var expression = new SubQueryExpression(queryModel);
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);

      _stageMock
          .Expect (mock => mock.PrepareSqlStatement (queryModel, _context))
          .Return (fakeStatement);
      _stageMock.Replay();

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _methodCallTransformerProvider);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      var expectedSelectProjection = new NamedExpression("test", Expression.MakeMemberAccess (selectProjection, memberInfo));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, ((SqlSubStatementExpression) result).SqlStatement.SelectProjection);
      Assert.That (((SqlSubStatementExpression) result).SqlStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement.DataInfo.DataType, Is.EqualTo(typeof (IQueryable<>).MakeGenericType(typeof(string))));
    }

    [Test]
    public void VisitBinaryExpression ()
    {
      var binaryExpression = Expression.And (Expression.Constant (1), Expression.Constant (1));
      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.SameAs (binaryExpression));
    }

    [Test]
    public void VisitBinaryExpression_QuerySourceReferenceExpressionsOnBothSide ()
    {
      var binaryExpression = Expression.Equal (_cookQuerySourceReferenceExpression, _cookQuerySourceReferenceExpression);
      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((BinaryExpression) result).Left, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((BinaryExpression) result).Right, Is.TypeOf (typeof (SqlTableReferenceExpression)));
    }

    [Test]
    public void VisitBinaryExpression_ReturnsSqlIsNullExpression_NullLeft ()
    {
      var leftExpression = Expression.Constant (null);
      var binaryExpression = Expression.Equal (leftExpression, _cookQuerySourceReferenceExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlIsNullExpression)));
      Assert.That (((SqlIsNullExpression) result).Expression, Is.TypeOf (typeof (SqlTableReferenceExpression)));
    }

    [Test]
    public void VisitBinaryExpression_ReturnsSqlIsNullExpression_NullRight ()
    {
      var leftExpression = Expression.Constant ("1");
      var rightExpression = Expression.Constant (null);
      var binaryExpression = Expression.Equal (leftExpression, rightExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlIsNullExpression)));
      Assert.That (((SqlIsNullExpression) result).Expression, Is.SameAs (leftExpression));
    }

    [Test]
    public void VisitBinaryExpression_ReturnsSqlIsNotNullExpression_NullLeft ()
    {
      var leftExpression = Expression.Constant (null);
      var rightExpression = Expression.Constant ("1");
      var binaryExpression = Expression.NotEqual (leftExpression, rightExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlIsNotNullExpression)));
      Assert.That (((SqlIsNotNullExpression) result).Expression, Is.SameAs (rightExpression));
    }

    [Test]
    public void VisitBinaryExpression_ReturnsSqlIsNotNullExpression_NullRight ()
    {
      var rightExpression = Expression.Constant (null);
      var leftExpression = Expression.Constant ("1");
      var binaryExpression = Expression.NotEqual (leftExpression, rightExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlIsNotNullExpression)));
      Assert.That (((SqlIsNotNullExpression) result).Expression, Is.SameAs (leftExpression));
    }

    [Test]
    public void VisitBinaryExpression_NotEqual_WithNullOnRightSide ()
    {
      var rightExpression = Expression.Constant (null);
      var binaryExpression = Expression.Coalesce (_cookQuerySourceReferenceExpression, rightExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((BinaryExpression) result).Left, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((BinaryExpression) result).Right, Is.TypeOf (typeof (ConstantExpression)));
    }

    [Test]
    public void VisitBinaryExpression_NotEqual_WithNullOnLeftSide ()
    {
      var leftExpression = Expression.Constant (null);
      var binaryExpression = Expression.Coalesce (leftExpression, _cookQuerySourceReferenceExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((BinaryExpression) result).Left, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((BinaryExpression) result).Right, Is.TypeOf (typeof (SqlTableReferenceExpression)));
    }

    [Test]
    public void VisitConditionalExpression ()
    {
      var testPredicate = Expression.Constant (true);
      var ifTrueExpression = Expression.Constant ("true");
      var ifFalseExpression = Expression.Constant ("false");
      var conditionalExpression = Expression.Condition (testPredicate, ifTrueExpression, ifFalseExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (conditionalExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlCaseExpression)));
      var sqlCaseExpression = (SqlCaseExpression) result;
      Assert.That (sqlCaseExpression.Type, Is.SameAs (typeof (string)));
      Assert.That (sqlCaseExpression.Cases, Has.Count.EqualTo (1));
      Assert.That (sqlCaseExpression.Cases[0].When, Is.EqualTo (testPredicate));
      Assert.That (sqlCaseExpression.Cases[0].Then, Is.EqualTo (ifTrueExpression));
      Assert.That (sqlCaseExpression.ElseCase, Is.EqualTo (ifFalseExpression));
    }

    [Test]
    public void VisitConditionalExpression_VisitsSubExpressions ()
    {
      var testPredicate = Expression.Equal (Expression.Constant ("test"), Expression.Constant (null, typeof (string)));
      var ifTrueExpression = Expression.Equal (Expression.Constant ("test"), Expression.Constant (null, typeof (string)));
      var ifFalseExpression = Expression.Equal (Expression.Constant ("test"), Expression.Constant (null, typeof (string)));
      var conditionalExpression = Expression.Condition (testPredicate, ifTrueExpression, ifFalseExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (conditionalExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlCaseExpression)));
      var sqlCaseExpression = (SqlCaseExpression) result;
      Assert.That (sqlCaseExpression.Cases, Has.Count.EqualTo (1));
      Assert.That (sqlCaseExpression.Cases[0].When, Is.TypeOf<SqlIsNullExpression>());
      Assert.That (sqlCaseExpression.Cases[0].Then, Is.TypeOf<SqlIsNullExpression> ());
      Assert.That (sqlCaseExpression.ElseCase, Is.TypeOf<SqlIsNullExpression> ());
    }

    [Test]
    public void VisitBinaryExpression_WithConditionalExpressionInBinaryExpression ()
    {
      var leftExpression = Expression.Constant ("Name");
      var testPredicate = Expression.Constant (true);
      var ifTrueExpression = Expression.Constant ("true");
      var ifFalseExpression = Expression.Constant ("false");
      var rightExpression = Expression.Condition (testPredicate, ifTrueExpression, ifFalseExpression);
      var binaryExpression = Expression.Equal (leftExpression, rightExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expectedResult = Expression.Equal (
          leftExpression, SqlCaseExpression.CreateIfThenElse (typeof (string), testPredicate, ifTrueExpression, ifFalseExpression));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMethodCallExpression ()
    {
      var method = typeof (string).GetMethod ("ToUpper", new Type[] { });
      var constantExpression = Expression.Constant ("Test");
      var methodCallExpression = Expression.Call (constantExpression, method);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (methodCallExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.TypeOf (typeof (SqlFunctionExpression)));
      Assert.That (((SqlFunctionExpression) result).SqlFunctioName, Is.EqualTo ("UPPER"));
      Assert.That (((SqlFunctionExpression) result).Args[0], Is.SameAs (constantExpression));
      Assert.That (((SqlFunctionExpression) result).Args.Count, Is.EqualTo (1));
    }

    [Test]
    public void VisitMethodCallExpression_TransformerNotRegistered_WrapsArgumentsIntoNamedExpressions ()
    {
      var methodCallExpression = Expression.Call (
          Expression.Constant (0), 
          ReflectionUtility.GetMethod (() => 0.ToString ("", null)), 
          Expression.Constant (""),
          Expression.Constant (null, typeof (IFormatProvider)));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (methodCallExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expected = Expression.Call (
          new NamedExpression ("Object", Expression.Constant (0)), 
          ReflectionUtility.GetMethod (() => 0.ToString ("", null)), 
          new NamedExpression ("Arg0", Expression.Constant ("")),
          new NamedExpression ("Arg1", Expression.Constant (null, typeof (IFormatProvider))));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitMethodCallExpression_TransformerNotRegistered_NoObject_WrapsArgumentsIntoNamedExpressions ()
    {
      var methodCallExpression = Expression.Call (
          ReflectionUtility.GetMethod (() => int.Parse ("")),
          Expression.Constant (""));
      
      var result = SqlPreparationExpressionVisitor.TranslateExpression (methodCallExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expected = Expression.Call (
          ReflectionUtility.GetMethod (() => int.Parse ("")),
          new NamedExpression ("Arg0", Expression.Constant ("")));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void VisitMethodCallExpression_ExpressionPropertiesVisitedBeforeTransformation ()
    {
      var method = MethodCallTransformerUtility.GetInstanceMethod (typeof (object), "ToString");
      var methodCallExpression = Expression.Call (_cookQuerySourceReferenceExpression, method);

      var transformerMock = MockRepository.GenerateMock<IMethodCallTransformer>();
      transformerMock
          .Expect (mock => mock.Transform (Arg.Is(methodCallExpression)))
          .Return (methodCallExpression);
      transformerMock.Replay();

      var registry = new MethodInfoBasedMethodCallTransformerRegistry();
      registry.Register (method, transformerMock);

      SqlPreparationExpressionVisitor.TranslateExpression (methodCallExpression, _context, _stageMock, registry);

      transformerMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMethodCallExpression_ExpressionPropertiesVisitedAfterTransformation ()
    {
      var method = MethodCallTransformerUtility.GetInstanceMethod (typeof (object), "ToString");
      var methodCallExpression = Expression.Call (_cookQuerySourceReferenceExpression, method);

      var transformerMock = MockRepository.GenerateMock<IMethodCallTransformer>();
      transformerMock
          .Expect (mock => mock.Transform (Arg.Is(methodCallExpression)))
          .Return (_cookQuerySourceReferenceExpression);
      transformerMock.Replay();

      var registry = new MethodInfoBasedMethodCallTransformerRegistry();
      registry.Register (method, transformerMock);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (methodCallExpression, _context, _stageMock, registry);

      Assert.That (result, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      transformerMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitNewExpression ()
    {
      var expression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)),
          new[] { Expression.Constant (0) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NewExpression) result).Members[0].Name, Is.EqualTo ("A"));
      Assert.That (((NewExpression) result).Members.Count, Is.EqualTo (1));
      Assert.That (((NamedExpression) ((NewExpression) result).Arguments[0]).Name, Is.EqualTo ("A"));
    }

    [Test]
    public void VisitNewExpression_NoMembers ()
    {
      var expression = Expression.New (TypeForNewExpression.GetConstructor (typeof (int)), new[] { Expression.Constant (0) });

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NewExpression) result).Members, Is.Null);
      Assert.That (((NamedExpression) ((NewExpression) result).Arguments[0]).Name, Is.EqualTo ("m0"));
    }

    [Test]
    public void VisitPartialEvaluationExceptionExpression_StripsExceptionAndVisitsInnerExpression ()
    {
      var evaluatedExpression = Expression.Property (Expression.Constant (""), "Length");
      var exception = new InvalidOperationException ("What?");
      var expression = new PartialEvaluationExceptionExpression (exception, evaluatedExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _methodCallTransformerProvider);

      var expectedExpression = new SqlLengthExpression (evaluatedExpression.Expression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void VisitConstantExpression_WithNoCollection_LeavesExpressionUnchanged ()
    {
      var constantExpression = Expression.Constant ("string");

      var result = SqlPreparationExpressionVisitor.TranslateExpression (constantExpression, _context, _stageMock, _methodCallTransformerProvider);

      Assert.That (result, Is.SameAs (constantExpression));
    }

    [Test]
    public void VisitConstantExpression_WithCollection_ReturnsSqlCollectionExpression ()
    {
      var constantExpression = Expression.Constant (new[] { 1, 2, 3 });

      var result = SqlPreparationExpressionVisitor.TranslateExpression (constantExpression, _context, _stageMock, _methodCallTransformerProvider);

      var expectedExpression = new SqlCollectionExpression (
          typeof (int[]), new[] { Expression.Constant (1), Expression.Constant (2), Expression.Constant (3) });
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    private MemberExpression CreateMemberExpressionWithInnerSqlCaseExpression<TMemberType> (Expression when, Expression then, Expression elseCase)
    {
      var valueTypeCaseExpression = new SqlCaseExpression (
          typeof (TypeWithMember<TMemberType>),
          new[] { new SqlCaseExpression.CaseWhenPair (when, then) },
          elseCase);
      var valueTypeMemberExpression = Expression.Property (valueTypeCaseExpression, "Property");
      return valueTypeMemberExpression;
    }

    private SqlCaseExpression CreateSqlCaseExpressionWithInnerMemberExpressionNoElse<TCaseType> (Expression when, Expression thenWithProperty)
    {
      return new SqlCaseExpression (
          typeof (TCaseType),
          new[] { new SqlCaseExpression.CaseWhenPair (when, Expression.Property (thenWithProperty, "Property")) },
          null);
    }

    class TypeWithMember<T>
    {
      private readonly T _t;

      [UsedImplicitly]
      public T Field = default (T);

      public TypeWithMember (T t)
      {
        _t = t;
      }

      [UsedImplicitly]
      public T Property
      {
        get { return _t; }
      }
    }
    
  }
}