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
    public void AppendCriterion_BinaryCondition_WithValue ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);

      ICriterion binaryCondition = new BinaryCondition (
          new Column (new Table("a", "b"), "foo"),
          new Column (new Table ("c", "d"), "bar"),
          BinaryCondition.ConditionKind.Equal);
      
      whereBuilder.BuildWherePart (binaryCondition);

      Assert.AreEqual (" WHERE [b].[foo] = [d].[bar]", commandText.ToString ());
      Assert.AreEqual (0, parameters.Count);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The binary condition kind 2147483647 is not supported.")]
    public void AppendCriterion_InvalidBinaryConditionKind ()
    {
      CheckAppendCriterion_BinaryCondition (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), (BinaryCondition.ConditionKind)int.MaxValue), "=");
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
    public void AppendCriterion_BinaryConditionIsNotNull ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      BinaryCondition binaryConditionEqual = new BinaryCondition (new Constant (null), new Constant ("foo"), BinaryCondition.ConditionKind.NotEqual);

      whereBuilder.BuildWherePart (binaryConditionEqual);

      Assert.AreEqual (" WHERE @1 IS NOT NULL", commandText.ToString ());
      Assert.AreEqual (1, parameters.Count);
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "foo") }));
    }

    [Test]
    public void AppendCriterion_BinaryConditionNullIsNotNull ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      BinaryCondition binaryConditionEqual = new BinaryCondition (new Constant (null), new Constant (null), BinaryCondition.ConditionKind.NotEqual);

      whereBuilder.BuildWherePart (binaryConditionEqual);

      Assert.AreEqual (" WHERE NULL IS NOT NULL", commandText.ToString ());
      Assert.AreEqual (0, parameters.Count);
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

    private void CheckAppendCriterion_ComplexCriterion (ICriterion criterion, string expectedOperator)
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
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "NULL constants are not supported as WHERE conditions.")]
    public void AppendCriterion_NULL ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      Constant constant = new Constant(null);

      whereBuilder.BuildWherePart (constant);
    }

    class PseudoCriterion : ICriterion { }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The criterion kind PseudoCriterion is not supported.")]
    public void InvalidCriterionKind_NotSupportedException()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      PseudoCriterion criterion = new PseudoCriterion ();

      whereBuilder.BuildWherePart (criterion);
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

    [Test]
    public void VirtualSide_EqualNull ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      ICondition condition = GetVirtualSideCondition(BinaryCondition.ConditionKind.Equal, new Constant (null));

      whereBuilder.BuildWherePart (condition);

      Assert.AreEqual
          (" WHERE NOT EXISTS (SELECT 1 FROM [detailTable] [xyz] WHERE [xyz].[Student_Detail_to_IndustrialSector_FK] = [is].[IndustrialSector_PK])",
              commandText.ToString());
      Assert.That (parameters, Is.Empty);
    }

    [Test]
    public void VirtualSide_NotEqualNull ()
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);
      ICondition condition = GetVirtualSideCondition (BinaryCondition.ConditionKind.NotEqual, new Constant (null));

      whereBuilder.BuildWherePart (condition);

      Assert.AreEqual
          (" WHERE EXISTS (SELECT 1 FROM [detailTable] [xyz] WHERE [xyz].[Student_Detail_to_IndustrialSector_FK] = [is].[IndustrialSector_PK])",
              commandText.ToString ());
      Assert.That (parameters, Is.Empty);
    }

    private ICondition GetVirtualSideCondition (BinaryCondition.ConditionKind kind, IValue rightSide)
    {
      Table table = new Table ("IndustrialSector", "is");
      MemberInfo member = typeof (IndustrialSector).GetProperty ("Student_Detail");
      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, member);
      relatedTable.SetAlias ("xyz");
      VirtualColumn virtualColumn = DatabaseInfoUtility.GetVirtualColumn (StubDatabaseInfo.Instance, table, relatedTable, member);
      return new BinaryCondition (virtualColumn, rightSide, kind);
    }

    private void CheckAppendCriterion_Value (ICriterion value, string expectedString)
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);

      whereBuilder.BuildWherePart (value);

      Assert.AreEqual (" WHERE " + expectedString, commandText.ToString ());
      Assert.AreEqual (0, parameters.Count);
    }

    private void CheckAppendCriterion_BinaryCondition (ICriterion binaryCondition, string expectedOperator)
    {
      StringBuilder commandText = new StringBuilder ();
      List<CommandParameter> parameters = new List<CommandParameter> ();
      WhereBuilder whereBuilder = new WhereBuilder (commandText, parameters);

      whereBuilder.BuildWherePart (binaryCondition);

      Assert.AreEqual (" WHERE @1 " + expectedOperator + " @2", commandText.ToString ());
      Assert.AreEqual (2, parameters.Count);
      Assert.That (parameters, Is.EqualTo (new object[] { new CommandParameter ("@1", "foo"), new CommandParameter ("@2", "foo") }));
    }
  }
}