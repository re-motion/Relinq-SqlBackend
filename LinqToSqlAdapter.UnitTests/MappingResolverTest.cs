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
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.LinqToSqlAdapter.UnitTests.TestDomain;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Moq;

namespace Remotion.Linq.LinqToSqlAdapter.UnitTests
{
  [TestFixture]
  public class MappingResolverTest
  {
    private UniqueIdentifierGenerator _generator;
    private MappingResolver _mappingResolver;

    private readonly Type _unmappedType = typeof (Type);
    private const string _unmappedTypeMsg = "System.Type";

    private PropertyInfo _unmappedInfo;
    private const string _unmappedInfoMsg = "System.Type.GUID";

    [SetUp]
    public void SetUp()
    {
      _generator = new UniqueIdentifierGenerator();
      _mappingResolver = new MappingResolver (new AttributeMappingSource().GetModel (typeof (DataContextTestClass)));
      _unmappedInfo = _unmappedType.GetProperty ("GUID");
      new AttributeMappingSource().GetModel (typeof (DataContextTestClass));
    }

    [Test]
    public void ResolveTableInfo()
    {
      var unresolvedTableInfo = new UnresolvedTableInfo (typeof (DataContextTestClass.Customer));

      var resolvedTableInfo = (ResolvedSimpleTableInfo) _mappingResolver.ResolveTableInfo (unresolvedTableInfo, _generator);

      Assert.That (resolvedTableInfo.ItemType, Is.EqualTo (typeof (DataContextTestClass.Customer)));
      Assert.That (resolvedTableInfo.TableAlias, Is.EqualTo ("t0"));
      Assert.That (resolvedTableInfo.TableName, Is.EqualTo ("Customers"));
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve table: " + _unmappedTypeMsg + " is not a mapped table")]
    public void ResolveTableInfo_ShouldThrowUnmappedException ()
    {
      _mappingResolver.ResolveTableInfo (new UnresolvedTableInfo (_unmappedType), _generator);
    }
    
    [Test]
    public void ResolveJoinInfo_ForeignKeyOnTheRight()
    {
      var customerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t1");
      var customerPrimaryKey = new SqlColumnDefinitionExpression (typeof (string), customerTableInfo.TableAlias, "CustomerID", true);
      var customerEntity = new SqlEntityDefinitionExpression (customerTableInfo.ItemType, customerTableInfo.TableAlias, null, e => e);

      var ordersMember = customerTableInfo.ItemType.GetProperty ("Orders");
      var unresolvedJoinInfo = new UnresolvedJoinInfo (customerEntity, ordersMember, JoinCardinality.Many);

      var resolvedJoinInfo = _mappingResolver.ResolveJoinInfo (unresolvedJoinInfo, _generator);

      var expectedOrderTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Order), "dbo.Order", "t0");
      var expectedOrderForeignKey = new SqlColumnDefinitionExpression (typeof (string), expectedOrderTableInfo.TableAlias, "CustomerID", false);
      SqlExpressionTreeComparer.CheckAreEqualTrees (Expression.Equal (customerPrimaryKey, expectedOrderForeignKey), resolvedJoinInfo.JoinCondition);

      Assert.That (resolvedJoinInfo.ItemType, Is.EqualTo (expectedOrderTableInfo.ItemType));
      Assert.That (resolvedJoinInfo.ForeignTableInfo.ItemType, Is.EqualTo (expectedOrderTableInfo.ItemType));
      Assert.That (resolvedJoinInfo.ForeignTableInfo.TableAlias, Is.EqualTo (expectedOrderTableInfo.TableAlias));
    }

    [Test]
    public void ResolveJoinInfo_ForeignKeyOnTheLeft()
    {
      var orderTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Order), "dbo.Order", "t1");

      var orderEntity = new SqlEntityDefinitionExpression (
          orderTableInfo.ItemType, orderTableInfo.TableAlias, null, e => e.GetColumn (typeof (string), "OrderID", true));

      var customerMember = orderTableInfo.ItemType.GetProperty ("Customer");
      var unresolvedJoinInfo = new UnresolvedJoinInfo (orderEntity, customerMember, JoinCardinality.One);

      var resolvedJoinInfo = _mappingResolver.ResolveJoinInfo (unresolvedJoinInfo, _generator);

      var orderForeignKey = new SqlColumnDefinitionExpression (typeof (string), orderTableInfo.TableAlias, "CustomerID", false);
      var expectedCustomerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t0");
      var expectedCustomerPrimaryKey = new SqlColumnDefinitionExpression (typeof (string), expectedCustomerTableInfo.TableAlias, "CustomerID", true);
      SqlExpressionTreeComparer.CheckAreEqualTrees (Expression.Equal (orderForeignKey, expectedCustomerPrimaryKey), resolvedJoinInfo.JoinCondition);

      Assert.That (resolvedJoinInfo.ItemType, Is.EqualTo (expectedCustomerTableInfo.ItemType));
      Assert.That (resolvedJoinInfo.ForeignTableInfo.ItemType, Is.EqualTo (expectedCustomerTableInfo.ItemType));
      Assert.That (resolvedJoinInfo.ForeignTableInfo.TableAlias, Is.EqualTo (expectedCustomerTableInfo.TableAlias));
    }

    [Test]
    public void ResolveJoinInfo_OneNullableColumn_TheOtherNot_LeadsToConversion ()
    {
      var regionTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Region), "dbo.Regions", "t1");
      var regionEntity = new SqlEntityDefinitionExpression (regionTableInfo.ItemType, regionTableInfo.TableAlias, null, e => e);

      var territoriesMember = regionTableInfo.ItemType.GetProperty ("Territories");
      var unresolvedJoinInfo = new UnresolvedJoinInfo (regionEntity, territoriesMember, JoinCardinality.Many);

      var resolvedJoinInfo = _mappingResolver.ResolveJoinInfo (unresolvedJoinInfo, _generator);

      var regionPrimaryKey = new SqlColumnDefinitionExpression (typeof (int), regionTableInfo.TableAlias, "RegionID", true);
      var expectedTerritoryTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Territory), "dbo.Territories", "t0");
      var expectedTerritoryForeignKey = new SqlColumnDefinitionExpression (typeof (int?), expectedTerritoryTableInfo.TableAlias, "RegionID", false);
      SqlExpressionTreeComparer.CheckAreEqualTrees (
          Expression.Equal (
            Expression.Convert (regionPrimaryKey, typeof (int?)), 
            expectedTerritoryForeignKey), resolvedJoinInfo.JoinCondition);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve type: " + _unmappedTypeMsg + " is not a mapped type")]
    public void ResolveJoinInfo_ShouldThrowUnmappedExceptionForType ()
    {
      var customerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t1");

      var customerDefinition = new SqlEntityDefinitionExpression (_unmappedType, customerTableInfo.TableAlias, null, e => e);
      PropertyInfo customerOrders = customerTableInfo.ItemType.GetProperty ("Orders");
      var joinInfo = new UnresolvedJoinInfo (customerDefinition, customerOrders, JoinCardinality.One);

      _mappingResolver.ResolveJoinInfo (joinInfo, _generator);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve member: " + _unmappedInfoMsg + " is not a mapped member")]
    public void ResolveJoinInfo_ShouldThrowUnmappedExceptionForMember ()
    {
      var customerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t1");

      var customerDefinition = new SqlEntityDefinitionExpression (customerTableInfo.ItemType, customerTableInfo.TableAlias, null, e => e);

      var joinInfo = new UnresolvedJoinInfo (customerDefinition, _unmappedInfo, JoinCardinality.One);

      _mappingResolver.ResolveJoinInfo (joinInfo, _generator);
    }

    [Test]
    public void ResolveSimpleTableInfo ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Region), "dbo.Region", "t0");

      SqlEntityDefinitionExpression resolvedExpr = _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);

      SqlColumnExpression expectedPrimaryColumn = new SqlColumnDefinitionExpression (typeof (int), simpleTableInfo.TableAlias, "RegionID", true);
      SqlColumnExpression expectedDescriptionColumn =
          new SqlColumnDefinitionExpression (typeof (string), simpleTableInfo.TableAlias, "RegionDescription", false);

      Assert.That (resolvedExpr.Type, Is.SameAs (typeof (DataContextTestClass.Region)));
      Assert.That (resolvedExpr.TableAlias, Is.EqualTo ("t0"));
      Assert.That (resolvedExpr.Name, Is.Null);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedPrimaryColumn, resolvedExpr.GetIdentityExpression());
      Assert.That (resolvedExpr.Columns, Has.Count.EqualTo (2));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedPrimaryColumn, resolvedExpr.Columns[0]);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedDescriptionColumn, resolvedExpr.Columns[1]);
    }

    [Test]
    public void ResolveSimpleTableInfo_MultiplePrimaryKeyProperties ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.ClassWithCompoundPrimaryKey), "ClassWithCompoundPrimaryKey", "t0");

      SqlEntityDefinitionExpression resolvedExpr = _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);

      SqlColumnExpression expectedPrimaryColumn1 = new SqlColumnDefinitionExpression (typeof (int), simpleTableInfo.TableAlias, "Key1", true);
      SqlColumnExpression expectedPrimaryColumn2 = new SqlColumnDefinitionExpression (typeof (string), simpleTableInfo.TableAlias, "Key2", true);
      var type = typeof (MappingResolver.CompoundIdentityTuple<int, string>);
      var expectedIdentityExpression = Expression.New (
          type.GetConstructors().Single(), 
          new[] { new NamedExpression ("Item1", expectedPrimaryColumn1), new NamedExpression ("Item2", expectedPrimaryColumn2) },
          new[] { type.GetProperty ("Item1"), type.GetProperty ("Item2") });

      Assert.That (resolvedExpr.Type, Is.SameAs (typeof (DataContextTestClass.ClassWithCompoundPrimaryKey)));
      Assert.That (resolvedExpr.TableAlias, Is.EqualTo ("t0"));
      Assert.That (resolvedExpr.Name, Is.Null);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedIdentityExpression, resolvedExpr.GetIdentityExpression ());
    }

    [Test]
    public void ResolveSimpleTableInfo_WithInheritanceHierarchy ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (ContactWithInheritanceHierarchy), "dbo.Contact", "t0");

      SqlEntityDefinitionExpression resolvedExpr = _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);
      var actualColumnNames = resolvedExpr.Columns.Select (c => c.ColumnName).ToArray ();

      var expectedMembersAndDeclaringTypes = new[] {  "ContactID", "ContactType", "Password", "PhotoColumn", "HomePage"};
      Assert.That (actualColumnNames, Is.EquivalentTo (expectedMembersAndDeclaringTypes));
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve type: " + _unmappedTypeMsg + " is not a mapped type")]
    public void ResolveSimpleTableInfo_ShouldThrowUnmappedException ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (_unmappedType, "dbo.Region", "t0");

      _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);
    }


    [Test]
    public void ResolveMemberExpression()
    {
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (PersonTestClass), "p", null, e => e);

      var memberInfo = typeof (PersonTestClass).GetProperty ("First");
      Expression result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = new SqlColumnDefinitionExpression (typeof (string), "p", "FirstName", true);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveMemberExpression_PrimaryKey()
    {
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (DataContextTestClass.Customer), "c", null, e => e);

      var memberInfo = typeof (DataContextTestClass.Customer).GetProperty ("CustomerID");
      Expression result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "CustomerID", true);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveMemberExpression_NonPrimaryKey()
    {
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (DataContextTestClass.Customer), "c", null, e => e);

      var memberInfo = typeof (DataContextTestClass.Customer).GetProperty ("CompanyName");
      Expression result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "CompanyName", false);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveMemberExpression_Association()
    {
      var referencedSqlExpression = new SqlEntityDefinitionExpression (typeof (DataContextTestClass.Customer), "c", null, e => e);

      var sqlEntityExpression = new SqlEntityReferenceExpression (typeof (DataContextTestClass.Order), "o", null, referencedSqlExpression);

      var memberInfo = typeof (DataContextTestClass.Order).GetProperty ("Customer");
      var result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = new SqlEntityRefMemberExpression (sqlEntityExpression, memberInfo);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve type: " + _unmappedTypeMsg + " is not a mapped type")]
    public void ResolveMemberExpression_ShouldThrowUnmappedExceptionForType()
    {
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (PersonTestClass), "p", null, e => e);

      var memberInfoStub = new Mock<MemberInfo>();
      memberInfoStub
         .SetupGet (stub => stub.DeclaringType)
         .Returns (_unmappedType);

      _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfoStub.Object);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException),
        ExpectedMessage = "Cannot resolve member: Remotion.Linq.LinqToSqlAdapter.UnitTests.TestDomain.PersonTestClass.stub is not a mapped member")]
    public void ResolveMemberExpression_ShouldThrowUnmappedExceptionForMember()
    {
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (PersonTestClass), "p", null, e => e);

      var memberInfoStub = new Mock<MemberInfo>();
      memberInfoStub
         .SetupGet (stub => stub.DeclaringType)
         .Returns (typeof (PersonTestClass));
      memberInfoStub
         .SetupGet (stub => stub.Name)
         .Returns ("stub");

      _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfoStub.Object);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = 
        "Cannot resolve members appplied to expressions representing columns. (Member: CustomerID, Column: [c].[CustomerID])")]
    public void ResolveMemberExpression_WithSqlColumnExpression ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "CustomerID", true);

      var memberInfo = typeof (DataContextTestClass.Customer).GetProperty ("CustomerID");
      var result = _mappingResolver.ResolveMemberExpression (columnExpression, memberInfo);

      var expectedExpression = columnExpression;

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveConstantExpression ()
    {
      var customer = new DataContextTestClass.Customer { CustomerID = "abc" };
      var constantExpr = Expression.Constant (customer);

      var result = _mappingResolver.ResolveConstantExpression (constantExpr);

      var expectedExpr = new SqlEntityConstantExpression (typeof (DataContextTestClass.Customer), customer, Expression.Constant (customer.CustomerID, typeof (string)));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpr, result);
    }

    [Test]
    public void ResolveConstantExpression_WithMultiplePrimaryKeyMembers ()
    {
      var entity = new DataContextTestClass.ClassWithCompoundPrimaryKey { Key1 = 1, Key2 = "two"};
      var constantExpr = Expression.Constant (entity);

      var result = _mappingResolver.ResolveConstantExpression (constantExpr);

      var type = typeof (MappingResolver.CompoundIdentityTuple<int, string>);
      var expectedExpr = new SqlEntityConstantExpression (typeof (DataContextTestClass.ClassWithCompoundPrimaryKey), entity, Expression.New (
          type.GetConstructors().Single(), 
          new[] { new NamedExpression ("Item1", Expression.Constant (1)), new NamedExpression ("Item2", Expression.Constant ("two")) },
          new[] { type.GetProperty ("Item1"), type.GetProperty ("Item2") }));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedExpr, result);
    }

    [Test]
    public void ResolveConstantExpression_Null ()
    {
      var constantExpr = Expression.Constant (null, typeof (DataContextTestClass.Customer));

      var result = _mappingResolver.ResolveConstantExpression (constantExpr);

      Assert.That (result, Is.SameAs (constantExpr));
    }

    [Test]
    public void ResolveConstantExpression_UnmappedType_ShouldReturnSameExpression ()
    {
      var constantExpr = Expression.Constant (0);

      var result = _mappingResolver.ResolveConstantExpression (constantExpr);

      Assert.That (result, Is.SameAs (constantExpr));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Entities without identity members are not supported by re-linq. "
        + "(Remotion.Linq.LinqToSqlAdapter.UnitTests.TestDomain.DataContextTestClass+FakeClassWithoutPrimaryKey)")]
    public void ResolveConstantExpression_NoPrimaryKey ()
    {
      var customer = new DataContextTestClass.FakeClassWithoutPrimaryKey { Name = "abc" };
      var constantExpr = Expression.Constant (customer);

      _mappingResolver.ResolveConstantExpression (constantExpr);
    }

    [Test]
    public void ResolveTypeCheck_ForObviouslyTrueCheck_ShouldReturnConstantExpression()
    {
      Expression customerExpression = Expression.Constant (new ContactWithInheritanceHierarchy.CustomerContact());
      Type desiredType = typeof (ContactWithInheritanceHierarchy);

      Expression result = _mappingResolver.ResolveTypeCheck (customerExpression, desiredType);
      
      Expression expectedExpression = Expression.Constant (true);
      SqlExpressionTreeComparer.CheckAreEqualTrees (result, expectedExpression);
    }

    [Test]
    public void ResolveTypeCheck_ForObviouslyFalseCheck_ShouldReturnConstantExpression ()
    {
      Expression customerExpression = Expression.Constant (new ContactWithInheritanceHierarchy.CustomerContact ());
      Type desiredType = typeof (PersonTestClass);

      Expression result = _mappingResolver.ResolveTypeCheck (customerExpression, desiredType);

      Expression expectedExpression = Expression.Constant (false);
      SqlExpressionTreeComparer.CheckAreEqualTrees (result, expectedExpression);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage =
        "Cannot perform a type check for type "
        + "Remotion.Linq.LinqToSqlAdapter.UnitTests.TestDomain.DataContextTestClass+FakeClassWithoutInheritanceCode - there is no inheritance "
        + "code for this type.")]
    public void ResolveTypeCheck_ShouldThrowUnmappedItemException_WhenNoInheritanceCode()
    {
      Expression contactExpression = Expression.Constant (new DataContextTestClass.FakeClassWithoutInheritanceCodeBase());
      Type desiredType = typeof (DataContextTestClass.FakeClassWithoutInheritanceCode);

      _mappingResolver.ResolveTypeCheck (contactExpression, desiredType);
    }

    [Test]
    public void ResolveTypeCheck_ShouldReturnBinaryExpression_WhenInheritanceCode()
    {
      Expression contactExpression = Expression.Constant (new ContactWithInheritanceHierarchy());
      Type desiredType = new ContactWithInheritanceHierarchy.CustomerContact().GetType();

      var discriminatorDataMember = contactExpression.Type.GetProperty ("ContactType");

      Expression result = _mappingResolver.ResolveTypeCheck (contactExpression, desiredType);
      
      Expression expectedExpression = Expression.Equal (
          Expression.MakeMemberAccess (contactExpression, discriminatorDataMember),
          Expression.Constant ("Customer")
          );
      SqlExpressionTreeComparer.CheckAreEqualTrees (result, expectedExpression);
    }

    [Test]
    public void TryResolveOptimizedIdentity_ForeignKeyOnTheRight ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.ClassWithOneToOneRelation_VirtualSide), "X", "x");
      var originatingEntity = new SqlEntityDefinitionExpression (tableInfo.ItemType, tableInfo.TableAlias, null, e => e);

      var relationMember = tableInfo.ItemType.GetProperty ("ForeignKeySide");
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (originatingEntity, relationMember);

      var result = _mappingResolver.TryResolveOptimizedIdentity (entityRefMemberExpression);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void TryResolveOptimizedIdentity_ForeignKeyOnTheLeft ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.ClassWithOneToOneRelation_ForeignKeySide), "X", "x");

      var originatingEntity = new SqlEntityDefinitionExpression (
          tableInfo.ItemType, tableInfo.TableAlias, null, e => e.GetColumn (typeof (int), "ID", true));

      var relationMember = tableInfo.ItemType.GetProperty ("VirtualSide");
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (originatingEntity, relationMember);

      var result = _mappingResolver.TryResolveOptimizedIdentity (entityRefMemberExpression);

      var orderForeignKey = new SqlColumnDefinitionExpression (typeof (int), tableInfo.TableAlias, "VirtualSideID", false);
      SqlExpressionTreeComparer.CheckAreEqualTrees (orderForeignKey, result);
    }

    [Test]
    public void TryResolveOptimizedMemberExpression_IdentityMember_ForeignKeyOnTheRight ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.ClassWithOneToOneRelation_VirtualSide), "X", "x");
      var originatingEntity = new SqlEntityDefinitionExpression (tableInfo.ItemType, tableInfo.TableAlias, null, e => e);

      var relationMember = tableInfo.ItemType.GetProperty ("ForeignKeySide");
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (originatingEntity, relationMember);

      var identityMember = typeof (DataContextTestClass.ClassWithOneToOneRelation_ForeignKeySide).GetProperty ("ID");
      var result = _mappingResolver.TryResolveOptimizedMemberExpression (entityRefMemberExpression, identityMember);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void TryResolveOptimizedMemberExpression_IdentityMember_ForeignKeyOnTheLeft ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.ClassWithOneToOneRelation_ForeignKeySide), "X", "x");

      var originatingEntity = new SqlEntityDefinitionExpression (
          tableInfo.ItemType, tableInfo.TableAlias, null, e => e.GetColumn (typeof (int), "ID", true));

      var relationMember = tableInfo.ItemType.GetProperty ("VirtualSide");
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (originatingEntity, relationMember);

      var identityMember = typeof (DataContextTestClass.ClassWithOneToOneRelation_VirtualSide).GetProperty ("ID");
      var result = _mappingResolver.TryResolveOptimizedMemberExpression (entityRefMemberExpression, identityMember);

      var orderForeignKey = new SqlColumnDefinitionExpression (typeof (int), tableInfo.TableAlias, "VirtualSideID", false);
      SqlExpressionTreeComparer.CheckAreEqualTrees (orderForeignKey, result);
    }

    [Test]
    public void TryResolveOptimizedMemberExpression_OtherMember ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.ClassWithOneToOneRelation_ForeignKeySide), "X", "x");

      var originatingEntity = new SqlEntityDefinitionExpression (
          tableInfo.ItemType, tableInfo.TableAlias, null, e => e.GetColumn (typeof (int), "ID", true));

      var relationMember = tableInfo.ItemType.GetProperty ("VirtualSide");
      var entityRefMemberExpression = new SqlEntityRefMemberExpression (originatingEntity, relationMember);

      var identityMember = typeof (DataContextTestClass.ClassWithOneToOneRelation_VirtualSide).GetProperty ("OtherMember");
      var result = _mappingResolver.TryResolveOptimizedMemberExpression (entityRefMemberExpression, identityMember);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetMetaDataMembers_RetrievesSameMembers_AsResolveSimpleTableInfo ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Region), "dbo.Region", "t0");

      MetaDataMember[] metaDataMembers = _mappingResolver.GetMetaDataMembers (simpleTableInfo.ItemType);

      var actualColumnNames = metaDataMembers.Select (m => m.MappedName).ToArray();

      var expectedMatchingEntity = _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);
      var expectedColumnNames = expectedMatchingEntity.Columns.Select (c => c.ColumnName).ToArray();
      Assert.That (actualColumnNames, Is.EqualTo (expectedColumnNames));
    }

    [Test]
    public void GetMetaDataMembers_WithInheritance_ShouldReturnMembersOfSubclasses ()
    {
      var members = _mappingResolver.GetMetaDataMembers (typeof (ContactWithInheritanceHierarchy));
      var actualMembersAndDeclaringTypes = members.Select (m => new { m.DeclaringType.Type, Member = m.Member.Name }).ToArray();

      var expectedMembersAndDeclaringTypes = new[] { 
          new { Type = typeof (ContactWithInheritanceHierarchy), Member = "ContactID"},
          new { Type = typeof (ContactWithInheritanceHierarchy), Member = "ContactType"},
          new { Type = typeof (ContactWithInheritanceHierarchy.EmployeeContact), Member = "Password"},
          new { Type = typeof (ContactWithInheritanceHierarchy.EmployeeContact), Member = "Photo"},
          new { Type = typeof (ContactWithInheritanceHierarchy.SupplierContact), Member = "HomePage"}
      };
      Assert.That (actualMembersAndDeclaringTypes, Is.EquivalentTo (expectedMembersAndDeclaringTypes));
    }
  }
}