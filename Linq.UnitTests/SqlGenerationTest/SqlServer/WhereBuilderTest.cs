using System;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;
using System.Collections.Generic;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class WhereBuilderTest
  {
    [Test]
    public void AppendCriterion_TrueValue()
    {
      ICriterion value = new Constant (true);
      const string expectedString = "1=1";
      CheckAppendCriterion_Value(value, expectedString);
    }

    [Test]
    public void AppendCriterion_FalseValue ()
    {
      ICriterion value = new Constant (false);
      const string expectedString = "1!=1";
      CheckAppendCriterion_Value (value, expectedString);
    }

    [Test]
    public void AppendCriterion_Column ()
    {
      ICriterion value = new Column (new Table ("foo", "foo_alias"), "col");
      const string expectedString = "[foo_alias].[col]=1";
      CheckAppendCriterion_Value (value, expectedString);
    }

    [Test]
    public void AppendCriterion_BinaryConditions()
    {
      CheckAppendCriterion_BinaryCondition_Constants(
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal), "=");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThan), "<");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThanOrEqual), "<=");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThan), ">");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThanOrEqual), ">=");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.NotEqual), "!=");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Like), "LIKE");
    }

    [Test]
    public void AppendCriterion_BinaryCondition_WithColumns ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (
          new Column (new Table("a", "b"), "foo"),
          new Column (new Table ("c", "d"), "bar"),
          BinaryCondition.ConditionKind.Equal);
      
      CheckAppendCriterion (binaryCondition, "[b].[foo] = [d].[bar]");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The binary condition kind 2147483647 is not supported.")]
    public void AppendCriterion_InvalidBinaryConditionKind ()
    {
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), (BinaryCondition.ConditionKind)int.MaxValue), "=");
    }

    [Test]
    public void AppendCriterion_BinaryConditionLeftNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant(null), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      CheckAppendCriterion (binaryCondition, "@1 IS NULL", new CommandParameter ("@1", "foo"));
    }

    [Test]
    public void AppendCriterion_BinaryConditionRightNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant ("foo"), new Constant (null), BinaryCondition.ConditionKind.Equal);
      CheckAppendCriterion (binaryCondition, "@1 IS NULL", new CommandParameter ("@1", "foo"));
    }

    [Test]
    public void AppendCriterion_BinaryConditionIsNotNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant (null), new Constant ("foo"), BinaryCondition.ConditionKind.NotEqual);
      CheckAppendCriterion (binaryCondition, "@1 IS NOT NULL", new CommandParameter ("@1", "foo"));
    }

    [Test]
    public void AppendCriterion_BinaryConditionNullIsNotNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant (null), new Constant (null), BinaryCondition.ConditionKind.NotEqual);
      CheckAppendCriterion (binaryCondition, "NULL IS NOT NULL");
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

    [Test]
    public void AppendCriterion_NotCriterion()
    {
      NotCriterion notCriterion = new NotCriterion(new Constant("foo"));
      CheckAppendCriterion (notCriterion, "NOT (@1)", new CommandParameter ("@1", "foo"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "NULL constants are not supported as WHERE conditions.")]
    public void AppendCriterion_NULL ()
    {
      CheckAppendCriterion (new Constant (null), null);
    }

    class PseudoCriterion : ICriterion { }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The criterion kind PseudoCriterion is not supported.")]
    public void InvalidCriterionKind_NotSupportedException()
    {
      CheckAppendCriterion (new PseudoCriterion(), null);
    }

    class PseudoCondition : ICondition { }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The condition kind PseudoCondition is not supported.")]
    public void InvalidConditionKind_NotSupportedException ()
    {
      CheckAppendCriterion (new PseudoCondition(), null);
    }

    private void CheckAppendCriterion (ICriterion criterion, string expectedString,
        params CommandParameter[] expectedParameters)
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);

      whereBuilder.BuildWherePart (criterion);

      Assert.AreEqual (" WHERE " + expectedString, commandText.ToString ());
      Assert.That (parameters, Is.EqualTo (expectedParameters));
    }

    private void CheckAppendCriterion_Value (ICriterion value, string expectedString)
    {
      CheckAppendCriterion (value, expectedString);
    }

    private void CheckAppendCriterion_BinaryCondition_Constants (BinaryCondition binaryCondition, string expectedOperator)
    {
      CheckAppendCriterion (binaryCondition, "@1 " + expectedOperator + " @2",
          new CommandParameter ("@1", "foo"), new CommandParameter ("@2", "foo"));
    }

    private void CheckAppendCriterion_ComplexCriterion (ICriterion criterion, string expectedOperator)
    {
      CheckAppendCriterion (criterion, "(@1 = @2) " + expectedOperator + " (@3 = @4)",
          new CommandParameter ("@1", "foo"),
          new CommandParameter ("@2", "foo"),
          new CommandParameter ("@3", "foo"),
          new CommandParameter ("@4", "foo")
          );
    }
  }
}