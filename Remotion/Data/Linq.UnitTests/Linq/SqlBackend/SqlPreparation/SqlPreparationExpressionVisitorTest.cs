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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationExpressionVisitorTest
  {
    private SqlPreparationContext _context;

    private MainFromClause _cookMainFromClause;
    private QuerySourceReferenceExpression _cookQuerySourceReferenceExpression;

    private MainFromClause _kitchenMainFromClause;

    private SqlTable _sqlTable;
    private ISqlPreparationStage _stageMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _context = new SqlPreparationContext();
      _cookMainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      _cookQuerySourceReferenceExpression = new QuerySourceReferenceExpression (_cookMainFromClause);
      _kitchenMainFromClause = ExpressionHelper.CreateMainFromClause_Kitchen();
      var source = new UnresolvedTableInfo (_cookMainFromClause.ItemType);
      _sqlTable = new SqlTable (source);
      _context.AddQuerySourceMapping (_cookMainFromClause, _sqlTable);
    }

    [Test]
    public void VisitQuerySourceReferenceExpression_CreatesSqlTableReferenceExpression ()
    {
      var result = SqlPreparationExpressionVisitor.TranslateExpression (_cookQuerySourceReferenceExpression, _context, _stageMock);

      Assert.That (result, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((SqlTableReferenceExpression) result).SqlTable, Is.SameAs (_sqlTable));
      Assert.That (result.Type, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void VisitMemberExpression_CreatesSqlMemberExpression ()
    {
      Expression memberExpression = Expression.MakeMemberAccess (_cookQuerySourceReferenceExpression, typeof (Cook).GetProperty ("FirstName"));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock);

      Assert.That (result, Is.TypeOf (typeof (SqlMemberExpression)));
      Assert.That (((SqlMemberExpression) result).SqlTable, Is.SameAs (_sqlTable));
      Assert.That (result.Type, Is.SameAs (typeof (string)));
    }

    [Test]
    public void VisitSeveralMemberExpression_CreatesSqlMemberExpression_AndJoin ()
    {
      var expression = ExpressionHelper.Resolve<Kitchen, string> (_kitchenMainFromClause, k => k.Cook.FirstName);

      var source = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Kitchen));
      var sqlTable = new SqlTable (source);

      _context.AddQuerySourceMapping (_kitchenMainFromClause, sqlTable);

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock);

      var kitchenCookMember = typeof (Kitchen).GetProperty ("Cook");
      var cookFirstNameMember = typeof (Cook).GetProperty ("FirstName");

      Assert.That (result, Is.TypeOf (typeof (SqlMemberExpression)));
      Assert.That (((SqlMemberExpression) result).MemberInfo, Is.EqualTo (cookFirstNameMember));

      var join = sqlTable.GetJoin (kitchenCookMember);
      Assert.That (((SqlMemberExpression) result).SqlTable, Is.SameAs (join));

      Assert.That (join.JoinInfo, Is.TypeOf (typeof (UnresolvedJoinInfo)));
      Assert.That (((UnresolvedJoinInfo) join.JoinInfo).MemberInfo, Is.EqualTo (kitchenCookMember));
      Assert.That (((UnresolvedJoinInfo) join.JoinInfo).Cardinality, Is.EqualTo (JoinCardinality.One));
    }

    [Test]
    public void VisitMemberExpression_NonQuerySourceReferenceExpression ()
    {
      var memberExpression = Expression.MakeMemberAccess (Expression.Constant ("Test"), typeof (string).GetProperty ("Length"));
      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context, _stageMock);

      Assert.That (result, Is.EqualTo (memberExpression));
    }


    [Test]
    public void VisitNotSupportedExpression_ThrowsNotImplentedException ()
    {
      var expression = new CustomExpression (typeof (int));
      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock);

      Assert.That (result, Is.EqualTo (expression));
    }

    [Test]
    public void VisitSubQueryExpression ()
    {
      var querModel = ExpressionHelper.CreateQueryModel (_kitchenMainFromClause);
      var expression = new SubQueryExpression (querModel);
      var fakeSqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));

      _stageMock
          .Expect (mock => mock.PrepareSqlStatement (querModel))
          .Return (fakeSqlStatement);
      _stageMock.Replay();

      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) result).SqlStatement, Is.SameAs (fakeSqlStatement));
      Assert.That (result.Type, Is.EqualTo (expression.Type));
    }

    [Test]
    public void VisitSubQueryExpression_WithContains ()
    {
      var querModel = ExpressionHelper.CreateQueryModel (_kitchenMainFromClause);
      var constantExpression = Expression.Constant (new Kitchen());
      var containsResultOperator = new ContainsResultOperator (constantExpression);
      querModel.ResultOperators.Add (containsResultOperator);
      var expression = new SubQueryExpression (querModel);
      var fakeSqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));

      _stageMock
          .Expect (mock => mock.PrepareSqlStatement (Arg<QueryModel>.Matches(q=> q.ResultOperators.Count == 0)))
          .Return (fakeSqlStatement);
      _stageMock.Replay ();
      
      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context, _stageMock);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (SqlInExpression)));
      Assert.That (((SqlInExpression) result).RightExpression, Is.TypeOf (typeof(SqlSubStatementExpression)));
      Assert.That (((SqlSubStatementExpression) ((SqlInExpression) result).RightExpression).SqlStatement, Is.EqualTo (fakeSqlStatement));
      Assert.That (((SqlInExpression) result).LeftExpression, Is.SameAs(constantExpression));
      Assert.That (result.Type, Is.EqualTo (((SqlInExpression) result).RightExpression.Type));
    }
  }
}