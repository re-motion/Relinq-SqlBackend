using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Data.Linq.SqlGeneration;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class SqlGeneratorVisitorIntegrationTest
  {
    private JoinedTableContext _context;

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext ();
    }

    [Test]
    public void VisitOrderingClause_WithNestedJoins ()
    {
      IQueryable<Student_Detail_Detail> query = TestQueryGenerator.CreateDoubleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause);

      PropertyInfo relationMember1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      Table studentDetailDetailTable = parsedQuery.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      JoinTree join1 = CreateJoin (relationMember1, studentDetailDetailTable, studentDetailDetailTable);

      PropertyInfo relationMember2 = typeof (Student_Detail).GetProperty ("Student");
      Table studentDetailTable = join1.LeftSide;
      JoinTree join2 = CreateJoin (relationMember2, join1, studentDetailTable);

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.Joins[studentDetailDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join2.GetSingleJoinForRoot(), join1.GetSingleJoinForRoot() }));
    }

    [Test]
    public void MultipleJoinsForSameTable ()
    {
      // 1)
      // order by sdd.Student_Detail.Student.First
      // order by sdd.IndustrialSector.ID
      // Joins[sdd] = { (sdd -> Student_Detail -> Student), (sdd -> IndustrialSector) }

      IQueryable<Student_Detail_Detail> query =
        TestQueryGenerator.CreateImplicitOrderByJoinWithMultipleJoins (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();

      OrderingClause orderingClause1 = orderBy.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy.OrderingList.Last ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);

      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);

      PropertyInfo relationalMemberForFirstOrdering1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      Table studentDetailDetailTable = parsedQuery.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      JoinTree join1 = CreateJoin (relationalMemberForFirstOrdering1, studentDetailDetailTable, studentDetailDetailTable);

      PropertyInfo relationalMemberForFirstOrdering2 = typeof (Student_Detail).GetProperty ("Student");
      Table studentDetailTable = join1.LeftSide;
      JoinTree join2 = CreateJoin (relationalMemberForFirstOrdering2, join1, studentDetailTable);

      PropertyInfo relationalMemberForLastOrdering = typeof (Student_Detail_Detail).GetProperty ("IndustrialSector");
      JoinTree join3 = CreateJoin (relationalMemberForLastOrdering, studentDetailDetailTable, studentDetailDetailTable);

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.Joins[studentDetailDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join2.GetSingleJoinForRoot (), join1.GetSingleJoinForRoot (), join3.GetSingleJoinForRoot () }));

    }

    [Test]
    public void OneJoinWithMultipleExpression ()
    {
      // 2)
      // order by sdd.Student_Detail.Student.First
      // order by sdd.Student_Detail.Student.Last
      // Joins[sdd] = { (sdd -> Student_Detail -> Student) }

      IQueryable<Student_Detail_Detail> query =
        TestQueryGenerator.CreateImplicitOrderByJoinCheckingCorrectNumberOfEntries (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();

      OrderingClause orderingClause1 = orderBy.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy.OrderingList.Last ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);

      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);

      PropertyInfo relationalMemberForFirstOrdering1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      Table studentDetailDetailTable = parsedQuery.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      JoinTree join1 = CreateJoin (relationalMemberForFirstOrdering1, studentDetailDetailTable, studentDetailDetailTable);

      PropertyInfo relationalMemberForFirstOrdering2 = typeof (Student_Detail).GetProperty ("Student");
      Table studentDetailTable = join1.LeftSide;
      JoinTree join2 = CreateJoin (relationalMemberForFirstOrdering2, join1, studentDetailTable);

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.Joins[studentDetailDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join2.GetSingleJoinForRoot (), join1.GetSingleJoinForRoot () }));

    }

    [Test]
    public void JoinWithDifferentLevels ()
    {
      // 3)
      // order by sdd.Student_Detail.Student.First
      // order by sdd.Student_Detail.IndustrialSector.ID
      // Joins[sdd] = { (sdd -> Student_Detail -> Student), (sdd -> Student_Detail -> IndustrialSector) }

      IQueryable<Student_Detail_Detail> query =
        TestQueryGenerator.CreateImplicitOrderByJoinWithDifferentLevels (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      OrderByClause orderBy = (OrderByClause) parsedQuery.QueryBody.BodyClauses.First ();

      OrderingClause orderingClause1 = orderBy.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy.OrderingList.Last ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);

      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);

      PropertyInfo relationalMemberForFirstOrdering1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      Table studentDetailDetailTable = parsedQuery.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      JoinTree join1 = CreateJoin (relationalMemberForFirstOrdering1, studentDetailDetailTable, studentDetailDetailTable);

      PropertyInfo relationalMemberForFirstOrdering2 = typeof (Student_Detail).GetProperty ("Student");
      Table studentDetailTable = join1.LeftSide;
      JoinTree join2 = CreateJoin (relationalMemberForFirstOrdering2, join1, studentDetailTable);

      PropertyInfo relationalMemberForLastOrdering = typeof (Student_Detail).GetProperty ("IndustrialSector");
      JoinTree join3 = CreateJoin (relationalMemberForLastOrdering, join1, studentDetailTable);

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.Joins[studentDetailDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join2.GetSingleJoinForRoot (), join1.GetSingleJoinForRoot (), join3.GetSingleJoinForRoot () }));
    }

    [Test]
    public void JoinWithMultipleKeys ()
    {
      // 4)
      // order by sdd1.Student_Detail.Student.First
      // order by sdd2.Student_Detail.Student.First
      // Joins[sdd1] = { (sdd1 -> Student_Detail -> Student) }
      // Joins[sdd2] = { (sdd2 -> Student_Detail -> Student) }

      IQueryable<Student_Detail_Detail> query =
        TestQueryGenerator.CreateImplicitOrderByJoinWithMultipleKeys
        (ExpressionHelper.CreateQuerySource_Detail_Detail (), ExpressionHelper.CreateQuerySource_Detail_Detail ());

      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);

      OrderByClause orderBy1 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Skip (1).First ();
      OrderByClause orderBy2 = (OrderByClause) parsedQuery.QueryBody.BodyClauses.Last ();

      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy2.OrderingList.First ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);

      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);

      PropertyInfo relationalMemberFirstOrderBy1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      Table studentDetailDetailTable1 = parsedQuery.MainFromClause.GetTable (StubDatabaseInfo.Instance);
      JoinTree join1 = CreateJoin (relationalMemberFirstOrderBy1, studentDetailDetailTable1, studentDetailDetailTable1);

      PropertyInfo relationalMemberFirstOrderBy2 = typeof (Student_Detail).GetProperty ("Student");
      Table studentDetailTable1 = join1.LeftSide;
      JoinTree join2 = CreateJoin (relationalMemberFirstOrderBy2, join1, studentDetailTable1);

      PropertyInfo relationalMemberSecondOrderBy1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      Table studentDetailDetailTable2 = ((AdditionalFromClause) parsedQuery.QueryBody.BodyClauses[0]).GetTable (StubDatabaseInfo.Instance);
      JoinTree join3 = CreateJoin (relationalMemberSecondOrderBy1, studentDetailDetailTable2, studentDetailDetailTable2);

      PropertyInfo relationalMemberSecondOrderBy2 = typeof (Student_Detail).GetProperty ("Student");
      Table studentDetailTable2 = join3.LeftSide;
      JoinTree join4 = CreateJoin (relationalMemberSecondOrderBy2, join3, studentDetailTable2);

      Assert.AreEqual (2, sqlGeneratorVisitor.Joins.Count);
      List<SingleJoin> actualJoins1 = sqlGeneratorVisitor.Joins[studentDetailDetailTable1];
      Assert.That (actualJoins1, Is.EqualTo (new object[] { join2.GetSingleJoinForRoot (), join1.GetSingleJoinForRoot() }));

      List<SingleJoin> actualJoins2 = sqlGeneratorVisitor.Joins[studentDetailDetailTable2];
      Assert.That (actualJoins2, Is.EqualTo (new object[] { join4.GetSingleJoinForRoot (), join3.GetSingleJoinForRoot() }));
    }

    private JoinTree CreateJoin (MemberInfo relationMember, IFieldSourcePath rightSide, Table rightSideTable)
    {
      Table leftSide = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, relationMember);
      return new JoinTree (leftSide, rightSide, new Column (leftSide, columns.B), new Column (rightSideTable, columns.A));
    }
  }
}