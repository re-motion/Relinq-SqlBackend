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
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Backend.FieldResolving
{
  [TestFixture]
  public class FieldResolverVisitorTest
  {
    private MainFromClause _studentClause;
    private QuerySourceReferenceExpression _studentReference;

    private MainFromClause _studentDetailClause;
    private QuerySourceReferenceExpression _studentDetailReference;

    [SetUp]
    public void SetUp ()
    {
      _studentClause = ExpressionHelper.CreateMainFromClause_Student ();
      _studentReference = new QuerySourceReferenceExpression (_studentClause);

      _studentDetailClause = ExpressionHelper.CreateMainFromClause_Detail ();
      _studentDetailReference = new QuerySourceReferenceExpression (_studentDetailClause);
    }

    [Test]
    public void QuerySourceReferenceExpression ()
    {
      var result = FieldResolverVisitor.ParseFieldAccess(StubDatabaseInfo.Instance, _studentReference, true);
      Assert.That (result.AccessedMember, Is.Null);
      Assert.That (result.JoinMembers, Is.Empty);
      Assert.That (result.QuerySourceReferenceExpression, Is.SameAs (_studentReference));
    }

    [Test]
    public void NestedQuerySourceReferenceExpression ()
    {
      Expression expressionTree = Expression.MakeMemberAccess (_studentReference, typeof (Cook).GetProperty ("FirstName"));
      FieldAccessInfo result = FieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (typeof (Cook).GetProperty ("FirstName")));
      Assert.That (result.JoinMembers, Is.Empty);
      Assert.That (result.QuerySourceReferenceExpression, Is.SameAs (_studentReference));
    }

    [Test]
    public void NestedMembers ()
    {
      Expression expressionTree = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (_studentDetailReference, typeof (Student_Detail).GetProperty ("Cook")),
          typeof (Cook).GetProperty ("FirstName"));
      FieldAccessInfo result = FieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);

      Assert.That (result.AccessedMember, Is.EqualTo (typeof (Cook).GetProperty ("FirstName")));
      Assert.That (result.JoinMembers, Is.EqualTo (new object[] { typeof (Student_Detail).GetProperty ("Cook") }));
      Assert.That (result.QuerySourceReferenceExpression, Is.SameAs (_studentDetailReference));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "Only MemberExpressions and QuerySourceReferenceExpressions "
        + "can be resolved, found 'null'.")]
    public void InvalidExpression ()
    {
      Expression expressionTree = Expression.Constant (null, typeof (Cook));
      FieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);
    }

    [Test]
    public void VisitMemberExpression_OptimizesAccessToRelatedPrimaryKey ()
    {
      Expression expressionTree = ExpressionHelper.Resolve<Student_Detail, int> (_studentDetailClause, sd => sd.Cook.ID);
      FieldAccessInfo result = FieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student_Detail> (sd => sd.Cook)));
      Assert.IsEmpty (result.JoinMembers);

      Expression optimizedExpressionTree = ExpressionHelper.Resolve<Student_Detail, Cook> (_studentDetailClause, sd => sd.Cook);
      CheckOptimization (result, optimizedExpressionTree);
    }

    [Test]
    public void VisitMemberExpression_AccessToRelatedPrimaryKey_OptimizeFalse ()
    {
      Expression expressionTree = ExpressionHelper.Resolve<Student_Detail, int> (_studentDetailClause, sd => sd.Cook.ID);
      FieldAccessInfo result = FieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, false);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Cook> (s => s.ID)));
      Assert.That (result.JoinMembers, Is.EqualTo (new[] { ExpressionHelper.GetMember<Student_Detail> (sd => sd.Cook) }));
    }

    [Test]
    public void VisitMemberExpression_OptimzationWithRelatedPrimaryKeyOverSeveralSteps ()
    {
      Expression expressionTree = ExpressionHelper.Resolve<Student_Detail, int> (_studentDetailClause, sd => sd.Cook.BuddyCook.ID);
      FieldAccessInfo result = FieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Cook> (s => s.BuddyCook)));
      Assert.That (result.JoinMembers, Is.EqualTo (new[] { ExpressionHelper.GetMember<Student_Detail> (sd => sd.Cook) }));

      Expression optimizedExpressionTree = ExpressionHelper.Resolve<Student_Detail, Cook> (_studentDetailClause, sd => sd.Cook.BuddyCook);
      CheckOptimization (result, optimizedExpressionTree);
    }

    private void CheckOptimization (FieldAccessInfo actualResult, Expression expectedEquivalentOptimization)
    {
      FieldAccessInfo optimizedResult = FieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, 
          expectedEquivalentOptimization, false);
      Assert.That (actualResult.AccessedMember, Is.EqualTo (optimizedResult.AccessedMember));
      Assert.That (actualResult.JoinMembers, Is.EqualTo (optimizedResult.JoinMembers));
    }
  }
}
