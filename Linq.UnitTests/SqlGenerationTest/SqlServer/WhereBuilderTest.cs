using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;
using System.Collections.Generic;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class WhereBuilderTest
  {
    private IDatabaseInfo _databaseInfo;
    private IQueryable<Student> _source;

    [SetUp]
    public void SetUp ()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
      _source = ExpressionHelper.CreateQuerySource ();
    }

    [Test]
    public void SimpleWhereQuery ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleWhereQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE [s].[LastColumn] = @1", result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia") }));
    }

    [Test]
    public void WhereQueryWithOrAndNot ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithOrAndNot (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE ((NOT ([s].[FirstColumn] = @1)) OR ([s].[FirstColumn] = @2)) AND ([s].[FirstColumn] = @3)",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia"),
          new CommandParameter ("@2", "Garcia"), new CommandParameter ("@3", "Garcia") }));
    }

    [Test]
    public void WhereQueryWithComparisons ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithDifferentComparisons (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE ((((([s].[FirstColumn] != @1) AND ([s].[IDColumn] > @2)) "
          + "AND ([s].[IDColumn] >= @3)) AND ([s].[IDColumn] < @4)) AND ([s].[IDColumn] <= @5)) AND ([s].[IDColumn] = @6)",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia"),
          new CommandParameter ("@2", 5), new CommandParameter ("@3", 6), new CommandParameter ("@4", 7),
          new CommandParameter ("@5", 6), new CommandParameter ("@6", 6)}));
    }

    [Test]
    public void WhereQueryWithNullChecks ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryNullChecks (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE ([s].[FirstColumn] IS NULL) OR ([s].[LastColumn] IS NOT NULL)",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithBooleanConstantTrue ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryBooleanConstantTrue (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE 1=1",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithBooleanConstantFalse ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryBooleanConstantFalse (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);

      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE 1!=1",
          result.A);

      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void WhereQueryWithStartsWith ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithStartsWith (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE [s].[FirstColumn] LIKE @1",
          result.A);
      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "Garcia%") }));
    }

    [Test]
    public void WhereQueryWithEndsWith ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateWhereQueryWithEndsWith (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (parsedQuery, _databaseInfo);
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString ();
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s] WHERE [s].[FirstColumn] LIKE @1",
          result.A);
      CommandParameter[] parameters = result.B;
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "%Garcia") }));
    }

    [Test]
    public void AppendCriterion_BinaryConditions()
    {
      CheckAppendCriterion_BinaryCondition(
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal), "=");
      CheckAppendCriterion_BinaryCondition (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThan), "<");
      CheckAppendCriterion_BinaryCondition (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThanOrEqual), "<=");
      CheckAppendCriterion_BinaryCondition (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThan), ">");
      CheckAppendCriterion_BinaryCondition (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThanOrEqual), ">=");
      CheckAppendCriterion_BinaryCondition (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.NotEqual), "!=");
      CheckAppendCriterion_BinaryCondition (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Like), "LIKE");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The binary condition kind 2147483647 is not supported.")]
    public void AppendCriterion_InvalidBinaryConditionKind ()
    {
      CheckAppendCriterion_BinaryCondition (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), (BinaryCondition.ConditionKind)int.MaxValue), "=");
    }

    private void CheckAppendCriterion_BinaryCondition (BinaryCondition binaryCondition, string expectedOperator)
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter>();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);

      whereBuilder.BuildWherePart (binaryCondition);

      Assert.AreEqual (" WHERE @1 " + expectedOperator + " @2", commandText.ToString());
      Assert.AreEqual (2, parameters.Count);
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "foo"), new CommandParameter ("@2", "foo") }));
    }

    [Test]
    public void AppendCriterion_BinaryConditionLeftNull ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      BinaryCondition binaryConditionEqual = new BinaryCondition (new Constant(null), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);

      whereBuilder.BuildWherePart (binaryConditionEqual);

      Assert.AreEqual (" WHERE @1 IS NULL", commandText.ToString ());
      Assert.AreEqual (1, parameters.Count);
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "foo") }));
    }

    [Test]
    public void AppendCriterion_BinaryConditionRightNull ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      BinaryCondition binaryConditionEqual = new BinaryCondition (new Constant ("foo"), new Constant (null), BinaryCondition.ConditionKind.Equal);

      whereBuilder.BuildWherePart (binaryConditionEqual);

      Assert.AreEqual (" WHERE @1 IS NULL", commandText.ToString ());
      Assert.AreEqual (1, parameters.Count);
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "foo") }));

    }

    [Test]
    public void AppendCriterion_ComplexCriterion ()
    {
      BinaryCondition binaryCondition1 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      BinaryCondition binaryCondition2 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      CheckAppendCriterion_ComplexCriterion
        (new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.And), "AND");

      CheckAppendCriterion_ComplexCriterion
        (new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.Or), "OR");
      
    }

    private void CheckAppendCriterion_ComplexCriterion (ComplexCriterion criterion, string expectedOperator)
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      whereBuilder.BuildWherePart (criterion);

      Assert.AreEqual (" WHERE (@1 = @2) "+ expectedOperator + " (@3 = @4)", commandText.ToString ());
      Assert.AreEqual (4, parameters.Count);
      Assert.That (parameters, Is.EqualTo (new object[]
                                             {
                                                  new CommandParameter ("@1", "foo"), 
                                                  new CommandParameter ("@2", "foo"), 
                                                  new CommandParameter ("@3", "foo"), 
                                                  new CommandParameter ("@4", "foo")
                                              }));
   }

    [Test]
    public void AppendCriterion_NotCriterion()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      NotCriterion notCriterion = new NotCriterion(new Constant("foo"));

      whereBuilder.BuildWherePart (notCriterion);

      Assert.AreEqual (" WHERE NOT (@1)", commandText.ToString ());
      Assert.AreEqual (1, parameters.Count);
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "foo") }));
    }

    [Test]
    public void AppendCriterion_NULL ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      Constant constant = new Constant(null);

      whereBuilder.BuildWherePart (constant);

      Assert.AreEqual (" WHERE NULL", commandText.ToString ());

    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The criterion kind Column is not supported.")]
    public void InvalidCriterionKind_NotSupportedException()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      Column colum = new Column (new Table ("a", "b"), "c");

      whereBuilder.BuildWherePart (colum);

      Assert.Fail();
    }

    class PseudoCondition : ICondition { }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The condition kind PseudoCondition is not supported.")]
    public void InvalidConditionKind_NotSupportedException ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      ICondition condition = new PseudoCondition();

      whereBuilder.BuildWherePart (condition);

      Assert.Fail ();
    }

    
  }
}