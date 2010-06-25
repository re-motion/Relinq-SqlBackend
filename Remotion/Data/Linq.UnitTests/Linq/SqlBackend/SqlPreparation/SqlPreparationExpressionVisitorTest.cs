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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
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
    private MethodCallTransformerRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _context = new SqlPreparationContext();
      _cookMainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      _cookQuerySourceReferenceExpression = new QuerySourceReferenceExpression (_cookMainFromClause);
      var source = new UnresolvedTableInfo (_cookMainFromClause.ItemType);
      _sqlTable = new SqlTable (source);
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

      Assert.That (result, Is.TypeOf (typeof (SqlCaseExpression)));
      Assert.That (((SqlCaseExpression) result).TestPredicate, Is.EqualTo (testPredicate));
      Assert.That (((SqlCaseExpression) result).ThenValue, Is.EqualTo (ifTrueExpression));
      Assert.That (((SqlCaseExpression) result).ElseValue, Is.EqualTo (ifFalseExpression));
    }

    [Test]
    public void VisitConditionalExpression_VisitSubExpressions ()
    {
      var testPredicate = Expression.Condition (Expression.Constant (true), Expression.Constant (true), Expression.Constant (false));
      var ifTrueExpression = Expression.Condition (Expression.Constant (true), Expression.Constant (true), Expression.Constant (false));
      var ifFalseExpression = Expression.Condition (Expression.Constant (true), Expression.Constant (true), Expression.Constant (false));
      var conditionalExpression = Expression.Condition (testPredicate, ifTrueExpression, ifFalseExpression);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (conditionalExpression, _context, _stageMock, _registry);

      Assert.That (result, Is.TypeOf (typeof (SqlCaseExpression)));
      Assert.That (((SqlCaseExpression) result).TestPredicate, Is.TypeOf (typeof (SqlCaseExpression)));
      Assert.That (((SqlCaseExpression) result).ThenValue, Is.TypeOf (typeof (SqlCaseExpression)));
      Assert.That (((SqlCaseExpression) result).ElseValue, Is.TypeOf (typeof (SqlCaseExpression)));
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
      Assert.That (((BinaryExpression) result).Right, Is.TypeOf (typeof (SqlCaseExpression)));

      Assert.That (((SqlCaseExpression) ((BinaryExpression) result).Right).TestPredicate, Is.SameAs (testPredicate));
      Assert.That (((SqlCaseExpression) ((BinaryExpression) result).Right).ThenValue, Is.SameAs (ifTrueExpression));
      Assert.That (((SqlCaseExpression) ((BinaryExpression) result).Right).ElseValue, Is.SameAs (ifFalseExpression));
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
    public void VisitMethodCallExpression_ExpressionPropertiesVisitedBeforeTransformation ()
    {
      var method = MethodCallTransformerUtility.GetInstanceMethod (typeof (object), "ToString");
      var methodCallExpression = Expression.Call (_cookQuerySourceReferenceExpression, method);

      var transformerMock = MockRepository.GenerateMock<IMethodCallTransformer>();
      transformerMock
          .Expect (mock => mock.Transform (Arg<MethodCallExpression>.Matches (m => m.Object is SqlTableReferenceExpression)))
          .Return (methodCallExpression);
      transformerMock.Replay();

      MethodCallTransformerRegistry registry = new MethodCallTransformerRegistry();
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
          .Expect (mock => mock.Transform (Arg<MethodCallExpression>.Matches (m => m.Object is SqlTableReferenceExpression)))
          .Return (_cookQuerySourceReferenceExpression);
      transformerMock.Replay();

      MethodCallTransformerRegistry registry = new MethodCallTransformerRegistry();
      registry.Register (method, transformerMock);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (methodCallExpression, _context, _stageMock, registry);

      Assert.That (result, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      transformerMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitNewExpression ()
    {
      var expression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor(new[]{typeof(int)}),
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
      var expression = Expression.New (typeof (TypeForNewExpression).GetConstructor(new[]{typeof(int)}), new[] { Expression.Constant (0) });

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NewExpression) result).Members, Is.Null);
      Assert.That (((NamedExpression) ((NewExpression) result).Arguments[0]).Name, Is.EqualTo ("m0"));
    }

    [Test]
    public void VisitNewExpression_PreventNestedNamedExpressions_SameName ()
    {
      var namedExpression = new NamedExpression ("A", Expression.Constant (0));
      var expression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }),
          new[] { namedExpression },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock, _registry);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) ((NewExpression) result).Arguments[0]).Expression, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((NamedExpression) ((NewExpression) result).Arguments[0]).Name, Is.EqualTo ("A"));
      Assert.That (((NewExpression) result).Arguments[0], Is.SameAs (namedExpression));
    }
  }
}