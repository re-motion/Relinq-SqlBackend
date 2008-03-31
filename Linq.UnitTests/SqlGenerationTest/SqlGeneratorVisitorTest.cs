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
using Rubicon.Data.Linq.UnitTests.TestQueryGenerators;

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
      IQueryable<Tuple<string, string>> query = SelectTestQueryGenerator.CreateSimpleQueryWithFieldProjection (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.SelectOrGroupClause;

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (sqlGeneratorVisitor.Columns, Is.EqualTo (new object[] { new Column (new Table ("studentTable", "s"), "FirstColumn"),
          new Column (new Table ("studentTable", "s"), "LastColumn") }));
    }

    [Test]
    public void VisitSelectClause_DistinctFalse ()
    {
      IQueryable<Tuple<string, string>> query = SelectTestQueryGenerator.CreateSimpleQueryWithFieldProjection (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.SelectOrGroupClause;

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitSelectClause (selectClause);

      Assert.IsFalse (selectClause.Distinct);
    }

    [Test]
    public void VisitSelectClause_DistinctTrue ()
    {
      IQueryable<string> query = DistinctTestQueryGenerator.CreateSimpleDistinctQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      SelectClause selectClause = (SelectClause) parsedQuery.SelectOrGroupClause;

      Assert.IsTrue (selectClause.Distinct);
    }

    [Test]
    public void VisitSelectClause_WithJoins ()
    {
      IQueryable<string> query = JoinTestQueryGenerator.CreateSimpleImplicitSelectJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      SelectClause selectClause = (SelectClause) parsedQuery.SelectOrGroupClause;
      sqlGeneratorVisitor.VisitSelectClause (selectClause);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IFromSource studentDetailTable = parsedQuery.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      Table studentTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember);
      Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);
      SingleJoin join = new SingleJoin (new Column (studentDetailTable, joinColumns.A), new Column (studentTable, joinColumns.B));
     
      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.Joins[studentDetailTable];
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
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      MainFromClause fromClause = parsedQuery.MainFromClause;

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitMainFromClause (fromClause);
      Assert.That (sqlGeneratorVisitor.FromSources, Is.EqualTo (new object[] { new Table ("studentTable", "s") }));
    }

    [Test]
    public void VisitAdditionalFromClause ()
    {
      IQueryable<Student> query = MixedTestQueryGenerator.CreateMultiFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      AdditionalFromClause fromClause = (AdditionalFromClause)parsedQuery.BodyClauses.First();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitAdditionalFromClause (fromClause);
      Assert.That (sqlGeneratorVisitor.FromSources, Is.EqualTo (new object[] { new Table ("studentTable", "s2") }));
    }

    [Test]
    public void VisitSubQueryFromClause ()
    {
      IQueryable<Student> query = SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      SubQueryFromClause subQueryFromClause = (SubQueryFromClause) parsedQuery.BodyClauses.First();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitSubQueryFromClause (subQueryFromClause);

      Assert.That (sqlGeneratorVisitor.FromSources, Is.EqualTo (new object[] { subQueryFromClause.GetFromSource (StubDatabaseInfo.Instance) }));
    }

    [Test]
    public void VisitWhereClause()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateSimpleWhereQuery (ExpressionHelper.CreateQuerySource());
      
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      WhereClause whereClause = (WhereClause)parsedQuery.BodyClauses.First();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitWhereClause (whereClause);

      Assert.AreEqual (new BinaryCondition (new Column (new Table ("studentTable", "s"), "LastColumn"),
          new Constant ("Garcia"), BinaryCondition.ConditionKind.Equal),
          sqlGeneratorVisitor.Criterion);
    }

    [Test]
    public void VisitWhereClause_MultipleTimes ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateMultiWhereQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      WhereClause whereClause1 = (WhereClause) parsedQuery.BodyClauses[0];
      WhereClause whereClause2 = (WhereClause) parsedQuery.BodyClauses[1];
      WhereClause whereClause3 = (WhereClause) parsedQuery.BodyClauses[2];

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitWhereClause (whereClause1);
      sqlGeneratorVisitor.VisitWhereClause (whereClause2);

      var condition1 = new BinaryCondition (new Column (new Table ("studentTable", "s"), "LastColumn"), new Constant ("Garcia"), 
          BinaryCondition.ConditionKind.Equal);
      var condition2 = new BinaryCondition (new Column (new Table ("studentTable", "s"), "FirstColumn"), new Constant ("Hugo"),
          BinaryCondition.ConditionKind.Equal);
      var combination12 = new ComplexCriterion (condition1, condition2, ComplexCriterion.JunctionKind.And);
      Assert.AreEqual (combination12, sqlGeneratorVisitor.Criterion);

      sqlGeneratorVisitor.VisitWhereClause (whereClause3);

      var condition3 = new BinaryCondition (new Column (new Table ("studentTable", "s"), "IDColumn"), new Constant (100),
          BinaryCondition.ConditionKind.GreaterThan);
      var combination123 = new ComplexCriterion (combination12, condition3, ComplexCriterion.JunctionKind.And);
      Assert.AreEqual (combination123, sqlGeneratorVisitor.Criterion);
    }

    
    [Test]
    public void VisitOrderingClause()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
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
      IQueryable<Student_Detail> query = JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      OrderingClause orderingClause = orderBy.OrderingList.First ();

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitOrderingClause (orderingClause);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IFromSource sourceTable = parsedQuery.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);

      SingleJoin join = new SingleJoin (new Column (sourceTable, columns.A), new Column (relatedTable, columns.B));

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.Joins[sourceTable];
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
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateThreeOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.BodyClauses.First ();

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

    [Test]
    public void VisitWhereClause_WithJoins ()
    {
      IQueryable<Student_Detail> query = JoinTestQueryGenerator.CreateSimpleImplicitWhereJoin (ExpressionHelper.CreateQuerySource_Detail ());
      
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      WhereClause whereClause = ClauseFinder.FindClause<WhereClause> (parsedQuery.SelectOrGroupClause);

      SqlGeneratorVisitor sqlGeneratorVisitor = new SqlGeneratorVisitor (parsedQuery, StubDatabaseInfo.Instance, _context);
      sqlGeneratorVisitor.VisitWhereClause (whereClause);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IFromSource sourceTable = parsedQuery.MainFromClause.GetFromSource (StubDatabaseInfo.Instance); // Student_Detail
      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);
      SingleJoin join = new SingleJoin (new Column (sourceTable, columns.A), new Column (relatedTable, columns.B));

      Assert.AreEqual (1, sqlGeneratorVisitor.Joins.Count);

      List<SingleJoin> actualJoins = sqlGeneratorVisitor.Joins[sourceTable];

      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void VisitWhereClause_UsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      VisitWhereClause_WithJoins ();
      Assert.AreEqual (1, _context.Count);
    }


    
  }
}