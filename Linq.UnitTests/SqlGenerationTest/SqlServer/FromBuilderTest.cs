using System.Linq;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class FromBuilderTest
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
    public void MultiFromQueryWithProjection ()
    {
      IQueryable<Tuple<string, string, int>> query = TestQueryGenerator.CreateMultiFromQueryWithProjection (_source, _source, _source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s1].[FirstColumn], [s2].[LastColumn], [s3].[IDColumn] FROM [sourceTable] [s1], [sourceTable] [s2], [sourceTable] [s3]",
          result.A);

      Assert.IsEmpty (result.B);
    }
  }
}