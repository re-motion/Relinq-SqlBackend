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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;
using Remotion.Data.Linq.UnitTests.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class SqlServerGeneratorIntegrationTest
  {
    private IQueryable<Cook> _source;
    private IQueryable<Kitchen> _detailSource;
    private IQueryable<Restaurant> _industrialSectorSource;
    private IQueryable<Company> _detailDetailSource;
    private SqlServerGenerator _sqlGenerator;

    [SetUp]
    public void SetUp ()
    {
      _source = ExpressionHelper.CreateStudentQueryable ();
      _detailSource = ExpressionHelper.CreateStudentDetailQueryable ();
      _industrialSectorSource = ExpressionHelper.CreateIndustrialSectorQueryable();
      _detailDetailSource = ExpressionHelper.CreateStudentDetailDetailQueryable ();
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
      IQueryable<Cook> query = SelectTestQueryGenerator.CreateSimpleQueryWithNonDBProjection (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT NULL FROM [studentTable] [s]"));
    }

    [Test]
    public void SimpleQuery ()
    {
      IQueryable<Cook> query = SelectTestQueryGenerator.CreateSimpleQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s]", result.Statement);
      Assert.That (result.SqlGenerationData, Is.Not.Null);
      Assert.That (result.SqlGenerationData.SelectEvaluation, Is.InstanceOfType (typeof (Column)));

      Assert.IsEmpty (result.Parameters);
    }

    [Test]
    [ExpectedException (typeof (SqlGenerationException),
        ExpectedMessage = @"The method .*\.Tuple\.Create is not supported by this code generator, and no custom generator has been registered\.",
        MatchType = MessageMatch.Regex)]
    public void MultiFromQueryWithProjection ()
    {
      IQueryable<Tuple<string, string, int>> query = MixedTestQueryGenerator.CreateMultiFromQueryWithProjection (_source, _source, _source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s1].[FirstNameColumn], [s2].[NameColumn], [s3].[IDColumn] FROM [studentTable] [s1], [studentTable] [s2], [studentTable] [s3]",
                       result.Statement);

      Assert.IsEmpty (result.Parameters);
    }

    [Test]
    public void SimpleWhereQuery ()
    {
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateSimpleWhereQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ([s].[NameColumn] = @1)", result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia") }));
    }

    [Test]
    public void MultiWhereQuery ()
    {
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateMultiWhereQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ((([s].[NameColumn] = @1) AND ([s].[FirstNameColumn] = @2)) AND ([s].[IDColumn] > @3))",
                       result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters,
                   Is.EqualTo (new object[] {new CommandParameter ("@1", "Garcia"), new CommandParameter ("@2", "Hugo"), new CommandParameter ("@3", 100)}));
    }

    [Test]
    public void WhereQueryWithOrAndNot ()
    {
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateWhereQueryWithOrAndNot (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] "
                       + "WHERE ((NOT ([s].[FirstNameColumn] = @1) OR ([s].[FirstNameColumn] = @2)) AND ([s].[FirstNameColumn] = @3))",
                       result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia"),
          new CommandParameter ("@2", "Garcia"), new CommandParameter ("@3", "Garcia") }));
    }

    [Test]
    public void WhereQueryWithComparisons ()
    {
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateWhereQueryWithDifferentComparisons (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ("
                       + "((((([s].[FirstNameColumn] IS NULL OR [s].[FirstNameColumn] <> @1) "
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
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateWhereQueryNullChecks (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ([s].[FirstNameColumn] IS NULL OR [s].[NameColumn] IS NOT NULL)",
                       result.Statement);

      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithBooleanConstantTrue ()
    {
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateWhereQueryBooleanConstantTrue (_source);
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
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateWhereQueryBooleanConstantFalse (_source);
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
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateWhereQueryWithStartsWith (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ([s].[FirstNameColumn] LIKE @1)",
                       result.Statement);
      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia%") }));
    }

    [Test]
    public void WhereQueryWithEndsWith ()
    {
      IQueryable<Cook> query = WhereTestQueryGenerator.CreateWhereQueryWithEndsWith (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE ([s].[FirstNameColumn] LIKE @1)",
                       result.Statement);
      CommandParameter[] parameters = result.Parameters;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "%Garcia") }));
    }

    [Test]
    public void SimpleOrderByQuery ()
    {
      IQueryable<Cook> query = OrderByTestQueryGenerator.CreateSimpleOrderByQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s1].* FROM [studentTable] [s1] ORDER BY [s1].[FirstNameColumn] ASC",
                       result.Statement);
    }

    [Test]
    public void ComplexOrderByQuery ()
    {
      IQueryable<Cook> query = OrderByTestQueryGenerator.CreateTwoOrderByQuery (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s1].* FROM [studentTable] [s1] ORDER BY [s1].[NameColumn] DESC, [s1].[FirstNameColumn] ASC",
                       result.Statement);
    }

    [Test]
    public void MultipleOrderBys ()
    {
      IQueryable<Cook> query = OrderByTestQueryGenerator.CreateOrderByQueryWithMultipleOrderBys (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] ORDER BY [s].[NameColumn] ASC, "+
                       "[s].[FirstNameColumn] ASC, [s].[NameColumn] DESC, [s].[HolidaysColumn] ASC", result.Statement);
    }

    [Test]
    public void SimpleImplicitJoin ()
    {
      // from sd in source orderby sd.Cook.FirstName select sd
      IQueryable<Kitchen> query = JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (_detailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT [sd].* FROM [detailTable] [sd] LEFT OUTER JOIN [studentTable] [#j0] "
                       + "ON [sd].[Kitchen_PK] = [#j0].[Kitchen_to_Cook_FK] ORDER BY [#j0].[FirstNameColumn] ASC",
                       result.Statement);
    }

    [Test]
    public void NestedImplicitJoin ()
    {
      // from sdd in source orderby sdd.MainKitchen.Cook.FirstName select sdd
      IQueryable<Company> query = JoinTestQueryGenerator.CreateDoubleImplicitOrderByJoin (_detailDetailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      const string expectedString = "SELECT [sdd].* FROM [detailDetailTable] [sdd] "
                                    + "LEFT OUTER JOIN [detailTable] [#j0] ON [sdd].[Company_PK] = [#j0].[Company_to_Kitchen_FK] "
                                    + "LEFT OUTER JOIN [studentTable] [#j1] ON [#j0].[Kitchen_PK] = [#j1].[Kitchen_to_Cook_FK] "
                                    + "ORDER BY [#j1].[FirstNameColumn] ASC";
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void JoinReuse()
    {
      // from sdd1 in ...
      // from sdd2 in ...
      // order by sdd1.MainKitchen.Cook.FirstName
      // order by sdd2.MainKitchen.Cook.FirstName
      // order by sdd1.MainKitchen.Cook.FirstName
      // select sdd1;

      IQueryable<Company> source1 = _detailDetailSource;
      IQueryable<Company> source2 = _detailDetailSource;
      IQueryable<Company> query = JoinTestQueryGenerator.CreateImplicitOrderByJoinWithJoinReuse (source1, source2);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      const string expectedString = "SELECT [sdd1].* "
                                    + "FROM "
                                    + "[detailDetailTable] [sdd1] "
                                    + "LEFT OUTER JOIN [detailTable] [#j0] ON [sdd1].[Company_PK] = [#j0].[Company_to_Kitchen_FK] "
                                    + "LEFT OUTER JOIN [studentTable] [#j1] ON [#j0].[Kitchen_PK] = [#j1].[Kitchen_to_Cook_FK], "
                                    + "[detailDetailTable] [sdd2] "
                                    + "LEFT OUTER JOIN [detailTable] [#j2] ON [sdd2].[Company_PK] = [#j2].[Company_to_Kitchen_FK] "
                                    + "LEFT OUTER JOIN [studentTable] [#j3] ON [#j2].[Kitchen_PK] = [#j3].[Kitchen_to_Cook_FK] "
                                    + "ORDER BY [#j1].[FirstNameColumn] ASC, [#j3].[FirstNameColumn] ASC, [#j1].[FirstNameColumn] ASC";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void JoinPartReuse ()
    {
      //from sdd in ...
      //orderby sdd.MainKitchen.Cook.FirstName
      //orderby sdd.MainKitchen.ID
      //select sdd;

      IQueryable<Company> source1 = _detailDetailSource;
      IQueryable<Company> query = JoinTestQueryGenerator.CreateImplicitOrderByJoinWithJoinPartReuse (source1);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      const string expectedString = "SELECT [sdd].* "
                                    + "FROM "
                                    + "[detailDetailTable] [sdd] "
                                    + "LEFT OUTER JOIN [detailTable] [#j0] ON [sdd].[Company_PK] = [#j0].[Company_to_Kitchen_FK] "
                                    + "LEFT OUTER JOIN [studentTable] [#j1] ON [#j0].[Kitchen_PK] = [#j1].[Kitchen_to_Cook_FK] "
                                    + "ORDER BY [#j0].[IDColumn] ASC, [#j1].[FirstNameColumn] ASC";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void SelectJoin()
    {
      // from sdd in source 
      // select new Tuple<string,int>{sdd.MainKitchen.Cook.FirstName,sdd.Restaurant.ID}

      IQueryable<Company> source = _detailDetailSource;

      IQueryable<Tuple<string, int>> query = JoinTestQueryGenerator.CreateComplexImplicitSelectJoin (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      
      const string expectedString = "SELECT [#j1].[FirstNameColumn], [#j2].[IDColumn] "
                                    + "FROM "
                                    + "[detailDetailTable] [sdd] "
                                    + "LEFT OUTER JOIN [detailTable] [#j0] ON [sdd].[Company_PK] = [#j0].[Company_to_Kitchen_FK] "
                                    + "LEFT OUTER JOIN [studentTable] [#j1] ON [#j0].[Kitchen_PK] = [#j1].[Kitchen_to_Cook_FK] "
                                    + "LEFT OUTER JOIN [industrialTable] [#j2] ON [sdd].[Company_PK] = [#j2].[Company_to_Restaurant_FK]";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }
    
    [Test]
    public void SelectJoin_WithRelationMember()
    {
      IQueryable<Kitchen> source = _detailSource;

      IQueryable<Cook> query = SelectTestQueryGenerator.CreateRelationMemberSelectQuery (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT [#j0].* FROM [detailTable] [sd] LEFT OUTER JOIN "
                                    + "[studentTable] [#j0] ON [sd].[Kitchen_PK] = [#j0].[Kitchen_to_Cook_FK]";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void Select_WithDistinct ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable();
      IQueryable<string> query = DistinctTestQueryGenerator.CreateSimpleDistinctQuery (source);

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT DISTINCT [s].[FirstNameColumn] FROM [studentTable] [s]";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void Select_WithDistinctAndWhere ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();
      IQueryable<string> query = DistinctTestQueryGenerator.CreateDisinctWithWhereQuery (source);

      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT DISTINCT [s].[FirstNameColumn] FROM [studentTable] [s] WHERE ([s].[FirstNameColumn] = @1)";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void WhereJoin_WithRelationMember ()
    {
      IQueryable<Kitchen> source = _detailSource;

      IQueryable<Kitchen> query = WhereTestQueryGenerator.CreateRelationMemberWhereQuery (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT [sd].* FROM [detailTable] [sd] WHERE [sd].[Student_Detail_to_IndustrialSector_FK] IS NOT NULL";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    public void WhereJoin_WithRelationMember_VirtualSide ()
    {
      IQueryable<Restaurant> source = ExpressionHelper.CreateIndustrialSectorQueryable ();

      IQueryable<Restaurant> query = WhereTestQueryGenerator.CreateRelationMemberVirtualSideWhereQuery (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      
      const string expectedString = "SELECT [industrial].* FROM [industrialTable] [industrial] "
                                    + "LEFT OUTER JOIN [detailTable] [#j0] ON [industrial].[Restaurant_PK] = [#j0].[Student_Detail_to_IndustrialSector_FK] "
                                    + "WHERE [#j0].[IDColumn] IS NOT NULL";

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual (expectedString, result.Statement);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Ordering by 'Remotion.Data.Linq.UnitTests.TestDomain.Kitchen.Cook' is not "
                                                                          + "supported because it is a relation member.")]
    public void OrderingJoin_WithRelationMember ()
    {
      IQueryable<Kitchen> source = _detailSource;

      IQueryable<Kitchen> query = OrderByTestQueryGenerator.CreateRelationMemberOrderByQuery (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      _sqlGenerator.BuildCommand (parsedQuery);
    }

    [Test]
    public void SimpleSubQueryInMainFromClause ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();

      IQueryable<Cook> query = SubQueryTestQueryGenerator.CreateSimpleSubQueryInMainFromClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM (SELECT TOP 1 [s2].* FROM [studentTable] [s2]) [s]", result.Statement);
    }

    [Test]
    public void SimpleSubQueryInAdditionalFromClause ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();

      IQueryable<Cook> query = SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] CROSS APPLY (SELECT [s3].* FROM [studentTable] [s3]) [s2]", result.Statement);
    }

    [Test]
    public void ComplexSubQueryInAdditionalFromClause ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();

      IQueryable<Cook> query = SubQueryTestQueryGenerator.CreateComplexSubQueryInAdditionalFromClause (source);
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
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();

      IQueryable<Cook> query = SubQueryTestQueryGenerator.CreateSimpleSubQueryInWhereClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE [s].[IDColumn] IN (SELECT [s2].[IDColumn] FROM [studentTable] [s2])", result.Statement);
      Assert.That (result.Parameters, Is.Empty);
    }

    [Test]
    public void SubQueryWithConstantInWhereClause ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();

      IQueryable<Cook> query = SubQueryTestQueryGenerator.CreateSubQueryWithConstantInWhereClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE @1 IN (SELECT [s2].[IDColumn] FROM [studentTable] [s2])", result.Statement);
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", 5) }));
    }

    [Test]
    public void SubQuerySelectingColumnsWithConstantInWhereClause ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();

      IQueryable<Cook> query = SubQueryTestQueryGenerator.CreateSubQuerySelectingColumnsWithConstantInWhereClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE @1 IN (SELECT [s2].[FirstNameColumn] FROM [studentTable] [s2])", result.Statement);
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", "Hugo") }));
    }

    [Test]
    public void QueryWithLet_Binary ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();
      IQueryable<string > query = LetTestQueryGenerator.CreateSimpleLetClause (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.AreEqual ("SELECT ([s].[FirstNameColumn] + [s].[NameColumn]) FROM [studentTable] [s]", result.Statement);    
    }

    [Test]
    public void QueryWithLetAndJoin_WithTable ()
    {
      IQueryable<Kitchen> source = _detailSource;
      IQueryable<Cook> query = LetTestQueryGenerator.CreateLet_WithJoin_WithTable (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [#j0].* FROM [detailTable] [sd] LEFT OUTER JOIN [studentTable] [#j0] ON [sd].[Kitchen_PK] = " +
                       "[#j0].[Kitchen_to_Cook_FK]", result.Statement);
    }

    [Test]
    public void QueryWithLetAndJoin_NoTable ()
    {
      IQueryable<Kitchen> source = _detailSource;
      IQueryable<string> query = LetTestQueryGenerator.CreateLet_WithJoin_NoTable (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [#j0].[FirstNameColumn] FROM [detailTable] [sd] LEFT OUTER JOIN [studentTable] [#j0] ON [sd].[Kitchen_PK] = " +
                       "[#j0].[Kitchen_to_Cook_FK]", result.Statement);
    }

    [Test]
    public void QueryWithLet_WithTable ()
    {
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();
      IQueryable<Cook> query = LetTestQueryGenerator.CreateLet_WithTable (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);

      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s]", result.Statement);
    }

    [Test]
    public void QueryWithMultiLet_Where ()
    {
      // from s in source let x = s.FirstName let y = s.ID where y > 1 select x
      IQueryable<Cook> source = ExpressionHelper.CreateStudentQueryable ();
      IQueryable<string> query = LetTestQueryGenerator.CreateMultiLet_WithWhere (source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);

      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      const string sql = "SELECT [s].[FirstNameColumn] FROM [studentTable] [s] WHERE ([s].[IDColumn] > @1)";
      Assert.AreEqual (sql, result.Statement);
    }

    [Test]
    public void QueryWithNewExpression ()
    {
      IQueryable<Tuple<string, string>> query = SelectTestQueryGenerator.CreateSimpleQueryWithFieldProjection (_source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s].[FirstNameColumn], [s].[NameColumn] FROM [studentTable] [s]"));
      Assert.That (result.SqlGenerationData.SelectEvaluation, Is.InstanceOfType (typeof (NewObject)));
    }

    [Test]
    public void QueryWithMultiFromClauses_WithMethodCalls ()
    {
      IQueryable<Cook> query = FromTestQueryGenerator.CreateMultiFromQuery_WithCalls (_source, _source);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s1].* FROM [studentTable] [s1], [studentTable] [s2]"));
    }

    [Test]
    public void QueryWithWhereClause_OnRelatedPrimaryKey_VirtualColumn ()
    {
      IQueryable<Kitchen> query = WhereTestQueryGenerator.CreateWhereQueryWithRelatedPrimaryKey_VirtualColumn (_detailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [sd].* FROM [detailTable] [sd] LEFT OUTER JOIN [studentTable] [#j0] ON [sd].[Kitchen_PK] = [#j0].[Kitchen_to_Cook_FK] WHERE ([#j0].[IDColumn] = @1)"));
      Assert.That (result.Parameters, Is.EqualTo (new[] {new CommandParameter("@1", 5)}));
    }

    [Test]
    public void QueryWithWhereClause_OnRelatedPrimaryKey_RealColumn ()
    {
      IQueryable<Kitchen> query = WhereTestQueryGenerator.CreateWhereQueryWithRelatedPrimaryKey_RealColumn (_detailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [sd].* FROM [detailTable] [sd] WHERE ([sd].[Student_Detail_to_IndustrialSector_FK] = @1)"));
      Assert.That (result.Parameters, Is.EqualTo (new[] { new CommandParameter ("@1", 5) }));
    }

    [Test]
    public void QueryWithMemberQuerySource ()
    {
      IQueryable<Cook> query = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySource (_industrialSectorSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s1].* FROM [industrialTable] [sector], [studentTable] [s1] WHERE (([sector].[IDColumn] IS NULL AND [s1].[Student_to_IndustrialSector_FK] IS NULL) OR [sector].[IDColumn] = [s1].[Student_to_IndustrialSector_FK])"));
      Assert.That (result.Parameters, Is.Empty);
    }

    [Test]
    public void QueryWithMemberQuerySourceAndJoin_OptimizedAway ()
    {
      IQueryable<Cook> query = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySourceAndOptimizableJoin (_detailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s1].* FROM [detailTable] [sd], [studentTable] [s1] WHERE (([sd].[Student_Detail_to_IndustrialSector_FK] IS NULL AND [s1].[Student_to_IndustrialSector_FK] IS NULL) OR [sd].[Student_Detail_to_IndustrialSector_FK] = [s1].[Student_to_IndustrialSector_FK])"));
      Assert.That (result.Parameters, Is.Empty);
    }

    [Test]
    public void QueryWithMemberQuerySourceAndJoin ()
    {
      IQueryable<Cook> query = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySourceAndJoin (_detailDetailSource);
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      CommandData result = _sqlGenerator.BuildCommand (parsedQuery);
      Assert.That (result.Statement, Is.EqualTo ("SELECT [s1].* FROM [detailDetailTable] [sdd] LEFT OUTER JOIN [industrialTable] [#j0] ON [sdd].[Company_PK] = [#j0].[Company_to_Restaurant_FK], [studentTable] [s1] WHERE (([#j0].[IDColumn] IS NULL AND [s1].[Student_to_IndustrialSector_FK] IS NULL) OR [#j0].[IDColumn] = [s1].[Student_to_IndustrialSector_FK])"));
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
