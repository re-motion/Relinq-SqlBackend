using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;
using Rubicon.Development.UnitTesting;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class BinaryConditionBuilderTest
  {
    [Test]
    public void BuildBinaryConditionPart_BinaryConditions ()
    {
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal),
          "(@1 = @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.NotEqual),
          "(@1 <> @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThan),
          "(@1 < @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThan),
          "(@1 > @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThanOrEqual),
          "(@1 <= @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "(@1 >= @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Like),
          "(@1 LIKE @2)");
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditions_WithColumns ()
    {
      Column c1 = new Column (new Table ("a", "b"), "foo");
      Column c2 = new Column (new Table ("c", "d"), "bar");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.Equal),
          "(([b].[foo] IS NULL AND [d].[bar] IS NULL) OR [b].[foo] = [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.NotEqual),
          "(([b].[foo] IS NULL AND [d].[bar] IS NOT NULL) OR ([b].[foo] IS NOT NULL AND [d].[bar] IS NULL) OR [b].[foo] <> [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.LessThan),
          "([b].[foo] < [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.GreaterThan),
          "([b].[foo] > [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.LessThanOrEqual),
          "(([b].[foo] IS NULL AND [d].[bar] IS NULL) OR [b].[foo] <= [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "(([b].[foo] IS NULL AND [d].[bar] IS NULL) OR [b].[foo] >= [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.Like),
          "([b].[foo] LIKE [d].[bar])");
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditions_WithColumn_LeftSide ()
    {
      Column c1 = new Column (new Table ("a", "b"), "foo");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.Equal),
          "([b].[foo] = @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.NotEqual),
          "([b].[foo] IS NULL OR [b].[foo] <> @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.LessThan),
          "([b].[foo] < @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.GreaterThan),
          "([b].[foo] > @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.LessThanOrEqual),
          "([b].[foo] <= @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "([b].[foo] >= @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.Like),
          "([b].[foo] LIKE @1)", new CommandParameter ("@1", "const"));
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditions_WithColumn_RightSide ()
    {
      Column c1 = new Column (new Table ("a", "b"), "foo");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.Equal),
          "(@1 = [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.NotEqual),
          "([b].[foo] IS NULL OR @1 <> [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.LessThan),
          "(@1 < [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.GreaterThan),
          "(@1 > [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.LessThanOrEqual),
          "(@1 <= [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "(@1 >= [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.Like),
          "(@1 LIKE [b].[foo])", new CommandParameter ("@1", "const"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The binary condition kind 2147483647 is not supported.")]
    public void BuildBinaryConditionPart_InvalidBinaryConditionKind ()
    {
      CheckBuildBinaryConditionPart (new BinaryCondition (new Constant ("foo"), new Constant ("foo"), (BinaryCondition.ConditionKind) int.MaxValue), null);
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditionLeftNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant (null), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      CheckBuildBinaryConditionPart (binaryCondition, "@1 IS NULL", new CommandParameter ("@1", "foo"));
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditionRightNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant ("foo"), new Constant (null), BinaryCondition.ConditionKind.Equal);
      CheckBuildBinaryConditionPart (binaryCondition, "@1 IS NULL", new CommandParameter ("@1", "foo"));
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditionIsNotNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant (null), new Constant ("foo"), BinaryCondition.ConditionKind.NotEqual);
      CheckBuildBinaryConditionPart (binaryCondition, "@1 IS NOT NULL", new CommandParameter ("@1", "foo"));
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditionNullIsNotNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant (null), new Constant (null), BinaryCondition.ConditionKind.NotEqual);
      CheckBuildBinaryConditionPart (binaryCondition, "NULL IS NOT NULL");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Value type PseudoValue is not supported.")]
    public void BuildBinaryConditionPart_InvalidValue ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new PseudoValue(), new Constant (null), BinaryCondition.ConditionKind.NotEqual);
      CheckBuildBinaryConditionPart (binaryCondition, null);
    }

    [Test]
    public void BuildBinaryConditionPart_ContainsCondition ()
    {
      MockRepository mockRepository = new MockRepository ();

      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQuery subQuery = new SubQuery (queryModel, null);
      
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      commandBuilder.AddParameter (1);

      BinaryConditionBuilder binaryConditionBuilderMock = mockRepository.CreateMock<BinaryConditionBuilder> (commandBuilder, StubDatabaseInfo.Instance);
      SqlGeneratorBase subQueryGeneratorMock = mockRepository.CreateMock<SqlGeneratorBase> (queryModel, StubDatabaseInfo.Instance);

      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (binaryConditionBuilderMock, "CreateSqlGeneratorForSubQuery", subQuery, StubDatabaseInfo.Instance,
          commandBuilder)).Return (subQueryGeneratorMock);
      Expect.Call (subQueryGeneratorMock.BuildCommandString ()).Do ((Func<Tuple<string, CommandParameter[]>>) delegate
      {
        commandBuilder.Append ("x");
        commandBuilder.AddParameter (0);
        return null;
      });

      mockRepository.ReplayAll ();
      BinaryCondition binaryCondition = new BinaryCondition(subQuery, new Constant ("foo"), BinaryCondition.ConditionKind.Contains);
      binaryConditionBuilderMock.BuildBinaryConditionPart (binaryCondition);
      mockRepository.VerifyAll ();

      Assert.AreEqual ("@2 IN (x)", commandBuilder.GetCommandText ());
      Assert.That (commandBuilder.GetCommandParameters (),
          Is.EqualTo (new[] { new CommandParameter ("@1", 1), new CommandParameter ("@2", "foo"), new CommandParameter ("@3", 0) }));
    }

    public class PseudoValue : IValue { }


    private static void CheckBuildBinaryConditionPart_Constants (BinaryCondition binaryCondition, string expectedString)
    {
      CheckBuildBinaryConditionPart (binaryCondition, expectedString,
          new CommandParameter ("@1", "foo"), new CommandParameter ("@2", "foo"));
    }

    private static void CheckBuildBinaryConditionPart (BinaryCondition condition, string expectedString,
       params CommandParameter[] expectedParameters)
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      BinaryConditionBuilder binaryConditionBuilder = new BinaryConditionBuilder (commandBuilder, StubDatabaseInfo.Instance);

      binaryConditionBuilder.BuildBinaryConditionPart (condition);

      Assert.AreEqual (expectedString, commandBuilder.GetCommandText());
      Assert.That (commandBuilder.GetCommandParameters(), Is.EqualTo (expectedParameters));
    }
  }
}