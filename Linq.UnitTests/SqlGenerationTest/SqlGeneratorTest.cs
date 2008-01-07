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

      CommandParameter[] parameters = sqlGenerator.GetCommandParameters();
      Assert.That (parameters, Is.EqualTo (new object[] {new CommandParameter("@1", "Garcia")}));
    }

    [Test]
    public void WhereQueryWithOrAndNot ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithOrAndNot (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);

      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE ((NOT ([s].[FirstColumn] = @1)) OR ([s].[FirstColumn] = @2)) AND ([s].[FirstColumn] = @3)",
          sqlGenerator.GetCommandString ());

      CommandParameter[] parameters = sqlGenerator.GetCommandParameters ();
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia"),
          new CommandParameter ("@2", "Garcia"), new CommandParameter ("@3", "Garcia") }));
    }

    [Test]
    public void WhereQueryWithComparisons ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithDifferentComparisons (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);

      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE ((((([s].[FirstColumn] != @1) AND ([s].[IDColumn] > @2)) "
        + "AND ([s].[IDColumn] >= @3)) AND ([s].[IDColumn] < @4)) AND ([s].[IDColumn] <= @5)) AND ([s].[IDColumn] = @6)",
          sqlGenerator.GetCommandString ());

      CommandParameter[] parameters = sqlGenerator.GetCommandParameters ();
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia"),
          new CommandParameter ("@2", 5), new CommandParameter ("@3", 6), new CommandParameter ("@4", 7),
          new CommandParameter ("@5", 6), new CommandParameter ("@6", 6)}));
    }

    [Test]
    public void WhereQueryWithNullChecks ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryNullChecks (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);

      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE ([s].[FirstColumn] IS NULL) OR ([s].[LastColumn] IS NOT NULL)",
        sqlGenerator.GetCommandString ());

      CommandParameter[] parameters = sqlGenerator.GetCommandParameters ();
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithBooleanConstantTrue ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryBooleanConstantTrue (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);

      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE 1=1",
        sqlGenerator.GetCommandString ());

      CommandParameter[] parameters = sqlGenerator.GetCommandParameters ();
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithBooleanConstantFalse ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryBooleanConstantFalse (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);

      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE 1!=1",
        sqlGenerator.GetCommandString ());

      CommandParameter[] parameters = sqlGenerator.GetCommandParameters ();
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithStartsWith()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithStartsWith (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE [s].[FirstColumn] LIKE @1",
          sqlGenerator.GetCommandString());
      CommandParameter[] parameters = sqlGenerator.GetCommandParameters ();
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia%") }));
    }

    [Test]
    public void WhereQueryWithEndsWith ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithEndsWith (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery, _databaseInfo);
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE [s].[FirstColumn] LIKE @1",
          sqlGenerator.GetCommandString ());
      CommandParameter[] parameters = sqlGenerator.GetCommandParameters ();
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "%Garcia") }));
    }
  }
}