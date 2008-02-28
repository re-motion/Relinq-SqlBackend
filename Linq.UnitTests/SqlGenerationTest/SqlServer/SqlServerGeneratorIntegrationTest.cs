using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class SqlServerGeneratorIntegrationTest
  {
    private IQueryable<Student> _source;

    [SetUp]
    public void SetUp ()
    {
      _source = ExpressionHelper.CreateQuerySource ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The query does not select any fields from the data source.")]
    public void SimpleQuery_WithNonDBFieldProjection ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQueryWithNonDBProjection (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance).BuildCommandString ();
    }

    [Test]
    public void SimpleQuery ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s]", result.A);

      Assert.IsEmpty (result.B);
    }

    [Test]
    public void MultiFromQueryWithProjection ()
    {
      IQueryable<Tuple<string, string, int>> query = TestQueryGenerator.CreateMultiFromQueryWithProjection (_source, _source, _source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s1].[FirstColumn], [s2].[LastColumn], [s3].[IDColumn] FROM [studentTable] [s1], [studentTable] [s2], [studentTable] [s3]",
          result.A);

      Assert.IsEmpty (result.B);
    }

    [Test]
    public void SimpleWhereQuery ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleWhereQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE [s].[LastColumn] = @1", result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia") }));
    }

    [Test]
    public void WhereQueryWithOrAndNot ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithOrAndNot (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ((NOT ([s].[FirstColumn] = @1)) OR ([s].[FirstColumn] = @2)) AND ([s].[FirstColumn] = @3)",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia"),
          new CommandParameter ("@2", "Garcia"), new CommandParameter ("@3", "Garcia") }));
    }

    [Test]
    public void WhereQueryWithComparisons ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithDifferentComparisons (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ((((([s].[FirstColumn] != @1) AND ([s].[IDColumn] > @2)) "
          + "AND ([s].[IDColumn] >= @3)) AND ([s].[IDColumn] < @4)) AND ([s].[IDColumn] <= @5)) AND ([s].[IDColumn] = @6)",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia"),
          new CommandParameter ("@2", 5), new CommandParameter ("@3", 6), new CommandParameter ("@4", 7),
          new CommandParameter ("@5", 6), new CommandParameter ("@6", 6)}));
    }

    [Test]
    public void WhereQueryWithNullChecks ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryNullChecks (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ([s].[FirstColumn] IS NULL) OR ([s].[LastColumn] IS NOT NULL)",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithBooleanConstantTrue ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryBooleanConstantTrue (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE 1=1",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithBooleanConstantFalse ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryBooleanConstantFalse (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE 1!=1",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithStartsWith ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithStartsWith (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE [s].[FirstColumn] LIKE @1",
          result.A);
      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia%") }));
    }

    [Test]
    public void WhereQueryWithEndsWith ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithEndsWith (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE [s].[FirstColumn] LIKE @1",
          result.A);
      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "%Garcia") }));
    }

    [Test]
    public void SimpleOrderByQuery ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleOrderByQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();

      Assert.AreEqual ("SELECT [s1].* FROM [studentTable] [s1] ORDER BY [s1].[FirstColumn] ASC",
          result.A);
    }

    [Test]
    public void ComplexOrderByQuery ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateTwoOrderByQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s1].* FROM [studentTable] [s1] ORDER BY [s1].[FirstColumn] ASC, [s1].[LastColumn] DESC",
          result.A);
    }

    [Test]
    public void SimpleImplicitJoin ()
    {
      // from sd in source orderby sd.Student.First select sd
      IQueryable<Student_Detail> query = TestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [sd].* FROM [detailTable] [sd] INNER JOIN [studentTable] [j0] "
          + "ON [sd].[Student_Detail_PK] = [j0].[Student_Detail_to_Student_FK] ORDER BY [j0].[FirstColumn] ASC",
          result.A);
    }

    [Test]
    public void NestedImplicitJoin ()
    {
      // from sdd in source orderby sdd.Student_Detail.Student.First select sdd
      IQueryable<Student_Detail_Detail> query = TestQueryGenerator.CreateDoubleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      string expectedString = "SELECT [sdd].* FROM [detailDetailTable] [sdd] "
          + "INNER JOIN [detailTable] [j0] ON [sdd].[Student_Detail_Detail_PK] = [j0].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "INNER JOIN [studentTable] [j1] ON [j0].[Student_Detail_PK] = [j1].[Student_Detail_to_Student_FK] "
          + "ORDER BY [j1].[FirstColumn] ASC";
      Assert.AreEqual (expectedString, result.A);
    }

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
          + "INNER JOIN [detailTable] [j0] ON [sdd1].[Student_Detail_Detail_PK] = [j0].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "INNER JOIN [studentTable] [j1] ON [j0].[Student_Detail_PK] = [j1].[Student_Detail_to_Student_FK], "
          + "[detailDetailTable] [sdd2] "
          + "INNER JOIN [detailTable] [j2] ON [sdd2].[Student_Detail_Detail_PK] = [j2].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "INNER JOIN [studentTable] [j3] ON [j2].[Student_Detail_PK] = [j3].[Student_Detail_to_Student_FK] "
          + "ORDER BY [j1].[FirstColumn] ASC, [j3].[FirstColumn] ASC, [j1].[FirstColumn] ASC";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString();
      Assert.AreEqual (expectedString, result.A);
    }

    [Test]
    public void JoinPartReuse ()
    {
      //from sdd in ...
      //orderby sdd.Student_Detail.Student.First
      //orderby sdd.Student_Detail.ID
      //select sdd;

      IQueryable<Student_Detail_Detail> source1 = ExpressionHelper.CreateQuerySource_Detail_Detail ();
      IQueryable<Student_Detail_Detail> query = TestQueryGenerator.CreateImplicitOrderByJoinWithJoinPartReuse (source1);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      string expectedString = "SELECT [sdd].* "
          + "FROM "
          + "[detailDetailTable] [sdd] "
          + "INNER JOIN [detailTable] [j0] ON [sdd].[Student_Detail_Detail_PK] = [j0].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "INNER JOIN [studentTable] [j1] ON [j0].[Student_Detail_PK] = [j1].[Student_Detail_to_Student_FK] "
          + "ORDER BY [j1].[FirstColumn] ASC, [j0].[IDColumn] ASC";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
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
          + "INNER JOIN [detailTable] [j0] ON [sdd].[Student_Detail_Detail_PK] = [j0].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "INNER JOIN [studentTable] [j1] ON [j0].[Student_Detail_PK] = [j1].[Student_Detail_to_Student_FK] "
          + "INNER JOIN [industrialTable] [j2] ON [sdd].[Student_Detail_Detail_PK] = [j2].[Student_Detail_Detail_to_IndustrialSector_FK]";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual (expectedString, result.A);
    }

    [Test]
    public void SelectJoin_WithRelationMember()
    {
      IQueryable<Student_Detail> source = ExpressionHelper.CreateQuerySource_Detail ();

      IQueryable<Student> query = TestQueryGenerator.CreateRelationMemberSelectQuery (source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      const string expectedString = "SELECT [j0].* FROM [detailTable] [sd] INNER JOIN "
          + "[studentTable] [j0] ON [sd].[Student_Detail_PK] = [j0].[Student_Detail_to_Student_FK]";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual (expectedString, result.A);
    }

    [Test]
    public void Select_WithDistinct ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource();
      IQueryable<string> query = TestQueryGenerator.CreateSimpleDisinctQuery (source);

      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      const string expectedString = "SELECT DISTINCT [s].[FirstColumn] FROM [studentTable] [s]";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual (expectedString, result.A);
    }

    [Test]
    public void Select_WithDistinctAndWhere ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<string> query = TestQueryGenerator.CreateDisinctWithWhereQuery (source);

      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      const string expectedString = "SELECT DISTINCT [s].[FirstColumn] FROM [studentTable] [s] WHERE [s].[FirstColumn] = @1";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual (expectedString, result.A);
    }

    [Test]
    public void WhereJoin_WithRelationMember ()
    {
      IQueryable<Student_Detail> source = ExpressionHelper.CreateQuerySource_Detail ();

      IQueryable<Student_Detail> query = TestQueryGenerator.CreateRelationMemberWhereQuery (source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      const string expectedString = "SELECT [sd].* FROM [detailTable] [sd] WHERE [sd].[Student_Detail_to_IndustrialSector_FK] IS NOT NULL";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual (expectedString, result.A);
    }

    [Test]
    [Ignore ("TODO: Implement querying of virtual side")]
    public void WhereJoin_WithRelationMember_VirtualSide ()
    {
      IQueryable<IndustrialSector> source = ExpressionHelper.CreateQuerySource_IndustrialSector ();

      IQueryable<IndustrialSector> query = TestQueryGenerator.CreateRelationMemberVirtualSideWhereQuery (source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);

      const string expectedString = "SELECT [industrial].* FROM [industrialTable] [industrial] "
        + "WHERE EXISTS ("
        + "SELECT [j0].* FROM [detailTable] [j0] WHERE [j0].[Student_Detail_to_IndustrialSector_FK] = [industrial].[IndustrialSector_PK]"
        + ")";

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual (expectedString, result.A);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Ordering by 'Rubicon.Data.Linq.UnitTests.Student_Detail.Student' is not "
        + "supported because it is a relation member.")]
    public void OrderingJoin_WithRelationMember ()
    {
      IQueryable<Student_Detail> source = ExpressionHelper.CreateQuerySource_Detail ();

      IQueryable<Student_Detail> query = TestQueryGenerator.CreateRelationMemberOrderByQuery(source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, StubDatabaseInfo.Instance);
      sqlGenerator.BuildCommandString ();
    }
  }
}