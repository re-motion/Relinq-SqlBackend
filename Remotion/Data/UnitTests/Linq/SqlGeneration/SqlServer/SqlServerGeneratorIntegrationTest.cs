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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration.SqlServer
{
  [TestFixture]
  public class SqlServerGeneratorIntegrationTest
  {
    private IQueryable<Student> _source;
    private IQueryable<Student_Detail> _detailSource;
    private IQueryable<IndustrialSector> _industrialSectorSource;
    private IQueryable<Student_Detail_Detail> _detailDetailSource;
    private SqlServerGenerator _sqlGenerator;

    [SetUp]
    public void SetUp ()
    {
      _source = ExpressionHelper.CreateQuerySource ();
      _detailSource = ExpressionHelper.CreateQuerySource_Detail ();
      _industrialSectorSource = ExpressionHelper.CreateQuerySource_IndustrialSector();
      _detailDetailSource = ExpressionHelper.CreateQuerySource_Detail_Detail ();
      _sqlGenerator = new SqlServerGenerator (StubDatabaseInfo.Instance);
    }

    [Test]
    public void DefaultParseContext ()
    {
      var generator = _sqlGenerator;
      Assert.AreEqual (ParseMode.TopLevelQuery, generator.ParseMode);
    }

    [Test]
    public void SimpleQuery_WithNullProjection ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQueryWithNonDBProjection (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT NULL FROM [studentTable] [s]"));
    }

    [Test]
    public void SimpleQuery ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s]", result.Statement);
      Assert.That (result.SqlGenerationData, Is.Not.Null);
      Assert.That (result.SqlGenerationData.SelectEvaluation, Is.InstanceOfType (typeof (Column)));

      Assert.IsEmpty (result.Parameters);
    }

    [Test]
    [ExpectedException (typeof (SqlGenerationException),
      ExpectedMessage = "The method Remotion.Collections.Tuple.NewTuple is not supported by this code generator, " + 
      "and no custom generator has been registered.")]
    public void MultiFromQueryWithProjection ()
    {
      IQueryable<Tuple<string, string, int>> query = MixedTestQueryGenerator.CreateMultiFromQueryWithProjection (_source, _source, _source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s1].[FirstColumn], [s2].[LastColumn], [s3].[IDColumn] FROM [studentTable] [s1], [studentTable] [s2], [studentTable] [s3]",
          result.Statement);

      Assert.IsEmpty (result.Parameters);
    }

    [Test]
    public void SimpleWhereQuery ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateSimpleWhereQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ([s].[LastColumn] = @1)", result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia") }));
    }

    [Test]
    public void MultiWhereQuery ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateMultiWhereQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ((([s].[LastColumn] = @1) AND ([s].[FirstColumn] = @2)) AND ([s].[IDColumn] > @3))",
          result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters,
          Is.EqualTo (new object[] {new CommandParameter ("@1", "Garcia"), new CommandParameter ("@2", "Hugo"), new CommandParameter ("@3", 100)}));
    }

    [Test]
    public void WhereQueryWithOrAndNot ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateWhereQueryWithOrAndNot (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] "
          + "WHERE ((NOT ([s].[FirstColumn] = @1) OR ([s].[FirstColumn] = @2)) AND ([s].[FirstColumn] = @3))",
          result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia"),
          new CommandParameter ("@2", "Garcia"), new CommandParameter ("@3", "Garcia") }));
    }

    [Test]
    public void WhereQueryWithComparisons ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateWhereQueryWithDifferentComparisons (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ("
          + "((((([s].[FirstColumn] IS NULL OR [s].[FirstColumn] <> @1) "
          + "AND ([s].[IDColumn] > @2)) "
          + "AND ([s].[IDColumn] >= @3)) "
          + "AND ([s].[IDColumn] < @4)) "
          + "AND ([s].[IDColumn] <= @5)) "
          + "AND ([s].[IDColumn] = @6)"
          + ")",
          result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia"),
          new CommandParameter ("@2", 5), new CommandParameter ("@3", 6), new CommandParameter ("@4", 7),
          new CommandParameter ("@5", 6), new CommandParameter ("@6", 6)}));
    }

    [Test]
    public void WhereQueryWithNullChecks ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateWhereQueryNullChecks (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ([s].[FirstColumn] IS NULL OR [s].[LastColumn] IS NOT NULL)",
          result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithBooleanConstantTrue ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateWhereQueryBooleanConstantTrue (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE (1=1)",
          result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithBooleanConstantFalse ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateWhereQueryBooleanConstantFalse (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE (1<>1)",
          result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithStartsWith ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateWhereQueryWithStartsWith (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ([s].[FirstColumn] LIKE @1)",
          result.Statement);
      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia%") }));
    }

    [Test]
    public void WhereQueryWithEndsWith ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateWhereQueryWithEndsWith (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ([s].[FirstColumn] LIKE @1)",
          result.Statement);
      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "%Garcia") }));
    }

    [Test]
    public void SimpleOrderByQuery ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateSimpleOrderByQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s1].* FROM [studentTable] [s1] ORDER BY [s1].[FirstColumn] ASC",
          result.Statement);
    }

    [Test]
    public void ComplexOrderByQuery ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateTwoOrderByQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s1].* FROM [studentTable] [s1] ORDER BY [s1].[LastColumn] DESC, [s1].[FirstColumn] ASC",
          result.Statement);
    }

    [Test]
    public void MultipleOrderBys ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateOrderByQueryWithMultipleOrderBys (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] ORDER BY [s].[LastColumn] ASC, "+
        "[s].[FirstColumn] ASC, [s].[LastColumn] DESC, [s].[ScoresColumn] ASC", result.Statement);
    }

    [Test]
    public void SimpleImplicitJoin ()
    {
      // from sd in source orderby sd.Student.First select sd
      IQueryable<Student_Detail> query = JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (_detailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [sd].* FROM [detailTable] [sd] LEFT OUTER JOIN [studentTable] [#j0] "
          + "ON [sd].[Student_Detail_PK] = [#j0].[Student_Detail_to_Student_FK] ORDER BY [#j0].[FirstColumn] ASC",
          result.Statement);
    }

    [Test]
    public void NestedImplicitJoin ()
    {
      // from sdd in source orderby sdd.Student_Detail.Student.First select sdd
      IQueryable<Student_Detail_Detail> query = JoinTestQueryGenerator.CreateDoubleImplicitOrderByJoin (_detailDetailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      const string expectedString = "SELECT [sdd].* FROM [detailDetailTable] [sdd] "
          + "LEFT OUTER JOIN [detailTable] [#j0] ON [sdd].[Student_Detail_Detail_PK] = [#j0].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "LEFT OUTER JOIN [studentTable] [#j1] ON [#j0].[Student_Detail_PK] = [#j1].[Student_Detail_to_Student_FK] "
          + "ORDER BY [#j1].[FirstColumn] ASC";
      Assert.AreEqual (expectedString, result.Statement);
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

      IQueryable<Student_Detail_Detail> source1 = _detailDetailSource;
      IQueryable<Student_Detail_Detail> source2 = _detailDetailSource;
      IQueryable<Student_Detail_Detail> query = JoinTestQueryGenerator.CreateImplicitOrderByJoinWithJoinReuse (source1, source2);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      const string expectedString = "SELECT [sdd1].* "
          + "FROM "
          + "[detailDetailTable] [sdd1] "
          + "LEFT OUTER JOIN [detailTable] [#j0] ON [sdd1].[Student_Detail_Detail_PK] = [#j0].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "LEFT OUTER JOIN [studentTable] [#j1] ON [#j0].[Student_Detail_PK] = [#j1].[Student_Detail_to_Student_FK], "
          + "[detailDetailTable] [sdd2] "
          + "LEFT OUTER JOIN [detailTable] [#j2] ON [sdd2].[Student_Detail_Detail_PK] = [#j2].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "LEFT OUTER JOIN [studentTable] [#j3] ON [#j2].[Student_Detail_PK] = [#j3].[Student_Detail_to_Student_FK] "
          + "ORDER BY [#j1].[FirstColumn] ASC, [#j3].[FirstColumn] ASC, [#j1].[FirstColumn] ASC";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void JoinPartReuse ()
    {
      //from sdd in ...
      //orderby sdd.Student_Detail.Student.First
      //orderby sdd.Student_Detail.ID
      //select sdd;

      IQueryable<Student_Detail_Detail> source1 = _detailDetailSource;
      IQueryable<Student_Detail_Detail> query = JoinTestQueryGenerator.CreateImplicitOrderByJoinWithJoinPartReuse (source1);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      const string expectedString = "SELECT [sdd].* "
          + "FROM "
          + "[detailDetailTable] [sdd] "
          + "LEFT OUTER JOIN [detailTable] [#j0] ON [sdd].[Student_Detail_Detail_PK] = [#j0].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "LEFT OUTER JOIN [studentTable] [#j1] ON [#j0].[Student_Detail_PK] = [#j1].[Student_Detail_to_Student_FK] "
          + "ORDER BY [#j0].[IDColumn] ASC, [#j1].[FirstColumn] ASC";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void SelectJoin()
    {
      // from sdd in source 
      // select new Tuple<string,int>{sdd.Student_Detail.Student.First,sdd.IndustrialSector.ID}

      IQueryable<Student_Detail_Detail> source = _detailDetailSource;

      IQueryable<Tuple<string, int>> query = JoinTestQueryGenerator.CreateComplexImplicitSelectJoin (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      const string expectedString = "SELECT [#j1].[FirstColumn], [#j2].[IDColumn] "
          + "FROM "
          + "[detailDetailTable] [sdd] "
          + "LEFT OUTER JOIN [detailTable] [#j0] ON [sdd].[Student_Detail_Detail_PK] = [#j0].[Student_Detail_Detail_to_Student_Detail_FK] "
          + "LEFT OUTER JOIN [studentTable] [#j1] ON [#j0].[Student_Detail_PK] = [#j1].[Student_Detail_to_Student_FK] "
          + "LEFT OUTER JOIN [industrialTable] [#j2] ON [sdd].[Student_Detail_Detail_PK] = [#j2].[Student_Detail_Detail_to_IndustrialSector_FK]";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }
    
    [Test]
    public void SelectJoin_WithRelationMember()
    {
      IQueryable<Student_Detail> source = _detailSource;

      IQueryable<Student> query = SelectTestQueryGenerator.CreateRelationMemberSelectQuery (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT [#j0].* FROM [detailTable] [sd] LEFT OUTER JOIN "
          + "[studentTable] [#j0] ON [sd].[Student_Detail_PK] = [#j0].[Student_Detail_to_Student_FK]";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void Select_WithDistinct ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource();
      IQueryable<string> query = DistinctTestQueryGenerator.CreateSimpleDistinctQuery (source);

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT DISTINCT [s].[FirstColumn] FROM [studentTable] [s]";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void Select_WithDistinctAndWhere ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<string> query = DistinctTestQueryGenerator.CreateDisinctWithWhereQuery (source);

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT DISTINCT [s].[FirstColumn] FROM [studentTable] [s] WHERE ([s].[FirstColumn] = @1)";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void WhereJoin_WithRelationMember ()
    {
      IQueryable<Student_Detail> source = _detailSource;

      IQueryable<Student_Detail> query = WhereTestQueryGenerator.CreateRelationMemberWhereQuery (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT [sd].* FROM [detailTable] [sd] WHERE [sd].[Student_Detail_to_IndustrialSector_FK] IS NOT NULL";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void WhereJoin_WithRelationMember_VirtualSide ()
    {
      IQueryable<IndustrialSector> source = ExpressionHelper.CreateQuerySource_IndustrialSector ();

      IQueryable<IndustrialSector> query = WhereTestQueryGenerator.CreateRelationMemberVirtualSideWhereQuery (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT [industrial].* FROM [industrialTable] [industrial] "
          + "LEFT OUTER JOIN [detailTable] [#j0] ON [industrial].[IndustrialSector_PK] = [#j0].[Student_Detail_to_IndustrialSector_FK] "
          + "WHERE [#j0].[IDColumn] IS NOT NULL";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Ordering by 'Remotion.Data.UnitTests.Linq.Student_Detail.Student' is not "
        + "supported because it is a relation member.")]
    public void OrderingJoin_WithRelationMember ()
    {
      IQueryable<Student_Detail> source = _detailSource;

      IQueryable<Student_Detail> query = OrderByTestQueryGenerator.CreateRelationMemberOrderByQuery (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

            _sqlGenerator.BuildCommand (parsedQuery);
    }

    [Test]
    public void SimpleSubQueryInAdditionalFromClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();

      IQueryable<Student> query = SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] CROSS APPLY (SELECT [s3].* FROM [studentTable] [s3]) [s2]", result.Statement);
    }

    [Test]
    public void ComplexSubQueryInAdditionalFromClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();

      IQueryable<Student> query = SubQueryTestQueryGenerator.CreateComplexSubQueryInAdditionalFromClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);


            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s2].* FROM [studentTable] [s] CROSS APPLY (SELECT [s3].* FROM [studentTable] [s3] " 
          + "WHERE ((([s3].[IDColumn] IS NULL AND [s].[IDColumn] IS NULL) OR [s3].[IDColumn] = [s].[IDColumn]) AND ([s3].[IDColumn] > @1))) [s2]",
          result.Statement);
      Assert.That (result.Parameters, Is.EqualTo (new[] {new CommandParameter ("@1", 3)}));
    }

    [Test]
    public void SimpleSubQueryInWhereClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();

      IQueryable<Student> query = SubQueryTestQueryGenerator.CreateSimpleSubQueryInWhereClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE [s].[IDColumn] IN ((SELECT [s2].[IDColumn] FROM [studentTable] [s2]))", result.Statement);
      Assert.That (result.Parameters, Is.Empty);
    }

    [Test]
    public void SubQueryWithConstantInWhereClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();

      IQueryable<Student> query = SubQueryTestQueryGenerator.CreateSubQueryWithConstantInWhereClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE @1 IN ((SELECT [s2].[IDColumn] FROM [studentTable] [s2]))", result.Statement);
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", 5) }));
    }

    [Test]
    public void SubQuerySelectingColumnsWithConstantInWhereClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();

      IQueryable<Student> query = SubQueryTestQueryGenerator.CreateSubQuerySelectingColumnsWithConstantInWhereClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE @1 IN ((SELECT [s2].[FirstColumn] FROM [studentTable] [s2]))", result.Statement);
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", "Hugo") }));
    }

    [Test]
    public void QueryWithLet_Binary ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<string > query = LetTestQueryGenerator.CreateSimpleLetClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT ([s].[FirstColumn] + [s].[LastColumn]) FROM [studentTable] [s]", result.Statement);    
    }

    [Test]
    public void QueryWithLetAndJoin_WithTable ()
    {
      IQueryable<Student_Detail> source = _detailSource;
      IQueryable<Student> query = LetTestQueryGenerator.CreateLet_WithJoin_WithTable (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [#j0].* FROM [detailTable] [sd] LEFT OUTER JOIN [studentTable] [#j0] ON [sd].[Student_Detail_PK] = " +
        "[#j0].[Student_Detail_to_Student_FK]", result.Statement);
    }

    [Test]
    public void QueryWithLetAndJoin_NoTable ()
    {
      IQueryable<Student_Detail> source = _detailSource;
      IQueryable<string> query = LetTestQueryGenerator.CreateLet_WithJoin_NoTable (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

            CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [#j0].[FirstColumn] FROM [detailTable] [sd] LEFT OUTER JOIN [studentTable] [#j0] ON [sd].[Student_Detail_PK] = " +
        "[#j0].[Student_Detail_to_Student_FK]", result.Statement);
    }

    [Test]
    public void QueryWithLet_WithTable ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = LetTestQueryGenerator.CreateLet_WithTable (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s]", result.Statement);
    }

    [Test]
    public void QueryWithMultiLet_Where ()
    {
      // from s in source let x = s.First let y = s.ID where y > 1 select x
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<string> query = LetTestQueryGenerator.CreateMultiLet_WithWhere (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      const string sql = "SELECT [s].[FirstColumn] FROM [studentTable] [s] WHERE ([s].[IDColumn] > @1)";
      Assert.AreEqual (sql, result.Statement);
    }

    [Test]
    public void QueryWithNewExpression ()
    {
      IQueryable<Tuple<string, string>> query = SelectTestQueryGenerator.CreateSimpleQueryWithFieldProjection (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s].[FirstColumn], [s].[LastColumn] FROM [studentTable] [s]"));
      Assert.That (result.SqlGenerationData.SelectEvaluation, Is.InstanceOfType (typeof (NewObject)));
    }

    [Test]
    public void QueryWithNewExpression_AndActivator ()
    {
      IQueryable<Tuple<string, string>> query = SelectTestQueryGenerator.CreateSimpleQueryWithFieldProjection (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s].[FirstColumn], [s].[LastColumn] FROM [studentTable] [s]"));

      object[] values = new[] { "Hugo", "Boss" };
      object selectedObject = result.SqlGenerationData.GetSelectedObjectActivator().CreateSelectedObject (values);
      Assert.That (selectedObject, Is.EqualTo (new Tuple<string, string> ("Hugo", "Boss")));
    }

    [Test]
    public void QueryWithMultiFromClauses_WithMethodCalls ()
    {
      IQueryable<Student> query = FromTestQueryGenerator.CreateMultiFromQuery_WithCalls (_source, _source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s1].* FROM [studentTable] [s1], [studentTable] [s2]"));
    }

    [Test]
    public void QueryWithWhereClause_OnRelatedPrimaryKey_VirtualColumn ()
    {
      IQueryable<Student_Detail> query = WhereTestQueryGenerator.CreateWhereQueryWithRelatedPrimaryKey_VirtualColumn (_detailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [sd].* FROM [detailTable] [sd] LEFT OUTER JOIN [studentTable] [#j0] ON [sd].[Student_Detail_PK] = [#j0].[Student_Detail_to_Student_FK] WHERE ([#j0].[IDColumn] = @1)"));
      Assert.That (result.Parameters, Is.EqualTo (new[] {new CommandParameter("@1", 5)}));
    }

    [Test]
    public void QueryWithWhereClause_OnRelatedPrimaryKey_RealColumn ()
    {
      IQueryable<Student_Detail> query = WhereTestQueryGenerator.CreateWhereQueryWithRelatedPrimaryKey_RealColumn (_detailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [sd].* FROM [detailTable] [sd] WHERE ([sd].[Student_Detail_to_IndustrialSector_FK] = @1)"));
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", 5) }));
    }

    [Test]
    public void QueryWithMemberQuerySource ()
    {
      IQueryable<Student> query = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySource (_industrialSectorSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s1].* FROM [industrialTable] [sector], [studentTable] [s1] WHERE (([sector].[IDColumn] IS NULL AND [s1].[Student_to_IndustrialSector_FK] IS NULL) OR [sector].[IDColumn] = [s1].[Student_to_IndustrialSector_FK])"));
      Assert.That (result.Parameters, Is.Empty);
    }

    [Test]
    public void QueryWithMemberQuerySourceAndJoin_OptimizedAway ()
    {
      IQueryable<Student> query = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySourceAndOptimizableJoin (_detailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s1].* FROM [detailTable] [sd], [studentTable] [s1] WHERE (([sd].[Student_Detail_to_IndustrialSector_FK] IS NULL AND [s1].[Student_to_IndustrialSector_FK] IS NULL) OR [sd].[Student_Detail_to_IndustrialSector_FK] = [s1].[Student_to_IndustrialSector_FK])"));
      Assert.That (result.Parameters, Is.Empty);
    }

    [Test]
    public void QueryWithMemberQuerySourceAndJoin ()
    {
      IQueryable<Student> query = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySourceAndJoin (_detailDetailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s1].* FROM [detailDetailTable] [sdd] LEFT OUTER JOIN [industrialTable] [#j0] ON [sdd].[Student_Detail_Detail_PK] = [#j0].[Student_Detail_Detail_to_IndustrialSector_FK], [studentTable] [s1] WHERE (([#j0].[IDColumn] IS NULL AND [s1].[Student_to_IndustrialSector_FK] IS NULL) OR [#j0].[IDColumn] = [s1].[Student_to_IndustrialSector_FK])"));
      Assert.That (result.Parameters, Is.Empty);
    }

    [Test]
    [Ignore ("TODO 594: Fix this")]
    public void QueryWithCount ()
    {
      var expression = SelectTestQueryGenerator.CreateCountQueryExpression (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (expression);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT COUNT(*) FROM (SELECT [s].* FROM [studentTable] [s]) x"));
      Assert.That (result.Parameters, Is.Empty);
    }
  }
}
