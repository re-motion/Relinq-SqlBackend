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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationQueryModelVisitorTest
  {
    private SqlPreparationContext _context;

    private SelectClause _selectClause;
    private MainFromClause _mainFromClause;
    private QueryModel _queryModel;

    [SetUp]
    public void SetUp ()
    {
      _context = new SqlPreparationContext ();

      _mainFromClause = ExpressionHelper.CreateMainFromClause_Cook();
      _selectClause = ExpressionHelper.CreateSelectClause (_mainFromClause);
      _queryModel = new QueryModel (_mainFromClause, _selectClause);
    }

    [Test]
    public void VisitFromClause_CreatesFromExpression ()
    {
      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.FromExpressions.Count, Is.EqualTo(1));
      Assert.That (result.FromExpressions[0], Is.Not.Null);
      Assert.That (result.FromExpressions[0].TableInfo, Is.TypeOf (typeof (UnresolvedTableInfo)));
      Assert.That (
          ((UnresolvedTableInfo) result.FromExpressions[0].TableInfo).ConstantExpression,
          Is.SameAs (_mainFromClause.FromExpression));
    }

    [Test]
    public void VistAdditionalFromClause_CreatesFromExpression ()
    {
      var constantExpression = Expression.Constant (0);
      var additionalFromClause = new AdditionalFromClause ("additional", typeof (int), constantExpression);
      _queryModel.BodyClauses.Add (additionalFromClause);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.FromExpressions.Count, Is.EqualTo (2));
      Assert.That (result.FromExpressions[1], Is.Not.Null);
      Assert.That (result.FromExpressions[1].TableInfo, Is.TypeOf (typeof (UnresolvedTableInfo)));
      Assert.That (
          ((UnresolvedTableInfo) result.FromExpressions[1].TableInfo).ConstantExpression,
          Is.SameAs (constantExpression));
    }

    [Test]
    public void VisitFromClause_AddMapping ()
    {
      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (_context.GetSqlTableForQuerySource (_mainFromClause), Is.Not.Null);
      Assert.That (_context.GetSqlTableForQuerySource (_mainFromClause), Is.SameAs (result.FromExpressions[0]));
    }

    [Test]
    public void VisitResultOperator_NoWhereClause ()
    {
      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);
      Assert.That (result.WhereCondition, Is.Null);
    }

    [Test]
    public void VisitWhereClause_WithCondition ()
    {
      var predicate = Expression.Constant (true);
      var whereClause = new WhereClause (predicate);
      _queryModel.BodyClauses.Add (whereClause);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.WhereCondition, Is.SameAs (predicate));
    }

    [Test]
    public void VisitWhereClause_ConditionIsPrepared ()
    {
      var isStarredExpression = Expression.MakeMemberAccess (
          new QuerySourceReferenceExpression (_mainFromClause),
          typeof (Cook).GetProperty ("IsStarredCook"));
      var whereClause = new WhereClause (isStarredExpression);
      _queryModel.BodyClauses.Add (whereClause);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.WhereCondition, Is.Not.SameAs (isStarredExpression));
      Assert.That (result.WhereCondition, Is.TypeOf (typeof (SqlMemberExpression)));
      Assert.That (((SqlMemberExpression) result.WhereCondition).SqlTable, Is.SameAs (_context.GetSqlTableForQuerySource (_mainFromClause)));
    }

    [Test]
    public void VisitWhereClause_MulipleWhereClauses ()
    {
      var predicate1 = Expression.Constant (true);
      var whereClause1 = new WhereClause (predicate1);
      _queryModel.BodyClauses.Add (whereClause1);

      var predicate2 = Expression.Constant (true);
      var whereClause2 = new WhereClause (predicate2);
      _queryModel.BodyClauses.Add (whereClause2);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.WhereCondition.NodeType, Is.EqualTo (ExpressionType.AndAlso));
      Assert.That (((BinaryExpression) result.WhereCondition).Left, Is.SameAs (predicate1));
      Assert.That (((BinaryExpression) result.WhereCondition).Right, Is.SameAs (predicate2));
    }

    [Test]
    public void VisitSelectClause_CreatesSelectProjection ()
    {
      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.SelectProjection, Is.Not.Null);
      Assert.That (result.SelectProjection, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That (((SqlTableReferenceExpression) result.SelectProjection).SqlTable, Is.SameAs (_context.GetSqlTableForQuerySource (_mainFromClause)));
    }

    [Test]
    public void VisitResultOperator_NoCount ()
    {
      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);
      Assert.That (result.IsCountQuery, Is.False);
    }
    
    [Test]
    public void VisitResultOperator_Count ()
    {
      var countResultOperator = new CountResultOperator ();
      _queryModel.ResultOperators.Add (countResultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.IsCountQuery, Is.True);
    }

    [Test]
    public void VisitResultOperator_NoDistinct ()
    {
      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);
      Assert.That (result.IsDistinctQuery, Is.False);
    }

    [Test]
    public void VisitResultOperator_Distinct ()
    {
      var distinctResultOperator = new DistinctResultOperator ();
      _queryModel.ResultOperators.Add (distinctResultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.IsDistinctQuery, Is.True);
    }

    [Test]
    public void VisitResultOperator_NoTop ()
    {
      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);
      Assert.That (result.TopExpression, Is.Null);
    }

    [Test]
    public void VisitResultOperator_First ()
    {
      var resultOperator = new FirstResultOperator (false);
      _queryModel.ResultOperators.Add (resultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.TopExpression, Is.Not.Null);
      Assert.That (result.TopExpression, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result.TopExpression).Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitResultOperator_Single ()
    {
      var resultOperator = new SingleResultOperator (false);
      _queryModel.ResultOperators.Add (resultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.TopExpression, Is.Not.Null);
      Assert.That (result.TopExpression, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result.TopExpression).Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitResultOperator_Take ()
    {
      var takeExpression = Expression.Constant (2);
      var resultOperator = new TakeResultOperator (takeExpression);
      _queryModel.ResultOperators.Add (resultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.TopExpression, Is.SameAs (takeExpression));
    }

    [Test]
    public void VisitResultOperator_Take_ExpressionIsPrepared ()
    {
      var idExpression = Expression.MakeMemberAccess (new QuerySourceReferenceExpression (_mainFromClause), typeof (Cook).GetProperty ("ID"));
      var resultOperator = new TakeResultOperator (idExpression);
      _queryModel.ResultOperators.Add (resultOperator);

      var result = SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);

      Assert.That (result.TopExpression, Is.Not.Null);
      Assert.That (result.TopExpression, Is.Not.SameAs (idExpression));
      Assert.That (result.TopExpression, Is.TypeOf (typeof (SqlMemberExpression)));
      Assert.That (((SqlMemberExpression) result.TopExpression).SqlTable, Is.SameAs (_context.GetSqlTableForQuerySource (_mainFromClause)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "DefaultIfEmpty(1) is not supported.")]
    public void VisitResultOperator_NotSupported ()
    {
      var resultOperator = new DefaultIfEmptyResultOperator (Expression.Constant (1));
      _queryModel.ResultOperators.Add (resultOperator);

      SqlPreparationQueryModelVisitor.TransformQueryModel (_queryModel, _context);
    }
  }
}