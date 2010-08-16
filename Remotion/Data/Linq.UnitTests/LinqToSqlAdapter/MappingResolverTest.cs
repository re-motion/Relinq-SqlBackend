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
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.LinqToSqlAdapter;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.LinqToSqlAdapter.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.LinqToSqlAdapter
{
  [TestFixture]
  public class MappingResolverTest
  {
    private UniqueIdentifierGenerator _generator;
    private IMappingResolver _mappingResolver;
    private IReverseMappingResolver _reverseMappingResolver;
    private MetaModel _metaModel;

    private readonly Type _unmappedType = typeof (Type);
    private const string _unmappedTypeMsg = "System.Type";

    private PropertyInfo _unmappedInfo;
    private const string _unmappedInfoMsg = "System.Type.GUID";

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator();
      _mappingResolver = new MappingResolver (new AttributeMappingSource().GetModel (typeof (DataContextTestClass)));
      _reverseMappingResolver = (IReverseMappingResolver) _mappingResolver;
      _unmappedInfo = _unmappedType.GetProperty ("GUID");
      _metaModel = new AttributeMappingSource ().GetModel (typeof (DataContextTestClass));
    }

    [Test]
    public void TestResolveTableInfo ()
    {
      UnresolvedTableInfo unresolvedTableInfo = new UnresolvedTableInfo (typeof (DataContextTestClass.Customer));

      ResolvedSimpleTableInfo resolvedTableInfo = (ResolvedSimpleTableInfo) _mappingResolver.ResolveTableInfo (unresolvedTableInfo, _generator);

      ResolvedSimpleTableInfo simpleTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "Customers", "t0");

      Assert.AreEqual (simpleTableInfo.ItemType, resolvedTableInfo.ItemType);
      Assert.AreEqual (simpleTableInfo.TableAlias, resolvedTableInfo.TableAlias);
      Assert.AreEqual (simpleTableInfo.TableName, resolvedTableInfo.TableName);
    }

    [Test]
    public void TestResolveSimpleTableInfo ()
    {
      ResolvedSimpleTableInfo simpleTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Region), "dbo.Region", "t0");

      SqlColumnExpression primaryColumn = new SqlColumnDefinitionExpression (typeof (int), simpleTableInfo.TableAlias, "RegionID", true);
      SqlColumnExpression descriptionColumn = new SqlColumnDefinitionExpression (
          typeof (string), simpleTableInfo.TableAlias, "RegionDescription", false);

      SqlEntityDefinitionExpression expectedExpr = new SqlEntityDefinitionExpression (
          simpleTableInfo.ItemType, simpleTableInfo.TableAlias, null, primaryColumn, primaryColumn, descriptionColumn);

      SqlEntityDefinitionExpression resolvedExpr = _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpr, resolvedExpr);
    }

    [Test]
    public void TestResolveJoinInfo ()
    {
      ResolvedSimpleTableInfo orderTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Order), "dbo.Order", "t0");
      ResolvedSimpleTableInfo customerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t1");

      SqlColumnDefinitionExpression customerPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), customerTableInfo.TableAlias, "CustomerID", true);
      SqlColumnDefinitionExpression orderForeignKey = new SqlColumnDefinitionExpression (
          typeof (string), orderTableInfo.TableAlias, "CustomerID", false);

      SqlEntityDefinitionExpression customerDefinition = new SqlEntityDefinitionExpression (
          customerTableInfo.ItemType, customerTableInfo.TableAlias, null, customerPrimaryKey);
      PropertyInfo customerOrders = customerTableInfo.ItemType.GetProperty ("Orders");
      UnresolvedJoinInfo joinInfo = new UnresolvedJoinInfo (customerDefinition, customerOrders, JoinCardinality.Many);

      ResolvedJoinInfo expectedJoinInfo = new ResolvedJoinInfo (orderTableInfo, customerPrimaryKey, orderForeignKey);
      ResolvedJoinInfo resolvedJoinInfo = _mappingResolver.ResolveJoinInfo (joinInfo, _generator);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedJoinInfo.LeftKey, resolvedJoinInfo.LeftKey);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedJoinInfo.RightKey, resolvedJoinInfo.RightKey);
      Assert.AreEqual (expectedJoinInfo.ItemType, resolvedJoinInfo.ItemType);
      Assert.AreEqual (expectedJoinInfo.ForeignTableInfo.ItemType, resolvedJoinInfo.ForeignTableInfo.ItemType);
      Assert.AreEqual (expectedJoinInfo.ForeignTableInfo.TableAlias, resolvedJoinInfo.ForeignTableInfo.TableAlias);
    }

    [Test]
    public void TestResolveJoinInfoReverse ()
    {
      ResolvedSimpleTableInfo customerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t0");
      ResolvedSimpleTableInfo orderTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Order), "dbo.Order", "t1");

      SqlColumnDefinitionExpression customerPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), customerTableInfo.TableAlias, "CustomerID", true);
      SqlColumnDefinitionExpression orderForeignKey = new SqlColumnDefinitionExpression (
          typeof (string), orderTableInfo.TableAlias, "CustomerID", false);
      SqlColumnDefinitionExpression orderPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), orderTableInfo.TableAlias, "OrderID", true);

      SqlEntityDefinitionExpression orderDefinition = new SqlEntityDefinitionExpression (
          orderTableInfo.ItemType, orderTableInfo.TableAlias, null, orderPrimaryKey);
      PropertyInfo orderCustomer = orderTableInfo.ItemType.GetProperty ("Customer");

      UnresolvedJoinInfo joinInfo = new UnresolvedJoinInfo (orderDefinition, orderCustomer, JoinCardinality.One);

      ResolvedJoinInfo expectedJoinInfo = new ResolvedJoinInfo (customerTableInfo, orderForeignKey, customerPrimaryKey);
      ResolvedJoinInfo resolvedJoinInfo = _mappingResolver.ResolveJoinInfo (joinInfo, _generator);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedJoinInfo.LeftKey, resolvedJoinInfo.LeftKey);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedJoinInfo.RightKey, resolvedJoinInfo.RightKey);
      Assert.AreEqual (expectedJoinInfo.ItemType, resolvedJoinInfo.ItemType);
      Assert.AreEqual (expectedJoinInfo.ForeignTableInfo.ItemType, resolvedJoinInfo.ForeignTableInfo.ItemType);
      Assert.AreEqual (expectedJoinInfo.ForeignTableInfo.TableAlias, resolvedJoinInfo.ForeignTableInfo.TableAlias);
    }

    [Test]
    public void TestReverseMapping ()
    {
      ResolvedSimpleTableInfo simpleTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Region), "dbo.Region", "t0");

      SqlEntityDefinitionExpression resolvedExpr = _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);
      MetaDataMember[] metaDataMembers = _reverseMappingResolver.GetMetaDataMembers (simpleTableInfo.ItemType);

      Assert.AreEqual (metaDataMembers[0].MappedName, resolvedExpr.PrimaryKeyColumn.ColumnName);

      for (int i = 1; i < metaDataMembers.Length; i++)
        Assert.AreEqual (metaDataMembers[i].MappedName, resolvedExpr.Columns[i].ColumnName);
    }

    [Test]
    public void ResolveMemberExpression ()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "p", "FirstName", true);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (PersonTestClass), "p", null, primaryKeyColumn);

      var memberInfo = typeof (PersonTestClass).GetProperty ("First");
      Expression result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = new SqlColumnDefinitionExpression (typeof (string), "p", "FirstName", true);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveMemberExpressionUsingNorthwindEntitiesPrimaryKey ()
    {
      //Test object
      Type type = typeof (DataContextTestClass.Customer);
      string columnName = "CustomerID";
      bool isPrimaryKey = true;

      //Expressions
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "c", columnName, isPrimaryKey);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (type, "c", null, primaryKeyColumn);

      var memberInfo = type.GetProperty (columnName);
      Expression result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = primaryKeyColumn;

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveMemberExpressionUsingNorthwindEntitiesNonPrimaryKey ()
    {
      //Test object
      Type type = typeof (DataContextTestClass.Customer);
      string columnName = "CompanyName";
      bool isPrimaryKey = false;

      //Expressions
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "c", columnName, isPrimaryKey);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (type, "c", null, primaryKeyColumn);

      var memberInfo = type.GetProperty (columnName);
      Expression result = _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      var expectedExpression = primaryKeyColumn;

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveMemberExpressionUsingNorthwindEntitiesAssociated ()
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
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve table: " + _unmappedTypeMsg + " is not a mapped table")]
    public void ResolveTableInfo_ShouldThrowUnmappedException ()
    {
      _mappingResolver.ResolveTableInfo (new UnresolvedTableInfo (_unmappedType), _generator);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve type: " + _unmappedTypeMsg + " is not a mapped type")]
    public void ResolveJoinInfo_ShouldThrowUnmappedExceptionForType ()
    {
      ResolvedSimpleTableInfo customerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t1");

      SqlColumnDefinitionExpression customerPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), customerTableInfo.TableAlias, "CustomerID", true);

      SqlEntityDefinitionExpression customerDefinition = new SqlEntityDefinitionExpression (
          _unmappedType, customerTableInfo.TableAlias, null, customerPrimaryKey);
      PropertyInfo customerOrders = customerTableInfo.ItemType.GetProperty ("Orders");
      UnresolvedJoinInfo joinInfo = new UnresolvedJoinInfo (customerDefinition, customerOrders, JoinCardinality.One);

      _mappingResolver.ResolveJoinInfo (joinInfo, _generator);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve member: " + _unmappedInfoMsg + " is not a mapped member")]
    public void ResolveJoinInfo_ShouldThrowUnmappedExceptionForMember ()
    {
      ResolvedSimpleTableInfo customerTableInfo = new ResolvedSimpleTableInfo (typeof (DataContextTestClass.Customer), "dbo.Customers", "t1");

      SqlColumnDefinitionExpression customerPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), customerTableInfo.TableAlias, "CustomerID", true);

      SqlEntityDefinitionExpression customerDefinition = new SqlEntityDefinitionExpression (
          customerTableInfo.ItemType, customerTableInfo.TableAlias, null, customerPrimaryKey);

      UnresolvedJoinInfo joinInfo = new UnresolvedJoinInfo (customerDefinition, _unmappedInfo, JoinCardinality.One);

      _mappingResolver.ResolveJoinInfo (joinInfo, _generator);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve type: " + _unmappedTypeMsg + " is not a mapped type")]
    public void TestResolveSimpleTableInfo_ShouldThrowUnmappedExceptionForType ()
    {
      ResolvedSimpleTableInfo simpleTableInfo = new ResolvedSimpleTableInfo (_unmappedType, "dbo.Region", "t0");

      _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve type: " + _unmappedTypeMsg + " is not a mapped type")]
    public void ResolveMemberExpression_ShouldThrowUnmappedExceptionForType ()
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
        ExpectedMessage = "Cannot resolve member: Remotion.Data.Linq.UnitTests.LinqToSqlAdapter.PersonTestClass.stub is not a mapped member")]
    public void ResolveMemberExpression_ShouldThrowUnmappedExceptionForMember ()
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
    public void ResolveTypeCheck_ShouldReturnConstantExpression ()
    {
      Expression customerExpression = Expression.Constant (new ContactTestClass.CustomerContact());
      Type desiredType = new ContactTestClass().GetType();

      Expression result = _mappingResolver.ResolveTypeCheck (customerExpression, desiredType);
      Expression excpectedExpression = Expression.Constant (true);

      ExpressionTreeComparer.CheckAreEqualTrees (result, excpectedExpression);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException),
        ExpectedMessage =
            "Cannot perform a type check for type Remotion.Data.Linq.UnitTests.LinqToSqlAdapter.DataContextTestClass+Customer - there is no inheritance code for this type."
        )]
    public void ResolveTypeCheck_ShouldThrowUnmappedItemExceptionForInheritanceCode ()
    {
      Expression contactExpression = Expression.Constant (new ContactTestClass ());
      Type desiredTypeNotAssignable = new DataContextTestClass.Customer().GetType();

      _mappingResolver.ResolveTypeCheck (contactExpression, desiredTypeNotAssignable);
    }

    [Test]
    public void ResolveTypeCheck_ShouldReturnBinaryExpression ()
    {
      Expression contactExpression = Expression.Constant (new ContactTestClass ());
      Type desiredType = new ContactTestClass.CustomerContact().GetType();

      var discriminatorDataMember = contactExpression.Type.GetProperty ("ContactType");

      Expression result = _mappingResolver.ResolveTypeCheck (contactExpression, desiredType);
      Expression excpectedExpression = Expression.Equal (
          Expression.MakeMemberAccess (contactExpression, discriminatorDataMember),
          Expression.Constant ("Customer")
          );

      ExpressionTreeComparer.CheckAreEqualTrees (result, excpectedExpression);
    }

    //A-TEAM

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve member CustomerID appplied to column CustomerID")]
    public void ResolveMemberExpressionGivingSqlColumnDefinitionExpression ()
    {
      var columnExpression = new SqlColumnDefinitionExpression (typeof (string), "c", "CustomerID", true);

      var memberInfo = typeof (DataContextTestClass.Customer).GetProperty ("CustomerID");
      var result = _mappingResolver.ResolveMemberExpression (columnExpression, memberInfo);

      var expectedExpression = columnExpression;

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    [ExpectedException (typeof (UnmappedItemException), ExpectedMessage = "Cannot resolve member CustomerID appplied to column CustomerID")]
    public void ResolveMemberExpressionGivingSqlColumnReferenceExpression ()
    {
      var primaryKeyColumn = new SqlColumnDefinitionExpression (typeof (string), "s", "CustomerID", true);
      var referencedSqlExpression = new SqlEntityDefinitionExpression (typeof (DataContextTestClass.Customer), "c", null, primaryKeyColumn);
      var columnRefExpression = new SqlColumnReferenceExpression (typeof (string), "c", "CustomerID", true, referencedSqlExpression);

      var memberInfo = typeof (DataContextTestClass.Customer).GetProperty ("CustomerID");
      var result = _mappingResolver.ResolveMemberExpression (columnRefExpression, memberInfo);

      var expectedExpression = columnRefExpression;

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void ResolveConstantExpression ()
    {
      var metamodel = new AttributeMappingSource().GetModel (typeof (DataContextTestClass));
      var table = metamodel.GetTable (typeof (DataContextTestClass.Customer));
      var dataMembers = table.RowType.DataMembers;
      var primaryKeys = new List<MetaDataMember>();

      foreach (var member in dataMembers)
      {
        if (member.IsPrimaryKey)
          primaryKeys.Add (member);
      }

      var customer = new DataContextTestClass.Customer();
      var constantExpr = Expression.Constant (customer);

      var result = _mappingResolver.ResolveConstantExpression (constantExpr);

      var expectedExpr = new SqlEntityConstantExpression (typeof (DataContextTestClass.Customer), customer, primaryKeys[0]);

      Assert.AreEqual (result.NodeType, expectedExpr.NodeType);
      Assert.IsTrue (result is SqlEntityConstantExpression);
      Assert.AreEqual (((SqlEntityConstantExpression) result).PrimaryKeyValue.ToString(), expectedExpr.PrimaryKeyValue.ToString());
      Assert.AreEqual (((SqlEntityConstantExpression) result).Type, result.Type);
      //TODO implement better checking than above, see below
      //WORKAROUND: CheckAreEqualTrees doesn't work
      //ExpressionTreeComparer.CheckAreEqualTrees (expectedExpr, result);
    }

    [Test]
    public void ResolveConstantExpressionShouldReturnSameQuery ()
    {
      var constantExpr = Expression.Constant (0);

      var result = _mappingResolver.ResolveConstantExpression (constantExpr);

      var expectedExpr = constantExpr;

      Assert.AreEqual (expectedExpr, result);
    }


    [Test]
    public void GetMetaMembers()
    {
      var members = _reverseMappingResolver.GetMetaDataMembers (typeof (ContactTestClass));

      var suppMembers = _metaModel.GetMetaType (typeof (ContactTestClass.SupplierContact)).DataMembers.ToDictionary(member => member.MappedName);
      var empMembers = _metaModel.GetMetaType (typeof (ContactTestClass.EmployeeContact)).DataMembers.ToDictionary (member => member.MappedName);

      var mergedDictionary = suppMembers.ToDictionary (suppMember => suppMember.Key, suppMember => suppMember.Value);

      foreach (var empMember in empMembers)
      {
        if (!mergedDictionary.ContainsKey (empMember.Key))
          mergedDictionary.Add (empMember.Key, empMember.Value);
      }

      foreach (var member in members)
      { 
        if (member.MappedName == "ContactID")
        {
          Assert.IsTrue (member.DeclaringType.Type == typeof (ContactTestClass));
        }
        if (member.MappedName == "HomePage")
        {
          Assert.IsTrue (member.DeclaringType.Type == typeof (ContactTestClass.SupplierContact));
        }
        Assert.IsTrue (mergedDictionary.ContainsKey (member.MappedName));
      }
    }
  }
}