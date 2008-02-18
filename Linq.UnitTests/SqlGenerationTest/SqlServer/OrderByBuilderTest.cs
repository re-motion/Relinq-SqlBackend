using System.Linq;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;
using Rubicon.Data.Linq.UnitTests;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class OrderByBuilderTest
  {
    private IDatabaseInfo _databaseInfo;
    private IQueryable<Student> _source;

    [SetUp]
    public void SetUp ()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
      _source = ExpressionHelper.CreateQuerySource ();

    }

    [Test]
    public void SimpleOrderByQuery ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleOrderByQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();

      Assert.AreEqual ("SELECT [s1].* FROM [sourceTable] [s1] ORDER BY [s1].[FirstColumn] ASC",
          result.A);
    }

    [Test]
    public void ComplexOrderByQuery ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateTwoOrderByQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s1].* FROM [sourceTable] [s1] ORDER BY [s1].[FirstColumn] ASC, [s1].[LastColumn] DESC",
          result.A);
    }

    [Test]
    public void SimpleImplicitJoin ()
    {
      // from sd in source orderby sd.Student.First select sd
      IQueryable<Student_Detail> query = TestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [sd].* FROM [detailTable] [sd] INNER JOIN [sourceTable] [j0] "
          + "ON [sd].[Student_Detail_PK] = [j0].[Student_FK] ORDER BY [j0].[FirstColumn] ASC",
          result.A);
    }

    [Test]
    public void NestedImplicitJoin ()
    {
      // from sdd in source orderby sdd.Student_Detail.Student.First select sdd
      IQueryable<Student_Detail_Detail> query = TestQueryGenerator.CreateDoubleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      string expectedString = "SELECT [sdd].* FROM [detailDetailTable] [sdd] "
          + "INNER JOIN [detailTable] [j0] ON [sdd].[Student_Detail_Detail_PK] = [j0].[Student_Detail_FK] "
          + "INNER JOIN [sourceTable] [j1] ON [j0].[Student_Detail_PK] = [j1].[Student_FK] "
          + "ORDER BY [j1].[FirstColumn] ASC";
      Assert.AreEqual (expectedString, result.A);
    }
  }
}