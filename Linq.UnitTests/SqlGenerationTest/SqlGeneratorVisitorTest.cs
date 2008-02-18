using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class SqlGeneratorVisitorTest
  {
    private JoinedTableContext _context;

    [SetUp]
    public void SetUp()
    {
      _context = new JoinedTableContext();
    }

    [Test]
    public void VisitSelectClause ()
    {
      IQueryable<Tuple<string, string>> query = TestQueryGenerator.CreateSimpleQueryWithFieldProjection (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (sqlGeneratorVisitor.Columns, Is.EqualTo (new object[] { new Column (new Table ("sourceTable", "s"), "FirstColumn"),
          new Column (new Table ("sourceTable", "s"), "LastColumn") }));
    }

    [Test]
    public void VisitSelectClause_WithJoins ()
    {
      IQueryable<string> query = TestQueryGenerator.CreateSimpleImplicitSelectJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      
      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;
      sqlGeneratorVisitor.VisitSelectClause (selectClause);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      Table studentDetailTable = parsedQuery.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      Table studentTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember);
      Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, relationMember);
      Join join = new Join (studentTable, studentDetailTable, new Column (studentTable, joinColumns.B),
          new Column (studentDetailTable, joinColumns.A));
     

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<Join> actualJoins = sqlGeneratorVisitor.Joins[studentDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void VisitSelectClause_UsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      VisitSelectClause_WithJoins();
      Assert.AreEqual (1, _context.Count);
    }

    [Test]
    public void VisitMainFromClause()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      MainFromClause fromClause = parsedQuery.MainFromClause;

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitMainFromClause (fromClause);
      Assert.That (sqlGeneratorVisitor.Tables, Is.EqualTo (new object[] { new Table ("sourceTable", "s") }));
    }

    [Test]
    public void VisitAdditionalFromClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateMultiFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      AdditionalFromClause fromClause = (AdditionalFromClause)parsedQuery.QueryBody.BodyClauses.First();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitAdditionalFromClause (fromClause);
      Assert.That (sqlGeneratorVisitor.Tables, Is.EqualTo (new object[] { new Table ("sourceTable", "s2") }));
    }

    [Test]
    public void VisitWhereClause()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleWhereQuery (ExpressionHelper.CreateQuerySource());
      
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      WhereClause whereClause = (WhereClause)parsedQuery.QueryBody.BodyClauses.First();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitWhereClause (whereClause);

      Assert.AreEqual (new BinaryCondition (new Column (new Table ("sourceTable", "s"), "LastColumn"),
          new Constant ("Garcia"), BinaryCondition.ConditionKind.Equal),
          sqlGeneratorVisitor.Criterion);
    }

    [Test]
    [Ignore ("TODO: Implement joins in WhereConditionParser")]
    public void VisitWhereClause_WithJoins()
    {
      Assert.Fail();
    }

    [Test]
    [Ignore ("TODO: Implement joins in WhereConditionParser")]
    public void VisitWhereClause_UsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      VisitWhereClause_WithJoins();
      Assert.AreEqual (1, _context.Count);
    }

    [Test]
    public void VisitOrderingClause()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause);

      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      Assert.That (sqlGeneratorVisitor.OrderingFields,
          Is.EqualTo (new object[] { new OrderingField (fieldDescriptor, OrderDirection.Asc) }));
    }

    [Test]
    public void VisitOrderingClause_WithJoins ()
    {
      IQueryable<Student_Detail> query = TestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      Table leftSide = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Table rightSide = parsedQuery.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, relationMember);

      Join join = new Join (leftSide, rightSide, new Column (leftSide, columns.B), new Column (rightSide, columns.A));

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<Join> actualJoins = sqlGeneratorVisitor.Joins[rightSide];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void VisitOrderingClause_UsesContext()
    {
      Assert.AreEqual (0, _context.Count);
      VisitOrderingClause_WithJoins();
      Assert.AreEqual (1, _context.Count);
    }

    [Test]
    public void VisitOrderByClause ()
    {
      IQueryable<Student> query =
        TestQueryGenerator.CreateThreeOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();

      FieldDescriptor fieldDescriptor1 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      FieldDescriptor fieldDescriptor2 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("Last"));

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitOrderByClause (orderBy1);
      Assert.That (sqlGeneratorVisitor.OrderingFields,
          Is.EqualTo (new object[]
              {
                new OrderingField (fieldDescriptor1, OrderDirection.Asc),
                new OrderingField (fieldDescriptor2, OrderDirection.Asc),
              }));
    }
  }
}