using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class SqlGeneratorVisitorIntegrationTest
  {
    private JoinedTableContext _context;
    private ParseMode _parseMode;

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext ();
      _parseMode = new ParseMode ();
    }

    [Test]
    public void VisitOrderingClause_WithNestedJoins ()
    {
      IQueryable<Student_Detail_Detail> query = JoinTestQueryGenerator.CreateDoubleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      DetailParser detailParser = new DetailParser (parsedQuery, StubDatabaseInfo.Instance, _context, _parseMode);
      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, ParseMode.TopLevelQuery,detailParser, new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause);

      PropertyInfo relationMember1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable = parsedQuery.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SingleJoin join1 = CreateJoin (studentDetailDetailTable, relationMember1);

      PropertyInfo relationMember2 = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable = join1.RightSide;
      SingleJoin join2 = CreateJoin (studentDetailTable, relationMember2);

      Assert.AreEqual (1, sqlGeneratorVisitor.SqlGenerationData.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[studentDetailDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join1, join2 }));
    }

    [Test]
    public void MultipleJoinsForSameTable ()
    {
      // 1)
      // order by sdd.Student_Detail.Student.First
      // order by sdd.IndustrialSector.ID
      // Joins[sdd] = { (sdd -> Student_Detail -> Student), (sdd -> IndustrialSector) }

      IQueryable<Student_Detail_Detail> query =
        JoinTestQueryGenerator.CreateImplicitOrderByJoinWithMultipleJoins (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();

      OrderingClause orderingClause1 = orderBy.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy.OrderingList.Last ();

      DetailParser detailParser = new DetailParser (parsedQuery, StubDatabaseInfo.Instance, _context,_parseMode);
      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, ParseMode.TopLevelQuery,detailParser, new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));

      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);

      PropertyInfo relationalMemberForFirstOrdering1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable = parsedQuery.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SingleJoin join1 = CreateJoin (studentDetailDetailTable, relationalMemberForFirstOrdering1);

      PropertyInfo relationalMemberForFirstOrdering2 = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable = join1.RightSide;
      SingleJoin join2 = CreateJoin (studentDetailTable, relationalMemberForFirstOrdering2);

      PropertyInfo relationalMemberForLastOrdering = typeof (Student_Detail_Detail).GetProperty ("IndustrialSector");
      SingleJoin join3 = CreateJoin (studentDetailDetailTable, relationalMemberForLastOrdering);

      Assert.AreEqual (1, sqlGeneratorVisitor.SqlGenerationData.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[studentDetailDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join1, join2, join3 }));

    }

    [Test]
    public void OneJoinWithMultipleExpression ()
    {
      // 2)
      // order by sdd.Student_Detail.Student.First
      // order by sdd.Student_Detail.Student.Last
      // Joins[sdd] = { (sdd -> Student_Detail -> Student) }

      IQueryable<Student_Detail_Detail> query =
        JoinTestQueryGenerator.CreateImplicitOrderByJoinCheckingCorrectNumberOfEntries (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();

      OrderingClause orderingClause1 = orderBy.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy.OrderingList.Last ();

      DetailParser detailParser = new DetailParser (parsedQuery, StubDatabaseInfo.Instance, _context,_parseMode);
      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, ParseMode.TopLevelQuery,detailParser, new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));

      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);

      PropertyInfo relationalMemberForFirstOrdering1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable = parsedQuery.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SingleJoin join1 = CreateJoin (studentDetailDetailTable, relationalMemberForFirstOrdering1);

      PropertyInfo relationalMemberForFirstOrdering2 = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable = join1.RightSide;
      SingleJoin join2 = CreateJoin (studentDetailTable, relationalMemberForFirstOrdering2);

      Assert.AreEqual (1, sqlGeneratorVisitor.SqlGenerationData.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[studentDetailDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join1, join2 }));

    }

    [Test]
    public void JoinWithDifferentLevels ()
    {
      // 3)
      // order by sdd.Student_Detail.Student.First
      // order by sdd.Student_Detail.IndustrialSector.ID
      // Joins[sdd] = { (sdd -> Student_Detail -> Student), (sdd -> Student_Detail -> IndustrialSector) }

      IQueryable<Student_Detail_Detail> query =
        JoinTestQueryGenerator.CreateImplicitOrderByJoinWithDifferentLevels (ExpressionHelper.CreateQuerySource_Detail_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();

      OrderingClause orderingClause1 = orderBy.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy.OrderingList.Last ();

      DetailParser detailParser = new DetailParser (parsedQuery, StubDatabaseInfo.Instance, _context,_parseMode);
      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, ParseMode.TopLevelQuery,detailParser, new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));

      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);

      PropertyInfo relationalMemberForFirstOrdering1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable = parsedQuery.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SingleJoin join1 = CreateJoin (studentDetailDetailTable, relationalMemberForFirstOrdering1);

      PropertyInfo relationalMemberForFirstOrdering2 = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable = join1.RightSide;
      SingleJoin join2 = CreateJoin (studentDetailTable, relationalMemberForFirstOrdering2);

      PropertyInfo relationalMemberForLastOrdering = typeof (Student_Detail).GetProperty ("IndustrialSector");
      SingleJoin join3 = CreateJoin (studentDetailTable, relationalMemberForLastOrdering);

      Assert.AreEqual (1, sqlGeneratorVisitor.SqlGenerationData.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[studentDetailDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join1, join2, join3 }));
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
        JoinTestQueryGenerator.CreateImplicitOrderByJoinWithMultipleKeys
        (ExpressionHelper.CreateQuerySource_Detail_Detail (), ExpressionHelper.CreateQuerySource_Detail_Detail ());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      OrderByClause orderBy1 = (OrderByClause) parsedQuery.BodyClauses.Skip (1).First ();
      OrderByClause orderBy2 = (OrderByClause) parsedQuery.BodyClauses.Last ();

      OrderingClause orderingClause1 = orderBy1.OrderingList.First ();
      OrderingClause orderingClause2 = orderBy2.OrderingList.First ();

      DetailParser detailParser = new DetailParser (parsedQuery, StubDatabaseInfo.Instance, _context,_parseMode);
      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, ParseMode.TopLevelQuery, detailParser, new ParseContext (parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _context));

      sqlGeneratorVisitor.VisitOrderingClause (orderingClause1);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause2);

      PropertyInfo relationalMemberFirstOrderBy1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable1 = parsedQuery.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SingleJoin join1 = CreateJoin (studentDetailDetailTable1, relationalMemberFirstOrderBy1);

      PropertyInfo relationalMemberFirstOrderBy2 = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable1 = join1.RightSide;
      SingleJoin join2 = CreateJoin (studentDetailTable1, relationalMemberFirstOrderBy2);

      PropertyInfo relationalMemberSecondOrderBy1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable2 = ((AdditionalFromClause) parsedQuery.BodyClauses[0]).GetFromSource (StubDatabaseInfo.Instance);
      SingleJoin join3 = CreateJoin (studentDetailDetailTable2, relationalMemberSecondOrderBy1);

      PropertyInfo relationalMemberSecondOrderBy2 = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable2 = join3.RightSide;
      SingleJoin join4 = CreateJoin (studentDetailTable2, relationalMemberSecondOrderBy2);

      Assert.AreEqual (2, sqlGeneratorVisitor.SqlGenerationData.Joins.Count);
      List<SingleJoin> actualJoins1 = sqlGeneratorVisitor.SqlGenerationData.Joins[studentDetailDetailTable1];
      Assert.That (actualJoins1, Is.EqualTo (new object[] { join1, join2 }));

      List<SingleJoin> actualJoins2 = sqlGeneratorVisitor.SqlGenerationData.Joins[studentDetailDetailTable2];
      Assert.That (actualJoins2, Is.EqualTo (new object[] { join3, join4 }));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The expression 'value(Remotion.Data.Linq.UnitTests.SqlGenerationTest." 
        + "SqlGeneratorVisitorIntegrationTest).GetNullSource()' could not be evaluated as a query source because it cannot be compiled: Query "
        + "sources cannot be null.")]
    public void InvalidQuerySource ()
    {
      var query = from s in ExpressionHelper.CreateQuerySource() from s2 in (from s3 in GetNullSource() select s3) select s;
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      QueryModel subQueryModel = ((SubQueryFromClause)parsedQuery.BodyClauses[0]).SubQueryModel;
      DetailParser detailParser = new DetailParser (subQueryModel, StubDatabaseInfo.Instance, _context,_parseMode);
      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (StubDatabaseInfo.Instance, ParseMode.TopLevelQuery, detailParser, new ParseContext (subQueryModel, subQueryModel.GetExpressionTree (), new List<FieldDescriptor> (), _context));
      sqlGeneratorVisitor.VisitQueryModel (subQueryModel);
    }

    private IQueryable<Student> GetNullSource ()
    {
      return null;
    }

    private SingleJoin CreateJoin (IColumnSource sourceTable, MemberInfo relationMember)
    {
      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);
      return new SingleJoin (new Column (sourceTable, columns.A), new Column (relatedTable, columns.B));
    }
  }
}