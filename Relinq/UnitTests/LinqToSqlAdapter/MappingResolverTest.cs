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
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.LinqToSqlAdapter.TestDomain;
using Remotion.Linq.LinqToSqlAdapter;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.LinqToSqlAdapter
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
      var customerEntity = new SqlEntityDefinitionExpression (customerTableInfo.ItemType, customerTableInfo.TableAlias, null, customerPrimaryKey);

      var ordersMember = customerTableInfo.ItemType.GetProperty ("Orders");
      var unresolvedJoinInfo = new UnresolvedJoinInfo (customerEntity, ordersMember, JoinCardinality.Many);

      var resolvedJoinInfo = _mappingResolver.ResolveJoinInfo (unresolvedJoinInfo, _generator);

      var expectedOrderTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Order), "dbo.Order", "t0");
      var expectedOrderForeignKey = new SqlColumnDefinitionExpression (typeof (string), expectedOrderTableInfo.TableAlias, "CustomerID", false);

      ExpressionTreeComparer.CheckAreEqualTrees (customerPrimaryKey, resolvedJoinInfo.LeftKey);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedOrderForeignKey, resolvedJoinInfo.RightKey);

      Assert.That (resolvedJoinInfo.ItemType, Is.EqualTo (expectedOrderTableInfo.ItemType));
      Assert.That (resolvedJoinInfo.ForeignTableInfo.ItemType, Is.EqualTo (expectedOrderTableInfo.ItemType));
      Assert.That (resolvedJoinInfo.ForeignTableInfo.TableAlias, Is.EqualTo (expectedOrderTableInfo.TableAlias));
    }

    [Test]
    public void ResolveJoinInfo_ForeignKeyOnTheLeft()
    {
      var orderTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Order), "dbo.Order", "t1");
      var orderForeignKey = new SqlColumnDefinitionExpression (typeof (string), orderTableInfo.TableAlias, "CustomerID", false);
      var orderPrimaryKey = new SqlColumnDefinitionExpression (typeof (string), orderTableInfo.TableAlias, "OrderID", true);

      var orderEntity = new SqlEntityDefinitionExpression (orderTableInfo.ItemType, orderTableInfo.TableAlias, null, orderPrimaryKey);

      var customerMember = orderTableInfo.ItemType.GetProperty ("Customer");
      var unresolvedJoinInfo = new UnresolvedJoinInfo (orderEntity, customerMember, JoinCardinality.One);

      var resolvedJoinInfo = _mappingResolver.ResolveJoinInfo (unresolvedJoinInfo, _generator);

      var expectedCustomerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t0");
      var expectedCustomerPrimaryKey = new SqlColumnDefinitionExpression (typeof (string), expectedCustomerTableInfo.TableAlias, "CustomerID", true);

      ExpressionTreeComparer.CheckAreEqualTrees (orderForeignKey, resolvedJoinInfo.LeftKey);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedCustomerPrimaryKey, resolvedJoinInfo.RightKey);

      Assert.That (resolvedJoinInfo.ItemType, Is.EqualTo (expectedCustomerTableInfo.ItemType));
      Assert.That (resolvedJoinInfo.ForeignTableInfo.ItemType, Is.EqualTo (expectedCustomerTableInfo.ItemType));
      Assert.That (resolvedJoinInfo.ForeignTableInfo.TableAlias, Is.EqualTo (expectedCustomerTableInfo.TableAlias));
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve type: " + _unmappedTypeMsg + " is not a mapped type")]
    public void ResolveJoinInfo_ShouldThrowUnmappedExceptionForType ()
    {
      var customerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t1");

      var customerPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), customerTableInfo.TableAlias, "CustomerID", true);

      var customerDefinition = new SqlEntityDefinitionExpression (
          _unmappedType, customerTableInfo.TableAlias, null, customerPrimaryKey);
      PropertyInfo customerOrders = customerTableInfo.ItemType.GetProperty ("Orders");
      var joinInfo = new UnresolvedJoinInfo (customerDefinition, customerOrders, JoinCardinality.One);

      _mappingResolver.ResolveJoinInfo (joinInfo, _generator);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve member: " + _unmappedInfoMsg + " is not a mapped member")]
    public void ResolveJoinInfo_ShouldThrowUnmappedExceptionForMember ()
    {
      var customerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t1");

      var customerPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), customerTableInfo.TableAlias, "CustomerID", true);

      var customerDefinition = new SqlEntityDefinitionExpression (
          customerTableInfo.ItemType, customerTableInfo.TableAlias, null, customerPrimaryKey);

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
      var expectedExpr = new SqlEntityDefinitionExpression (
          simpleTableInfo.ItemType, simpleTableInfo.TableAlias, null, expectedPrimaryColumn, expectedPrimaryColumn, expectedDescriptionColumn);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpr, resolvedExpr);
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
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "p", "FirstName", true);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (PersonTestClass), "p", null, primaryKeyColumn);

      var memberInfo = typeof (PersonTestClass).GetProperty ("First");
      Expression result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = new SqlColumnDefinitionExpression (typeof (string), "p", "FirstName", true);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveMemberExpression_PrimaryKey()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "c", "CustomerID", true);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (DataContextTestClass.Customer), "c", null, primaryKeyColumn);

      var memberInfo = typeof (DataContextTestClass.Customer).GetProperty ("CustomerID");
      Expression result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      ExpressionTreeComparer.CheckAreEqualTrees (primaryKeyColumn, result);
    }

    [Test]
    public void ResolveMemberExpression_NonPrimaryKey()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "c", "CustomerID", true);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (DataContextTestClass.Customer), "c", null, primaryKeyColumn);

      var memberInfo = typeof (DataContextTestClass.Customer).GetProperty ("CompanyName");
      Expression result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "CompanyName", false);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveMemberExpression_Association()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "c", "CustomerID", true);
      var referencedSqlExpression = new SqlEntityDefinitionExpression (typeof (DataContextTestClass.Customer), "c", null, primaryKeyColumn);

      var sqlEntityExpression = new SqlEntityReferenceExpression (typeof (DataContextTestClass.Order), "o", null, referencedSqlExpression);

      var memberInfo = typeof (DataContextTestClass.Order).GetProperty ("Customer");
      var result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = new SqlEntityRefMemberExpression (sqlEntityExpression, memberInfo);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve type: " + _unmappedTypeMsg + " is not a mapped type")]
    public void ResolveMemberExpression_ShouldThrowUnmappedExceptionForType()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "p", "FirstName", true);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (PersonTestClass), "p", null, primaryKeyColumn);

      var memberInfoStub = MockRepository.GenerateStub<MemberInfo>();
      memberInfoStub
          .Stub (stub => stub.DeclaringType)
          .Return (_unmappedType);

      _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfoStub);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException),
        ExpectedMessage = "Cannot resolve member: Remotion.Linq.UnitTests.LinqToSqlAdapter.TestDomain.PersonTestClass.stub is not a mapped member")]
    public void ResolveMemberExpression_ShouldThrowUnmappedExceptionForMember()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "p", "FirstName", true);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (PersonTestClass), "p", null, primaryKeyColumn);

      var memberInfoStub = MockRepository.GenerateStub<MemberInfo>();
      memberInfoStub
          .Stub (stub => stub.DeclaringType)
          .Return (typeof (PersonTestClass));
      memberInfoStub
          .Stub (stub => stub.Name)
          .Return ("stub");

      _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfoStub);
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

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveConstantExpression ()
    {
      var customer = new DataContextTestClass.Customer { CustomerID = "abc" };
      var constantExpr = Expression.Constant (customer);

      var result = _mappingResolver.ResolveConstantExpression (constantExpr);

      var expectedExpr = new SqlEntityConstantExpression (typeof (DataContextTestClass.Customer), customer, Expression.Constant (customer.CustomerID, typeof (string)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpr, result);
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
        "Entities without identity members are not supported by re-linq. (FakeClassWithoutPrimaryKey)")]
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
      ExpressionTreeComparer.CheckAreEqualTrees (result, expectedExpression);
    }

    [Test]
    public void ResolveTypeCheck_ForObviouslyFalseCheck_ShouldReturnConstantExpression ()
    {
      Expression customerExpression = Expression.Constant (new ContactWithInheritanceHierarchy.CustomerContact ());
      Type desiredType = typeof (PersonTestClass);

      Expression result = _mappingResolver.ResolveTypeCheck (customerExpression, desiredType);

      Expression expectedExpression = Expression.Constant (false);
      ExpressionTreeComparer.CheckAreEqualTrees (result, expectedExpression);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage =
        "Cannot perform a type check for type "
        + "Remotion.Linq.UnitTests.LinqToSqlAdapter.TestDomain.DataContextTestClass+FakeClassWithoutInheritanceCode - there is no inheritance "
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
      ExpressionTreeComparer.CheckAreEqualTrees (result, expectedExpression);
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