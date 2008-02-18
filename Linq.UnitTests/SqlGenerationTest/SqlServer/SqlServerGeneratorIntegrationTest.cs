using System;
using System.Linq;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class SqlServerGeneratorIntegrationTest
  {
    [Test]
    public void JoinReuse()
    {
      // from sdd1 in ...
      // from sdd2 in ...
      // order by sdd1.Student_Detail.Student.First
      // order by sdd2.Student_Detail.Student.First
      // order by sdd1.Student_Detail.Student.First
      // select sdd1;

      IQueryable<Student_Detail_Detail> source1 = ExpressionHelper.CreateQuerySource_Detail_Detail ();
      IQueryable<Student_Detail_Detail> source2 = ExpressionHelper.CreateQuerySource_Detail_Detail ();
      IQueryable<Student_Detail_Detail> query = TestQueryGenerator.CreateImplicitOrderByJoinWithJoinReuse (source1, source2);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      string expectedString = "SELECT [sdd1].* "
          + "FROM "
          + "[detailDetailTable] [sdd1] "
          + "INNER JOIN [detailTable] [j0] ON [sdd1].[Student_Detail_Detail_PK] = [j0].[Student_Detail_FK] "
          + "INNER JOIN [sourceTable] [j1] ON [j0].[Student_Detail_PK] = [j1].[Student_FK], "
          + "[detailDetailTable] [sdd2] "
          + "INNER JOIN [detailTable] [j2] ON [sdd2].[Student_Detail_Detail_PK] = [j2].[Student_Detail_FK] "
          + "INNER JOIN [sourceTable] [j3] ON [j2].[Student_Detail_PK] = [j3].[Student_FK] "
          + "ORDER BY [j1].[FirstColumn] ASC, [j3].[FirstColumn] ASC, [j1].[FirstColumn] ASC";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString();
      Assert.AreEqual (expectedString, result.A);
    }

    [Test]
    public void SelectJoin()
    {
      // from sdd in source 
      // select new Tuple<string,int>{sdd.Student_Detail.Student.First,sdd.IndustrialSector.ID}

      IQueryable<Student_Detail_Detail> source = ExpressionHelper.CreateQuerySource_Detail_Detail ();

      IQueryable<Tuple<string, int>> query = TestQueryGenerator.CreateComplexImplicitSelectJoin (source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      string expectedString = "SELECT [j1].[FirstColumn], [j2].[IDColumn] "
          + "FROM "
          + "[detailDetailTable] [sdd] "
          + "INNER JOIN [detailTable] [j0] ON [sdd].[Student_Detail_Detail_PK] = [j0].[Student_Detail_FK] "
          + "INNER JOIN [sourceTable] [j1] ON [j0].[Student_Detail_PK] = [j1].[Student_FK] "
          + "INNER JOIN [industrialTable] [j2] ON [sdd].[Student_Detail_Detail_PK] = [j2].[IndustrialSector_FK]";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual (expectedString, result.A);
    }
  }
}