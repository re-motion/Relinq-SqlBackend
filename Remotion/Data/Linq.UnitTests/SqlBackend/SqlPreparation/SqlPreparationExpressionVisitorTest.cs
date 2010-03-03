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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationExpressionVisitorTest
  {
    private SqlPreparationContext _context;
    private MainFromClause _mainFromClause;
    private QuerySourceReferenceExpression _querySourceReferenceExpression;
    private SqlTable _sqlTable;


    [SetUp]
    public void SetUp ()
    {
      _context = new SqlPreparationContext();
      _mainFromClause = ClauseObjectMother.CreateMainFromClause ();
      _querySourceReferenceExpression = new QuerySourceReferenceExpression (_mainFromClause);
      var source = new ConstantTableSource ((ConstantExpression) _mainFromClause.FromExpression);
      _sqlTable = new SqlTable (source);
      _context.AddQuerySourceMapping (_mainFromClause, _sqlTable);
    }

    [Test]
    public void VisitQuerySourceReferenceExpression_CreatesSqlTableReferenceExpression ()
    {
      var result = SqlPreparationExpressionVisitor.TranslateExpression (_querySourceReferenceExpression, _context);

      Assert.That (result, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((SqlTableReferenceExpression) result).SqlTable, Is.SameAs (_sqlTable));
      Assert.That (result.Type, Is.SameAs (typeof (Cook[])));
    }

    [Test]
    public void VisitMemberExpression_CreatesSqlMemberExpression ()
    {
      Expression memberExpression = Expression.MakeMemberAccess (_querySourceReferenceExpression, typeof (Cook).GetProperty ("FirstName"));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (memberExpression, _context);

      Assert.That (result, Is.TypeOf (typeof(SqlMemberExpression)));
      Assert.That (((SqlMemberExpression) result).SqlTable, Is.SameAs (_sqlTable));
      // TODO: Check Member
      Assert.That (result.Type, Is.SameAs (typeof (Cook[]))); // TODO: should be typeof (string) because FirstName returns string
    }

    [Test]
    public void VisitSeveralMemberExpression_CreatesSqlMemberExpressions ()
    {
      Kitchen[] kitchen = new Kitchen[1];
      kitchen[0] = new Kitchen { Name = "Test" };

      var mainFromClause = new MainFromClause ("k", typeof (Kitchen), Expression.Constant (kitchen));  // TODO: Use ExpressionHelper.CreateMainFromClause_Kitchen
      var source = new ConstantTableSource ((ConstantExpression) mainFromClause.FromExpression); // TODO: Add object mother method CreateSqlTable (mainFromClause)
      var sqlTable = new SqlTable (source);
      var context = new SqlPreparationContext ();
      context.AddQuerySourceMapping (mainFromClause, sqlTable);
      
      // TODO: Use ExpressionHelper.Resolve<Kitchen, string> (mainFromClause, k => k.Cook.FirstName) to get the nested MemberExpression
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (mainFromClause);
      MemberExpression innerExpression = Expression.MakeMemberAccess (querySourceReferenceExpression, typeof (Kitchen).GetMember("Cook")[0]);
      Expression outerExpression = Expression.MakeMemberAccess (innerExpression, typeof (Cook).GetProperty ("FirstName"));

      var result = SqlPreparationExpressionVisitor.TranslateExpression (outerExpression, context);

      // TODO: Assert.That (result, Is.TypeOf (SqlMemberExpression));
      // TODO: var expectedJoin = sqlTable.GetOrAddJoin (typeof (Kitchen).GetProperty ("Cook"));
      // TODO: Assert.That (result.SqlTable, Is.SameAs (expectedJoin));
      // TODO: Assert.That (result.Member, Is.EqualTo (typeof (Cook).GetProperty ("FirstName"));
      //
      // TODO: Assert.That (expectedJoin.TableSource, Is.TypeOf (JoinedTableSource));
      // TODO: Assert.That (((JoinedTableSource) expectedJoin.TableSource).Member, Is.EqualTo (typeof (Kitchen).GetProperty ("Cook")));

      // TODO: Remove this
      var tableSource = ((SqlMemberExpression) result).SqlTable.TableSource;
      Assert.That (result, Is.TypeOf (typeof (SqlMemberExpression)));
      Assert.That (tableSource, Is.EqualTo (source));
    }

    // TODO: Add test showing that MemberExpressions on non-QuerySourceReferenceExpressions (e.g. "constant".Length) are left unchanged

    [Test]
    public void VisitNotSupportedExpression_ThrowsNotImplentedException ()
    {
      var expression = new NotSupportedExpression (typeof (int));
      var result = SqlPreparationExpressionVisitor.TranslateExpression (expression, _context);

      Assert.That (result, Is.EqualTo (expression));
    }

  }
}