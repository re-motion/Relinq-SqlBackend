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
          + "INNER JOIN [detailTable] [j0] ON [sdd1].[Student_Detail_Detail_PK] = [j0].[Student_Detail_FK] "
          + "INNER JOIN [sourceTable] [j1] ON [j0].[Student_Detail_PK] = [j1].[Student_FK], "
          + "[detailDetailTable] [sdd2] "
          + "INNER JOIN [detailTable] [j2] ON [sdd2].[Student_Detail_Detail_PK] = [j2].[Student_Detail_FK] "
          + "INNER JOIN [sourceTable] [j3] ON [j2].[Student_Detail_PK] = [j3].[Student_FK] "
          + "ORDER BY [j1].[FirstColumn] ASC, [j3].[FirstColumn] ASC, [j1].[FirstColumn] ASC";

      Assert.AreEqual (expectedString, sqlGenerator.GetCommandString ());
    }
  }
}