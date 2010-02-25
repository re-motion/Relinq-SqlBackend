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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.DetailParsing;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratorVisitorIntegrationTest
  {
    private JoinedTableContext _context;
    private ParseMode _parseMode;

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext(StubDatabaseInfo.Instance);
      _parseMode = new ParseMode();
    }

    [Test]
    public void VisitOrderByClause_WithNestedJoins ()
    {
      IQueryable<Student_Detail_Detail> query =
          JoinTestQueryGenerator.CreateDoubleImplicitOrderByJoin (ExpressionHelper.CreateStudentDetailDetailQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var detailParserRegistries = new DetailParserRegistries (StubDatabaseInfo.Instance, _parseMode);
      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          detailParserRegistries,
          new ParseContext (parsedQuery, new List<FieldDescriptor>(), _context));

      var orderBy = (OrderByClause) parsedQuery.BodyClauses[0];
      sqlGeneratorVisitor.VisitOrderByClause (orderBy, parsedQuery, 0);

      PropertyInfo relationMember1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable = _context.GetColumnSource (parsedQuery.MainFromClause);
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
      // order by sdd.Student_Detail.Student.FirstName
      // order by sdd.IndustrialSector.ID
      // Joins[sdd] = { (sdd -> Student_Detail -> Student), (sdd -> IndustrialSector) }

      IQueryable<Student_Detail_Detail> query =
          JoinTestQueryGenerator.CreateImplicitOrderByJoinWithMultipleJoins (ExpressionHelper.CreateStudentDetailDetailQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var detailParserRegistries = new DetailParserRegistries (StubDatabaseInfo.Instance, _parseMode);
      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          detailParserRegistries,
          new ParseContext (parsedQuery, new List<FieldDescriptor>(), _context));

      var orderBy = (OrderByClause) parsedQuery.BodyClauses[0];
      sqlGeneratorVisitor.VisitOrderByClause (orderBy, parsedQuery, 0);

      PropertyInfo relationalMemberForFirstOrdering1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable = _context.GetColumnSource (parsedQuery.MainFromClause);
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
      // order by sdd.Student_Detail.Student.FirstName
      // order by sdd.Student_Detail.Student.Name
      // Joins[sdd] = { (sdd -> Student_Detail -> Student) }

      IQueryable<Student_Detail_Detail> query =
          JoinTestQueryGenerator.CreateImplicitOrderByJoinCheckingCorrectNumberOfEntries (ExpressionHelper.CreateStudentDetailDetailQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var detailParserRegistries = new DetailParserRegistries (StubDatabaseInfo.Instance, _parseMode);
      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          detailParserRegistries,
          new ParseContext (parsedQuery, new List<FieldDescriptor>(), _context));

      var orderBy = (OrderByClause) parsedQuery.BodyClauses[0];
      sqlGeneratorVisitor.VisitOrderByClause (orderBy, parsedQuery, 0);

      PropertyInfo relationalMemberForFirstOrdering1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable = _context.GetColumnSource (parsedQuery.MainFromClause);
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
      // order by sdd.Student_Detail.Student.FirstName
      // order by sdd.Student_Detail.IndustrialSector.ID
      // Joins[sdd] = { (sdd -> Student_Detail -> Student), (sdd -> Student_Detail -> IndustrialSector) }

      IQueryable<Student_Detail_Detail> query =
          JoinTestQueryGenerator.CreateImplicitOrderByJoinWithDifferentLevels (ExpressionHelper.CreateStudentDetailDetailQueryable());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var orderBy = (OrderByClause) parsedQuery.BodyClauses.First();

      var detailParserRegistries = new DetailParserRegistries (StubDatabaseInfo.Instance, _parseMode);
      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          detailParserRegistries,
          new ParseContext (parsedQuery, new List<FieldDescriptor>(), _context));

      sqlGeneratorVisitor.VisitOrderByClause (orderBy, parsedQuery, 0);

      PropertyInfo relationalMemberForFirstOrdering1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable = _context.GetColumnSource (parsedQuery.MainFromClause);
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
      // order by sdd1.Student_Detail.Student.FirstName
      // order by sdd2.Student_Detail.Student.FirstName
      // Joins[sdd1] = { (sdd1 -> Student_Detail -> Student) }
      // Joins[sdd2] = { (sdd2 -> Student_Detail -> Student) }

      IQueryable<Student_Detail_Detail> query =
          JoinTestQueryGenerator.CreateImplicitOrderByJoinWithMultipleKeys
              (ExpressionHelper.CreateStudentDetailDetailQueryable(), ExpressionHelper.CreateStudentDetailDetailQueryable());

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      var detailParserRegistries = new DetailParserRegistries (StubDatabaseInfo.Instance, _parseMode);
      var sqlGeneratorVisitor = new SqlGeneratorVisitor (
          StubDatabaseInfo.Instance,
          ParseMode.TopLevelQuery,
          detailParserRegistries,
          new ParseContext (parsedQuery, new List<FieldDescriptor>(), _context));

      var orderBy1 = (OrderByClause) parsedQuery.BodyClauses[1];
      var orderBy2 = (OrderByClause) parsedQuery.BodyClauses[2];

      sqlGeneratorVisitor.VisitOrderByClause (orderBy1, parsedQuery, 0);
      sqlGeneratorVisitor.VisitOrderByClause (orderBy2, parsedQuery, 1);

      PropertyInfo relationalMemberFirstOrderBy1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable1 = _context.GetColumnSource (parsedQuery.MainFromClause);
      SingleJoin join1 = CreateJoin (studentDetailDetailTable1, relationalMemberFirstOrderBy1);

      PropertyInfo relationalMemberFirstOrderBy2 = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource studentDetailTable1 = join1.RightSide;
      SingleJoin join2 = CreateJoin (studentDetailTable1, relationalMemberFirstOrderBy2);

      PropertyInfo relationalMemberSecondOrderBy1 = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      IColumnSource studentDetailDetailTable2 = _context.GetColumnSource ((AdditionalFromClause) parsedQuery.BodyClauses[0]);
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

    private SingleJoin CreateJoin (IColumnSource sourceTable, MemberInfo relationMember)
    {
      Table relatedTable = StubDatabaseInfo.Instance.GetTableForRelation (relationMember, null); // Student
      return StubDatabaseInfo.Instance.GetJoinForMember (relationMember, sourceTable, relatedTable);
    }
  }
}
