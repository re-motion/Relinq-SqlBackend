using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.IntegrationTests.Utilities;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using System.Linq.Expressions;

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
    public void  ResolveMemberExpression()
    {

      var memberInfo = typeof (Person).GetProperty ("First");

      var sqlEntityExpression = new SqlEntityDefinitionExpression (typeof (Person), "p", null, new SqlColumnDefinitionExpression (typeof (string), "p", "First", true));

      SqlColumnExpression result = (SqlColumnExpression)_mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      Assert.AreEqual (result.IsPrimaryKey, true);
      Assert.AreEqual (result.Type, typeof (string));
      Assert.AreEqual (result.ColumnName, "First");
    }

    [Test]
    public void ResolveMemberExpressionUsingNorthwindEntitiesPrimaryKey ()
    {
      //Test object
      Type type = typeof (Customer);
      string columnName = "CustomerID";
      bool isPrimaryKey = true;


      var memberInfo = type.GetProperty (columnName);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (type, "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", columnName, isPrimaryKey));

      SqlColumnExpression result = (SqlColumnExpression) _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      Assert.AreEqual (result.IsPrimaryKey, isPrimaryKey);
      Assert.AreEqual (result.Type, typeof (string));
      Assert.AreEqual (result.ColumnName, columnName);
    }

    [Test]
    public void ResolveMemberExpressionUsingNorthwindEntitiesNonPrimaryKey ()
    {
      //Test object
      Type type = typeof (Customer);
      string columnName = "CompanyName";
      bool isPrimaryKey = false;


      var memberInfo = type.GetProperty (columnName);
      var sqlEntityExpression = new SqlEntityDefinitionExpression (type, "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", columnName, isPrimaryKey));

      SqlColumnExpression result = (SqlColumnExpression) _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      Assert.AreEqual (result.IsPrimaryKey, isPrimaryKey);
      Assert.AreEqual (result.Type, typeof (string));
      Assert.AreEqual (result.ColumnName, columnName);
    }

    [Test]
    public void ResolveMemberExpressionUsingNorthwindEntitiesAssociated ()
    {
      //Test object
      Type type = typeof (Order);
      string columnName = "Customer"; //foreign key
      bool isPrimaryKey = false;

      var memberInfo = type.GetProperty (columnName);

      var sqlEntityExpression = new SqlEntityDefinitionExpression (type, "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", columnName, isPrimaryKey));

      SqlEntityRefMemberExpression result = (SqlEntityRefMemberExpression) _mappingResolver.ResolveMemberExpression (sqlEntityExpression, memberInfo);

      Assert.AreEqual (result.Type, typeof (string));
    }

  }
}
