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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;
using Rhino.Mocks;
using Mocks_Is = Rhino.Mocks.Constraints.Is;
using Mocks_List = Rhino.Mocks.Constraints.List;

namespace Remotion.Data.Linq.UnitTests.Backend.FieldResolving
{
  [TestFixture]
  public class FieldResolverTest
  {
    private JoinedTableContext _context;
    private IResolveFieldAccessPolicy _policy;

    private MainFromClause _studentClause;
    private QuerySourceReferenceExpression _studentReference;

    private MainFromClause _studentDetailClause;
    private QuerySourceReferenceExpression _studentDetailReference;

    private MainFromClause _studentDetailDetailClause;
    private QuerySourceReferenceExpression _studentDetailDetailReference;
    
    private PropertyInfo _studentDetail_Student_Property;
    private PropertyInfo _studentDetailDetail_StudentDetail_Property;
    private PropertyInfo _student_ID_Property;
    private PropertyInfo _student_OtherStudent_Property;
    private PropertyInfo _student_First_Property;

    private MemberExpression _student_First_Expression;
    private MemberExpression _studentDetail_Student_Expression;
    private MemberExpression _studentDetail_Student_First_Expression;
    private MemberExpression _studentDetailDetail_StudentDetail_Student_First_Expression;
    private MemberExpression _studentDetailDetail_StudentDetail_Student_Expression;
    private MemberExpression _studentDetailDetail_StudentDetail_Expression;

    private MockRepository _mockRepository;
    private IResolveFieldAccessPolicy _policyMock;

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext(StubDatabaseInfo.Instance);
      _policy = new SelectFieldAccessPolicy();
      _studentClause = ExpressionHelper.CreateMainFromClause_Student ();
      _studentReference = new QuerySourceReferenceExpression (_studentClause);

      _studentDetailClause = ExpressionHelper.CreateMainFromClause_Detail ();
      _studentDetailReference = new QuerySourceReferenceExpression (_studentDetailClause);

      _studentDetailDetailClause = ExpressionHelper.CreateMainFromClause_Detail_Detail ();
      _studentDetailDetailReference = new QuerySourceReferenceExpression (_studentDetailDetailClause);

      _student_ID_Property = typeof (Student).GetProperty ("ID");
      _student_First_Property = typeof (Student).GetProperty ("First");
      _student_OtherStudent_Property = typeof (Student).GetProperty ("OtherStudent");
      _studentDetail_Student_Property = typeof (Student_Detail).GetProperty ("Student");
      _studentDetailDetail_StudentDetail_Property = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");

      _student_First_Expression = Expression.MakeMemberAccess (_studentReference, _student_First_Property);
      _studentDetail_Student_Expression = Expression.MakeMemberAccess (_studentDetailReference, _studentDetail_Student_Property);
      _studentDetail_Student_First_Expression = Expression.MakeMemberAccess (_studentDetail_Student_Expression, _student_First_Property);
      _studentDetailDetail_StudentDetail_Expression = 
          Expression.MakeMemberAccess (_studentDetailDetailReference, _studentDetailDetail_StudentDetail_Property);
      _studentDetailDetail_StudentDetail_Student_Expression = 
          Expression.MakeMemberAccess (_studentDetailDetail_StudentDetail_Expression, _studentDetail_Student_Property);
      _studentDetailDetail_StudentDetail_Student_First_Expression =
          Expression.MakeMemberAccess (_studentDetailDetail_StudentDetail_Student_Expression, _student_First_Property);

      _mockRepository = new MockRepository ();
      _policyMock = _mockRepository.StrictMock<IResolveFieldAccessPolicy> ();
    }

    [Test]
    public void Resolve_QuerySourceReferenceExpression_Succeeds ()
    {
      // s

      IColumnSource table = _context.GetColumnSource (_studentClause);
      FieldDescriptor fieldDescriptor = new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentReference, _context);

      var column = new Column (table, "*");
      var expected = new FieldDescriptor (null, new FieldSourcePath (table, new SingleJoin[0]), column);

      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void Resolve_QuerySourceReferenceExpression_WithJoin ()
    {
      var joinClause = ExpressionHelper.CreateJoinClause();
      var referenceExpression = new QuerySourceReferenceExpression (joinClause);
      new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (referenceExpression, _context);
    }

    [Test]
    public void Resolve_SimpleMemberAccess_Succeeds ()
    {
      // s.First

      FieldDescriptor fieldDescriptor = 
          new FieldResolver (StubDatabaseInfo.Instance, _policy)
          .ResolveField (_student_First_Expression, _context);
      Assert.That (fieldDescriptor.Column, Is.EqualTo (new Column (new Table ("studentTable", "s"), "FirstColumn")));
    }

    [Test]
    public void Resolve_Join ()
    {
      // sd.Student.First

      FieldDescriptor fieldDescriptor =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetail_Student_First_Expression, _context);

      Assert.That (fieldDescriptor.Column, Is.EqualTo (new Column (new Table ("studentTable", null), "FirstColumn")));
      Assert.That (fieldDescriptor.Member, Is.EqualTo (_student_First_Property));

      IColumnSource expectedSourceTable = fieldDescriptor.SourcePath.FirstSource;
      var expectedRelatedTable = new Table ("studentTable", null);
      var join = new SingleJoin (
          new Column (expectedSourceTable, "Student_Detail_PK"), new Column (expectedRelatedTable, "Student_Detail_to_Student_FK"));
      var expectedPath = new FieldSourcePath (expectedSourceTable, new[] { join });

      Assert.That (fieldDescriptor.SourcePath, Is.EqualTo (expectedPath));
    }

    [Test]
    public void Resolve_DoubleJoin ()
    {
      // sdd.Student_Detail.Student.First

      FieldDescriptor fieldDescriptor =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetailDetail_StudentDetail_Student_First_Expression, _context);

      Assert.That (fieldDescriptor.Column, Is.EqualTo (new Column (new Table ("studentTable", null), "FirstColumn")));
      Assert.That (fieldDescriptor.Member, Is.EqualTo (_student_First_Property));

      IColumnSource expectedDetailDetailTable = fieldDescriptor.SourcePath.FirstSource;
      var expectedDetailTable = new Table ("detailTable", null); // Student_Detail
      var join1 = new SingleJoin (
          new Column (expectedDetailDetailTable, "Student_Detail_Detail_PK"),
          new Column (expectedDetailTable, "Student_Detail_Detail_to_Student_Detail_FK"));

      var expectedStudentTable = new Table ("studentTable", null); // Student
      var join2 = new SingleJoin (
          new Column (expectedDetailTable, "Student_Detail_PK"), new Column (expectedStudentTable, "Student_Detail_to_Student_FK"));

      var expectedPath = new FieldSourcePath (expectedDetailDetailTable, new[] { join1, join2 });
      Assert.That (fieldDescriptor.SourcePath, Is.EqualTo (expectedPath));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = 
        "'Remotion.Data.Linq.UnitTests.TestDomain.Student.First' is not a relation member.")]
    public void Resolve_Join_InvalidMember ()
    {
      // s.First.Length
      Expression fieldExpression =
          Expression.MakeMemberAccess (
              _student_First_Expression,
              typeof (string).GetProperty ("Length"));

      new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (fieldExpression, _context);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage =
        "The member 'Remotion.Data.Linq.UnitTests.TestDomain.Student.NonDBProperty' does not identify a queryable column.")]
    public void Resolve_SimpleMemberAccess_InvalidField ()
    {
      Expression fieldExpression = Expression.MakeMemberAccess (
          _studentReference,
          typeof (Student).GetProperty ("NonDBProperty"));

      new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (fieldExpression, _context);

    }

    [Test]
    public void Resolve_UsesContext ()
    {
      // sd.Student.First

      FieldDescriptor fieldDescriptor1 =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetail_Student_First_Expression, _context);
      FieldDescriptor fieldDescriptor2 =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetail_Student_First_Expression, _context);

      IColumnSource table1 = fieldDescriptor1.SourcePath.Joins[0].RightSide;
      IColumnSource table2 = fieldDescriptor2.SourcePath.Joins[0].RightSide;

      Assert.That (table2, Is.SameAs (table1));
    }

    [Test]
    public void Resolve_EntityField_Simple ()
    {
      //sd.Student

      FieldDescriptor fieldDescriptor =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetail_Student_Expression, _context);

      IColumnSource detailTable = _context.GetColumnSource (_studentDetailClause);
      Table studentTable = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetTableForRelation (_studentDetail_Student_Property, null);
      var joinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentDetail_Student_Property);
      var join = new SingleJoin (new Column (detailTable, joinColumns.Value.PrimaryKey), new Column (studentTable, joinColumns.Value.ForeignKey));
      var column = new Column (studentTable, "*");

      var expected = new FieldDescriptor (_studentDetail_Student_Property, new FieldSourcePath (detailTable, new[] { join }), column);
      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    public void Resolve_EntityField_Nested ()
    {
      //sdd.Student_Detail.Student

      FieldDescriptor fieldDescriptor =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetailDetail_StudentDetail_Student_Expression, _context);

      IColumnSource detailDetailTable = _context.GetColumnSource (_studentDetailDetailClause);
      Table detailTable = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetTableForRelation (_studentDetailDetail_StudentDetail_Property, null);
      Table studentTable = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetTableForRelation (_studentDetail_Student_Property, null);
      var innerJoinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentDetailDetail_StudentDetail_Property);
      var outerJoinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentDetail_Student_Property);

      var join1 = new SingleJoin (new Column (detailDetailTable, innerJoinColumns.Value.PrimaryKey), new Column (detailTable, innerJoinColumns.Value.ForeignKey));
      var join2 = new SingleJoin (new Column (detailTable, outerJoinColumns.Value.PrimaryKey), new Column (studentTable, outerJoinColumns.Value.ForeignKey));
      var column = new Column (studentTable, "*");

      var expected = new FieldDescriptor (_studentDetail_Student_Property, new FieldSourcePath (detailDetailTable, new[] { join1, join2 }), column);
      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    public void Resolve_FieldFromSubQuery ()
    {
      // from x in (...)
      // select x.ID
      var fromClause = new AdditionalFromClause ("x", typeof (Student), new SubQueryExpression (ExpressionHelper.CreateQueryModel_Student ()));

      PropertyInfo member = typeof (Student).GetProperty ("ID");
      Expression fieldExpression = Expression.MakeMemberAccess (new QuerySourceReferenceExpression (fromClause), member);

      FieldDescriptor fieldDescriptor =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (fieldExpression, _context);
      var subQuery = (SubQuery) _context.GetColumnSource (fromClause);
      var column = new Column (subQuery, "IDColumn");
      var expected = new FieldDescriptor (member, new FieldSourcePath (subQuery, new SingleJoin[0]), column);

      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    public void Resolver_UsesPolicyToAdjustRelationMembers ()
    {
      _policyMock.Expect (mock => mock.OptimizeRelatedKeyAccess ()).Return (false);
      var newJoinMembers = new[] { _studentDetailDetail_StudentDetail_Property, _studentDetail_Student_Property };
      _policyMock.Expect (
          mock => mock.AdjustMemberInfosForRelation (Arg<IEnumerable<MemberInfo>>.List.Equal (new[] { _studentDetailDetail_StudentDetail_Property }), Arg.Is<MemberInfo> (_studentDetail_Student_Property)))
          .Return (new MemberInfoChain (newJoinMembers, _student_ID_Property));

      _policyMock.Replay();

      FieldDescriptor actualFieldDescriptor = new FieldResolver (StubDatabaseInfo.Instance, _policyMock)
          .ResolveField (_studentDetailDetail_StudentDetail_Student_Expression, _context);
      
      _policyMock.VerifyAllExpectations();

      var expectedPath = new FieldSourcePathBuilder().BuildFieldSourcePath (
          StubDatabaseInfo.Instance,
          _context,
          _context.GetColumnSource (_studentDetailDetailClause),
          newJoinMembers);
      var expectedFieldDescriptor = new FieldDescriptor (_studentDetail_Student_Property, expectedPath, new Column (expectedPath.LastSource, "IDColumn"));
      Assert.That (actualFieldDescriptor, Is.EqualTo (expectedFieldDescriptor));
    }

    [Test]
    public void Resolver_UsesPolicyToAdjustFromIdentifierAccess ()
    {
      _policyMock.Expect (mock => mock.OptimizeRelatedKeyAccess()).Return (false);

      Expression fieldExpression = _studentDetailDetailReference;

      var newJoinMembers = new[] { _student_OtherStudent_Property };
      _policyMock
          .Expect (mock => mock.AdjustMemberInfosForDirectAccessOfQuerySource (_studentDetailDetailReference))
          .Return (new MemberInfoChain (newJoinMembers, _student_ID_Property));

      _policyMock.Replay ();

      FieldDescriptor actualFieldDescriptor = 
          new FieldResolver (StubDatabaseInfo.Instance, _policyMock)
          .ResolveField (fieldExpression, _context);
      
      _policyMock.VerifyAllExpectations();

      FieldSourcePath path = new FieldSourcePathBuilder().BuildFieldSourcePath (
          StubDatabaseInfo.Instance,
          _context,
          _context.GetColumnSource (_studentDetailDetailClause),
          newJoinMembers);
      var expectedFieldDescriptor = new FieldDescriptor (null, path, new Column (path.LastSource, "IDColumn"));
      Assert.That (actualFieldDescriptor, Is.EqualTo (expectedFieldDescriptor));
    }

    [Test]
    public void Resolver_PolicyOptimization_True ()
    {
      IResolveFieldAccessPolicy policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      Assert.That (policy.OptimizeRelatedKeyAccess(), Is.True);
      Expression fieldExpression = ExpressionHelper.Resolve<Student_Detail, int> (_studentDetailClause, sd => sd.IndustrialSector.ID);

      var resolver = new FieldResolver (StubDatabaseInfo.Instance, policy);

      FieldDescriptor result = resolver.ResolveField (fieldExpression, _context);

      Assert.That (
          result.Column, Is.EqualTo (new Column (_context.GetColumnSource (_studentDetailClause), "Student_Detail_to_IndustrialSector_FK")));
    }

    [Test]
    public void Resolver_PolicyOptimization_False ()
    {
      IResolveFieldAccessPolicy policy = new SelectFieldAccessPolicy();
      Assert.That (policy.OptimizeRelatedKeyAccess(), Is.False);
      Expression fieldExpression = ExpressionHelper.Resolve<Student_Detail, int>(_studentDetailClause, sd => sd.IndustrialSector.ID);

      var resolver = new FieldResolver (StubDatabaseInfo.Instance, policy);
      FieldDescriptor result = resolver.ResolveField (fieldExpression, _context);

      Assert.That (result.Column, Is.EqualTo (new Column (result.SourcePath.LastSource, "IDColumn")));
    }
  }
}
