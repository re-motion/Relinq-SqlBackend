using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Collections;
using Rubicon.Data.Linq.SqlGeneration;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class SqlGeneratorTest
  {
    private IDatabaseInfo _databaseInfo;
    private IQueryable<Student> _source;

    [SetUp]
    public void SetUp()
    {
      _databaseInfo = new StubDatabaseInfo();
      _source = ExpressionHelper.CreateQuerySource();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The query does not select any fields from the data source.")]
    public void SimpleQuery_WithNonDBFieldProjection ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQueryWithNonDBProjection (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      new SqlGenerator (parsedQuery, _databaseInfo).GetCommandString ();
    }

    [Test]
    public void SimpleQuery()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s]", sqlGenerator.GetCommandString());

      Assert.IsEmpty (sqlGenerator.GetCommandParameters ());
    }

    [Test]
    public void MultiFromQueryWithProjection ()
    {
      IQueryable<Tuple<string, string, int>> query = TestQueryGenerator.CreateMultiFromQueryWithProjection (_source, _source, _source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);
      Assert.AreEqual ("SELECT [s1].[FirstColumn], [s2].[LastColumn], [s3].[IDColumn] FROM [sourceTable] [s1], [sourceTable] [s2], [sourceTable] [s3]",
          sqlGenerator.GetCommandString());
      
      Assert.IsEmpty (sqlGenerator.GetCommandParameters());
    }

    [Test]
    public void SimpleWhereQuery()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleWhereQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);

      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE [s].[LastColumn] = @1", sqlGenerator.GetCommandString ());

      QueryParameter[] parameters = sqlGenerator.GetCommandParameters();
      Assert.That (parameters, Is.EqualTo (new object[] {new QueryParameter("@1", "Garcia")}));
    }
  }
}