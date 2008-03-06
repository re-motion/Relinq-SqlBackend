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
      const string expectedString = "(1=1)";
      CheckAppendCriterion_Value(value, expectedString);
    }

    [Test]
    public void AppendCriterion_FalseValue ()
    {
      ICriterion value = new Constant (false);
      const string expectedString = "(1<>1)";
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
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal),
          "(@1 = @2)");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.NotEqual),
          "(@1 <> @2)");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThan),
          "(@1 < @2)");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThan),
          "(@1 > @2)");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThanOrEqual),
          "(@1 <= @2)");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "(@1 >= @2)");
      CheckAppendCriterion_BinaryCondition_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Like), 
          "(@1 LIKE @2)");
    }

    [Test]
    public void AppendCriterion_BinaryConditions_WithColumns ()
    {
      Column c1 = new Column (new Table ("a", "b"), "foo");
      Column c2 = new Column (new Table ("c", "d"), "bar");
      CheckAppendCriterion (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.Equal),
          "(([b].[foo] IS NULL AND [d].[bar] IS NULL) OR [b].[foo] = [d].[bar])");
      CheckAppendCriterion (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.NotEqual),
          "(([b].[foo] IS NULL AND [d].[bar] IS NOT NULL) OR ([b].[foo] IS NOT NULL AND [d].[bar] IS NULL) OR [b].[foo] <> [d].[bar])");
      CheckAppendCriterion (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.LessThan),
          "([b].[foo] < [d].[bar])");
      CheckAppendCriterion (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.GreaterThan),
          "([b].[foo] > [d].[bar])");
      CheckAppendCriterion (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.LessThanOrEqual),
          "(([b].[foo] IS NULL AND [d].[bar] IS NULL) OR [b].[foo] <= [d].[bar])");
      CheckAppendCriterion (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "(([b].[foo] IS NULL AND [d].[bar] IS NULL) OR [b].[foo] >= [d].[bar])");
      CheckAppendCriterion (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.Like),
          "([b].[foo] LIKE [d].[bar])");
    }

    [Test]
    public void AppendCriterion_BinaryConditions_WithColumn_LeftSide ()
    {
      Column c1 = new Column (new Table ("a", "b"), "foo");
      CheckAppendCriterion (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.Equal),
          "([b].[foo] = @1)", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.NotEqual),
          "([b].[foo] IS NULL OR [b].[foo] <> @1)", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.LessThan),
          "([b].[foo] < @1)", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.GreaterThan),
          "([b].[foo] > @1)", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.LessThanOrEqual),
          "([b].[foo] <= @1)", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "([b].[foo] >= @1)", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.Like),
          "([b].[foo] LIKE @1)", new CommandParameter("@1", "const"));
    }

    [Test]
    public void AppendCriterion_BinaryConditions_WithColumn_RightSide ()
    {
      Column c1 = new Column (new Table ("a", "b"), "foo");
      CheckAppendCriterion (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.Equal),
          "(@1 = [b].[foo])", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.NotEqual),
          "([b].[foo] IS NULL OR @1 <> [b].[foo])", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.LessThan),
          "(@1 < [b].[foo])", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.GreaterThan),
          "(@1 > [b].[foo])", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.LessThanOrEqual),
          "(@1 <= [b].[foo])", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "(@1 >= [b].[foo])", new CommandParameter("@1", "const"));
      CheckAppendCriterion (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.Like),
          "(@1 LIKE [b].[foo])", new CommandParameter("@1", "const"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The binary condition kind 2147483647 is not supported.")]
    public void AppendCriterion_InvalidBinaryConditionKind ()
    {
      CheckAppendCriterion (new BinaryCondition (new Constant ("foo"), new Constant ("foo"), (BinaryCondition.ConditionKind)int.MaxValue), null);
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
      CheckAppendCriterion (notCriterion, "NOT @1", new CommandParameter ("@1", "foo"));
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

    private void CheckAppendCriterion_BinaryCondition_Constants (BinaryCondition binaryCondition, string expectedString)
    {
      CheckAppendCriterion (binaryCondition, expectedString,
          new CommandParameter ("@1", "foo"), new CommandParameter ("@2", "foo"));
    }

    private void CheckAppendCriterion_ComplexCriterion (ICriterion criterion, string expectedOperator)
    {
      CheckAppendCriterion (criterion, "((@1 = @2) " + expectedOperator + " (@3 = @4))",
          new CommandParameter ("@1", "foo"),
          new CommandParameter ("@2", "foo"),
          new CommandParameter ("@3", "foo"),
          new CommandParameter ("@4", "foo")
          );
    }

    

  }
}