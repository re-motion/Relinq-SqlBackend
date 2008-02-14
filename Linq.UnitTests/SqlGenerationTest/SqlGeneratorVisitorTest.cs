using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    [Test]
    public void VisitSelectClause_IdentityProjection()
    {    
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (sqlGeneratorVisitor.Columns, Is.EqualTo (new object[] {new Column(new Table ("sourceTable", "s"), "*")}));
    }

    [Test]
    public void VisitSelectClause_FieldProjection ()
    {
      IQueryable<Tuple<string, string>> query = TestQueryGenerator.CreateSimpleQueryWithFieldProjection (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (sqlGeneratorVisitor.Columns, Is.EqualTo (new object[] { new Column (new Table ("sourceTable", "s"), "FirstColumn"),
          new Column (new Table ("sourceTable", "s"), "LastColumn") }));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The select clause contains an expression that cannot be parsed",
        MatchType = MessageMatch.Contains)]
    public void VisitSelectClause_SpecialProjection()
    {
      IQueryable<Tuple<Student, string, string, string>> query = TestQueryGenerator.CreateSimpleQueryWithSpecialProjection (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.QueryBody.SelectOrGroupClause;

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (sqlGeneratorVisitor.Columns, Is.EqualTo (new object[]
          {
              new Column(new Table ("sourceTable", "s"), "*"), new Column(new Table ("sourceTable", "s"), "LastColumn")
          }));
    }

    [Test]
    public void VisitMainFromClause()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      MainFromClause fromClause = parsedQuery.MainFromClause;

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitMainFromClause (fromClause);
      Assert.That (sqlGeneratorVisitor.Tables, Is.EqualTo (new object[] { new Table ("sourceTable", "s") }));
    }

    [Test]
    public void VisitAdditionalFromClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateMultiFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      AdditionalFromClause fromClause = (AdditionalFromClause)parsedQuery.QueryBody.BodyClauses.First();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitAdditionalFromClause (fromClause);
      Assert.That (sqlGeneratorVisitor.Tables, Is.EqualTo (new object[] { new Table ("sourceTable", "s2") }));
    }

    [Test]
    public void VisitWhereClause()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleWhereQuery (ExpressionHelper.CreateQuerySource());
      
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      WhereClause whereClause = (WhereClause)parsedQuery.QueryBody.BodyClauses.First();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitWhereClause (whereClause);

      Assert.AreEqual (new BinaryCondition (new Column (new Table ("sourceTable", "s"), "LastColumn"),
          new Constant ("Garcia"), BinaryCondition.ConditionKind.Equal),
          sqlGeneratorVisitor.Criterion);
    }

    [Test]
    public void VisitOrderingClause()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause);

      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      Assert.That (sqlGeneratorVisitor.OrderingFields,
          Is.EqualTo (new object[] { new OrderingField (fieldDescriptor, OrderDirection.Asc) }));

    }

    [Test]
    public void VisitTwoOrderingClause ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateTwoOrderByQuery (ExpressionHelper.CreateQuerySource());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First();
      OrderByClause orderBy2 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Last();
      OrderingClause orderingClause1 = orderBy1.OrderingList.First();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      OrderingClause orderingClause2 = orderBy2.OrderingList.Last();
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);

      FieldDescriptor fieldDescriptor1 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      FieldDescriptor fieldDescriptor2 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("Last"));

      Assert.That (sqlGeneratorVisitor.OrderingFields,
          Is.EqualTo (new object[]
              {
                  new OrderingField (fieldDescriptor1, OrderDirection.Asc),
                  new OrderingField (fieldDescriptor2, OrderDirection.Desc)
              }));
    }

    [Test]
    public void VisitMixedOrderingClause ()
    {
      IQueryable<Student> query =
        TestQueryGenerator.CreateThreeOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderByClause orderBy2 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Last ();
      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy1.OrderingList.Last ();
      OrderingClause orderingClause3 = orderBy2.OrderingList.Last ();

      FieldDescriptor fieldDescriptor1 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      FieldDescriptor fieldDescriptor2 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("Last"));

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause3);
      Assert.That (sqlGeneratorVisitor.OrderingFields,
          Is.EqualTo (new object[]
              {
                new OrderingField (fieldDescriptor1, OrderDirection.Asc),
                new OrderingField (fieldDescriptor2, OrderDirection.Asc),
                new OrderingField (fieldDescriptor2, OrderDirection.Desc)
              }));
    }

    [Test]
    public void VisitOrderingClause_WithJoins()
    {
      IQueryable<Student_Detail> query = TestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      Table table = parsedQuery.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      Join join = CreateJoin(relationMember, table, table);

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<Join> actualJoins = sqlGeneratorVisitor.Joins[table];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void VisitOrderingClause_WithNestedJoins ()
    {
      IQueryable<Student_Detail_Detail> query = TestQueryGenerator.CreateDoubleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, parsedQuery);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause);

      PropertyInfo relationMember1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      Table studentDetailDetailTable = parsedQuery.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      Join join1 = CreateJoin (relationMember1, studentDetailDetailTable, studentDetailDetailTable);

      PropertyInfo relationMember2 = typeof (Student_Detail).GetProperty ("Student");
      Table studentDetailTable = join1.LeftSide;
      Join join2 = CreateJoin (relationMember2, join1, studentDetailTable);

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<Join> actualJoins = sqlGeneratorVisitor.Joins[studentDetailDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join2 }));
    }

    [Test]
    [Ignore ("TODO")]
    public void MultipleJoinsForSameTable()
    {


      // 1)
      // order by sdd.Student_Detail.Student.First
      // order by sdd.IndustrialSector.ID
      // Joins[sdd] = { (sdd -> Student_Detail -> Student), (sdd -> IndustrialSector) }

      // 2)
      // order by sdd.Student_Detail.Student.First
      // order by sdd.Student_Detail.Student.Last
      // Joins[sdd] = { (sdd -> Student_Detail -> Student) }

      // 3)
      // order by sdd.Student_Detail.Student.First
      // order by sdd.Student_Detail.IndustrialSector.ID
      // Joins[sdd] = { (sdd -> Student_Detail -> Student), (sdd -> Student_Detail -> IndustrialSector) }

      // 4)
      // order by sdd1.Student_Detail.Student.First
      // order by sdd2.Student_Detail.Student.First
      // Joins[sdd1] = { (sdd1 -> Student_Detail -> Student) }
      // Joins[sdd2] = { (sdd2 -> Student_Detail -> Student) }
      Assert.Fail ("Implement");
    }


    private Join CreateJoin (MemberInfo relationMember, IFieldSourcePath rightSide, Table rightSideTable)
    {
      Table leftSide = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, relationMember);
      return new Join (leftSide, rightSide, new Column (leftSide, columns.B), new Column (rightSideTable, columns.A));
    }

  }
}