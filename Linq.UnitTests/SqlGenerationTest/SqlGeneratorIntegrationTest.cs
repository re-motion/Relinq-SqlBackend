using System;
using System.Linq;
using NUnit.Framework;
using Rubicon.Data.Linq.SqlGeneration;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class SqlGeneratorIntegrationTest
  {
    [Test]
    [Ignore ("Integration test for current story")]
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
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, StubDatabaseInfo.Instance);

      string expectedString = "SELECT [sdd1].* "
          + "FROM "
          + "[detailDetailTable] [sdd1] "
          + "INNER JOIN [detailTable] [join1] ON [sdd1].[Student_Detail_Detail_PK] = [join1].[Student_Detail_FK]) "
          + "INNER JOIN [sourceTable] [join2] on [join1].[Student_Detail_PK] = [join2].[Student_FK], "
          + "[detailDetailTable] [sdd2] "
          + "INNER JOIN [detailTable] [join3] ON [sdd2].[Student_Detail_Detail_PK] = [join3].[Student_Detail_FK]) "
          + "INNER JOIN [sourceTable] [join4] on [join3].[Student_Detail_PK] = [join4].[Student_FK], "
          + "ORDER BY [join2].[First] "
          + "ORDER BY [join4].[First] "
          + "ORDER BY [join2].[First] ";

      Assert.AreEqual (expectedString, sqlGenerator.GetCommandString ());
    }
  }
}