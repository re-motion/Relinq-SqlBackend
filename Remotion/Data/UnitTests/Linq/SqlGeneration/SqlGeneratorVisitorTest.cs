// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratorVisitorTest
  {
    #region Setup/Teardown

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext();
      _parseMode = new ParseMode();
      _detailParserRegistries = new DetailParserRegistries (StubDatabaseInfo.Instance, _parseMode);
    }

    #endregion

    private JoinedTableContext _context;
    private ParseMode _parseMode;
    private DetailParserRegistries _detailParserRegistries;

    [Test]
    public void VisitAdditionalFromClause ()
    {
      IQueryable<Student> query = MixedTestQueryGenerator.CreateMultiFromWhereQuery (
          ExpressionHelper.CreateQuerySource(), ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var fromClause = (AdditionalFromClause) parsedQuery.BodyClauses.First();

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitAdditionalFromClause (fromClause);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.FromSources, Is.EqualTo (new object[] { new Table ("studentTable", "s2") }));
    }


    [Test]
    public void VisitLetClause ()
    {
      IQueryable<string> query = LetTestQueryGenerator.CreateSimpleLetClause (ExpressionHelper.CreateQuerySource());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var letClause = (LetClause) parsedQuery.BodyClauses.First();

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitLetClause (letClause);

      var expectedResult =
          new BinaryEvaluation (
              new Column (new Table ("studentTable", "s"), "FirstColumn"),
              new Column (new Table ("studentTable", "s"), "LastColumn"),
              BinaryEvaluation.EvaluationKind.Add);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.LetEvaluations.First().Evaluation, Is.EqualTo (expectedResult));
      Assert.AreEqual (letClause.Identifier.Name, sqlGeneratorVisitor.SqlGenerationData.LetEvaluations.First().Name);
    }

    [Test]
    public void VisitLetClause_WithJoin ()
    {
      IQueryable<string> query = LetTestQueryGenerator.CreateLet_WithJoin_NoTable (ExpressionHelper.CreateQuerySource_Detail());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var letClause = (LetClause) parsedQuery.BodyClauses.First();
      IColumnSource studentDetailTable = parsedQuery.MainFromClause.GetColumnSource (StubDatabaseInfo.Instance);

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitLetClause (letClause);
      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      Table studentTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember);


      var c1 = new Column (studentDetailTable, "Student_Detail_PK");
      var c2 = new Column (studentTable, "Student_Detail_to_Student_FK");

      var expectedJoin = new SingleJoin (c1, c2);
      Assert.AreEqual (1, sqlGeneratorVisitor.SqlGenerationData.Joins.Count);

      SingleJoin actualJoin = sqlGeneratorVisitor.SqlGenerationData.Joins[studentDetailTable].First();
      Assert.AreEqual (expectedJoin, actualJoin);
    }

    [Test]
    public void VisitMainFromClause ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      MainFromClause fromClause = parsedQuery.MainFromClause;

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitMainFromClause (fromClause);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.FromSources, Is.EqualTo (new object[] { new Table ("studentTable", "s") }));
    }

    [Test]
    public void VisitMemberFromClause ()
    {
      IQueryable<Student> query = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySource (ExpressionHelper.CreateQuerySource_IndustrialSector());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var fromClause = (MemberFromClause) parsedQuery.BodyClauses.First();

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitMemberFromClause (fromClause);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.FromSources, Is.EqualTo (new object[] { new Table ("studentTable", "s1") }));

      var expectedLeftSideTable = new Table ("industrialTable", "sector");
      var expectedLeftSide = new Column (expectedLeftSideTable, "IDColumn");
      var expectedRightSide = new Column (sqlGeneratorVisitor.SqlGenerationData.FromSources[0], "Student_to_IndustrialSector_FK");
      var expectedCondition = new BinaryCondition (expectedLeftSide, expectedRightSide, BinaryCondition.ConditionKind.Equal);
      Assert.That (sqlGeneratorVisitor.SqlGenerationData.Criterion, Is.EqualTo (expectedCondition));
    }

    [Test]
    public void VisitOrderByClause ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateThreeOrderByQuery (ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy1 = (OrderByClause) parsedQuery.BodyClauses.First();

      FieldDescriptor fieldDescriptor1 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      FieldDescriptor fieldDescriptor2 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("Last"));

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitOrderByClause (orderBy1);
      Assert.That (
          sqlGeneratorVisitor.SqlGenerationData.OrderingFields,
          Is.EqualTo (
              new object[]
              {
                  new OrderingField (fieldDescriptor1, OrderingDirection.Asc),
                  new OrderingField (fieldDescriptor2, OrderingDirection.Asc),
              }));
    }

    [Test]
    public void VisitOrderingClause ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.First();
      Ordering ordering = orderBy.OrderingList.First();

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitOrdering (ordering);

      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      Assert.That (
          sqlGeneratorVisitor.SqlGenerationData.OrderingFields,
          Is.EqualTo (new object[] { new OrderingField (fieldDescriptor, OrderingDirection.Asc) }));
    }

    [Test]
    public void VisitOrderingClause_UsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      VisitOrderingClause_WithJoins();
      Assert.AreEqual (1, _context.Count);
    }

    [Test]
    public void VisitOrderingClause_WithJoins ()
    {
      IQueryable<Student_Detail> query = JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.First();
      Ordering ordering = orderBy.OrderingList.First();

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitOrdering (ordering);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource sourceTable = parsedQuery.MainFromClause.GetColumnSource (StubDatabaseInfo.Instance);
      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);

      var join = new SingleJoin (new Column (sourceTable, columns.A), new Column (relatedTable, columns.B));

      Assert.AreEqual (1, sqlGeneratorVisitor.SqlGenerationData.Joins.Count);
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[sourceTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void VisitSelectClause ()
    {
      IQueryable<Tuple<string, string>> query = SelectTestQueryGenerator.CreateSimpleQueryWithFieldProjection (ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var selectClause = (SelectClause) parsedQuery.SelectOrGroupClause;

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitSelectClause (selectClause);

      var expectedNewObject = new NewObject (
          typeof (Tuple<string, string>).GetConstructors()[0],
          new IEvaluation[]
          {
              new Column (new Table ("studentTable", "s"), "FirstColumn"),
              new Column (new Table ("studentTable", "s"), "LastColumn")
          });
      Assert.That (sqlGeneratorVisitor.SqlGenerationData.SelectEvaluation, Is.EqualTo (expectedNewObject));
    }

    [Test]
    public void VisitSelectClause_MethodCall ()
    {
      IClause clause = ExpressionHelper.CreateClause();
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var selectClause = new SelectClause (clause, Expression.Constant (0));

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitSelectClause (selectClause);

      Assert.AreEqual (sqlGeneratorVisitor.SqlGenerationData.SelectEvaluation, new Constant (0));
    }

    [Test]
    public void VisitSelectClause_ResultModifier ()
    {
      IQueryable<string> query = DistinctTestQueryGenerator.CreateSimpleDistinctQuery (ExpressionHelper.CreateQuerySource());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      var selectClause = (SelectClause) parsedQuery.SelectOrGroupClause;
      sqlGeneratorVisitor.VisitSelectClause (selectClause);

      var distinctModification = new DistinctResultModification (selectClause);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.ResultModifiers.First().SelectClause, Is.EqualTo (distinctModification.SelectClause));
    }

    [Test]
    public void VisitSelectClause_UsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      VisitSelectClause_WithJoins();
      Assert.AreEqual (1, _context.Count);
    }

    [Test]
    public void VisitSelectClause_WithJoins ()
    {
      IQueryable<string> query = JoinTestQueryGenerator.CreateSimpleImplicitSelectJoin (ExpressionHelper.CreateQuerySource_Detail());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      var selectClause = (SelectClause) parsedQuery.SelectOrGroupClause;
      sqlGeneratorVisitor.VisitSelectClause (selectClause);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable = parsedQuery.MainFromClause.GetColumnSource (StubDatabaseInfo.Instance);
      Table studentTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember);
      Tuple<string, string> joinSelectEvaluations = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);
      var join = new SingleJoin (new Column (studentDetailTable, joinSelectEvaluations.A), new Column (studentTable, joinSelectEvaluations.B));

      Assert.AreEqual (1, sqlGeneratorVisitor.SqlGenerationData.Joins.Count);

      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[studentDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void VisitSelectClause_WithNullProjection ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateSimpleWhereQuery (ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var selectClause = (SelectClause) parsedQuery.SelectOrGroupClause;

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitSelectClause (selectClause);
      Assert.That (sqlGeneratorVisitor.SqlGenerationData.SelectEvaluation, Is.EqualTo (new Column (new Table ("studentTable", "s"), "*")));
    }

    [Test]
    [Ignore ("TODO 1222: Should work again after integrating SubQueryFromClause")]
    public void VisitSubQueryFromClause ()
    {
      IQueryable<Student> query = SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var subQueryFromClause = (SubQueryFromClause) parsedQuery.BodyClauses.First();

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitSubQueryFromClause (subQueryFromClause);

      Assert.That (
          sqlGeneratorVisitor.SqlGenerationData.FromSources,
          Is.EqualTo (new object[] { subQueryFromClause.GetColumnSource (StubDatabaseInfo.Instance) }));
    }

    [Test]
    public void VisitWhereClause ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateSimpleWhereQuery (ExpressionHelper.CreateQuerySource());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var whereClause = (WhereClause) parsedQuery.BodyClauses.First();

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitWhereClause (whereClause);

      Assert.AreEqual (
          new BinaryCondition (
              new Column (new Table ("studentTable", "s"), "LastColumn"),
              new Constant ("Garcia"),
              BinaryCondition.ConditionKind.Equal),
          sqlGeneratorVisitor.SqlGenerationData.Criterion);
    }

    [Test]
    public void VisitWhereClause_MultipleTimes ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateMultiWhereQuery (ExpressionHelper.CreateQuerySource());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var whereClause1 = (WhereClause) parsedQuery.BodyClauses[0];
      var whereClause2 = (WhereClause) parsedQuery.BodyClauses[1];
      var whereClause3 = (WhereClause) parsedQuery.BodyClauses[2];

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitWhereClause (whereClause1);
      sqlGeneratorVisitor.VisitWhereClause (whereClause2);

      var condition1 = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "LastColumn"),
          new Constant ("Garcia"),
          BinaryCondition.ConditionKind.Equal);
      var condition2 = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "FirstColumn"),
          new Constant ("Hugo"),
          BinaryCondition.ConditionKind.Equal);
      var combination12 = new ComplexCriterion (condition1, condition2, ComplexCriterion.JunctionKind.And);
      Assert.AreEqual (combination12, sqlGeneratorVisitor.SqlGenerationData.Criterion);

      sqlGeneratorVisitor.VisitWhereClause (whereClause3);

      var condition3 = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "IDColumn"),
          new Constant (100),
          BinaryCondition.ConditionKind.GreaterThan);
      var combination123 = new ComplexCriterion (combination12, condition3, ComplexCriterion.JunctionKind.And);
      Assert.AreEqual (combination123, sqlGeneratorVisitor.SqlGenerationData.Criterion);
    }

    [Test]
    public void VisitWhereClause_UsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      VisitWhereClause_WithJoins();
      Assert.AreEqual (1, _context.Count);
    }

    [Test]
    public void VisitWhereClause_WithJoins ()
    {
      IQueryable<Student_Detail> query = JoinTestQueryGenerator.CreateSimpleImplicitWhereJoin (ExpressionHelper.CreateQuerySource_Detail());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var whereClause = ClauseFinder.FindClause<WhereClause> (parsedQuery.SelectOrGroupClause);

      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _context));
      sqlGeneratorVisitor.VisitWhereClause (whereClause);

      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource sourceTable = parsedQuery.MainFromClause.GetColumnSource (StubDatabaseInfo.Instance); // Student_Detail
      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);
      var join = new SingleJoin (new Column (sourceTable, columns.A), new Column (relatedTable, columns.B));

      Assert.AreEqual (1, sqlGeneratorVisitor.SqlGenerationData.Joins.Count);

      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[sourceTable];

      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }
  }
}