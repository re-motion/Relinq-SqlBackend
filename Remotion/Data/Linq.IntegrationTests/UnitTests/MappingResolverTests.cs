using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.IntegrationTests.Utilities;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests
{
  [TestFixture]
  public class MappingResolverTests
  {
    private UniqueIdentifierGenerator _generator;
    private IMappingResolver _mappingResolver;

    [SetUp]
    public void SetUp()
    {
       _generator=new UniqueIdentifierGenerator();
       _mappingResolver = new NorthwindMappingResolver ();
    }

    [Test]
    public void TestMetaModelMapping()
    {
      System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource ();

      var table = mappingSource.GetModel (typeof (Northwind)).GetTable (typeof (Customer));
      Assert.AreEqual ("dbo.Customers",table.TableName);

      string companyName = "CompanyName";

      string expectedType = "NVarChar(40) NOT NULL";
      string resolvedType=string.Empty;
      
      foreach (var metaDataMember in table.RowType.DataMembers)
      {
        if(!metaDataMember.Name.Equals (companyName))
          continue;

        resolvedType = metaDataMember.DbType;
      }

      Assert.AreEqual (expectedType, resolvedType);
    }

    [Test]
    public void TestResolveTableInfo()
    {
      UnresolvedTableInfo unresolvedTableInfo = new UnresolvedTableInfo (typeof(Customer));
      
      ResolvedSimpleTableInfo resolvedTableInfo = (ResolvedSimpleTableInfo) _mappingResolver.ResolveTableInfo (unresolvedTableInfo, _generator);

      ResolvedSimpleTableInfo simpleTableInfo=new ResolvedSimpleTableInfo (typeof(Customer),"dbo.Customers","t0");
      
      Assert.AreEqual (simpleTableInfo.ItemType, resolvedTableInfo.ItemType);
      Assert.AreEqual (simpleTableInfo.TableAlias, resolvedTableInfo.TableAlias);
      Assert.AreEqual (simpleTableInfo.TableName, resolvedTableInfo.TableName);
    }

    [Test]
    public void TestResolveSimpleTableInfo()
    {
      ResolvedSimpleTableInfo simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Region), "dbo.Region", "t0");

      SqlColumnExpression primaryColumn = new SqlColumnDefinitionExpression(typeof(int), simpleTableInfo.TableAlias, "RegionID", true);
      SqlColumnExpression descriptionColumn = new SqlColumnDefinitionExpression(typeof(string),simpleTableInfo.TableAlias, "RegionDescription", false);
      SqlColumnExpression territoriesColumn = new SqlColumnDefinitionExpression (typeof (EntitySet<Territory>), simpleTableInfo.TableAlias, "Region_Territory", false);

      SqlEntityDefinitionExpression expectedExpr = new SqlEntityDefinitionExpression (simpleTableInfo.ItemType, simpleTableInfo.TableAlias, null, primaryColumn, descriptionColumn, territoriesColumn);

      SqlEntityDefinitionExpression resolvedExpr = _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpr, resolvedExpr);
    }

    [Test]
    public void TestResolveJoinInfo()
    {
      ResolvedSimpleTableInfo orderTableInfo = new ResolvedSimpleTableInfo (typeof (Order), "dbo.Order", "t0");
      ResolvedSimpleTableInfo customerTableInfo = new ResolvedSimpleTableInfo (typeof (Customer), "dbo.Customers", "t1");

      SqlColumnDefinitionExpression customerPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), customerTableInfo.TableAlias, "CustomerID", true);
      SqlColumnDefinitionExpression orderForeignKey = new SqlColumnDefinitionExpression (
          typeof (string), orderTableInfo.TableAlias, "CustomerID", false);

      SqlEntityDefinitionExpression customerDefinition = new SqlEntityDefinitionExpression (customerTableInfo.ItemType, customerTableInfo.TableAlias, null, customerPrimaryKey);
      PropertyInfo customerOrders = customerTableInfo.ItemType.GetProperty ("Orders");
      UnresolvedJoinInfo joinInfo = new UnresolvedJoinInfo (customerDefinition, customerOrders, JoinCardinality.Many);

      ResolvedJoinInfo expectedJoinInfo = new ResolvedJoinInfo (orderTableInfo, customerPrimaryKey, orderForeignKey);
      ResolvedJoinInfo resolvedJoinInfo = _mappingResolver.ResolveJoinInfo (joinInfo, _generator);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedJoinInfo.LeftKey, resolvedJoinInfo.LeftKey);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedJoinInfo.RightKey,resolvedJoinInfo.RightKey);
      Assert.AreEqual (expectedJoinInfo.ItemType,resolvedJoinInfo.ItemType);
      Assert.AreEqual (expectedJoinInfo.ForeignTableInfo.ItemType, resolvedJoinInfo.ForeignTableInfo.ItemType);
      Assert.AreEqual (expectedJoinInfo.ForeignTableInfo.TableAlias, resolvedJoinInfo.ForeignTableInfo.TableAlias);
    }

    [Test]
    public void TestResolveJoinInfoReverse ()
    {
      ResolvedSimpleTableInfo customerTableInfo = new ResolvedSimpleTableInfo (typeof (Customer), "dbo.Customers", "t0");
      ResolvedSimpleTableInfo orderTableInfo = new ResolvedSimpleTableInfo (typeof (Order), "dbo.Order", "t1");

      SqlColumnDefinitionExpression customerPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), customerTableInfo.TableAlias, "CustomerID", true);
      SqlColumnDefinitionExpression orderForeignKey = new SqlColumnDefinitionExpression (
          typeof (string), orderTableInfo.TableAlias, "CustomerID", false);
      SqlColumnDefinitionExpression orderPrimaryKey = new SqlColumnDefinitionExpression (
          typeof (string), orderTableInfo.TableAlias, "OrderID", true);

      SqlEntityDefinitionExpression orderDefinition = new SqlEntityDefinitionExpression (orderTableInfo.ItemType, orderTableInfo.TableAlias, null, orderPrimaryKey);
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
  }
}
