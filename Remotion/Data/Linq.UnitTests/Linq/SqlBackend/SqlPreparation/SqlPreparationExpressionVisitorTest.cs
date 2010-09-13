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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationExpressionVisitorTest
  {
    private ISqlPreparationContext _context;
    private MainFromClause _cookMainFromClause;
    private QuerySourceReferenceExpression _cookQuerySourceReferenceExpression;
    private SqlTable _sqlTable;
    private ISqlPreparationStage _stageMock;
    private IMethodCallTransformerRegistry _registry;

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
      _registry = MethodCallTransformerRegistry.CreateDefault();
    }

    [Test]
    public void VisitExpression ()
    {
      var visitor = new TestableSqlPreparationExpressionVisitorTest (_context, _stageMock, _registry);
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      _context.AddExpressionMapping (_cookQuerySourceReferenceExpression, tableReferenceExpression);

      var result = visitor.VisitExpression (_cookQuerySourceReferenceExpression);

      Assert.That (result, Is.SameAs (tableReferenceExpression));
    }

    [Test]
    public void VisitQuerySourceReferenceExpression_CreatesSqlTableReferenceExpression ()
    {
      var result = SqlPreparationExpressionVisitor.TranslateExpression (_cookQuerySourceReferenceExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((SqlTableReferenceExpression) result).SqlTable, Is.SameAs (_sqlTable));
      Assert.That (result.Type, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void VisitNotSupportedExpression_ThrowsNotImplentedException ()
    {
      var expression = new CustomExpression (typeof (int));
      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (subStatementExpression, _context, _stageMock, _registry);

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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (subStatementExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.Not.SameAs (subStatementExpression));
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.EqualTo (expectedStatement));
    }

    [Test]
    public void VisitMemberExpression_WithNoPoperty ()
    {
      var memberExpression = MemberExpression.MakeMemberAccess (
          Expression.Constant (new TypeForNewExpression (5)), typeof (TypeForNewExpression).GetField ("C"));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.SameAs (memberExpression));
    }

    [Test]
    public void VisitMemberExpression_WithProperty_NotRegistered ()
    {
      var memberExpression = MemberExpression.MakeMemberAccess (
          Expression.Constant (new TypeForNewExpression (5)), typeof (TypeForNewExpression).GetProperty ("B"));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.SameAs (memberExpression));
    }

    [Test]
    public void VisitMemberExpression_WithProperty_Registered ()
    {
      var stringExpression = Expression.Constant ("test");
      var memberExpression = MemberExpression.MakeMemberAccess (stringExpression, typeof (String).GetProperty ("Length"));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _registry);

      var expectedResult = new SqlFunctionExpression (typeof (int), "LEN", stringExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMemberExpression_WithInnerConditionalExpression ()
    {
      var testPredicate = Expression.Constant (true);
      var thenValue = Expression.Constant (new TypeForNewExpression(1));
      var elseValue = Expression.Constant (new TypeForNewExpression(2));
      var conditionalExpression = Expression.Condition (testPredicate, thenValue, elseValue);
      var memberInfo = typeof (TypeForNewExpression).GetProperty ("A");
      var memberExpression = Expression.MakeMemberAccess (conditionalExpression, memberInfo);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _registry);
      
      var expectedResult = Expression.Condition(
          testPredicate, Expression.MakeMemberAccess (thenValue, memberInfo), Expression.MakeMemberAccess (elseValue, memberInfo));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMemberExpression_WithInnerConditionalExpression_RevisitsResult ()
    {
      var testPredicate = Expression.Constant (true);
      var thenValue = Expression.Constant ("testValue");
      var elseValue = Expression.Constant ("elseValue");
      var conditionalExpression = Expression.Condition (testPredicate, thenValue, elseValue);
      var memberInfo = typeof (string).GetProperty ("Length");
      var memberExpression = Expression.MakeMemberAccess (conditionalExpression, memberInfo);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _registry);

      var expectedResult = Expression.Condition(
          testPredicate, new SqlFunctionExpression (typeof (int), "LEN", thenValue), new SqlFunctionExpression (typeof (int), "LEN", elseValue));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMemberExpression_WithInnerCoalesceExpression ()
    {
      var left = Expression.Constant (new TypeForNewExpression (1));
      var right = Expression.Constant (new TypeForNewExpression (2));
      var coalesceExpression = Expression.Coalesce (left, right);
      var memberInfo = typeof (TypeForNewExpression).GetProperty ("A");
      var memberExpression = Expression.MakeMemberAccess (coalesceExpression, memberInfo);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _registry);

      var expectedResult = Expression.Condition (
          new SqlIsNotNullExpression (coalesceExpression.Left),
          Expression.MakeMemberAccess (coalesceExpression.Left, memberInfo),
          Expression.MakeMemberAccess (coalesceExpression.Right, memberInfo));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void VisitMemberExpression_WithInnerCoalesceExpression_RevisitsResult ()
    {
      var left = Expression.Constant("left");
      var right = Expression.Constant("right");
      var coalesceExpression = Expression.Coalesce (left, right);
      var memberInfo = typeof (string).GetProperty ("Length");
      var memberExpression = Expression.MakeMemberAccess (coalesceExpression, memberInfo);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _registry);

      var expectedResult = Expression.Condition (
          new SqlIsNotNullExpression (coalesceExpression.Left),
          new SqlFunctionExpression(typeof(int), "LEN", coalesceExpression.Left),
          new SqlFunctionExpression (typeof (int), "LEN", coalesceExpression.Right));

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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock, _registry);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      // TODO Review 3088: Use an expected expression for the SelectProjection
      Assert.That (((NamedExpression) ((SqlSubStatementExpression) result).SqlStatement.SelectProjection).Expression, Is.TypeOf(typeof(MemberExpression)));
      var resultMemberExpression =
          (MemberExpression) ((NamedExpression) ((SqlSubStatementExpression) result).SqlStatement.SelectProjection).Expression;
      Assert.That (resultMemberExpression.Expression, Is.SameAs (selectProjection));
      Assert.That (resultMemberExpression.Member, Is.SameAs (memberInfo));
      
      Assert.That (((SqlSubStatementExpression) result).SqlStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement.DataInfo.DataType, Is.EqualTo(typeof (IQueryable<>).MakeGenericType(typeof(string))));
    }

    [Test]
    public void VisitBinaryExpression ()
    {
      var binaryExpression = Expression.And (Expression.Constant (1), Expression.Constant (1));
      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.SameAs (binaryExpression));
    }

    [Test]
    public void VisitBinaryExpression_QuerySourceReferenceExpressionsOnBothSide ()
    {
      var binaryExpression = Expression.Equal (_cookQuerySourceReferenceExpression, _cookQuerySourceReferenceExpression);
      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((BinaryExpression) result).Left, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((BinaryExpression) result).Right, Is.TypeOf (typeof (SqlTableReferenceExpression)));
    }

    [Test]
    public void VisitBinaryExpression_ReturnsSqlIsNullExpression_NullLeft ()
    {
      var leftExpression = Expression.Constant (null);
      var binaryExpression = Expression.Equal (leftExpression, _cookQuerySourceReferenceExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (SqlIsNullExpression)));
      Assert.That (((SqlIsNullExpression) result).Expression, Is.TypeOf (typeof (SqlTableReferenceExpression)));
    }

    [Test]
    public void VisitBinaryExpression_ReturnsSqlIsNullExpression_NullRight ()
    {
      var leftExpression = Expression.Constant ("1");
      var rightExpression = Expression.Constant (null);
      var binaryExpression = Expression.Equal (leftExpression, rightExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (SqlIsNullExpression)));
      Assert.That (((SqlIsNullExpression) result).Expression, Is.SameAs (leftExpression));
    }

    [Test]
    public void VisitBinaryExpression_ReturnsSqlIsNotNullExpression_NullLeft ()
    {
      var leftExpression = Expression.Constant (null);
      var rightExpression = Expression.Constant ("1");
      var binaryExpression = Expression.NotEqual (leftExpression, rightExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (SqlIsNotNullExpression)));
      Assert.That (((SqlIsNotNullExpression) result).Expression, Is.SameAs (rightExpression));
    }

    [Test]
    public void VisitBinaryExpression_ReturnsSqlIsNotNullExpression_NullRight ()
    {
      var rightExpression = Expression.Constant (null);
      var leftExpression = Expression.Constant ("1");
      var binaryExpression = Expression.NotEqual (leftExpression, rightExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (SqlIsNotNullExpression)));
      Assert.That (((SqlIsNotNullExpression) result).Expression, Is.SameAs (leftExpression));
    }

    [Test]
    public void VisitBinaryExpression_NotEqual_WithNullOnRightSide ()
    {
      var rightExpression = Expression.Constant (null);
      var binaryExpression = Expression.Coalesce (_cookQuerySourceReferenceExpression, rightExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((BinaryExpression) result).Left, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((BinaryExpression) result).Right, Is.TypeOf (typeof (ConstantExpression)));
    }

    [Test]
    public void VisitBinaryExpression_NotEqual_WithNullOnLeftSide ()
    {
      var leftExpression = Expression.Constant (null);
      var binaryExpression = Expression.Coalesce (leftExpression, _cookQuerySourceReferenceExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _registry);

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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (conditionalExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (ConditionalExpression)));
      Assert.That (((ConditionalExpression) result).Test, Is.EqualTo (testPredicate));
      Assert.That (((ConditionalExpression) result).IfTrue, Is.EqualTo (ifTrueExpression));
      Assert.That (((ConditionalExpression) result).IfFalse, Is.EqualTo (ifFalseExpression));
    }

    [Test]
    public void VisitConditionalExpression_VisitSubExpressions ()
    {
      var testPredicate = Expression.Condition (Expression.Constant (true), Expression.Constant (true), Expression.Constant (false));
      var ifTrueExpression = Expression.Condition (Expression.Constant (true), Expression.Constant (true), Expression.Constant (false));
      var ifFalseExpression = Expression.Condition (Expression.Constant (true), Expression.Constant (true), Expression.Constant (false));
      var conditionalExpression = Expression.Condition (testPredicate, ifTrueExpression, ifFalseExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (conditionalExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (ConditionalExpression)));
      Assert.That (((ConditionalExpression) result).Test, Is.TypeOf (typeof (ConditionalExpression)));
      Assert.That (((ConditionalExpression) result).IfTrue, Is.TypeOf (typeof (ConditionalExpression)));
      Assert.That (((ConditionalExpression) result).IfFalse, Is.TypeOf (typeof (ConditionalExpression)));
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

      var result = SqlPreparationExpressionVisitor.TranslateExpression (binaryExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((BinaryExpression) result).Left, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((BinaryExpression) result).Right, Is.TypeOf (typeof (ConditionalExpression)));

      Assert.That (((ConditionalExpression) ((BinaryExpression) result).Right).Test, Is.SameAs (testPredicate));
      Assert.That (((ConditionalExpression) ((BinaryExpression) result).Right).IfTrue, Is.SameAs (ifTrueExpression));
      Assert.That (((ConditionalExpression) ((BinaryExpression) result).Right).IfFalse, Is.SameAs (ifFalseExpression));
    }

    [Test]
    public void VisitMethodCallExpression ()
    {
      var method = typeof (string).GetMethod ("ToUpper", new Type[] { });
      var constantExpression = Expression.Constant ("Test");
      var methodCallExpression = Expression.Call (constantExpression, method);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (methodCallExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (SqlFunctionExpression)));
      Assert.That (((SqlFunctionExpression) result).SqlFunctioName, Is.EqualTo ("UPPER"));
      Assert.That (((SqlFunctionExpression) result).Args[0], Is.SameAs (constantExpression));
      Assert.That (((SqlFunctionExpression) result).Args.Count, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The method 'System.String.Concat' is not supported by this code "
                                                                          + "generator, and no custom transformer has been registered. "
                                                                          + "Expression: '\"Test\".Concat(\"Test\")'")]
    public void VisitMethodCallExpression_TransformerNotRegistered_ThrowsException ()
    {
      var method = typeof (string).GetMethod ("Concat", new[] {typeof(string)});
      var constantExpression = Expression.Constant ("Test");
      var methodCallExpression = Expression.Call (constantExpression, method, constantExpression);

      SqlPreparationExpressionVisitor.TranslateExpression (methodCallExpression, _context, _stageMock, _registry);
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
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }),
          new[] { Expression.Constant (0) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

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
      var expression = Expression.New (typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), new[] { Expression.Constant (0) });

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NewExpression) result).Members, Is.Null);
      Assert.That (((NamedExpression) ((NewExpression) result).Arguments[0]).Name, Is.EqualTo ("m0"));
    }
    
  }
}