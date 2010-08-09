using System.Data.Linq;
using System.Data.Linq.Mapping;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
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
      SqlColumnExpression territoriesColumn = new SqlColumnDefinitionExpression(typeof(EntitySet<Territory>),simpleTableInfo.TableAlias, "Territories", false);

      SqlEntityDefinitionExpression expectedExpr = new SqlEntityDefinitionExpression (simpleTableInfo.ItemType, simpleTableInfo.TableAlias, simpleTableInfo.TableName, primaryColumn, descriptionColumn, territoriesColumn);

      SqlEntityDefinitionExpression resolvedExpr = _mappingResolver.ResolveSimpleTableInfo (simpleTableInfo, _generator);

      Assert.AreEqual (expectedExpr.Type, resolvedExpr.Type);
      Assert.AreEqual (expectedExpr.TableAlias, resolvedExpr.TableAlias);
      Assert.AreEqual (expectedExpr.PrimaryKeyColumn, resolvedExpr.PrimaryKeyColumn);
      Assert.AreEqual (expectedExpr.Columns, resolvedExpr.Columns);
    }
  }
}
