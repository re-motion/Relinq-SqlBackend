using System;
using System.Linq;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class SelectBuilderTest
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
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The query does not select any fields from the data source.")]
    public void SimpleQuery_WithNonDBFieldProjection ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQueryWithNonDBProjection (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      new SqlServerGenerator (parsedQuery, _databaseInfo).BuildCommandString ();
    }

    [Test]
    public void SimpleQuery ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s]", result.A);

      Assert.IsEmpty (result.B);
    }

  }
}