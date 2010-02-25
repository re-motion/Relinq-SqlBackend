// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.DetailParsing;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;
using Remotion.Data.Linq.UnitTests.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratorVisitorTest
  {
    private JoinedTableContext _context;
    private ParseMode _parseMode;
    private DetailParserRegistries _detailParserRegistries;
    private QueryModel _queryModel;

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext (StubDatabaseInfo.Instance);
      _parseMode = new ParseMode();
      _detailParserRegistries = new DetailParserRegistries (StubDatabaseInfo.Instance, _parseMode);
      _queryModel = ExpressionHelper.CreateQueryModel_Student ();
    }

    [Test]
    public void VisitAdditionalFromClause ()
    {
      IQueryable<Cook> query = MixedTestQueryGenerator.CreateMultiFromWhereQuery (
          ExpressionHelper.CreateStudentQueryable(), ExpressionHelper.CreateStudentQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var fromClause = (AdditionalFromClause) parsedQuery.BodyClauses[0];

      SqlGeneratorVisitor sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitAdditionalFromClause (fromClause, parsedQuery, 0);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.FromSources, Is.EqualTo (new object[] { new Table ("studentTable", "s2") }));
    }

    [Test]
    public void VisitAdditionalFromClause_WithMemberExpression ()
    {
      IQueryable<Cook> query = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySource (ExpressionHelper.CreateIndustrialSectorQueryable ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var fromClause = (AdditionalFromClause) parsedQuery.BodyClauses[0];

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitAdditionalFromClause (fromClause, parsedQuery, 0);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.FromSources, Is.EqualTo (new object[] { new Table ("studentTable", "s1") }));

      var expectedLeftSideTable = new Table ("industrialTable", "sector");
      var expectedLeftSide = new Column (expectedLeftSideTable, "IDColumn");
      var expectedRightSide = new Column (sqlGeneratorVisitor.SqlGenerationData.FromSources[0], "Student_to_IndustrialSector_FK");
      var expectedCondition = new BinaryCondition (expectedLeftSide, expectedRightSide, BinaryCondition.ConditionKind.Equal);
      Assert.That (sqlGeneratorVisitor.SqlGenerationData.Criterion, Is.EqualTo (expectedCondition));
    }

    [Test]
    public void VisitMainFromClause ()
    {
      IQueryable<Cook> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateStudentQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      MainFromClause fromClause = parsedQuery.MainFromClause;

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitMainFromClause (fromClause, _queryModel);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.FromSources, Is.EqualTo (new object[] { new Table ("studentTable", "s") }));
    }

    [Test]
    public void VisitMainFromClause_WithMemberExpression ()
    {
      IQueryable<Cook> query = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySource_InMainFromClauseOfSubQuery (ExpressionHelper.CreateIndustrialSectorQueryable ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var fromClause = ((SubQueryExpression) ((AdditionalFromClause) parsedQuery.BodyClauses[0]).FromExpression).QueryModel.MainFromClause;

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitMainFromClause (fromClause, parsedQuery);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.FromSources, Is.EqualTo (new object[] { new Table ("studentTable", "s2") }));

      var expectedLeftSideTable = new Table ("industrialTable", "sector");
      var expectedLeftSide = new Column (expectedLeftSideTable, "IDColumn");
      var expectedRightSide = new Column (sqlGeneratorVisitor.SqlGenerationData.FromSources[0], "Student_to_IndustrialSector_FK");
      var expectedCondition = new BinaryCondition (expectedLeftSide, expectedRightSide, BinaryCondition.ConditionKind.Equal);
      Assert.That (sqlGeneratorVisitor.SqlGenerationData.Criterion, Is.EqualTo (expectedCondition));
    }

    [Test]
    public void VisitOrderByClause ()
    {
      IQueryable<Cook> query = OrderByTestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateStudentQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      FieldDescriptor expectedFieldDescriptor = 
          ExpressionHelper.CreateFieldDescriptor (_context, parsedQuery.MainFromClause, typeof (Cook).GetProperty ("FirstName"));

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);

      var orderByClause = (OrderByClause) parsedQuery.BodyClauses[0];
      sqlGeneratorVisitor.VisitOrderByClause (orderByClause, parsedQuery, 0);

      Assert.That (
          sqlGeneratorVisitor.SqlGenerationData.OrderingFields,
          Is.EqualTo ( new [] { new OrderingField (expectedFieldDescriptor, OrderingDirection.Asc), }));
    }

    [Test]
    public void VisitOrderByClause_UsesContext ()
    {
      Assert.That (_context.Count, Is.EqualTo (0));
      VisitOrderByClause_WithJoins();
      Assert.That (_context.Count, Is.EqualTo (1));
    }

    [Test]
    public void VisitOrderByClause_WithJoins ()
    {
      IQueryable<Kitchen> query = JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateStudentDetailQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);

      var orderBy = (OrderByClause) parsedQuery.BodyClauses[0];
      sqlGeneratorVisitor.VisitOrderByClause (orderBy, parsedQuery, 0);

      PropertyInfo relationMember = typeof (Kitchen).GetProperty ("Cook");
      IColumnSource sourceTable = _context.GetColumnSource (parsedQuery.MainFromClause);
      Table relatedTable = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetTableForRelation (relationMember, null); // Cook
      var join = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetJoinForMember (relationMember, sourceTable, relatedTable);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.Joins.Count, Is.EqualTo (1));
      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[sourceTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void VisitOrderByClause_Multiple ()
    {
      IQueryable<Cook> query = OrderByTestQueryGenerator.CreateThreeOrderByQuery (ExpressionHelper.CreateStudentQueryable ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderByClause1 = (OrderByClause) parsedQuery.BodyClauses[0];
      var orderByClause2 = (OrderByClause) parsedQuery.BodyClauses[1];

      FieldDescriptor firstFieldDescriptor = ExpressionHelper.CreateFieldDescriptor (_context, parsedQuery.MainFromClause, typeof (Cook).GetProperty ("FirstName"));
      FieldDescriptor lastFieldDescriptor = ExpressionHelper.CreateFieldDescriptor (_context, parsedQuery.MainFromClause, typeof (Cook).GetProperty ("Name"));

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);

      sqlGeneratorVisitor.VisitOrderByClause (orderByClause1, parsedQuery, 0);
      sqlGeneratorVisitor.VisitOrderByClause (orderByClause2, parsedQuery, 1);
      
      Assert.That (
          sqlGeneratorVisitor.SqlGenerationData.OrderingFields,
          Is.EqualTo (
              new object[]
              {
                  new OrderingField (lastFieldDescriptor, OrderingDirection.Desc),
                  new OrderingField (firstFieldDescriptor, OrderingDirection.Asc),
                  new OrderingField (lastFieldDescriptor, OrderingDirection.Asc),
              }));
    }

    [Test]
    public void VisitSelectClause ()
    {
      IQueryable<Tuple<string, string>> query = SelectTestQueryGenerator.CreateSimpleQueryWithFieldProjection (ExpressionHelper.CreateStudentQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var selectClause = parsedQuery.SelectClause;

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitSelectClause (selectClause, _queryModel);

      var expectedNewObject = new NewObject (
          typeof (Tuple<string, string>).GetConstructors()[0],
          new IEvaluation[]
          {
              new Column (new Table ("studentTable", "s"), "FirstNameColumn"),
              new Column (new Table ("studentTable", "s"), "NameColumn")
          });
      Assert.That (sqlGeneratorVisitor.SqlGenerationData.SelectEvaluation, Is.EqualTo (expectedNewObject));
    }

    [Test]
    public void VisitSelectClause_MethodCall ()
    {
      IQueryable<Cook> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateStudentQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var selectClause = new SelectClause (Expression.Constant (0));

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitSelectClause (selectClause, _queryModel);

      Assert.That (new Constant (0), Is.EqualTo (sqlGeneratorVisitor.SqlGenerationData.SelectEvaluation));
    }

    [Test]
    public void VisitQueryModel_ResultOperator ()
    {
      IQueryable<string> query = DistinctTestQueryGenerator.CreateSimpleDistinctQuery (ExpressionHelper.CreateStudentQueryable());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitQueryModel (parsedQuery);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.ResultOperators[0], Is.SameAs (parsedQuery.ResultOperators[0]));
    }

    [Test]
    public void VisitSelectClause_UsesContext ()
    {
      Assert.That (_context.Count, Is.EqualTo (0));
      VisitSelectClause_WithJoins();
      Assert.That (_context.Count, Is.EqualTo (1));
    }

    [Test]
    public void VisitSelectClause_WithJoins ()
    {
      IQueryable<string> query = JoinTestQueryGenerator.CreateSimpleImplicitSelectJoin (ExpressionHelper.CreateStudentDetailQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      var selectClause = parsedQuery.SelectClause;
      sqlGeneratorVisitor.VisitSelectClause (selectClause, _queryModel);

      PropertyInfo relationMember = typeof (Kitchen).GetProperty ("Cook");
      IColumnSource studentDetailTable = _context.GetColumnSource (parsedQuery.MainFromClause);
      Table studentTable = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetTableForRelation (relationMember, null);
      var join = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetJoinForMember (relationMember, studentDetailTable, studentTable);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.Joins.Count, Is.EqualTo (1));

      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[studentDetailTable];
      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void VisitSelectClause_WithNullProjection ()
    {
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateSimpleWhereQuery (ExpressionHelper.CreateStudentQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var selectClause = parsedQuery.SelectClause;

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitSelectClause (selectClause, _queryModel);
      Assert.That (sqlGeneratorVisitor.SqlGenerationData.SelectEvaluation, Is.EqualTo (new Column (new Table ("studentTable", "s"), "*")));
    }

    [Test]
    public void VisitAdditionalFromClause_WithSubQuery ()
    {
      IQueryable<Cook> query = SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (ExpressionHelper.CreateStudentQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var fromClause = (AdditionalFromClause) parsedQuery.BodyClauses[0];

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitAdditionalFromClause (fromClause, parsedQuery, 0);

      Assert.That (
          sqlGeneratorVisitor.SqlGenerationData.FromSources,
          Is.EqualTo (new object[] { _context.GetColumnSource (fromClause) }));
    }

    [Test]
    public void VisitWhereClause ()
    {
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateSimpleWhereQuery (ExpressionHelper.CreateStudentQueryable());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var whereClause = (WhereClause) parsedQuery.BodyClauses[0];

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitWhereClause (whereClause, parsedQuery, 0);

      Assert.That (
                  sqlGeneratorVisitor.SqlGenerationData.Criterion, Is.EqualTo (
                            new BinaryCondition (
                                            new Column (new Table ("studentTable", "s"), "NameColumn"),
                                                          new Constant ("Garcia"),
                                                                        BinaryCondition.ConditionKind.Equal)));
    }

    [Test]
    public void VisitWhereClause_MultipleTimes ()
    {
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateMultiWhereQuery (ExpressionHelper.CreateStudentQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var whereClause1 = (WhereClause) parsedQuery.BodyClauses[0];
      var whereClause2 = (WhereClause) parsedQuery.BodyClauses[1];
      var whereClause3 = (WhereClause) parsedQuery.BodyClauses[2];

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitWhereClause (whereClause1, parsedQuery, 0);
      sqlGeneratorVisitor.VisitWhereClause (whereClause2, parsedQuery, 1);

      var condition1 = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "NameColumn"),
          new Constant ("Garcia"),
          BinaryCondition.ConditionKind.Equal);
      var condition2 = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "FirstNameColumn"),
          new Constant ("Hugo"),
          BinaryCondition.ConditionKind.Equal);
      var combination12 = new ComplexCriterion (condition1, condition2, ComplexCriterion.JunctionKind.And);
      Assert.That (sqlGeneratorVisitor.SqlGenerationData.Criterion, Is.EqualTo (combination12));

      sqlGeneratorVisitor.VisitWhereClause (whereClause3, parsedQuery, 2);

      var condition3 = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "IDColumn"),
          new Constant (100),
          BinaryCondition.ConditionKind.GreaterThan);
      var combination123 = new ComplexCriterion (combination12, condition3, ComplexCriterion.JunctionKind.And);
      Assert.That (sqlGeneratorVisitor.SqlGenerationData.Criterion, Is.EqualTo (combination123));
    }

    [Test]
    public void VisitWhereClause_UsesContext ()
    {
      Assert.That (_context.Count, Is.EqualTo (0));
      VisitWhereClause_WithJoins();
      Assert.That (_context.Count, Is.EqualTo (1));
    }

    [Test]
    public void VisitWhereClause_WithJoins ()
    {
      IQueryable<Kitchen> query = JoinTestQueryGenerator.CreateSimpleImplicitWhereJoin (ExpressionHelper.CreateStudentDetailQueryable());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var whereClause = (WhereClause) parsedQuery.BodyClauses[0];

      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (parsedQuery);
      sqlGeneratorVisitor.VisitWhereClause (whereClause, parsedQuery, 0);

      PropertyInfo relationMember = typeof (Kitchen).GetProperty ("Cook");
      IColumnSource sourceTable = _context.GetColumnSource (parsedQuery.MainFromClause); // Kitchen
      Table relatedTable = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetTableForRelation (relationMember, null); // Cook
      var join = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetJoinForMember (relationMember, sourceTable, relatedTable);

      Assert.That (sqlGeneratorVisitor.SqlGenerationData.Joins.Count, Is.EqualTo (1));

      List<SingleJoin> actualJoins = sqlGeneratorVisitor.SqlGenerationData.Joins[sourceTable];

      Assert.That (actualJoins, Is.EqualTo (new object[] { join }));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitJoinClause ()
    {
      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (ExpressionHelper.CreateQueryModel_Student());
      sqlGeneratorVisitor.VisitJoinClause (ExpressionHelper.CreateJoinClause (), _queryModel, 1);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitGroupJoinClause ()
    {
      var sqlGeneratorVisitor = CreateSqlGeneratorVisitor (ExpressionHelper.CreateQueryModel_Student ());
      sqlGeneratorVisitor.VisitGroupJoinClause (ExpressionHelper.CreateGroupJoinClause (), _queryModel, 1);
    }

    private SqlGeneratorVisitor CreateSqlGeneratorVisitor (QueryModel parsedQuery)
    {
      return new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          _detailParserRegistries,
          new ParseContext (parsedQuery, new List<FieldDescriptor> (), _context));
    }
  }
}
