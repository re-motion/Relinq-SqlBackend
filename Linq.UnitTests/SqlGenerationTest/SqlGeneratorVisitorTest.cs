using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class SqlGeneratorVisitorTest
  {
    private SqlGeneratorVisitor _sqlGeneratorVisitor;

    [SetUp]
    public void SetUp()
    {
      _sqlGeneratorVisitor = new SqlGeneratorVisitor (new StubDatabaseInfo());
    }

    [Test]
    public void VisitSelectClause_IdentityProjection()
    {    
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;
      _sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (_sqlGeneratorVisitor.Columns, Is.EqualTo (new object[] {new Column(new Table ("sourceTable", "s"), "*")}));
    }

    [Test]
    public void VisitSelectClause_FieldProjection ()
    {
      IQueryable<Tuple<string, string>> query = TestQueryGenerator.CreateSimpleQueryWithFieldProjection (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;
      _sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (_sqlGeneratorVisitor.Columns, Is.EqualTo (new object[] { new Column (new Table ("sourceTable", "s"), "FirstColumn"),
          new Column (new Table ("sourceTable", "s"), "LastColumn") }));
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "The select clause contains an expression that cannot be parsed",
        MatchType = MessageMatch.Contains)]
    public void VisitSelectClause_SpecialProjection()
    {
      IQueryable<Tuple<Student, string, string, string>> query = TestQueryGenerator.CreateSimpleQueryWithSpecialProjection (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;
      _sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (_sqlGeneratorVisitor.Columns, Is.EqualTo (new object[]
          {
              new Column(new Table ("sourceTable", "s"), "*"), new Column(new Table ("sourceTable", "s"), "LastColumn")
          }));
    }

    [Test]
    public void VisitMainFromClause()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      MainFromClause fromClause = parsedQuery.FromClause;
      _sqlGeneratorVisitor.VisitMainFromClause (fromClause);
      Assert.That (_sqlGeneratorVisitor.Tables, Is.EqualTo (new object[] { new Table ("sourceTable", "s") }));
    }

    [Test]
    public void VisitAdditionalFromClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateMultiFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      AdditionalFromClause fromClause = (AdditionalFromClause)parsedQuery.QueryBody.FromLetWhereClauses.First();
      _sqlGeneratorVisitor.VisitAdditionalFromClause (fromClause);
      Assert.That (_sqlGeneratorVisitor.Tables, Is.EqualTo (new object[] { new Table ("sourceTable", "s2") }));
    }

    [Test]
    public void VisitWhereClause()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleWhereQuery (ExpressionHelper.CreateQuerySource());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      WhereClause whereClause = (WhereClause)parsedQuery.QueryBody.FromLetWhereClauses.First();
      _sqlGeneratorVisitor.VisitWhereClause (whereClause);

      Assert.AreEqual (new BinaryCondition (new Column (new Table ("sourceTable", "s"), "LastColumn"),
          new Constant ("Garcia"), BinaryCondition.ConditionKind.Equal),
          _sqlGeneratorVisitor.Criterion);
    }


    
  }
}