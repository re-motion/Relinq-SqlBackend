using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.SqlGeneration;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.UnitTests.TestQueryGenerators;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class SqlGeneratorBaseTest
  {
    private MockRepository _mockRepository;
    private ISelectBuilder _selectBuilder;
    private IFromBuilder _fromBuilder;
    private IWhereBuilder _whereBuilder;
    private IOrderByBuilder _orderByBuilder;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _selectBuilder = _mockRepository.CreateMock<ISelectBuilder> ();
      _fromBuilder = _mockRepository.CreateMock<IFromBuilder> ();
      _whereBuilder = _mockRepository.CreateMock<IWhereBuilder> ();
      _orderByBuilder = _mockRepository.CreateMock<IOrderByBuilder> ();
    }

    [Test]
    public void BuildCommandString_CallsPartBuilders()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      SqlGeneratorMock generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseContext.TopLevelQuery);
      
      // Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns,false);
      _fromBuilder.BuildFromPart (generator.Visitor.FromSources, generator.Visitor.Joins);
      _whereBuilder.BuildWherePart (generator.Visitor.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.OrderingFields);

      _mockRepository.ReplayAll();

      Tuple<string, CommandParameter[]> result = generator.BuildCommandString();
      Assert.IsNotNull (result);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void BuildCommandString_ReturnsCommandAndParameters ()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseContext.TopLevelQuery);

      // Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns,false);
      LastCall.Do ((Proc<List<Column>,bool>) delegate
      {
        generator.CommandText.Append ("Select");
      });

      _fromBuilder.BuildFromPart (generator.Visitor.FromSources, generator.Visitor.Joins);
      LastCall.Do ((Proc<List<IColumnSource>, JoinCollection>) delegate
      {
        generator.CommandText.Append ("From");
      });

      var parameter = new CommandParameter("fritz", "foo");
      _whereBuilder.BuildWherePart (generator.Visitor.Criterion);
      LastCall.Do ((Proc<ICriterion>) delegate { generator.CommandText.Append ("Where");
                                                 generator.CommandParameters.Add (parameter); });

      _orderByBuilder.BuildOrderByPart (generator.Visitor.OrderingFields);
      LastCall.Do ((Proc<List<OrderingField>>) delegate
      {
        generator.CommandText.Append ("OrderBy");
      });

      _mockRepository.ReplayAll ();

      Tuple<string, CommandParameter[]> result = generator.BuildCommandString ();
      Assert.AreEqual ("SelectFromWhereOrderBy", result.A);
      Assert.That (result.B, Is.EqualTo (new object[] {parameter}));

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void ProcessQuery_PassesQueryToVisitor()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseContext.TopLevelQuery);

      // Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns,false);
      _fromBuilder.BuildFromPart (generator.Visitor.FromSources, generator.Visitor.Joins);
      _whereBuilder.BuildWherePart (generator.Visitor.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommandString ();
    }

    [Test]
    public void ProcessQuery_WithDifferentParseContext ()
    {
      var query = ExpressionHelper.CreateQueryModel ();
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseContext.SubQueryInWhere);

      // Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns, false);
      _fromBuilder.BuildFromPart (generator.Visitor.FromSources, generator.Visitor.Joins);
      _whereBuilder.BuildWherePart (generator.Visitor.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommandString ();
    }

    [Test]
    public void ProcessQuery_CreatesAliases()
    {
      IQueryable<Student_Detail> source = ExpressionHelper.CreateQuerySource_Detail();
      QueryModel query = ExpressionHelper.ParseQuery (JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (source));
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseContext.TopLevelQuery);

      // Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns,false);
      _fromBuilder.BuildFromPart (generator.Visitor.FromSources, generator.Visitor.Joins);
      _whereBuilder.BuildWherePart (generator.Visitor.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommandString ();
    }

    [Test]
    public void CreateDistinct ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      QueryModel query = ExpressionHelper.ParseQuery (DistinctTestQueryGenerator.CreateSimpleDistinctQuery (source));
      var generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder, ParseContext.TopLevelQuery);

      //Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns, true);
      _fromBuilder.BuildFromPart (generator.Visitor.FromSources, generator.Visitor.Joins);
      _whereBuilder.BuildWherePart (generator.Visitor.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommandString ();

    }
  }
}