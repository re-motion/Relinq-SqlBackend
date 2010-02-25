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
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Backend.FieldResolving
{
  [TestFixture]
  public class FieldResolverTest
  {
    private JoinedTableContext _context;
    private IResolveFieldAccessPolicy _policy;

    private MainFromClause _cookClause;
    private QuerySourceReferenceExpression _cookReference;

    private MainFromClause _kitchenClause;
    private QuerySourceReferenceExpression _kitchenReference;

    private MainFromClause _companyClause;
    private QuerySourceReferenceExpression _companyReference;
    
    private PropertyInfo _kitchen_Cook_Property;
    private PropertyInfo _company_Kitchen_Property;
    private PropertyInfo _cook_ID_Property;
    private PropertyInfo _cook_Substitution_Property;
    private PropertyInfo _cook_FirstName_Property;

    private MemberExpression _cook_FirstName_Expression;
    private MemberExpression _kitchen_Cook_Expression;
    private MemberExpression _kitchen_Cook_FirstName_Expression;
    private MemberExpression _company_kitchen_Cook_FirstName_Expression;
    private MemberExpression _company_Kitchen_Cook_Expression;
    private MemberExpression _company_Kitchen_Expression;

    private MockRepository _mockRepository;
    private IResolveFieldAccessPolicy _policyMock;

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext(StubDatabaseInfo.Instance);
      _policy = new SelectFieldAccessPolicy();
      _cookClause = ExpressionHelper.CreateMainFromClause_Cook ();
      _cookReference = new QuerySourceReferenceExpression (_cookClause);

      _kitchenClause = ExpressionHelper.CreateMainFromClause_Kitchen ();
      _kitchenReference = new QuerySourceReferenceExpression (_kitchenClause);

      _companyClause = ExpressionHelper.CreateMainFromClause_Detail_Detail ();
      _companyReference = new QuerySourceReferenceExpression (_companyClause);

      _cook_ID_Property = typeof (Cook).GetProperty ("ID");
      _cook_FirstName_Property = typeof (Cook).GetProperty ("FirstName");
      _cook_Substitution_Property = typeof (Cook).GetProperty ("Substitution");
      _kitchen_Cook_Property = typeof (Kitchen).GetProperty ("Cook");
      _company_Kitchen_Property = typeof (Company).GetProperty ("MainKitchen");

      _cook_FirstName_Expression = Expression.MakeMemberAccess (_cookReference, _cook_FirstName_Property);
      _kitchen_Cook_Expression = Expression.MakeMemberAccess (_kitchenReference, _kitchen_Cook_Property);
      _kitchen_Cook_FirstName_Expression = Expression.MakeMemberAccess (_kitchen_Cook_Expression, _cook_FirstName_Property);
      _company_Kitchen_Expression = 
          Expression.MakeMemberAccess (_companyReference, _company_Kitchen_Property);
      _company_Kitchen_Cook_Expression = 
          Expression.MakeMemberAccess (_company_Kitchen_Expression, _kitchen_Cook_Property);
      _company_kitchen_Cook_FirstName_Expression =
          Expression.MakeMemberAccess (_company_Kitchen_Cook_Expression, _cook_FirstName_Property);

      _mockRepository = new MockRepository ();
      _policyMock = _mockRepository.StrictMock<IResolveFieldAccessPolicy> ();
    }

    [Test]
    public void Resolve_QuerySourceReferenceExpression_Succeeds ()
    {
      // s

      IColumnSource table = _context.GetColumnSource (_cookClause);
      FieldDescriptor fieldDescriptor = new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_cookReference, _context);

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
      // s.FirstName

      FieldDescriptor fieldDescriptor = 
          new FieldResolver (StubDatabaseInfo.Instance, _policy)
          .ResolveField (_cook_FirstName_Expression, _context);
      Assert.That (fieldDescriptor.Column, Is.EqualTo (new Column (new Table ("cookTable", "s"), "FirstNameColumn")));
    }

    [Test]
    public void Resolve_Join ()
    {
      // sd.Cook.FirstName

      FieldDescriptor fieldDescriptor =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_kitchen_Cook_FirstName_Expression, _context);

      Assert.That (fieldDescriptor.Column, Is.EqualTo (new Column (new Table ("cookTable", null), "FirstNameColumn")));
      Assert.That (fieldDescriptor.Member, Is.EqualTo (_cook_FirstName_Property));

      IColumnSource expectedSourceTable = fieldDescriptor.SourcePath.FirstSource;
      var expectedRelatedTable = new Table ("cookTable", null);
      var join = new SingleJoin (
          new Column (expectedSourceTable, "Kitchen_PK"), new Column (expectedRelatedTable, "Kitchen_to_Cook_FK"));
      var expectedPath = new FieldSourcePath (expectedSourceTable, new[] { join });

      Assert.That (fieldDescriptor.SourcePath, Is.EqualTo (expectedPath));
    }

    [Test]
    public void Resolve_DoubleJoin ()
    {
      // sdd.MainKitchen.Cook.FirstName

      FieldDescriptor fieldDescriptor =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_company_kitchen_Cook_FirstName_Expression, _context);

      Assert.That (fieldDescriptor.Column, Is.EqualTo (new Column (new Table ("cookTable", null), "FirstNameColumn")));
      Assert.That (fieldDescriptor.Member, Is.EqualTo (_cook_FirstName_Property));

      IColumnSource expectedDetailDetailTable = fieldDescriptor.SourcePath.FirstSource;
      var expectedDetailTable = new Table ("kitchenTable", null); // MainKitchen
      var join1 = new SingleJoin (
          new Column (expectedDetailDetailTable, "Company_PK"),
          new Column (expectedDetailTable, "Company_to_Kitchen_FK"));

      var expectedCookTable = new Table ("cookTable", null); // Cook
      var join2 = new SingleJoin (
          new Column (expectedDetailTable, "Kitchen_PK"), new Column (expectedCookTable, "Kitchen_to_Cook_FK"));

      var expectedPath = new FieldSourcePath (expectedDetailDetailTable, new[] { join1, join2 });
      Assert.That (fieldDescriptor.SourcePath, Is.EqualTo (expectedPath));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = 
        "'Remotion.Data.Linq.UnitTests.TestDomain.Cook.FirstName' is not a relation member.")]
    public void Resolve_Join_InvalidMember ()
    {
      // s.FirstName.Length
      Expression fieldExpression =
          Expression.MakeMemberAccess (
              _cook_FirstName_Expression,
              typeof (string).GetProperty ("Length"));

      new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (fieldExpression, _context);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage =
        "The member 'Remotion.Data.Linq.UnitTests.TestDomain.Cook.NonDBStringProperty' does not identify a queryable column.")]
    public void Resolve_SimpleMemberAccess_InvalidField ()
    {
      Expression fieldExpression = Expression.MakeMemberAccess (
          _cookReference,
          typeof (Cook).GetProperty ("NonDBStringProperty"));

      new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (fieldExpression, _context);

    }

    [Test]
    public void Resolve_UsesContext ()
    {
      // sd.Cook.FirstName

      FieldDescriptor fieldDescriptor1 =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_kitchen_Cook_FirstName_Expression, _context);
      FieldDescriptor fieldDescriptor2 =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_kitchen_Cook_FirstName_Expression, _context);

      IColumnSource table1 = fieldDescriptor1.SourcePath.Joins[0].RightSide;
      IColumnSource table2 = fieldDescriptor2.SourcePath.Joins[0].RightSide;

      Assert.That (table2, Is.SameAs (table1));
    }

    [Test]
    public void Resolve_EntityField_Simple ()
    {
      //sd.Cook

      FieldDescriptor fieldDescriptor =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_kitchen_Cook_Expression, _context);

      IColumnSource kitchenTable = _context.GetColumnSource (_kitchenClause);
      Table cookTable = StubDatabaseInfo.Instance.GetTableForRelation (_kitchen_Cook_Property, null);
      var join = StubDatabaseInfo.Instance.GetJoinForMember (_kitchen_Cook_Property, kitchenTable, cookTable);
      var column = new Column (cookTable, "*");

      var expected = new FieldDescriptor (_kitchen_Cook_Property, new FieldSourcePath (kitchenTable, new[] { join }), column);
      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    public void Resolve_EntityField_Nested ()
    {
      //sdd.MainKitchen.Cook

      FieldDescriptor fieldDescriptor =
          new FieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_company_Kitchen_Cook_Expression, _context);

      IColumnSource companyTable = _context.GetColumnSource (_companyClause);
      Table kitchenTable = StubDatabaseInfo.Instance.GetTableForRelation (_company_Kitchen_Property, null);
      Table cookTable = StubDatabaseInfo.Instance.GetTableForRelation (_kitchen_Cook_Property, null);
      var join1 = StubDatabaseInfo.Instance.GetJoinForMember (_company_Kitchen_Property, companyTable, kitchenTable);
      var join2 = StubDatabaseInfo.Instance.GetJoinForMember (_kitchen_Cook_Property, kitchenTable, cookTable);

      var column = new Column (cookTable, "*");

      var expected = new FieldDescriptor (_kitchen_Cook_Property, new FieldSourcePath (companyTable, new[] { join1, join2 }), column);
      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    public void Resolve_FieldFromSubQuery ()
    {
      // from x in (...)
      // select x.ID
      var fromClause = new AdditionalFromClause ("x", typeof (Cook), new SubQueryExpression (ExpressionHelper.CreateQueryModel_Cook ()));

      PropertyInfo member = typeof (Cook).GetProperty ("ID");
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
      var newJoinMembers = new[] { _company_Kitchen_Property, _kitchen_Cook_Property };
      _policyMock.Expect (
          mock => mock.AdjustMemberInfosForRelation (Arg<IEnumerable<MemberInfo>>.List.Equal (new[] { _company_Kitchen_Property }), Arg.Is<MemberInfo> (_kitchen_Cook_Property)))
          .Return (new MemberInfoChain (newJoinMembers, _cook_ID_Property));

      _policyMock.Replay();

      FieldDescriptor actualFieldDescriptor = new FieldResolver (StubDatabaseInfo.Instance, _policyMock)
          .ResolveField (_company_Kitchen_Cook_Expression, _context);
      
      _policyMock.VerifyAllExpectations();

      var expectedPath = new FieldSourcePathBuilder().BuildFieldSourcePath (
          StubDatabaseInfo.Instance,
          _context,
          _context.GetColumnSource (_companyClause),
          newJoinMembers);
      var expectedFieldDescriptor = new FieldDescriptor (_kitchen_Cook_Property, expectedPath, new Column (expectedPath.LastSource, "IDColumn"));
      Assert.That (actualFieldDescriptor, Is.EqualTo (expectedFieldDescriptor));
    }

    [Test]
    public void Resolver_UsesPolicyToAdjustFromIdentifierAccess ()
    {
      _policyMock.Expect (mock => mock.OptimizeRelatedKeyAccess()).Return (false);

      Expression fieldExpression = _companyReference;

      var newJoinMembers = new[] { _cook_Substitution_Property };
      _policyMock
          .Expect (mock => mock.AdjustMemberInfosForDirectAccessOfQuerySource (_companyReference))
          .Return (new MemberInfoChain (newJoinMembers, _cook_ID_Property));

      _policyMock.Replay ();

      FieldDescriptor actualFieldDescriptor = 
          new FieldResolver (StubDatabaseInfo.Instance, _policyMock)
          .ResolveField (fieldExpression, _context);
      
      _policyMock.VerifyAllExpectations();

      FieldSourcePath path = new FieldSourcePathBuilder().BuildFieldSourcePath (
          StubDatabaseInfo.Instance,
          _context,
          _context.GetColumnSource (_companyClause),
          newJoinMembers);
      var expectedFieldDescriptor = new FieldDescriptor (null, path, new Column (path.LastSource, "IDColumn"));
      Assert.That (actualFieldDescriptor, Is.EqualTo (expectedFieldDescriptor));
    }

    [Test]
    public void Resolver_PolicyOptimization_True ()
    {
      IResolveFieldAccessPolicy policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      Assert.That (policy.OptimizeRelatedKeyAccess(), Is.True);
      Expression fieldExpression = ExpressionHelper.Resolve<Kitchen, int> (_kitchenClause, sd => sd.Restaurant.ID);

      var resolver = new FieldResolver (StubDatabaseInfo.Instance, policy);

      FieldDescriptor result = resolver.ResolveField (fieldExpression, _context);

      Assert.That (
          result.Column, Is.EqualTo (new Column (_context.GetColumnSource (_kitchenClause), "Kitchen_to_Restaurant_FK")));
    }

    [Test]
    public void Resolver_PolicyOptimization_False ()
    {
      IResolveFieldAccessPolicy policy = new SelectFieldAccessPolicy();
      Assert.That (policy.OptimizeRelatedKeyAccess(), Is.False);
      Expression fieldExpression = ExpressionHelper.Resolve<Kitchen, int>(_kitchenClause, sd => sd.Restaurant.ID);

      var resolver = new FieldResolver (StubDatabaseInfo.Instance, policy);
      FieldDescriptor result = resolver.ResolveField (fieldExpression, _context);

      Assert.That (result.Column, Is.EqualTo (new Column (result.SourcePath.LastSource, "IDColumn")));
    }
  }
}
