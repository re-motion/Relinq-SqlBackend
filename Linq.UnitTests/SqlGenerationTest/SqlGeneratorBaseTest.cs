using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration;
using NUnit.Framework.SyntaxHelpers;

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
      QueryExpression query = new QueryExpression (ExpressionHelper.CreateMainFromClause (), ExpressionHelper.CreateQueryBody ());
      SqlGeneratorMock generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder);
      
      // Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns);
      _fromBuilder.BuildFromPart (generator.Visitor.Tables, generator.Visitor.Joins);
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
      QueryExpression query = new QueryExpression (ExpressionHelper.CreateMainFromClause (), ExpressionHelper.CreateQueryBody ());
      SqlGeneratorMock generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder);

      // Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns);
      LastCall.Do ((Proc<List<Column>>) delegate
      {
        generator.CommandText.Append ("Select");
      });

      _fromBuilder.BuildFromPart (generator.Visitor.Tables, generator.Visitor.Joins);
      LastCall.Do ((Proc<List<Table>, IDictionary<Table, List<Join>>>) delegate
      {
        generator.CommandText.Append ("From");
      });

      CommandParameter parameter = new CommandParameter("fritz", "foo");
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
      QueryExpression query = new QueryExpression (ExpressionHelper.CreateMainFromClause (), ExpressionHelper.CreateQueryBody ());
      SqlGeneratorMock generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder);

      // Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns);
      _fromBuilder.BuildFromPart (generator.Visitor.Tables, generator.Visitor.Joins);
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
      QueryExpression query = ExpressionHelper.ParseQuery (TestQueryGenerator.CreateSimpleImplicitOrderByJoin (source));
      SqlGeneratorMock generator = new SqlGeneratorMock (query, StubDatabaseInfo.Instance, _selectBuilder, _fromBuilder, _whereBuilder, _orderByBuilder);

      // Expect
      _selectBuilder.BuildSelectPart (generator.Visitor.Columns);
      _fromBuilder.BuildFromPart (generator.Visitor.Tables, generator.Visitor.Joins);
      _whereBuilder.BuildWherePart (generator.Visitor.Criterion);
      _orderByBuilder.BuildOrderByPart (generator.Visitor.OrderingFields);

      _mockRepository.ReplayAll ();

      generator.CheckBaseProcessQueryMethod = true;
      generator.BuildCommandString ();
    }
  }
}